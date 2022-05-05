// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace ClimatesCalories
{
    public class Camping
    {
        public static DFPosition CampMapPixel = null;
        public static bool CampDeployed = false;
        public static bool DungeonTent = false;
        public static Vector3 TentPosition;
        public static Quaternion TentRotation;
        public static GameObject Tent = null;
        public static Matrix4x4 TentMatrix;
        public static GameObject Fire = null;
        public static Vector3 FirePosition;
        public static bool FireLit = false;
        public static int CampDmg;
        public const int tentModelID = 41606;
        public const int templateIndex_Tent = 515;
        protected const DaggerfallMessageBox.MessageBoxButtons cancelButton = (DaggerfallMessageBox.MessageBoxButtons)2;
        protected const DaggerfallMessageBox.MessageBoxButtons restButton = (DaggerfallMessageBox.MessageBoxButtons)35;
        protected const DaggerfallMessageBox.MessageBoxButtons packButton = (DaggerfallMessageBox.MessageBoxButtons)36;
        public static bool ironmanOptionsCamp = false;

        public static bool UseCampEquip(DaggerfallUnityItem item, ItemCollection collection)
        {
            if (GameManager.Instance.AreEnemiesNearby(true))
            {
                DaggerfallUI.MessageBox("There are enemies nearby.");
                return false;
            }
            else if (CampDeployed)
            {
                DestroyCamp();
            }
            if (GameManager.Instance.PlayerEnterExit.IsPlayerInside && !GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon)
            {
                    DaggerfallUI.MessageBox("You can not set up your tent indoors.");
                    return false;
            }
            else
            {
                item.LowerCondition(1, GameManager.Instance.PlayerEntity, collection);
                if (item.currentCondition > 0)
                {
                    CampDmg = item.maxCondition - item.currentCondition;
                    collection.RemoveItem(item);
                    DeployTent();
                    return true;
                }
                else
                {
                    DaggerfallUI.MessageBox("Your camping equipment broke.");
                    collection.RemoveItem(item);
                    return false;
                }
            }
        }

        public static void DeployTent(bool fromSave = false)
        {
            if (fromSave == false)
            {
                CampMapPixel = GameManager.Instance.PlayerGPS.CurrentMapPixel;
                SetTentPositionAndRotation();
                DaggerfallUI.MessageBox("You set up camp");
            }
            else
            {
                PlaceTentOnGround();
            }
            //Attempt to load a model replacement
            Tent = MeshReplacement.ImportCustomGameobject(tentModelID, null, TentMatrix);
            Fire = MeshReplacement.ImportCustomFlatGameobject(210, 1, FirePosition, null);
            //Fire = GameObjectHelper.CreateDaggerfallBillboardGameObject(210, 1, null);
            if (Tent == null)
            {
                Tent = GameObjectHelper.CreateDaggerfallMeshGameObject(tentModelID, null);
            }
            if (Fire == null)
            {
                Fire = GameObjectHelper.CreateDaggerfallBillboardGameObject(210, 1, null);
            }
            //Set the model's position in the world

            Tent.transform.SetPositionAndRotation(TentPosition, TentRotation);
            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon)
            {
                FirePosition = (Tent.transform.position + Tent.transform.forward) + (Tent.transform.up * 0.9f);
                Tent.SetActive(false);
            }
            else
            {
                FirePosition = Tent.transform.position + (Tent.transform.forward * 3) + (Tent.transform.up * 0.8f);
                Tent.SetActive(true);
            }

            Fire.transform.SetPositionAndRotation(FirePosition, TentRotation);
            Fire.SetActive(true);
            AddTorchAudioSource(Fire);
            GameObject lightsNode = new GameObject("Lights");
            lightsNode.transform.parent = Fire.transform;
            AddLight(DaggerfallUnity.Instance, Fire, lightsNode.transform);
            CampDeployed = true;
            FireLit = true;
        }        

        public static void RestOrPackTent(RaycastHit hit)
        {
            DaggerfallMessageBox campPopUp = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            if (hit.transform.gameObject.GetInstanceID() == Tent.GetInstanceID())
            {

                string[] message = { "Do you wish to rest or pack up the camp?" };
                campPopUp.SetText(message);
                campPopUp.OnButtonClick += CampPopUp_OnButtonClick;
                campPopUp.AddButton(packButton);
                campPopUp.AddButton(restButton);               
                campPopUp.AddButton(cancelButton);
                campPopUp.Show();
            }
            else
            {
                DaggerfallUI.MessageBox("This is not your camp.");
            }
        }

        public static void RestOrPackFire(RaycastHit hit)
        {
            if (!GameManager.Instance.AreEnemiesNearby(true) && ironmanOptionsCamp)
            {
                Debug.Log("[Climates&Calories] Sending mod message to Ironman Options.");

                ModManager.Instance.SendModMessage("Ironman Options", "campSave");
            }
            DaggerfallMessageBox campPopUp = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            if (Fire != null)
            {
                if (hit.transform.gameObject.GetInstanceID() == Fire.GetInstanceID())
                {
                    string[] message = { "Do you wish to rest?" };
                    campPopUp.SetText(message);
                    campPopUp.OnButtonClick += CampPopUp_OnButtonClick;
                    campPopUp.AddButton(packButton);
                    campPopUp.AddButton(restButton);                    
                    campPopUp.AddButton(cancelButton);
                    campPopUp.Show();
                }
                else
                {
                    ClimateCalories.camping = true;
                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenRestWindow);
                }
            }
            else
            {
                ClimateCalories.camping = true;
                DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenRestWindow);
            }
        }

        private static void CampPopUp_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {

            if (messageBoxButton == restButton)
            {
                sender.CloseWindow();
                if (GameManager.Instance.AreEnemiesNearby(true))
                {
                    DaggerfallUI.MessageBox("There are enemies nearby.");
                }
                else
                {
                    IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
                    ClimateCalories.camping = true;
                    uiManager.PushWindow(new DaggerfallRestWindow(uiManager, true));
                }
            }
            else if (messageBoxButton == packButton)
            {
                if (!GameManager.Instance.PlayerEnterExit.IsPlayerInside && GameManager.Instance.AreEnemiesNearby(true))
                {
                    DaggerfallUI.MessageBox("There are enemies nearby.");
                }
                else
                {
                    DaggerfallUnityItem CampEquip = ItemBuilder.CreateItem(ItemGroups.UselessItems2, ClimateCalories.templateIndex_CampEquip);
                    CampEquip.LowerCondition(CampDmg, GameManager.Instance.PlayerEntity);
                    DestroyCamp();
                    GameManager.Instance.PlayerEntity.Items.AddItem(CampEquip);
                    CampDeployed = false;
                    FireLit = false;
                    TentMatrix = new Matrix4x4();
                    sender.CloseWindow();
                }
            }
            else
            {
                sender.CloseWindow();
            }
        }

        public static void DestroyCamp()
        {
            if (Tent != null)
            {
                Object.Destroy(Tent);
                Object.Destroy(Fire);
                Tent = null;
                Fire = null;
            }
        }

        public static void Destroy_OnTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            DestroyCamp();
        }

        private static void SetTentPositionAndRotation()
        {
            GameObject player = GameManager.Instance.PlayerObject;
            TentPosition = player.transform.position + (player.transform.forward * 3);
            TentMatrix = player.transform.localToWorldMatrix;

            RaycastHit hit;
            Ray ray = new Ray(TentPosition, Vector3.down);
            if (Physics.Raycast(ray, out hit, 10))
            {
                Debug.Log("Setting tent position and rotation");
                TentPosition = hit.point + (Vector3.down * 0.2f);
                TentRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                Debug.Log("Setting tent position and rotation failed");
            }
        }

        private static void PlaceTentOnGround()
        {
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon && !GameManager.Instance.PlayerEnterExit.IsPlayerInside)
            {
                RaycastHit hit;
                Ray rayDown = new Ray(TentPosition, Vector3.down);
                if (Physics.Raycast(rayDown, out hit, 1000))
                {
                    TentPosition = hit.point + (Vector3.down * 0.2f);
                }
                else
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    Vector3 newTentPos = TentPosition;
                    newTentPos.y = player.transform.position.y;
                    Ray rayFromPlayer = new Ray(newTentPos + Vector3.up, Vector3.down);
                    if (Physics.Raycast(rayFromPlayer, out hit, 1000))
                    {
                        TentPosition = hit.point + (Vector3.down * 0.2f);
                        //FirePosition = Tent.transform.position + (Tent.transform.forward * 3) + (Tent.transform.up * 0.6f);
                    }
                    else
                    {
                        Ray rayUp = new Ray(newTentPos + (Vector3.up * 500f), Vector3.down);
                        if (Physics.Raycast(rayUp, out hit, 1000))
                        {
                            TentPosition = hit.point + (Vector3.down * 0.2f);
                            //FirePosition = Tent.transform.position + (Tent.transform.forward * 3) + (Tent.transform.up * 0.6f);
                        }
                    }
                }                
                TentRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }

        public static void OnNewMagicRound_PlaceCamp()
        {
            if (CampDeployed && Tent != null && !GameManager.Instance.PlayerEnterExit.IsPlayerInside)
            {
                //GameObject player = GameObject.FindGameObjectWithTag("Player");
                //float distance = Vector3.Distance(player.transform.position, Tent.transform.position);
                DFPosition player = GameManager.Instance.PlayerGPS.CurrentMapPixel;
                int campX = CampMapPixel.X;
                int campY = CampMapPixel.Y;
                bool sameMapPixel = true;
                if (campX != player.X || campY != player.Y)
                    sameMapPixel = false;
                Debug.Log("[Climates & Calories] OnNewMagicRound_PlaceCamp CampMapPixel = " + Camping.CampMapPixel.ToString());
                Debug.Log("[Climates & Calories] OnNewMagicRound_PlaceCamp player = " + player.ToString());
                Debug.Log("[Climates & Calories] sameMapPixel = " + sameMapPixel.ToString());
                Debug.Log("[Climates & Calories] Tent active = " + Tent.activeSelf.ToString());
                if (!sameMapPixel)
                {
                    Tent.SetActive(false);
                    Fire.SetActive(false);
                }
                else
                {
                    Tent.SetActive(true);
                    Fire.SetActive(true);
                    PlaceTentOnGround();
                }
            }
        }

        private static GameObject AddLight(DaggerfallUnity dfUnity, GameObject obj, Transform parent)
        {
            Vector3 position = FirePosition;
            GameObject go = GameObjectHelper.InstantiatePrefab(dfUnity.Option_DungeonLightPrefab.gameObject, string.Empty, parent, position);
            Light light = go.GetComponent<Light>();
            Color32 fireColor = new Color32(255, 147, 41, 255);
            if (light != null)
            {
                light.color = fireColor;
                light.intensity = 1;
                light.range = 20;
                light.type = LightType.Point;
                light.shadows = LightShadows.Hard;
                light.shadowStrength = 1f;
                light.spotAngle = 140;
            }

            return go;
        }

        private static void AddTorchAudioSource(GameObject go)
        {
            DaggerfallAudioSource c = go.AddComponent<DaggerfallAudioSource>();
            c.AudioSource.dopplerLevel = 0;
            c.AudioSource.rolloffMode = AudioRolloffMode.Linear;
            c.AudioSource.maxDistance = 5f;
            c.AudioSource.volume = 0.7f;
            c.SetSound(SoundClips.Burning, AudioPresets.LoopIfPlayerNear);
        }
    }
}