// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
using System;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;
using System.Collections.Generic;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect.Utility;

namespace ClimatesCalories
{
    [FullSerializer.fsObject("v1")]
    public class ClimateCaloriesSaveData
    {
        public int WetCount;
        public int AttCount;
        public uint Starvation;
        public bool Hungry;
        public bool Starving;
        public int HuntingTimer;
        public int RotDays;
        public DFPosition TentMapPixel;
        public bool TentDeployed;
        public Vector3 TentPosition;
        public Quaternion TentRotation;
        public Matrix4x4 TentMatrix;
    }

    public class ClimateCalories : MonoBehaviour, IHasModSaveData
    {
        public const int templateIndex_CampEquip = 530;
        public const int templateIndex_Rations = 531;
        public const int templateIndex_Waterskin = 539;


        static Mod mod;
        static ClimateCalories instance;

        public Type SaveDataType
        {
            get { return typeof(ClimateCaloriesSaveData); }
        }

        public object NewSaveData()
        {
            return new ClimateCaloriesSaveData
            {
                WetCount = 0,
                AttCount = 0,
                Starvation = 0,
                Hungry = true,
                Starving = false,
                HuntingTimer = 0,
                RotDays = 0,
                TentMapPixel = new DFPosition(),
                TentDeployed = false,
                TentPosition = new Vector3(),
                TentRotation = new Quaternion(),
                TentMatrix = new Matrix4x4()
            };
        }

        public object GetSaveData()
        {
            return new ClimateCaloriesSaveData
            {
                WetCount = wetCount,
                AttCount = attCount,
                Starvation = FillingFood.starvDays,
                Hungry = FillingFood.hungry,
                Starving = FillingFood.starving,
                HuntingTimer = Hunting.huntingTimer,
                RotDays = FillingFood.daysRot,
                TentMapPixel = TentCamp.TentMapPixel,
                TentDeployed = TentCamp.TentDeployed,
                TentPosition = TentCamp.TentPosition,
                TentRotation = TentCamp.TentRotation,
                TentMatrix = TentCamp.TentMatrix
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var climateCaloriesSaveData = (ClimateCaloriesSaveData)saveData;
            wetCount = climateCaloriesSaveData.WetCount;
            attCount = climateCaloriesSaveData.AttCount;
            FillingFood.starvDays = climateCaloriesSaveData.Starvation;
            FillingFood.hungry = climateCaloriesSaveData.Hungry;
            FillingFood.starving = climateCaloriesSaveData.Starving;
            Hunting.huntingTimer = climateCaloriesSaveData.HuntingTimer;
            FillingFood.daysRot = climateCaloriesSaveData.RotDays;
            TentCamp.TentMapPixel = climateCaloriesSaveData.TentMapPixel;
            TentCamp.TentDeployed = climateCaloriesSaveData.TentDeployed;
            TentCamp.TentPosition = climateCaloriesSaveData.TentPosition;
            TentCamp.TentRotation = climateCaloriesSaveData.TentRotation;
            TentCamp.TentMatrix = climateCaloriesSaveData.TentMatrix;

            if (TentCamp.TentDeployed)
            {
                TentCamp.DeployTent(true);
            }
        }

        public const int tentModelID = 41606;
        public const int templateIndex_Tent = 540;

        static bool statusLookUp = true;
        static bool statusInterval = true;
        static int txtIntervals = 5;
        static bool nudePen = true;
        static bool feetPen = true;
        static bool metalHeatCool = true;
        static public bool wetPen = true;
        static bool txtSeverity = true;
        static bool clothDmg = true;
        static bool encumbranceRPR = false;
        public static bool tediousTravel = false;
        static bool climatesCloaksOld = false;
        static bool fillingFoodOld = false;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);         
            go.AddComponent<ClimateCalories>();
            instance = go.AddComponent<ClimateCalories>();
            mod.SaveDataInterface = instance;

            StartGameBehaviour.OnStartGame += ClimatesCalories_OnStartGame;
            EntityEffectBroker.OnNewMagicRound += ClimatesCaloriesEffects_OnNewMagicRound;

            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;

            itemHelper.RegisterCustomItem(ItemApple.templateIndex, ItemGroups.UselessItems2, typeof(ItemApple));
            itemHelper.RegisterCustomItem(ItemOrange.templateIndex, ItemGroups.UselessItems2, typeof(ItemOrange));
            itemHelper.RegisterCustomItem(ItemBread.templateIndex, ItemGroups.UselessItems2, typeof(ItemBread));
            itemHelper.RegisterCustomItem(ItemFish.templateIndex, ItemGroups.UselessItems2, typeof(ItemFish));
            itemHelper.RegisterCustomItem(ItemSaltedFish.templateIndex, ItemGroups.UselessItems2, typeof(ItemSaltedFish));
            itemHelper.RegisterCustomItem(ItemMeat.templateIndex, ItemGroups.UselessItems2, typeof(ItemMeat));

            DaggerfallUnity.Instance.ItemHelper.RegisterItemUseHandler(templateIndex_CampEquip, TentCamp.UseCampingEquipment);
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(templateIndex_CampEquip, ItemGroups.UselessItems2);
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(templateIndex_Waterskin, ItemGroups.UselessItems2);
            DaggerfallUnity.Instance.ItemHelper.RegisterCustomItem(templateIndex_Rations, ItemGroups.UselessItems2);
            PlayerActivate.RegisterCustomActivation(mod, 210, 1, CampfireActivation);
            PlayerActivate.RegisterCustomActivation(mod, 41116, CampfireActivation);
            PlayerActivate.RegisterCustomActivation(mod, 41606, TentCamp.PackUpTent);
        }

        private static void CampfireActivation(RaycastHit hit)
        {
            camping = true;
            DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenRestWindow);
        }

        void Awake()
        {
            Mod rr = ModManager.Instance.GetMod("RoleplayRealism");
            Mod tt = ModManager.Instance.GetMod("TediousTravel");
            Mod ff = ModManager.Instance.GetMod("Filling Food");
            Mod cc = ModManager.Instance.GetMod("Climates&Cloaks");
            if (rr != null)
            {
                ModSettings rrSettings = rr.GetSettings();
                encumbranceRPR = rrSettings.GetBool("Modules", "encumbranceEffects");
            }
            if (tt != null)
            {
                tediousTravel = true;
                txtSeverity = false;
            }
            if (ff != null)
            {
                fillingFoodOld = true;
            }
            if (cc != null)
            {
                climatesCloaksOld = true;
            }
            if (ff != null || cc != null)
            {
                EntityEffectBroker.OnNewMagicRound += ClimateIncomp_OnNewMagicRound;
            }

            mod.IsReady = true;
        }

        static private int txtCount = 4;
        static private int wetWeather = 0;
        static private int wetEnvironment = 0;
        static public int wetCount = 0;
        static private int attCount = 0;
        static private int debuffValue = 0;
     
        static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
        static PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        static EntityEffectManager playerEffectManager = playerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();
        static RaceTemplate playerRace = playerEntity.RaceTemplate;
        static DaggerfallUnity dfUnity = DaggerfallUnity.Instance;

        static private int offSet = -5; //used to make small adjustments to the mod. Negative numbers makes the character freeze more easily.
        static public int baseNatTemp = Dungeon(Climate() + Month() + DayNight()) + Weather();
        static public int natTemp = Resist(baseNatTemp);
        static public int armorTemp = Armor(baseNatTemp);
        static private int charTemp = Resist(RaceTemp() + Clothes(baseNatTemp) + armorTemp - Water(natTemp)) + offSet;
        static public int pureClothTemp = Clothes(baseNatTemp);
        static private int natCharTemp = Resist(baseNatTemp + RaceTemp()+ offSet);
        static public int totalTemp = natTemp + charTemp;
        static private int absTemp = Mathf.Abs(totalTemp);
        static public bool cloak = Cloak();
        static public bool hood = HoodUp();
        static public bool gotDrink = WaterToDrink();
        static public int thirst = 0;
        static public bool camping = false;
        static private bool groundSleep = false;
        static private int sleepTemp = 0;
        static private bool playerIsWading = GameManager.Instance.PlayerMotor.OnExteriorWater == PlayerMotor.OnExteriorWaterMethod.Swimming;

        static private float tickTimeInterval;
        const float stdInterval = 0.5f;
        static private bool lookingUp = false;
        bool statusClosed = true;
        static int travelCounter = 0;

        void Start()
        {
            DaggerfallUI.UIManager.OnWindowChange += UIManager_OnWindowChange;
        }

        void Update()
        {
            if (GameManager.Instance.PlayerMouseLook.Pitch <= -70 && !GameManager.Instance.PlayerMotor.IsSwimming && !GameManager.Instance.PlayerMotor.IsClimbing)
            {
                lookingUp = true;
            }
        
            if (!dfUnity.IsReady || !playerEnterExit || GameManager.IsGamePaused)
                return;

            

            FillingFood.gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            FillingFood.ateTime = playerEntity.LastTimePlayerAteOrDrankAtTavern;
            FillingFood.hunger = FillingFood.gameMinutes - FillingFood.ateTime;
            if (FillingFood.hunger <= 240 && FillingFood.hungry)
            {
                FillingFood.hungry = false;
                FillingFood.starving = false;
                FillingFood.starvDays = 0;
                EntityEffectBroker.OnNewMagicRound += FillingFood.FoodEffects_OnNewMagicRound;
                DaggerfallUI.AddHUDText("You feel invigorated by the meal.");
            }
            else if (FillingFood.starvDays >= 1 && !FillingFood.starving)
            {
                FillingFood.starvDays = (FillingFood.hunger / 1440);
                FillingFood.starving = true;
                DaggerfallUI.AddHUDText("You are starving...");
            }
            else if (FillingFood.starvDays < 1 && FillingFood.starving)
            {
                FillingFood.starving = false;
            }
        }

        private void UIManager_OnWindowChange(object sender, EventArgs e)
        {
            if (DaggerfallUI.UIManager.WindowCount == 0)
                statusClosed = true;

            if (DaggerfallUI.UIManager.WindowCount == 2 && statusClosed)
            {
                DaggerfallMessageBox msgBox = DaggerfallUI.UIManager.TopWindow as DaggerfallMessageBox;
                if (msgBox != null && msgBox.ExtraProceedBinding == InputManager.Instance.GetBinding(InputManager.Actions.Status))
                {
                    // Setup next status info box.
                    TemperatureCalculator();
                    DaggerfallMessageBox newBox = new DaggerfallMessageBox(DaggerfallUI.UIManager, msgBox);

                    List<string> messages = new List<string>();
                    messages.Add(AdviceText.TxtClimate());
                    messages.Add(AdviceText.TxtClothing());
                    messages.Add(AdviceText.TxtAdvice());
                    messages.Add(string.Empty);
                    if (encumbranceRPR)
                    {
                        messages.Add(AdviceText.TxtEncumbrance());
                        messages.Add(AdviceText.TxtEncAdvice());
                        messages.Add(string.Empty);
                    }
                    messages.Add(AdviceText.TxtFood());                   
                    newBox.SetText(messages.ToArray());

                    newBox.ExtraProceedBinding = InputManager.Instance.GetBinding(InputManager.Actions.Status); // set proceed binding
                    newBox.ClickAnywhereToClose = true;
                    msgBox.AddNextMessageBox(newBox);
                    statusClosed = false;
                }
            }
        }

        private static void TemperatureCalculator()
        {
            gotDrink = WaterToDrink();
            baseNatTemp = Climate() + Month() + DayNight() + Weather();
            natTemp = Resist(baseNatTemp);
            armorTemp = Armor(baseNatTemp);
            pureClothTemp = Clothes(natTemp);
            charTemp = Resist(RaceTemp() + pureClothTemp + armorTemp - Water(natTemp)) + offSet;
            natCharTemp = Resist(baseNatTemp + RaceTemp()) + offSet;
            totalTemp = ItemTemp(Dungeon(natTemp) + charTemp);
            absTemp = Mathf.Abs(totalTemp);
            cloak = Cloak();
            hood = HoodUp();

            AdviceText.AdviceDataUpdate();
        }

        // Alternative Textbox code.
        static DaggerfallMessageBox tempInfoBox;
        public static void TextPopup(string[] message)
        {
            if (tempInfoBox == null)
            {
                tempInfoBox = new DaggerfallMessageBox(DaggerfallUI.UIManager);
                tempInfoBox.AllowCancel = true;
                tempInfoBox.ClickAnywhereToClose = true;
                tempInfoBox.ParentPanel.BackgroundColor = Color.clear;
            }

            tempInfoBox.SetText(message);
            DaggerfallUI.UIManager.PushWindow(tempInfoBox);
        }

        private static void ClimatesCalories_OnStartGame(object sender, EventArgs e)
        {
            if (playerGPS.CurrentLocation.Name == "Privateer's Hold")
            {
                wetCount = 100;
                EntityEffectBroker.OnNewMagicRound += Privateer_OnNewMagicRound;
            }
        }

        private static void Privateer_OnNewMagicRound()
        {
            string[] messages = new string[] { "You are cold and wet from the shipwreck.", "You should use the campfire to get dry." };
            TextPopup(messages);
            EntityEffectBroker.OnNewMagicRound -= Privateer_OnNewMagicRound;
        }

        private static void ClimateIncomp_OnNewMagicRound()
        {
            if (climatesCloaksOld)
            {
                DaggerfallUI.SetMidScreenText("Climates&Calories is not compatible with Climates&Cloaks.");
                return;
            }
            if (fillingFoodOld)
            {
                DaggerfallUI.SetMidScreenText("Climates&Calories is not compatible with FillingFood.");
                return;
            }
        }
       
        private static bool cloakly = true;

        private static void ClimatesCaloriesEffects_OnNewMagicRound()
        {
            if (!SaveLoadManager.Instance.LoadInProgress)
            {
                debuffValue = 0;
                //When inside or camping, the counters reset faster and no temp effects are applied.
                if (playerEnterExit.IsPlayerInsideBuilding || (GameManager.IsGamePaused && camping))
                {
                    txtCount = txtIntervals;
                    wetCount = Mathf.Max(wetCount - 2, 0);
                    attCount = Mathf.Max(attCount - 2, 0);
                    FillingFood.FoodRotCounter();
                    FillingFood.Starvation();
                    debuffValue = (int)FillingFood.starvDays * 2;
                    DebuffAtt(debuffValue);
                }
                //When fast traveling counters resets.
                else if (DaggerfallUI.Instance.FadeBehaviour.FadeInProgress && GameManager.Instance.IsPlayerOnHUD)
                {
                    if (travelCounter > 1)
                    {
                        txtCount = txtIntervals;
                        wetCount = 0;
                        attCount = 0;
                        playerEntity.LastTimePlayerAteOrDrankAtTavern = FillingFood.gameMinutes - 260;
                        FillingFood.hungry = false;
                        FillingFood.starving = false;
                        FillingFood.starvDays = 0;
                        travelCounter = 0;
                    }

                    FillingFood.FoodRotCounter();
                }
                //Sleeping outside. Since there is no way currently to interrupt Rest, except with mosnters, I keep track of temp during sleep and appy effects when waking up.
                else if (GameManager.IsGamePaused && !playerEnterExit.IsPlayerInsideBuilding && !Hunting.huntingTime)
                {
                    FillingFood.FoodRotCounter();
                    FillingFood.Starvation();
                    txtCount = txtIntervals;
                    wetCount += wetWeather + wetEnvironment;
                    if (natTemp > 10)
                    {
                        wetCount -= (natTemp / 10);
                        wetCount = Mathf.Max(wetCount, 0);
                    }
                    if (wetCount >= 1 && wetWeather == 0 && wetEnvironment == 0)
                    {
                        wetCount--;
                    }

                    TemperatureCalculator();
                    if (absTemp > sleepTemp)
                    {
                        sleepTemp = absTemp;
                        if (sleepTemp > 10)
                        {
                            groundSleep = true;
                        }
                    }
                    
                }
                //If not camping, bed sleeping or traveling, apply normal C&C effects.
                else
                {                   
                    //Code specifically to mess with FuzzyBean. Anyone else reading this: ignore it and don't tell Fuzzy ;)
                    if (playerEntity.Name == "Daddy Azura" && playerEnterExit.IsPlayerInsideDungeon && cloakly && !GameManager.Instance.AreEnemiesNearby())
                    {
                        int roll = UnityEngine.Random.Range(0, 100);
                        if (roll > 90)
                        {
                            DaggerfallUI.AddHUDText("You are haunted by the ghost of Cloakly IX.");
                            DaggerfallUI.Instance.PlayOneShot(SoundClips.AnimalCat);
                            cloakly = false;
                        }
                    }

                    Debug.Log("[Climates & Calories] NORMAL ROUND");

                    travelCounter = 0;

                    FillingFood.FoodRotCounter();
                    FillingFood.FoodRotter();
                    FillingFood.Starvation();
                    
                    playerIsWading = GameManager.Instance.PlayerMotor.OnExteriorWater == PlayerMotor.OnExteriorWaterMethod.Swimming;
                    int fatigueDmg = 0;
                    camping = false;

                    if (groundSleep)
                    {
                        groundSleep = false;
                        DaggerfallUI.AddHUDText("Sleeping outside was rough...");
                        Debug.Log("[Climates & Calories] sleepTemp = " + sleepTemp.ToString());
                        if (sleepTemp >= playerEntity.CurrentFatigue)
                        {
                            sleepTemp = playerEntity.CurrentFatigue - 1;
                            Debug.Log("[Climates & Calories] adjusted sleepTemp = " + sleepTemp.ToString());
                        }
                        fatigueDmg += sleepTemp;
                        sleepTemp = 0;
                    }

                    TemperatureCalculator();
                    wetCount += wetWeather + wetEnvironment;
                    if (natTemp > 10)
                    {
                        wetCount -= (natTemp / 10);
                        wetCount = Mathf.Max(wetCount, 0);
                    }
                    if (wetCount >= 1 && wetWeather == 0 && wetEnvironment == 0)
                    {
                        wetCount--;
                    }
                    txtCount++;

                    if (natCharTemp > 10 && gotDrink && !GameManager.IsGamePaused)
                    {
                        thirst++;
                        if(thirst >5)
                        {
                            thirst = 0;
                            DrinkWater();
                        }
                    }

                    //Basic mod effect starts here at +/- 10+ by decreasing fatigue.
                    if (absTemp > 10)
                    {
                        fatigueDmg += absTemp / 20;
                        if (playerRace.ID != (int)Races.Argonian)
                        {
                            if (absTemp < 30) { fatigueDmg /= 2; }
                            else { fatigueDmg *= 2; }
                        }
                        //Temperature +/- 30+ and starts debuffing attributes.
                        if (absTemp > 30)
                        {
                            attCount++;
                        }
                        else { attCount = 0; }
                        //Temperature +/- 50+ and starts causing damage.
                        if (absTemp > 50)
                        {
                            { playerEntity.DecreaseHealth((absTemp - 40) / 10); }
                        }
                    }

                    //If hot or cold, clothing might get damaged
                    if ((baseNatTemp > 10 || baseNatTemp < -10) && clothDmg)
                    {
                        int dmgRoll = UnityEngine.Random.Range(0, 100);
                        if (dmgRoll <= 2) { ClothDmg(); }
                    }

                    //If wet, armor might get damaged
                    if (wetCount > 5)
                    {
                        int dmgRoll = UnityEngine.Random.Range(0, 100);
                        if (dmgRoll < 5) { ArmorDmg(); }
                    }

                    //If you look up, midtext displays how the weather is.
                    if (lookingUp)
                    {
                        UpText(natTemp);
                        lookingUp = false;
                        if (statusLookUp)
                        {
                            CharTxt(totalTemp);
                            txtCount = 0;
                        }
                    }

                    //Apply damage for being naked or walking on foot.
                    if (playerRace.ID != (int)Races.Argonian && playerRace.ID != (int)Races.Khajiit && !playerEntity.IsInBeastForm)
                    {
                        NakedDmg(natTemp);
                        if (!playerIsWading & !GameManager.IsGamePaused)
                        {
                            FeetDmg(natTemp);
                        }
                    }

                    //Displays toptext at intervals
                    if (statusInterval)
                    {
                        if (txtCount >= txtIntervals && GameManager.Instance.IsPlayerOnHUD)
                        {
                            CharTxt(totalTemp);
                        }
                    }

                    if (txtCount >= txtIntervals) { txtCount = 0; }

                    //To counter a bug where you have 0 Stamina with no averse effects.
                    if (playerEntity.CurrentFatigue <= 1)
                    {
                        if (tediousTravel)
                        {
                            GameObject go = GameObject.Find("tedious");
                            TediousTravel.TediousTravel tt = go.GetComponent<TediousTravel.TediousTravel>();
                            tt.InterruptFastTravel();
                        }
                        playerEntity.DecreaseHealth(2);
                        if (!GameManager.IsGamePaused) { DaggerfallUI.AddHUDText("You are exhausted and need to rest..."); }
                    }

                    playerEntity.DecreaseFatigue(fatigueDmg, true);

                    //int starvDays = (int)FillingFood.starvDays;
                    //debuffValue = starvDays * 2;

                    if (attCount > 0)
                    {
                        int countOrTemp = Mathf.Min(absTemp - 30, attCount);
                        int tempAttDebuff = Mathf.Max(0, countOrTemp);
                        if (playerEntity.RaceTemplate.ID == (int)Races.Argonian)
                        {
                            if (absTemp > 50) { tempAttDebuff *= 2; }
                            else { tempAttDebuff /= 2; }
                        }
                        debuffValue += tempAttDebuff;
                    }

                    DebuffAtt(debuffValue);
                    Hunting.HuntingRound();
                }
            }
        }

        static bool WaterToDrink()
        {

            List<DaggerfallUnityItem> skins = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, templateIndex_Waterskin);
            foreach (DaggerfallUnityItem skin in skins)
            {
                if (skin.weightInKg > 0.1)
                {
                    if (skin.weightInKg < 2 && (GameManager.Instance.PlayerMotor.IsSwimming || playerIsWading || playerEnterExit.BuildingType == DFLocation.BuildingTypes.Tavern || playerEnterExit.BuildingType == DFLocation.BuildingTypes.Temple))
                    {
                        skin.weightInKg = 2;
                        DaggerfallUI.AddHUDText("You refill your water.");
                    }
                    return true;
                }
            }
            return false;
        }

        static void DrinkWater()
        {
            List<DaggerfallUnityItem> skins = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, templateIndex_Waterskin);
            foreach (DaggerfallUnityItem skin in skins)
            {
                if (skin.weightInKg > 0.1)
                {
                    skin.weightInKg -= 0.1f;
                    if (skin.weightInKg <= 0.1)
                        skin.shortName = "Empty Waterskin";
                        DaggerfallUI.AddHUDText("You drain your waterskin.");
                    break;
                }
            }
        }

        static void RefillWater(float waterAmount)
        {
            float wLeft = 0;
            float skinRoom = 0;
            float fill = 0;
            List<DaggerfallUnityItem> skins = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ClimateCalories.templateIndex_Waterskin);
            foreach (DaggerfallUnityItem skin in skins)
            {
                if (skin.weightInKg < 2)
                {
                    wLeft = waterAmount - skin.weightInKg;
                    skinRoom = 2 - skin.weightInKg;
                    fill = Mathf.Min(skinRoom, wLeft);
                    waterAmount -= fill;
                    skin.weightInKg += Mathf.Min(fill, 2f);
                    DaggerfallUI.AddHUDText("You refill your water.");
                }
            }
        }

        //Adjust temperature for waterskin ( or barrel of grog when added) in inventory.
        static int ItemTemp(int charNatTemp)
        {
            if (charNatTemp > 9 && gotDrink)
            {
                charNatTemp -= 20;
                charNatTemp = Mathf.Max(charNatTemp, 0);
            }
            return charNatTemp;
        }

        //Resist adjusts the number (usually NatTemp or CharTemp) for class resistances.
        static int Resist(int temp)
        {
            int resFire = playerEntity.Resistances.LiveFire;
            int resFrost = playerEntity.Resistances.LiveFrost;

            if (playerEntity.RaceTemplate.ID == (int)Races.Werewolf || playerEntity.RaceTemplate.ID == (int)Races.Wereboar)
            {
                if (playerEntity.IsInBeastForm)
                {
                    resFrost += 40;
                    resFire += 30;
                }
                else
                {
                    resFrost += 10;
                    resFire += 10;
                }
            }
            else if (playerEntity.RaceTemplate.ID == (int)Races.Vampire)
            {
                resFrost += 25;
            }
            if (temp < 0)
            {
                if (playerEntity.RaceTemplate.CriticalWeaknessFlags == DFCareer.EffectFlags.Frost) { resFrost -= 50; }
                else if (playerEntity.RaceTemplate.LowToleranceFlags == DFCareer.EffectFlags.Frost) { resFrost -= 25; }
                else if (playerEntity.RaceTemplate.ResistanceFlags == DFCareer.EffectFlags.Frost) { resFrost += 25; }
                else if (playerEntity.RaceTemplate.ImmunityFlags == DFCareer.EffectFlags.Frost) { resFrost += 50; }
                temp = Mathf.Min(temp + resFrost, 0);
            }
            else
            {
                if (playerEntity.RaceTemplate.CriticalWeaknessFlags == DFCareer.EffectFlags.Fire) { resFire -= 50; }
                else if (playerEntity.RaceTemplate.LowToleranceFlags == DFCareer.EffectFlags.Fire) { resFire -= 25; }
                else if (playerEntity.RaceTemplate.ResistanceFlags == DFCareer.EffectFlags.Fire) { resFire += 25; }
                else if (playerEntity.RaceTemplate.ImmunityFlags == DFCareer.EffectFlags.Fire) { resFire += 50; }
                temp = Mathf.Max(temp - resFire, 0);
            }
            return temp;
        }

        //If inside dungeon, the temperature effects is decreased.
        static int Dungeon(int natTemp)
        {
            if (playerEnterExit.IsPlayerInsideDungeon)
            {
                if (natTemp > -15)
                {
                    natTemp = Mathf.Max((natTemp/2)-20, -15);
                }
                else
                {
                    natTemp = Mathf.Min((natTemp / 2) + 20, -15);
                }
            }
            return natTemp;
        }

        //If naked, may take damage from temperatures.
        static void NakedDmg(int natTemp)
        {
            if (!nudePen) { return; }
            DaggerfallUnityItem chest = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            DaggerfallUnityItem legs = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
            DaggerfallUnityItem aChest = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
            DaggerfallUnityItem aLegs = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
            bool cTop = false;
            bool cBottom = false;

            if (chest != null)
            {
                switch (chest.TemplateIndex)
                {
                    case (int)MensClothing.Short_tunic:
                    case (int)MensClothing.Toga:
                    case (int)MensClothing.Short_shirt:
                    case (int)MensClothing.Short_shirt_with_belt:
                    case (int)MensClothing.Short_shirt_closed_top:
                    case (int)MensClothing.Short_shirt_closed_top2:
                    case (int)MensClothing.Short_shirt_unchangeable:
                    case (int)MensClothing.Long_shirt:
                    case (int)MensClothing.Long_shirt_with_belt:
                    case (int)MensClothing.Long_shirt_unchangeable:
                    case (int)MensClothing.Eodoric:
                    case (int)MensClothing.Kimono:
                    case (int)MensClothing.Open_Tunic:
                    case (int)MensClothing.Long_shirt_closed_top:
                    case (int)MensClothing.Long_shirt_closed_top2:
                    case (int)WomensClothing.Vest:
                    case (int)WomensClothing.Eodoric:
                    case (int)WomensClothing.Short_shirt:
                    case (int)WomensClothing.Short_shirt_belt:
                    case (int)WomensClothing.Short_shirt_closed:
                    case (int)WomensClothing.Short_shirt_closed_belt:
                    case (int)WomensClothing.Short_shirt_unchangeable:
                    case (int)WomensClothing.Long_shirt:
                    case (int)WomensClothing.Long_shirt_belt:
                    case (int)WomensClothing.Long_shirt_unchangeable:
                    case (int)WomensClothing.Peasant_blouse:
                    case (int)WomensClothing.Long_shirt_closed:
                    case (int)WomensClothing.Open_tunic:
                    case (int)MensClothing.Anticlere_Surcoat:
                    case (int)MensClothing.Formal_tunic:
                    case (int)MensClothing.Reversible_tunic:
                    case (int)MensClothing.Dwynnen_surcoat:
                    case (int)WomensClothing.Long_shirt_closed_belt:
                        cTop = true;
                        break;
                    case (int)MensClothing.Priest_robes:
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Evening_gown:
                    case (int)WomensClothing.Casual_dress:
                    case (int)WomensClothing.Strapless_dress:
                    case (int)WomensClothing.Formal_eodoric:                    
                    case (int)WomensClothing.Priestess_robes:
                    case (int)WomensClothing.Plain_robes:
                    case (int)WomensClothing.Day_gown:
                        cTop = true;
                        cBottom = true;
                        break;
                }
            }
            if (!cBottom && legs != null)
            {
                switch (legs.TemplateIndex)
                {
                    case (int)MensClothing.Khajiit_suit:
                    case (int)WomensClothing.Khajiit_suit:
                        cTop = true;
                        cBottom = true;
                        break;
                    case (int)WomensClothing.Wrap:
                    case (int)MensClothing.Wrap:
                    case (int)MensClothing.Short_skirt:
                    case (int)WomensClothing.Tights:
                    case (int)MensClothing.Long_Skirt:
                    case (int)WomensClothing.Long_skirt:
                    case (int)MensClothing.Casual_pants:
                    case (int)MensClothing.Breeches:
                    case (int)WomensClothing.Casual_pants:
                        cBottom = true;
                        break;
                }
            }
            if (cloak || aChest != null) { cTop = true; }
            if (aLegs != null) { cBottom = true; }
            if (!cTop || !cBottom)
            {
                if (playerEnterExit.IsPlayerInSunlight && natTemp > 10)
                {
                    if ((playerEntity.RaceTemplate.ID == (int)Races.DarkElf || playerEntity.RaceTemplate.ID == (int)Races.Redguard))
                    {
                        if (natTemp > 30)
                        {
                            playerEntity.DecreaseHealth(1);
                            if (txtCount >= txtIntervals && cTop)
                            {
                                DaggerfallUI.AddHUDText("The sun burns your bare skin.");
                            }
                            else if (txtCount >= txtIntervals && cBottom)
                            {
                                DaggerfallUI.AddHUDText("The sun burns your bare legs.");
                            }
                        }
                    }
                    else
                    {
                        playerEntity.DecreaseHealth(1);
                        if (txtCount >= txtIntervals && cTop)
                        {
                            DaggerfallUI.AddHUDText("The sun burns your bare skin.");
                        }
                        else if (txtCount >= txtIntervals && cBottom)
                        {
                            DaggerfallUI.AddHUDText("The sun burns your bare legs.");
                        }
                    }
                }
                else if (natTemp < -10)
                {
                    playerEntity.DecreaseHealth(1);
                    if (txtCount >= txtIntervals)
                    { DaggerfallUI.AddHUDText("The cold air numbs your bare skin."); }
                }
            }
        }

        //If bare feet, may take damage from temperatures.
        static void FeetDmg(int natTemp)
        {
            int endBonus = playerEntity.Stats.LiveEndurance/2;
            if (playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet) == null
               && (Mathf.Abs(natTemp) - endBonus > 0)
               && GameManager.Instance.TransportManager.TransportMode == TransportModes.Foot
               && feetPen)
            {
                playerEntity.DecreaseHealth(1);
                if (natTemp > 0 && txtCount >= txtIntervals)
                {
                    DaggerfallUI.AddHUDText("Your bare feet are getting burned.");
                }
                else if (txtCount >= txtIntervals)
                {
                    DaggerfallUI.AddHUDText("Your bare feet are freezing.");
                }
            }
        }

        static int Climate()
        {
            switch (playerGPS.CurrentClimateIndex)
            {
                case (int)MapsFile.Climates.Desert2:
                    return 50;
                case (int)MapsFile.Climates.Desert:
                    return 40;
                case (int)MapsFile.Climates.Subtropical:
                    return 30;
                case (int)MapsFile.Climates.Rainforest:
                    return 20;
                case (int)MapsFile.Climates.Swamp:
                    return 10;
                case (int)MapsFile.Climates.Woodlands:
                    return -10;
                case (int)MapsFile.Climates.HauntedWoodlands:
                    return -20;
                case (int)MapsFile.Climates.MountainWoods:
                    return -30;
                case (int)MapsFile.Climates.Mountain:
                    return -40;
            }
            return 0;
        }

        static int Month()
        {
            switch (DaggerfallUnity.Instance.WorldTime.Now.MonthValue)
            {
                //Spring
                case DaggerfallDateTime.Months.FirstSeed:
                    return -5;
                case DaggerfallDateTime.Months.RainsHand:
                    return 0;
                case DaggerfallDateTime.Months.SecondSeed:
                    return 5;
                //Summer
                case DaggerfallDateTime.Months.Midyear:
                    return 10;
                case DaggerfallDateTime.Months.SunsHeight:
                    return 20;
                case DaggerfallDateTime.Months.LastSeed:
                    return +15;
                //Fall
                case DaggerfallDateTime.Months.Hearthfire:
                    return 0;
                case DaggerfallDateTime.Months.Frostfall:
                    return -5;
                case DaggerfallDateTime.Months.SunsDusk:
                    return -10;
                //Winter
                case DaggerfallDateTime.Months.EveningStar:
                    return -15;
                case DaggerfallDateTime.Months.MorningStar:
                    return -20;
                case DaggerfallDateTime.Months.SunsDawn:
                    return -15;
            }
            return 0;
        }

        static int Weather()
        {
            int temp = 0;
            wetWeather = 0;
            if (!playerEnterExit.IsPlayerInsideDungeon && !playerEnterExit.IsPlayerInsideBuilding)
            {
                bool isRaining = GameManager.Instance.WeatherManager.IsRaining;
                bool isOvercast = GameManager.Instance.WeatherManager.IsOvercast;
                bool isStorming = GameManager.Instance.WeatherManager.IsStorming;
                bool isSnowing = GameManager.Instance.WeatherManager.IsSnowing;

                if (isRaining)
                {
                    temp -= 10;
                    if (wetPen)
                    {
                        if (cloak && hood)
                        {
                            wetWeather = 0;
                        }
                        else if (cloak && !hood)
                        {
                            wetWeather = 1;
                        }
                        else
                        { wetWeather = 3; }
                    }
                }
                else if (isStorming)
                {
                    temp -= 15;
                    if (wetPen)
                    {
                        if (cloak && hood)
                        {
                            wetWeather = 1;
                        }
                        else if (cloak && !hood)
                        {
                            wetWeather = 2;
                        }
                        else
                        { wetWeather = 5; }
                    }
                }
                else if (isSnowing)
                {
                    temp -= 10;
                    if (wetPen)
                    {
                        if (cloak && hood)
                        {
                            wetWeather = 0;
                        }
                        else if (cloak && hood)
                        {
                            wetWeather = 1;
                        }
                        else
                        { wetWeather = 2; }
                    }
                }
                else if (isOvercast)
                {
                    temp -= 8;
                }
                else if (playerRace.ID == (int)Races.Vampire && playerEnterExit.IsPlayerInSunlight)
                {
                    int heat = Resist(Climate() + Month() + DayNight());
                    if (heat > 0 && DaggerfallUnity.Instance.WorldTime.Now.IsDay && !hood)
                    {
                        playerEntity.DecreaseHealth(heat / 5);
                    }
                }
            }
            return temp;
        }

        static int DayNight()
        {
            int clock = DaggerfallUnity.Instance.WorldTime.Now.Hour;

            if ((clock >= 16 || clock <= 7) && !playerEnterExit.IsPlayerInsideDungeon)
            {
                int climate = playerGPS.CurrentClimateIndex;
                int night = 1;

                switch (climate)
                {
                    case (int)MapsFile.Climates.Desert2:
                        night = 4;
                        break;
                    case (int)MapsFile.Climates.Desert:
                        night = 3;
                        break;
                    case (int)MapsFile.Climates.Rainforest:
                    case (int)MapsFile.Climates.Subtropical:
                    case (int)MapsFile.Climates.Swamp:
                    case (int)MapsFile.Climates.Woodlands:
                    case (int)MapsFile.Climates.HauntedWoodlands:
                    case (int)MapsFile.Climates.MountainWoods:
                        night = 1;
                        break;
                    case (int)MapsFile.Climates.Mountain:
                        night = 2;
                        break;
                }
                if ((clock >= 16 && clock <= 19) || (clock >= 4 && clock <= 7))
                { return -10 * night; }
                else
                { return -20 * night; }
            }
            return 0;
        }

        static bool Cloak()
        {
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);

            if (cloak1 != null || cloak2 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static public bool ArmorCovered()
        {
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            if (cloak)
            {
                return true;
            }
            if (chestCloth != null)
            {
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Priest_robes:
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Evening_gown:
                    case (int)WomensClothing.Casual_dress:
                    case (int)WomensClothing.Priestess_robes:
                    case (int)WomensClothing.Plain_robes:
                    case (int)WomensClothing.Day_gown:
                        return true;
                }
            }
            return false;
        }

        static bool HoodUp()
        {
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            bool up = false;
            if (cloak1 != null)
            {
                switch (cloak1.CurrentVariant)
                {
                    case 0:
                    case 3:
                    case 4:
                        up = false;
                        break;
                    case 1:
                    case 2:
                    case 5:
                        up = true;
                        break;
                }
            }
            if (cloak2 != null && !up)
            {
                switch (cloak2.CurrentVariant)
                {
                    case 0:
                    case 3:
                    case 4:
                        up = false;
                        break;
                    case 1:
                    case 2:
                    case 5:
                        up = true;
                        break;
                }
            }
            else if (chestCloth != null)
            {
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Plain_robes:
                        switch (chestCloth.CurrentVariant)
                        {
                            case 0:
                                up = true;
                                break;
                            case 1:
                                up = true;
                                break;
                        }
                        break;
                }
            }
            return up;
        }

        static void ClothDmg()
        {
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            DaggerfallUnityItem feetCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet);
            DaggerfallUnityItem legsCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
            DaggerfallUnityItem rArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightArm);
            DaggerfallUnityItem lArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftArm);
            DaggerfallUnityItem chest = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
            DaggerfallUnityItem legs = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
            DaggerfallUnityItem head = playerEntity.ItemEquipTable.GetItem(EquipSlots.Head);
            DaggerfallUnityItem gloves = playerEntity.ItemEquipTable.GetItem(EquipSlots.Gloves);

            int roll = UnityEngine.Random.Range(1, 10);
            int lRoll = UnityEngine.Random.Range(1, 10);
            DaggerfallUnityItem cloth = cloak2;
            DaggerfallUnityItem lArmor = chest;

            if (cloak)
            {
                if (cloak2 == null ) { cloth = cloak1; }
                switch (roll)
                {
                    case 1:
                    case 2:
                    case 3:
                        break;
                    case 4:
                    case 5:
                        if (cloak1 != null) { cloth = cloak1; }
                        break;
                    case 6:
                    case 7:
                        if (chestCloth != null) { cloth = chestCloth; }
                        break;
                    case 8:
                    case 9:
                        if (legsCloth != null) { cloth = legsCloth; }
                        break;
                    case 10:
                        if (gloves != null) { cloth = gloves; }
                        break;
                }
            }
            else
            {
                cloth = chestCloth;
                switch (roll)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        if (legsCloth != null) { cloth = legsCloth; }
                        break;
                    case 9:
                    case 10:
                        if (gloves != null) { cloth = gloves; }
                        break;
                }
            }

            if (cloth != null)
            {
                cloth.LowerCondition(1, playerEntity);
                if (GameManager.Instance.TransportManager.TransportMode == TransportModes.Foot && feetCloth != null)
                {
                    feetCloth.LowerCondition(1, playerEntity);
                    if (feetCloth.currentCondition < (feetCloth.maxCondition / 10))
                    {
                        DaggerfallUI.AddHUDText("Your " + feetCloth.ItemName.ToString() + " is getting worn out...");
                    }
                }
                if (cloth.currentCondition < (cloth.maxCondition / 10))
                {
                    DaggerfallUI.AddHUDText("Your " + cloth.ItemName.ToString() + " is getting worn out...");
                }
            }

            switch (lRoll)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    break;
                case 5:
                case 6:
                case 7:
                    if (legs != null) { lArmor = legs; }
                    break;
                case 8:
                    if (lArm != null) { lArmor = lArm; }
                    break;
                case 9:
                    if (rArm != null) { lArmor = rArm; }
                    break;
                case 10:
                    if (head != null) { lArmor = head; }
                    break;
            }

            if (lArmor != null)
            {
                if (Leather(lArmor))
                {
                    lArmor.LowerCondition(1, playerEntity);
                    if (lArmor.currentCondition < (cloth.maxCondition / 10))
                    {
                        DaggerfallUI.AddHUDText("Your " + lArmor.ItemName.ToString() + " is getting worn out...");
                    }
                }
            }
        }

        static void ArmorDmg()
        {
            DaggerfallUnityItem rArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightArm);
            DaggerfallUnityItem lArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftArm);
            DaggerfallUnityItem chest = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
            DaggerfallUnityItem legs = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
            DaggerfallUnityItem head = playerEntity.ItemEquipTable.GetItem(EquipSlots.Head);

            int roll = UnityEngine.Random.Range(1, 10);
            DaggerfallUnityItem armor = chest;
            if (chest == null) { armor = legs; }
                switch (roll)
                {
                    case 1:
                    case 2:
                    case 3:
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    if (legs != null) { armor = legs; }
                        break;
                    case 8:
                    if (head != null) { armor = head; }
                        break;                 
                    case 9:
                        if (lArm != null) { armor = lArm; }
                        break;
                    case 10:
                        if (rArm != null) { armor = rArm; }
                        break;
                }

            if (armor != null)
            {
                if (!Leather(armor))
                {
                    int armorDmg = Mathf.Max(armor.maxCondition / 100, 1);
                    armor.LowerCondition(armorDmg, playerEntity);
                    if (armor.currentCondition < (armor.maxCondition / 10))
                    {
                        DaggerfallUI.AddHUDText("Your " + armor.ItemName.ToString() + " is getting rusty...");
                    }
                }
            }

        }

        static bool Leather(DaggerfallUnityItem armor)
        {
            if (armor != null)
            {
                if (armor.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                { return true; }
                else
                { return false; }
            }
            else
            { return false; }
        }

        static int RaceTemp()
        {
            switch (playerEntity.BirthRaceTemplate.ID)
            {
                case (int)Races.Nord:
                    return 5;
                case (int)Races.Breton:
                    return 5;
                case (int)Races.HighElf:
                case (int)Races.WoodElf:
                    return 0;
                case (int)Races.Khajiit:
                case (int)Races.DarkElf:
                case (int)Races.Redguard:
                    return -5;
                case (int)Races.Argonian:
                    return -10;
            }
            return 0;
        }

        static int Clothes(int natTemp)
        {
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            DaggerfallUnityItem feetCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet);
            DaggerfallUnityItem legsCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
            int chest = 0;
            int feet = 0;
            int legs = 0;
            int cloak = 0;
            int temp = 0;

            if (chestCloth != null)
            {                
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Straps:
                    case (int)MensClothing.Armbands:
                    case (int)MensClothing.Fancy_Armbands:
                    case (int)MensClothing.Champion_straps:
                    case (int)MensClothing.Sash:
                    case (int)MensClothing.Challenger_Straps:
                    case (int)MensClothing.Eodoric:
                    case (int)MensClothing.Vest:
                    case (int)WomensClothing.Brassier:
                    case (int)WomensClothing.Formal_brassier:
                    case (int)WomensClothing.Eodoric:
                    case (int)WomensClothing.Formal_eodoric:
                    case (int)WomensClothing.Vest:
                        chest = 1;
                        break;
                    case (int)MensClothing.Short_shirt:
                    case (int)MensClothing.Short_shirt_with_belt:
                    case (int)WomensClothing.Short_shirt:
                    case (int)WomensClothing.Short_shirt_belt:
                        chest = 5;
                        break;
                    case (int)MensClothing.Short_tunic:
                    case (int)MensClothing.Toga:
                    case (int)MensClothing.Short_shirt_closed_top:
                    case (int)MensClothing.Short_shirt_closed_top2:
                    case (int)MensClothing.Short_shirt_unchangeable:
                    case (int)MensClothing.Long_shirt:
                    case (int)MensClothing.Long_shirt_with_belt:
                    case (int)MensClothing.Long_shirt_unchangeable:
                    case (int)WomensClothing.Short_shirt_closed:
                    case (int)WomensClothing.Short_shirt_closed_belt:
                    case (int)WomensClothing.Short_shirt_unchangeable:
                    case (int)WomensClothing.Long_shirt:
                    case (int)WomensClothing.Long_shirt_belt:
                    case (int)WomensClothing.Long_shirt_unchangeable:
                    case (int)WomensClothing.Peasant_blouse:
                    case (int)WomensClothing.Strapless_dress:
                        chest = 8;
                        break;
                    case (int)MensClothing.Open_Tunic:
                    case (int)MensClothing.Long_shirt_closed_top:
                    case (int)MensClothing.Long_shirt_closed_top2:
                    case (int)MensClothing.Kimono:
                    case (int)WomensClothing.Evening_gown:
                    case (int)WomensClothing.Casual_dress:
                    case (int)WomensClothing.Long_shirt_closed:
                    case (int)WomensClothing.Open_tunic:
                        chest = 10;
                        break;
                    case (int)MensClothing.Priest_robes:
                    case (int)MensClothing.Anticlere_Surcoat:
                    case (int)MensClothing.Formal_tunic:
                    case (int)MensClothing.Reversible_tunic:
                    case (int)MensClothing.Dwynnen_surcoat:
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Priestess_robes:
                    case (int)WomensClothing.Plain_robes:
                    case (int)WomensClothing.Long_shirt_closed_belt:
                    case (int)WomensClothing.Day_gown:
                        chest = 15;
                        break;
                }
            }

            if (feetCloth != null)
            {
                switch (feetCloth.TemplateIndex)
                {
                    case (int)MensClothing.Sandals:
                    case (int)WomensClothing.Sandals:
                        feet = 0;
                        break;
                    case (int)MensClothing.Shoes:
                    case (int)WomensClothing.Shoes:
                        feet = 2;
                        break;
                    case (int)MensClothing.Tall_Boots:
                    case (int)WomensClothing.Tall_boots:
                        feet = 4;
                        break;
                    case (int)MensClothing.Boots:
                    case (int)WomensClothing.Boots:
                        if (feetCloth.CurrentVariant == 0)
                        {
                            feet = 5;
                        }
                        else
                        {
                            feet = 0;
                        }
                        break;
                    default:
                        feet = 5;
                        break;
                }

            }
            if (legsCloth != null)
            {
                switch (legsCloth.TemplateIndex)
                {
                    case (int)MensClothing.Loincloth:
                    case (int)WomensClothing.Loincloth:
                        legs = 1;
                        break;
                    case (int)MensClothing.Khajiit_suit:
                    case (int)WomensClothing.Khajiit_suit:
                        legs = 2;
                        break;
                    case (int)MensClothing.Wrap:
                    case (int)MensClothing.Short_skirt:
                    case (int)WomensClothing.Tights:
                    case (int)WomensClothing.Wrap:
                        legs = 4;
                        break;
                    case (int)MensClothing.Long_Skirt:
                    case (int)WomensClothing.Long_skirt:
                        legs = 8;
                        break;
                    case (int)MensClothing.Casual_pants:
                    case (int)MensClothing.Breeches:
                    case (int)WomensClothing.Casual_pants:
                        legs = 10;
                        break;
                }
            }
            if (cloak1 != null)
            {
                int cloak1int = 0;
                switch (cloak1.CurrentVariant)
                {
                    case 0: //closed, hood down
                        cloak1int = 4;
                        break;
                    case 1: //closed, hood up
                        cloak1int = 5;
                        break;
                    case 2: //one shoulder, hood up
                        cloak1int = 3;
                        break;
                    case 3: //one shoulder, hood down
                        cloak1int = 2;
                        break;
                    case 4: //open, hood down
                        cloak1int = 1;
                        break;
                    case 5: //open, hood up
                        cloak1int = 2;
                        break;
                }
                switch (cloak1.TemplateIndex)
                {
                    case (int)MensClothing.Casual_cloak:
                    case (int)WomensClothing.Casual_cloak:
                        cloak += cloak1int;
                        break;
                    case (int)MensClothing.Formal_cloak:
                    case (int)WomensClothing.Formal_cloak:
                        cloak += (cloak1int * 3);
                        break;
                }

            }
            if (cloak2 != null)
            {
                int cloak2int = 0;
                switch (cloak2.CurrentVariant)
                {
                    case 0: //closed, hood down
                        cloak2int = 4;
                        break;
                    case 1: //closed, hood up
                        cloak2int = 5;
                        break;
                    case 2: //one shoulder, hood up
                        cloak2int = 3;
                        break;
                    case 3: //one shoulder, hood down
                        cloak2int = 2;
                        break;
                    case 4: //open, hood down
                        cloak2int = 1;
                        break;
                    case 5: //open, hood up
                        cloak2int = 2;
                        break;
                }
                switch (cloak2.TemplateIndex)
                {
                    case (int)MensClothing.Casual_cloak:
                    case (int)WomensClothing.Casual_cloak:
                        cloak += cloak2int;
                        break;
                    case (int)MensClothing.Formal_cloak:
                    case (int)WomensClothing.Formal_cloak:
                        cloak += (cloak2int * 3);
                        break;
                }
            }
            pureClothTemp = chest + feet + legs + cloak;
            temp = Mathf.Max(pureClothTemp - wetCount, 0);
            if (natTemp > 30 && playerEnterExit.IsPlayerInSunlight && hood)
            { temp -= 10; }
            return temp;
        }

        static int Armor(int natTemp)
        {
            DaggerfallUnityItem rArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightArm);
            DaggerfallUnityItem lArm = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftArm);
            DaggerfallUnityItem chest = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestArmor);
            DaggerfallUnityItem legs = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsArmor);
            DaggerfallUnityItem head = playerEntity.ItemEquipTable.GetItem(EquipSlots.Head);
            int temp = 0;
            int metal = 0;

            if (chest != null)
            {
                switch (chest.NativeMaterialValue & 0xF00)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        temp += 1;
                        break;
                    case (int)ArmorMaterialTypes.Chain:
                        temp += 1;
                        metal += 1;
                        break;
                    default:
                        temp += 3;
                        metal += 4;
                        break;
                }
            }

            if (legs != null)
            {
                switch (legs.NativeMaterialValue & 0xF00)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        temp += 1;
                        break;
                    case (int)ArmorMaterialTypes.Chain:
                        temp += 1;
                        metal += 1;
                        break;
                    default:
                        temp += 2;
                        metal += 3;
                        break;
                }
            }

            if (lArm != null)
            {
                switch (lArm.NativeMaterialValue & 0xF00)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        temp += 1;
                        break;
                    case (int)ArmorMaterialTypes.Chain:
                        temp += 1;
                        break;
                    default:
                        temp += 1;
                        metal += 1;
                        break;
                }

            }
            if (rArm != null)
            {
                switch (rArm.NativeMaterialValue & 0xF00)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        temp += 1;
                        break;
                    case (int)ArmorMaterialTypes.Chain:
                        temp += 1;
                        break;
                    default:
                        temp += 1;
                        metal += 1;
                        break;
                }
            }
            if (head != null)
            {
                switch (head.NativeMaterialValue & 0xF00)
                {
                    case (int)ArmorMaterialTypes.Leather:
                        temp += 2;
                        break;
                    case (int)ArmorMaterialTypes.Chain:
                        temp += 2;
                        metal += 1;
                        break;
                    default:
                        temp += 1;
                        metal += 1;
                        break;
                }
            }
            if (metalHeatCool)
            {
                int metalTemp = (metal * natTemp) / 20;

                if (metalTemp > 0 && playerEnterExit.IsPlayerInSunlight && !ArmorCovered())
                {
                    temp += metalTemp;
                    if (txtCount > txtIntervals && metalTemp > 5) { DaggerfallUI.AddHUDText("Your armor is starting to heat up."); }
                }
                else if (metalTemp < 0)
                {
                    temp += (metalTemp +1) / 2;
                    if (txtCount > txtIntervals && temp < 0) { DaggerfallUI.AddHUDText("Your armor is getting cold."); }
                }
            }
            if (temp > 0) { temp = Mathf.Max(temp - wetCount, 0); }
            return temp;
        }

        static int Water(int natTemp)
        {
            if (!wetPen) { return 0; }
            int temp = 0;
            wetEnvironment = 0;
            if (GameManager.Instance.PlayerMotor.IsSwimming) { wetEnvironment = 300; }
            if (playerIsWading) { wetEnvironment += 50; }
            if (wetCount > 0)
            {
                if (wetCount > 300) { wetCount = 300; }
                temp = (wetCount / 10);
            }
            return temp;
        }

        static void DebuffAtt(int deBuff)
        {
            int currentEn = playerEntity.Stats.PermanentEndurance;
            int currentSt = playerEntity.Stats.PermanentStrength;
            int currentAg = playerEntity.Stats.PermanentAgility;
            int currentInt = playerEntity.Stats.PermanentIntelligence;
            int currentWill = playerEntity.Stats.PermanentWillpower;
            int currentPer = playerEntity.Stats.PermanentPersonality;
            int currentSpd = playerEntity.Stats.PermanentSpeed;
            int[] statMods = new int[DaggerfallStats.Count];
            statMods[(int)DFCareer.Stats.Endurance] = -Mathf.Min(deBuff, currentEn - 5);
            statMods[(int)DFCareer.Stats.Strength] = -Mathf.Min(deBuff, currentSt - 5);
            statMods[(int)DFCareer.Stats.Agility] = -Mathf.Min(deBuff, currentAg - 5);
            statMods[(int)DFCareer.Stats.Intelligence] = -Mathf.Min(deBuff, currentInt - 5);
            statMods[(int)DFCareer.Stats.Willpower] = -Mathf.Min(deBuff, currentWill - 5);
            statMods[(int)DFCareer.Stats.Personality] = -Mathf.Min(deBuff, currentPer - 5);
            statMods[(int)DFCareer.Stats.Speed] = -Mathf.Min(deBuff, currentSpd - 5);
            playerEffectManager.MergeDirectStatMods(statMods);
        }

        static private void UpText(int natTemp)
        {
            if (!playerEnterExit.IsPlayerInsideDungeon)
            { SkyTxt(natTemp); }
            else
            { DungTxt(natTemp); }
        }

        static private void CharTxt(int totalTemp)
        {
            if (wetPen && wetCount > 0) { WetTxt(totalTemp); }
            string tempText = "";
            if (totalTemp > 10)
            {
                if (totalTemp > 50)
                {
                    if (playerRace.ID == (int)Races.Argonian) { tempText = "Soon you will be... too warm... to move..."; }
                    else tempText = "You cannot go on much longer in this heat...";
                }
                else if (totalTemp > 30)
                {
                    if (playerRace.ID == (int)Races.Argonian) { tempText = "The heat... is slowing you down..."; }
                    else tempText = "You are getting dizzy from the heat...";
                }
                else if (totalTemp > 20 && !txtSeverity)
                {
                    if (playerRace.ID == (int)Races.Khajiit) { tempText = "You breathe quickly, trying to cool down..."; }
                    else if (playerRace.ID == (int)Races.Argonian) { tempText = "You are absorbing too much heat..."; }
                    else tempText = "You wipe the sweat from your brow...";
                }
                else if (totalTemp > 10 && !txtSeverity)
                {
                    if (GameManager.Instance.IsPlayerInsideDungeon)
                    { tempText = ""; }
                    else { tempText = "You are a bit warm..."; }
                }
            }
            else if (totalTemp < -10)
            {
                if (totalTemp < -50)
                {
                    if (playerRace.ID == (int)Races.Argonian) { tempText = "Soon you will be... too cold... to move..."; }
                    else tempText = "Your teeth are chattering uncontrollably!";
                }
                else if (totalTemp < -30)
                {
                    if (playerRace.ID == (int)Races.Argonian) { tempText = "The cold... is slowing... you down..."; }
                    else tempText = "The cold is seeping into your bones...";
                }
                else if (totalTemp < -20 && !txtSeverity)
                {
                    if (playerRace.ID == (int)Races.Argonian) { tempText = "You are losing too much heat..."; }
                    else tempText = "You shiver from the cold...";
                }
                else if (totalTemp < -10 && !txtSeverity)
                {
                    if (GameManager.Instance.IsPlayerInsideDungeon)
                    { tempText = ""; }
                    else { tempText = "You are a bit chilly..."; }
                }
            }
            DaggerfallUI.AddHUDText(tempText);
        }

        static private void SkyTxt(int natTemp)
        {
            string tempText = "The weather is temperate.";

            if (natTemp > 2)
            {
                if (natTemp > 60) { tempText = "As hot as the Ashlands of Morrowind."; }
                else if (natTemp > 50) { tempText = "The heat is suffocating."; }
                else if (natTemp > 40) { tempText = "The heat is unrelenting."; }
                else if (natTemp > 30) { tempText = "The air is scorching."; }
                else if (natTemp > 20) { tempText = "The weather is hot."; }
                else if (natTemp > 10) { tempText = "The weather is nice and warm."; }
            }
            else if (natTemp < -3)
            {
                if (natTemp < -60) { tempText = "As cold as the peaks of Skyrim"; }
                else if (natTemp < -50) { tempText = "The cold weather is deadly."; }
                else if (natTemp < -40) { tempText = "The cold is unrelenting."; }
                else if (natTemp < -30) { tempText = "The weather is freezing."; }
                else if (natTemp < -20) { tempText = "The weather is cold."; }
                else if (natTemp < -10) { tempText = "The weather is nice and cool."; }
            }
            DaggerfallUI.SetMidScreenText(tempText);
        }

        static private void DungTxt(int natTemp)
        {
            natTemp = Dungeon(natTemp);
            string tempText = "The air is temperate.";
            if (natTemp > 2)
            {
                if (natTemp > 60) { tempText = "You feel you are trapped in an oven."; }
                else if (natTemp > 50) { tempText = "The air is so warm it is suffocating."; }
                else if (natTemp > 40) { tempText = "The heat in here is awful."; }
                else if (natTemp > 30) { tempText = "The air in here is swelteringly hot."; }
                else if (natTemp > 20) { tempText = "The air in here is very warm."; }
                else if (natTemp > 10) { tempText = "The air in this place is stuffy and warm."; }
                else { tempText = "The air is somewhat warm."; }
            }
            else if (natTemp < -3)
            {
                if (natTemp < -60) { tempText = "You feel you are trapped in a glacier."; }
                else if (natTemp < -50) { tempText = "This place is as cold as ice."; }
                else if (natTemp < -40) { tempText = "The cold is unrelenting."; }
                else if (natTemp < -30) { tempText = "The air in here is freezing."; }
                else if (natTemp < -20) { tempText = "The air in here is very cold."; }
                else if (natTemp < -10) { tempText = "The air in here is chilly."; }
                else { tempText = "The air is cool."; }
            }
            DaggerfallUI.SetMidScreenText(tempText);
        }

        static private void WetTxt(int totalTemp)
        {
            string wetString = ""; 
            if (wetCount > 200) { wetString = "You are completely drenched."; }
            else if (wetCount > 100) { wetString = "You are soaking wet."; }
            else if (wetCount > 50) { wetString = "You are quite wet."; }
            else if (wetCount > 20) { wetString = "You are somewhat wet."; }
            else if (wetCount > 10) { wetString = "You are a bit wet."; }
            DaggerfallUI.AddHUDText(wetString);
            if (totalTemp < -10 && !GameManager.Instance.PlayerMotor.IsSwimming) { DaggerfallUI.AddHUDText("You should make camp and dry off."); }
        }
    }

    public class FillingFood
    {
        DaggerfallUnity dfUnity;
        PlayerEnterExit playerEnterExit;

        //Hunting code WIP
        static float lastTickTime;
        static float tickTimeInterval;

        //Hunting Quest test
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        static public bool hungry = true;
        static public bool starving = false;
        static public uint starvDays = 0;
        static private int starvCounter = 0;
        static public bool rations = RationsToEat();
        static private int foodCount = 0;
        static public uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
        static public uint ateTime = GameManager.Instance.PlayerEntity.LastTimePlayerAteOrDrankAtTavern;
        static public uint hunger = gameMinutes - ateTime;

        private static void GiveMeat(int meatAmount)
        {
            for (int i = 0; i < meatAmount; i++)
            {
                GameManager.Instance.PlayerEntity.Items.AddItem(ItemBuilder.CreateItem(ItemGroups.UselessItems2, ItemMeat.templateIndex));
            }
        }

        static public void Starvation()
        {
            gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            ateTime = GameManager.Instance.PlayerEntity.LastTimePlayerAteOrDrankAtTavern;
            hunger = gameMinutes - ateTime;
            starvDays = (hunger / 1440);
            starvCounter += (int)starvDays;
            rations = RationsToEat();
            if (hunger > 240)
            {
                hungry = true;
            }
            if (starvDays >= 1)
            {
                starving = true;
            }
            if (hungry && starving && rations && starvCounter > 5)
            {
                EatRation();
            }
            else if (!rations && starving && starvCounter > 5)
            {
                playerEntity.DecreaseFatigue(1);
            }
            else if (!starving)
            {
                starvDays = 0;
            }
        }

        static private void EatRation()
        {
            List<DaggerfallUnityItem> sacks = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ClimateCalories.templateIndex_Rations);
            foreach (DaggerfallUnityItem sack in sacks)
            {
                if (sack.weightInKg > 0.1)
                {
                    sack.weightInKg -= 0.1f;
                    playerEntity.LastTimePlayerAteOrDrankAtTavern = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime() - 250;
                    if (sack.weightInKg <= 0.1)
                    {
                        GameManager.Instance.PlayerEntity.Items.RemoveItem(sack);
                        DaggerfallUI.AddHUDText("You empty your ration sack.");
                    }
                    break;
                }
            }
        }

        static private bool RationsToEat()
        {
            List<DaggerfallUnityItem> sacks = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ClimateCalories.templateIndex_Rations);
            foreach (DaggerfallUnityItem sack in sacks)
            {
                if (sack.weightInKg > 0.1)
                {
                    return true;
                }
            }
            return false;
        }

        static private void FoodRot()
        {
            bool rotted = false;
            int rotChance = 0;
            foreach (ItemCollection playerItems in new ItemCollection[] { GameManager.Instance.PlayerEntity.Items, GameManager.Instance.PlayerEntity.WagonItems, GameManager.Instance.PlayerEntity.OtherItems })
            {
                for (int i = 0; i < playerItems.Count; i++)
                {
                    DaggerfallUnityItem item = playerItems.GetItem(i);
                    if (item is AbstractItemFood)
                    {
                        rotChance = UnityEngine.Random.Range(1, 100);
                        AbstractItemFood food = item as AbstractItemFood;
                        if (rotChance > food.maxCondition && !food.RotFood())
                        {
                            food.RotFood();
                            rotted = true;
                        }
                    }
                }
            }
            if (rotted)
            {
                daysRot = 0;
                rotted = false;
                DaggerfallUI.AddHUDText("Your food is getting a bit ripe...");
            }
        }

        private static int rotCounter = 0;
        public static int daysRot = 0;

        public static void FoodRotter()
        {        
            if (daysRot >= 1)
            {
                for (int i = 0; i < daysRot; i++)
                {
                    FoodRot();
                }
                daysRot = 0;
            }
        }

        public static void FoodRotCounter()
        {
            rotCounter++;
            if (rotCounter > 720)
            {
                rotCounter = 0;
                daysRot++;
            }
        }

        public static void FoodEffects_OnNewMagicRound()
        {
            if (hunger < 240)
            {
                foodCount += (240 - (int)hunger);
                if (foodCount >= 500)
                {
                    playerEntity.IncreaseFatigue(1, true);
                    foodCount = 0;
                }
            }
            else
            {
                hungry = true;
                DaggerfallUI.AddHUDText("Your stomach rumbles...");
                EntityEffectBroker.OnNewMagicRound -= FoodEffects_OnNewMagicRound;
            }
        }
    }

    public class TentCamp
    {

        public static DFPosition TentMapPixel = null;
        public static bool TentDeployed = false;
        public static Vector3 TentPosition;
        public static Quaternion TentRotation;
        public static GameObject Tent = null;
        public static Matrix4x4 TentMatrix;

        public const int tentModelID = 41606;
        public const int templateIndex_Tent = 515;

        public static bool UseCampingEquipment(DaggerfallUnityItem item, ItemCollection collection)
        {
            if (GameManager.Instance.PlayerEnterExit.IsPlayerInside == true)
            {
                string[] messages = new string[] { "You can not pitch your tent indoors." };
                ClimateCalories.TextPopup(messages);
                return false;
            }
            else if (TentDeployed == false)
            {
                item.LowerCondition(1, GameManager.Instance.PlayerEntity, collection);
                if (item.currentCondition < 5)
                {
                    DaggerfallUI.AddHUDText("Your camping equipment is falling apart...");
                }
                DeployTent();
                return true;
            }
            else
            {
                string[] messages = new string[] { "You have already pitched your tent." };
                ClimateCalories.TextPopup(messages);
                return false;
            }
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
                TentPosition = hit.point;
                TentRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            else
            {
                Debug.Log("Setting tent position and rotation failed");
            }
        }

        public static void DeployTent(bool fromSave = false)
        {
            if (fromSave == false)
            {
                TentMapPixel = GameManager.Instance.PlayerGPS.CurrentMapPixel;
                SetTentPositionAndRotation();
            }
            //Attempt to load a model replacement
            Tent = MeshReplacement.ImportCustomGameobject(tentModelID, null, TentMatrix);
            if (Tent == null)
            {
                Tent = GameObjectHelper.CreateDaggerfallMeshGameObject(tentModelID, null);
            }
            //Set the model's position in the world
            Tent.transform.SetPositionAndRotation(TentPosition, TentRotation);
            Tent.SetActive(true);
            TentDeployed = true;
        }

        public static void PackUpTent(RaycastHit hit)
        {
            DaggerfallMessageBox tentPopUp = new DaggerfallMessageBox(DaggerfallUI.UIManager, DaggerfallUI.UIManager.TopWindow);
            if (hit.transform.gameObject.GetInstanceID() == Tent.GetInstanceID())
            {
                string[] message = { "Pack up your tent?" };
                tentPopUp.SetText(message);
                tentPopUp.OnButtonClick += TentPopUp_OnButtonClick;
                tentPopUp.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                tentPopUp.AddButton(DaggerfallMessageBox.MessageBoxButtons.No, true);
                tentPopUp.Show();
            }
            else
            {
                string[] message = { "This is not your tent." };
                ClimateCalories.TextPopup(message);
            }
            
        }

        private static void TentPopUp_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                UnityEngine.Object.Destroy(Tent);
                Tent = null;
                TentDeployed = false;
                TentMatrix = new Matrix4x4();
                sender.CloseWindow();
            }
            else
            {
                IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
                ClimateCalories.camping = true;
                uiManager.PushWindow(new DaggerfallRestWindow(uiManager, true));
            }
        }
    }
}