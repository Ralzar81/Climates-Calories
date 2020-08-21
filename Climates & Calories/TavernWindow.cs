
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace ClimatesCalories
{

    public class TavernWindow : DaggerfallTavernWindow
    {
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

        #region UI Rects

        Rect roomButtonRect = new Rect(5, 5, 120, 7);
        Rect talkButtonRect = new Rect(5, 14, 120, 7);
        Rect foodButtonRect = new Rect(5, 23, 120, 7);
        Rect drinksButtonRect = new Rect(5, 32, 120, 7);
        Rect exitButtonRect = new Rect(5, 41, 120, 7);

        #endregion

        #region UI Controls

        Panel mainPanel = new Panel();
        //protected new Button roomButton;
        //protected new Button talkButton;
        protected new Button foodButton;
        protected Button drinksButton;
        //protected new Button exitButton;

        #endregion

        #region UI Textures

        protected new Texture2D baseTexture;

        #endregion

        #region Fields

        const string baseTextureName = "RALZARTAVERN";
        const int tooManyDaysFutureId = 16;
        const int offerPriceId = 262;
        const int notEnoughGoldId = 454;
        const int howManyAdditionalDaysId = 5100;
        const int howManyDaysId = 5102;

        //protected new StaticNPC merchantNPC;
        //protected new PlayerGPS.DiscoveredBuilding buildingData;
        //protected new RoomRental_v1 rentedRoom;
        //protected new int daysToRent = 0;
        //protected new int tradePrice = 0;

        bool isCloseWindowDeferred = false;
        bool isTalkWindowDeferred = false;
        bool isFoodDeferred = false;
        bool isDrinksDeferred = false;

        #endregion



        public TavernWindow(IUserInterfaceManager uiManager, StaticNPC npc)
            : base(uiManager, npc)
        {

        }


        protected override void Setup()
        {
            //base.Setup();

            // Load all textures
            Texture2D tex;
            TextureReplacement.TryImportTexture(baseTextureName, true, out tex);
            Debug.Log("Texture is:" + tex.ToString());
            baseTexture = tex;

            // Create interface panel
            mainPanel.HorizontalAlignment = HorizontalAlignment.Center;
            mainPanel.VerticalAlignment = VerticalAlignment.Middle;
            mainPanel.BackgroundTexture = baseTexture;
            mainPanel.Position = new Vector2(0, 50);
            mainPanel.Size = new Vector2(130, 53);

            // Room button
            roomButton = DaggerfallUI.AddButton(roomButtonRect, mainPanel);
            roomButton.OnMouseClick += RoomButton_OnMouseClick;
            //roomButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernRoom);

            // Talk button
            talkButton = DaggerfallUI.AddButton(talkButtonRect, mainPanel);
            talkButton.OnMouseClick += TalkButton_OnMouseClick;
            //talkButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernTalk);
            talkButton.OnKeyboardEvent += TalkButton_OnKeyboardEvent;

            // Food button
            foodButton = DaggerfallUI.AddButton(foodButtonRect, mainPanel);
            foodButton.OnMouseClick += FoodButton_OnMouseClick;
            //foodButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernFood);
            foodButton.OnKeyboardEvent += FoodButton_OnKeyboardEvent;

            // Drinks button
            drinksButton = DaggerfallUI.AddButton(drinksButtonRect, mainPanel);
            drinksButton.OnMouseClick += DrinksButton_OnMouseClick;
            //drinksButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernFood);
            drinksButton.OnKeyboardEvent += FoodButton_OnKeyboardEvent;

            // Exit button
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            exitButton.OnMouseClick += ExitButton_OnMouseClick;
            //exitButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernExit);
            exitButton.OnKeyboardEvent += ExitButton_OnKeyboardEvent;

            NativePanel.Components.Add(mainPanel);

        }


        #region Event Handlers

        private void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
        }

        protected new void ExitButton_OnKeyboardEvent(BaseScreenComponent sender, Event keyboardEvent)
        {
            if (keyboardEvent.type == EventType.KeyDown)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                isCloseWindowDeferred = true;
            }
            else if (keyboardEvent.type == EventType.KeyUp && isCloseWindowDeferred)
            {
                isCloseWindowDeferred = false;
                CloseWindow();
            }
        }

        private void RoomButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            int mapId = GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.MapId;
            int buildingKey = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;
            GameManager.Instance.PlayerEntity.RemoveExpiredRentedRooms();
            rentedRoom = GameManager.Instance.PlayerEntity.GetRentedRoom(mapId, buildingKey);

            DaggerfallInputMessageBox inputMessageBox = new DaggerfallInputMessageBox(uiManager, this);
            inputMessageBox.SetTextTokens((rentedRoom == null) ? howManyDaysId : howManyAdditionalDaysId, this);
            inputMessageBox.TextPanelDistanceY = 0;
            inputMessageBox.InputDistanceX = 24;
            //inputMessageBox.InputDistanceY = -4;
            inputMessageBox.TextBox.Numeric = true;
            inputMessageBox.TextBox.MaxCharacters = 3;
            inputMessageBox.TextBox.Text = "1";
            inputMessageBox.OnGotUserInput += InputMessageBox_OnGotUserInput;
            inputMessageBox.Show();
        }

        protected override void InputMessageBox_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            daysToRent = 0;
            bool result = int.TryParse(input, out daysToRent);
            if (!result || daysToRent < 1)
                return;

            int daysAlreadyRented = 0;
            if (rentedRoom != null)
            {
                daysAlreadyRented = (int)((rentedRoom.expiryTime - DaggerfallUnity.Instance.WorldTime.Now.ToSeconds()) / DaggerfallDateTime.SecondsPerDay);
                if (daysAlreadyRented < 0)
                    daysAlreadyRented = 0;
            }

            if (daysToRent + daysAlreadyRented > 350)
            {
                DaggerfallUI.MessageBox(tooManyDaysFutureId);
            }
            else if (GameManager.Instance.GuildManager.GetGuild(FactionFile.GuildGroups.KnightlyOrder).FreeTavernRooms())
            {
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("roomFreeForKnightSuchAsYou"));
                RentRoom();
            }
            else
            {
                int cost = FormulaHelper.CalculateRoomCost(daysToRent);
                tradePrice = FormulaHelper.CalculateTradePrice(cost, buildingData.quality, false);

                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRandomTokens(offerPriceId);
                messageBox.SetTextTokens(tokens, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmRenting_OnButtonClick;
                uiManager.PushWindow(messageBox);
            }
        }

        protected override void ConfirmRenting_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                if (playerEntity.GetGoldAmount() >= tradePrice)
                {
                    playerEntity.DeductGoldAmount(tradePrice);
                    RentRoom();
                }
                else
                    DaggerfallUI.MessageBox(notEnoughGoldId);
            }
        }

        protected override void RentRoom()
        {
            int mapId = GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.MapId;
            string sceneName = DaggerfallInterior.GetSceneName(mapId, buildingData.buildingKey);
            if (rentedRoom == null)
            {
                // Get rest markers and select a random marker index for allocated bed
                // We store marker by index as building positions are not stable, they can move from terrain mods or floating Y
                Vector3[] restMarkers = playerEnterExit.Interior.FindMarkers(DaggerfallInterior.InteriorMarkerTypes.Rest);
                int markerIndex = Random.Range(0, restMarkers.Length);

                // Create room rental and add it to player rooms
                RoomRental_v1 room = new RoomRental_v1()
                {
                    name = buildingData.displayName,
                    mapID = mapId,
                    buildingKey = buildingData.buildingKey,
                    allocatedBedIndex = markerIndex,
                    expiryTime = DaggerfallUnity.Instance.WorldTime.Now.ToSeconds() + (ulong)(DaggerfallDateTime.SecondsPerDay * daysToRent)
            };
                playerEntity.RentedRooms.Add(room);
                SaveLoadManager.StateManager.AddPermanentScene(sceneName);
                Debug.LogFormat("Rented room for {1} days. {0}", sceneName, daysToRent);
            }
            else
            {
                rentedRoom.expiryTime += (ulong)(DaggerfallDateTime.SecondsPerDay * daysToRent);
                Debug.LogFormat("Rented room for additional {1} days. {0}", sceneName, daysToRent);
            }
        }

        private void TalkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
            GameManager.Instance.TalkManager.TalkToStaticNPC(merchantNPC);
        }

        void TalkButton_OnKeyboardEvent(BaseScreenComponent sender, Event keyboardEvent)
        {
            if (keyboardEvent.type == EventType.KeyDown)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                isTalkWindowDeferred = true;
            }
            else if (keyboardEvent.type == EventType.KeyUp && isTalkWindowDeferred)
            {
                isTalkWindowDeferred = false;
                CloseWindow();
                GameManager.Instance.TalkManager.TalkToStaticNPC(merchantNPC);
            }
        }


        private void FoodButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DoFood();
        }

        void FoodButton_OnKeyboardEvent(BaseScreenComponent sender, Event keyboardEvent)
        {
            if (keyboardEvent.type == EventType.KeyDown)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                isFoodDeferred = true;
            }
            else if (keyboardEvent.type == EventType.KeyUp && isFoodDeferred)
            {
                isFoodDeferred = false;
                DoFood();
            }
        }

        private void DrinksButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DoDrinks();
        }

        void DrinksButton_OnKeyboardEvent(BaseScreenComponent sender, Event keyboardEvent)
        {
            if (keyboardEvent.type == EventType.KeyDown)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                isDrinksDeferred = true;
            }
            else if (keyboardEvent.type == EventType.KeyUp && isDrinksDeferred)
            {
                isDrinksDeferred = false;
                DoDrinks();
            }
        }

        #endregion









        public static int drunk = 0;


        protected void DoFood()
        {
            CloseWindow();

            int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;

            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();

            DaggerfallListPickerWindow foodAndDrinkPicker = new DaggerfallListPickerWindow(uiManager, this);
            foodAndDrinkPicker.OnItemPicked += Food_OnItemPicked;

            string[] tavernMenu;
            if (tavernQuality < 11)
            {
                if (IsSouth())
                    tavernMenu = southTavernFood1;
                else
                    tavernMenu = northTavernFood1;
            }
            else
            {
                if (IsSouth())
                    tavernMenu = southTavernFood2;
                else
                    tavernMenu = northTavernFood2;
            }

            foreach (string menuItem in tavernMenu)
                foodAndDrinkPicker.ListBox.AddItem(menuItem);

            uiManager.PushWindow(foodAndDrinkPicker);
        }

        protected void Food_OnItemPicked(int index, string foodOrDrinkName)
        {
            int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
            int price;
            if (tavernQuality < 11)
            {
                if (IsSouth())
                    price = southFoodPrices1[index];
                else
                    price = northFoodPrices1[index];
            }
            else
            {
                if (IsSouth())
                    price = southFoodPrices2[index];
                else
                    price = northFoodPrices2[index];
            }
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            uint calories = (uint)price * 10;

            int holidayID = FormulaHelper.GetHolidayId(gameMinutes, 0);

            if (playerEntity.GetGoldAmount() < price)
            {
                DaggerfallUI.MessageBox("You do not have enough gold.");
            }
            else
            {
                playerEntity.DeductGoldAmount(price);
                TavernFood(calories);
            }
        }

        static void TavernFood(uint cals)
        {
            DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
            PassTime(1800);
            DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();

            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            uint hunger = gameMinutes - playerEntity.LastTimePlayerAteOrDrankAtTavern;

            if (hunger >= cals)
            {
                if (hunger > cals + 240)
                {
                    playerEntity.LastTimePlayerAteOrDrankAtTavern = gameMinutes - 240;
                }
                playerEntity.LastTimePlayerAteOrDrankAtTavern += cals;
            }
            else
            {
                DaggerfallUI.MessageBox("You are too full to finish your meal. The rest goes to waste.");
                playerEntity.LastTimePlayerAteOrDrankAtTavern = gameMinutes;
            }
        }


        static readonly string[] northTavernFood1 =  {
            "10 gold          Cabbage Stew",
            "12 gold          Salted Herring",
            "13 gold          Pork Pie",
            "20 gold          Roasted Bass"
        };
        byte[] northFoodPrices1 = { 10, 12, 13, 20 };

        static readonly string[] northTavernFood2 =  {
            "11 gold          Dried Flounder",
            "15 gold          Pork Sausages",
            "16 gold          Roasted Fowl",
            "24 gold          Mutton Stew"
        };
        byte[] northFoodPrices2 = { 11, 15, 16, 24 };

        static readonly string[] southTavernFood1 =  {
            "10 gold          Bean Stew",
            "12 gold          Oiled Sardines",
            "13 gold          Chicken Stew",
            "20 gold          Roasted Goat"
        };
        byte[] southFoodPrices1 = { 10, 12, 13, 20 };

        static readonly string[] southTavernFood2 =  {
            "11 gold          Bean Stew",
            "15 gold          Goat Rolls",
            "16 gold          Curry",
            "24 gold          Mutton Stew"
        };
        byte[] southFoodPrices2 = { 11, 15, 16, 24 };














        protected void DoDrinks()
        {
            CloseWindow();

            if (drunk > (playerEntity.Stats.LiveEndurance + playerEntity.Stats.LiveWillpower + playerEntity.Stats.LivePersonality) / 2)
            {
                DaggerfallUI.MessageBox("I think you've had enough.");
            }
            else
            {
                int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;
                uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();

                DaggerfallListPickerWindow foodAndDrinkPicker = new DaggerfallListPickerWindow(uiManager, this);
                foodAndDrinkPicker.OnItemPicked += Drinks_OnItemPicked;

                string[] tavernMenu;
                if (tavernQuality < 11)
                {
                    if (IsSouth())
                        tavernMenu = southTavernDrinks1;
                    else
                        tavernMenu = northTavernDrinks1;
                }
                else
                {
                    if (IsSouth())
                        tavernMenu = southTavernDrinks2;
                    else
                        tavernMenu = northTavernDrinks2;
                }

                foreach (string menuItem in tavernMenu)
                    foodAndDrinkPicker.ListBox.AddItem(menuItem);

                uiManager.PushWindow(foodAndDrinkPicker);
            }
        }

        protected void Drinks_OnItemPicked(int index, string foodOrDrinkName)
        {
            int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
            int price;
            if (tavernQuality < 11)
            {
                if (IsSouth())
                    price = southDrinksPrices1[index];
                else
                    price = northDrinksPrices1[index];
            }
            else
            {
                if (IsSouth())
                    price = southDrinksPrices2[index];
                else
                    price = northDrinksPrices2[index];
            }
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            int alcohol = alcoholLevel[index];

            int holidayID = FormulaHelper.GetHolidayId(gameMinutes, GameManager.Instance.PlayerGPS.CurrentRegionIndex);

            // Note: In-game holiday description for both New Life Festival and Harvest's End say they offer free drinks.
            if (holidayID == (int)DFLocation.Holidays.Harvest_End || holidayID == (int)DFLocation.Holidays.New_Life)
            {
                if (index >= 5 && price > 10)
                    price = 0;
                Debug.Log("[Climates & Calories] Holiday Drink");
            }
            if (playerEntity.GetGoldAmount() < price)
            {
                DaggerfallUI.MessageBox("You do not have enough gold.");
            }
            else
            {
                playerEntity.DeductGoldAmount(price);
                TavernDrink(alcohol);
                Debug.Log("[Climates & Calories] Drink Price = " + price.ToString());
            }
        }

        static void TavernDrink(int alcohol)
        {
            DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
            PassTime(600);
            DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
            drunk += alcohol;
            Debug.Log("drunk = " + drunk.ToString());
            if (drunk > playerEntity.Stats.LiveEndurance)
                ShitFaced();
            else if (drunk > playerEntity.Stats.LiveEndurance / 2)
                DaggerfallUI.AddHUDText("You are getting drunk...");
            else if (alcohol > 0)
            {
                DaggerfallUI.AddHUDText("The drink fortifies you.");
                playerEntity.IncreaseFatigue(alcohol);
            }
            else
                DaggerfallUI.AddHUDText("The drink refreshes you.");
            playerEntity.IncreaseFatigue(1);
        }






        static readonly string[] northTavernDrinks1 =  {
            " 0 gold          Water",
            " 1 gold          Milk",
            " 5 gold          Apple Cider",
            " 6 gold          Brown Ale",
            " 7 gold          Mead",
            " 8 gold          Red Wine",
            "10 gold          Potato Liquor"
        };
        byte[] northDrinksPrices1 = { 0, 1, 5, 6, 7, 8, 10 };

        static readonly string[] northTavernDrinks2 =  {
            " 0 gold          Water",
            " 1 gold          Apple Juice",
            " 5 gold          Pear Cider",
            " 7 gold          Golden Ale",
            " 8 gold          Cherry Wine",
            " 9 gold          Mulled Wine",
            "13 gold          Quality Wine"
        };
        byte[] northDrinksPrices2 = { 0, 1, 5, 7, 8, 9, 13 };

        static readonly string[] southTavernDrinks1 =  {
            " 0 gold          Water",
            " 1 gold          Goats Milk",
            " 4 gold          Fermented milk",
            " 5 gold          Beer",
            " 6 gold          Peach Wine",
            " 7 gold          Fig Liquor",
            " 8 gold          Jujube Wine"
        };
        byte[] southDrinksPrices1 = { 0, 1, 4, 5, 6, 7, 8 };

        static readonly string[] southTavernDrinks2 =  {
            " 0 gold          Water",
            " 1 gold          Kefir",
            " 5 gold          Beer",
            " 6 gold          Ginger Wine",
            " 7 gold          Fig Wine",
            " 8 gold          Pomgrenade Liquor",
            "13 gold          Quality Liquor"
        };
        byte[] southDrinksPrices2 = { 0, 1, 5, 6, 7, 8, 13 };

        byte[] alcoholLevel = { 0, 0, 10, 12, 16, 20, 25 };


        static bool IsSouth()
        {
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            switch (playerGPS.CurrentClimateIndex)
            {
                case (int)MapsFile.Climates.Desert2:
                case (int)MapsFile.Climates.Desert:
                case (int)MapsFile.Climates.Subtropical:
                case (int)MapsFile.Climates.Rainforest:
                case (int)MapsFile.Climates.Swamp:
                    return true;
                case (int)MapsFile.Climates.Woodlands:
                case (int)MapsFile.Climates.HauntedWoodlands:
                case (int)MapsFile.Climates.MountainWoods:
                case (int)MapsFile.Climates.Mountain:
                    return false;
            }
            return false;
        }

        public static void Drunk()
        {
            if (drunk > 0)
                drunk--;
            else
                drunk = 0;

            if(drunk > playerEntity.Stats.LiveEndurance / 2)
            {
                EntityEffectManager playerEffectManager = GameManager.Instance.PlayerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();

                int alcEffect = drunk - playerEntity.Stats.LiveEndurance / 2;
                int[] statMods = new int[DaggerfallStats.Count];
                int currentAg = playerEntity.Stats.PermanentAgility;
                int currentInt = playerEntity.Stats.PermanentIntelligence;
                int currentWill = playerEntity.Stats.PermanentWillpower;
                int currentPer = playerEntity.Stats.PermanentPersonality;
                int currentSpd = playerEntity.Stats.PermanentSpeed;
                statMods[(int)DFCareer.Stats.Agility] = -Mathf.Min(alcEffect, currentAg - 5);
                statMods[(int)DFCareer.Stats.Intelligence] = -Mathf.Min(alcEffect, currentInt - 5);
                statMods[(int)DFCareer.Stats.Willpower] = -Mathf.Min(alcEffect, currentWill - 5);
                statMods[(int)DFCareer.Stats.Personality] = 20 - Mathf.Min(alcEffect, currentPer - 5);
                statMods[(int)DFCareer.Stats.Speed] = -Mathf.Min(alcEffect, currentSpd - 5);
                playerEffectManager.MergeDirectStatMods(statMods);
            }
        }

        static void PassTime(int timeRaised)
        {
            DaggerfallDateTime timeNow = DaggerfallUnity.Instance.WorldTime.Now;
            timeNow.RaiseTime(timeRaised);
        }

        static void ShitFaced()
        {
            int stats = playerEntity.Stats.LiveLuck + playerEntity.Stats.LivePersonality;
            int roll = Random.Range(0, 200) - stats;
            int playerGold = playerEntity.GetGoldAmount();
            int goldPenalty = Random.Range(1, 3);

            if (roll < 1)
            {
                DaggerfallUI.AddHUDText("You are very drunk...");
            }
            else
            {
                drunk = 0;
                Sleep.sleepyCounter = 0;
                Sleep.wakeOrSleepTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
                DaggerfallUI.Instance.FadeBehaviour.SmashHUDToBlack();
                if (playerGold < 5)
                {
                    PassTime(Random.Range(30000, 110000));
                    if (playerEnterExit.IsPlayerInside)
                        playerEnterExit.TransitionExterior();
                    RandomLocation();
                }
                else
                {
                    playerEntity.DeductGoldAmount(playerGold / goldPenalty);
                    DrunkBed();
                    PassTime(Random.Range(50000, 160000));
                    if (goldPenalty > 1)
                        DaggerfallUI.AddHUDText("Your gold pouch seems lighter...");
                }
                Sleep.sleepyCounter = 0;
                Sleep.wakeOrSleepTime = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
                DaggerfallUI.MessageBox("What happened last night...?.");
                playerEntity.CurrentHealth = playerEntity.MaxHealth;
                playerEntity.CurrentFatigue = playerEntity.MaxFatigue / 3;
                DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
            }
        }

        static void DrunkBed()
        {
            int mapId = GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.MapId;
            int buildingKey = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData.buildingKey;

            RoomRental_v1 rentedRoom = GameManager.Instance.PlayerEntity.GetRentedRoom(mapId, buildingKey);
            PlayerGPS.DiscoveredBuilding buildingData = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData;

            string sceneName = DaggerfallInterior.GetSceneName(mapId, buildingData.buildingKey);

            Vector3[] restMarkers = playerEnterExit.Interior.FindMarkers(DaggerfallInterior.InteriorMarkerTypes.Rest);
            Vector3 allocatedBed;

            if (rentedRoom == null)
            {
                // Get rest markers and select a random marker index for allocated bed
                // We store marker by index as building positions are not stable, they can move from terrain mods or floating Y
                int markerIndex = Random.Range(0, restMarkers.Length);

                // Create room rental and add it to player rooms
                RoomRental_v1 room = new RoomRental_v1()
                {
                    name = buildingData.displayName,
                    mapID = mapId,
                    buildingKey = buildingData.buildingKey,
                    allocatedBedIndex = markerIndex,
                    expiryTime = DaggerfallUnity.Instance.WorldTime.Now.ToSeconds() + (ulong)(DaggerfallDateTime.SecondsPerDay * 1)
                };
                playerEntity.RentedRooms.Add(room);
                SaveLoadManager.StateManager.AddPermanentScene(sceneName);
                Debug.LogFormat("Rented room for {1} days. {0}", sceneName, 1);
            }
            rentedRoom = GameManager.Instance.PlayerEntity.GetRentedRoom(mapId, buildingKey);

            int bedIndex = (rentedRoom.allocatedBedIndex >= 0 && rentedRoom.allocatedBedIndex < restMarkers.Length) ? rentedRoom.allocatedBedIndex : 0;
            allocatedBed = restMarkers[bedIndex];

            if (allocatedBed != Vector3.zero)
            {
                PlayerMotor playerMotor = GameManager.Instance.PlayerMotor;
                playerMotor.transform.position = allocatedBed;
                playerMotor.FixStanding(0.4f, 0.4f);
            }
        }

        private static void RandomLocation()
        {
            int startX = GameManager.Instance.PlayerGPS.CurrentMapPixel.X;
            int startY = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;
            int endPosX = startX + Random.Range(-1, 2);
            int endPosY = startY + Random.Range(-1, 2);
            GameManager.Instance.StreamingWorld.TeleportToCoordinates(endPosX, endPosY, StreamingWorld.RepositionMethods.DirectionFromStartMarker);
        }
    }
}