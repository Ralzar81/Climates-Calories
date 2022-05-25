
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
        protected new Button foodButton;
        protected Button drinksButton;
        protected TextLabel roomLabel = new TextLabel();
        protected TextLabel talkLabel = new TextLabel();
        protected TextLabel foodLabel = new TextLabel();
        protected TextLabel drinksLabel = new TextLabel();
        protected TextLabel goodbyeLabel = new TextLabel();

        #endregion

        #region UI Textures

        protected new Texture2D baseTexture;

        #endregion

        #region Fields

        const string baseTextureName = "BLANKMENU_TAVERN";
        const int tooManyDaysFutureId = 16;
        const int offerPriceId = 262;
        const int notEnoughGoldId = 454;
        const int howManyAdditionalDaysId = 5100;
        const int howManyDaysId = 5102;

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
            roomLabel.Position = new Vector2(0, 1);
            roomLabel.ShadowPosition = Vector2.one;
            roomLabel.HorizontalAlignment = HorizontalAlignment.Center;
            roomLabel.Text = "ROOM";
            roomButton = DaggerfallUI.AddButton(roomButtonRect, mainPanel);
            roomButton.Components.Add(roomLabel);
            roomButton.OnMouseClick += RoomButton_OnMouseClick;
            //roomButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernRoom);

            // Talk button
            talkLabel.Position = new Vector2(0, 10);
            talkLabel.ShadowPosition = Vector2.one;
            talkLabel.HorizontalAlignment = HorizontalAlignment.Center;
            talkLabel.Text = "TALK";
            talkButton = DaggerfallUI.AddButton(talkButtonRect, mainPanel);
            talkButton.Components.Add(talkLabel);
            talkButton.OnMouseClick += TalkButton_OnMouseClick;
            //talkButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernTalk);
            talkButton.OnKeyboardEvent += TalkButton_OnKeyboardEvent;

            // Food button
            foodLabel.Position = new Vector2(0, 19);
            foodLabel.ShadowPosition = Vector2.one;
            foodLabel.HorizontalAlignment = HorizontalAlignment.Center;
            foodLabel.Text = "FOOD";
            foodButton = DaggerfallUI.AddButton(foodButtonRect, mainPanel);
            foodButton.Components.Add(foodLabel);
            foodButton.OnMouseClick += FoodButton_OnMouseClick;
            //foodButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernFood);
            foodButton.OnKeyboardEvent += FoodButton_OnKeyboardEvent;

            // Drinks button
            drinksLabel.Position = new Vector2(0, 1);
            roomLabel.ShadowPosition = Vector2.one;
            drinksLabel.HorizontalAlignment = HorizontalAlignment.Center;
            drinksLabel.Text = "DRINKS";
            drinksButton = DaggerfallUI.AddButton(drinksButtonRect, mainPanel);
            drinksButton.Components.Add(drinksLabel);
            drinksButton.OnMouseClick += DrinksButton_OnMouseClick;
            //drinksButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TavernFood);
            drinksButton.OnKeyboardEvent += FoodButton_OnKeyboardEvent;

            // Exit button
            goodbyeLabel.Position = new Vector2(0, 10);
            goodbyeLabel.ShadowPosition = Vector2.one;
            goodbyeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            goodbyeLabel.Text = "GOODBYE";
            exitButton = DaggerfallUI.AddButton(exitButtonRect, mainPanel);
            drinksButton.Components.Add(goodbyeLabel);
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
                int quality = Mathf.Max((buildingData.quality / 2), 4);
                int cost = FormulaHelper.CalculateRoomCost(daysToRent) * quality;
                
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
        private static int drunkCounter = 0;
        private static bool breakfast = false;

        protected void DoFood()
        {
            CloseWindow();
            breakfast = false;
            int hour = DaggerfallUnity.Instance.WorldTime.Now.Hour;
            if ((hour >= 5) && (hour < DaggerfallDateTime.DawnHour))
            {
                DaggerfallUI.MessageBox("Sorry, breakfast starts at dawn.");
                return;
            }
            else if ((hour >= DaggerfallDateTime.MidnightHour) && (hour < DaggerfallDateTime.DawnHour))
            {
                DaggerfallUI.MessageBox("Sorry, the kitchen is closed for the night.");
                return;
            }
            else if ((hour >= DaggerfallDateTime.DawnHour) && (hour <= DaggerfallDateTime.MidMorningHour))
            {
                breakfast = true;
            }

            int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;

            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();

            DaggerfallListPickerWindow foodAndDrinkPicker = new DaggerfallListPickerWindow(uiManager, this);
            foodAndDrinkPicker.OnItemPicked += Food_OnItemPicked;

            string menu;
            if (breakfast)
                menu = "breakfast";
            else
                menu = regionMenuDay();

            string[] tavernMenu;
            if (tavernQuality < 5)
            {
                if (menu == "breakfast")
                    tavernMenu = breakLow;
                else if (menu == "s")
                    tavernMenu = sLow;
                else if (menu == "se")
                    tavernMenu = seLow;
                else if (menu == "ne")
                    tavernMenu = neLow;
                else if (menu == "b")
                    tavernMenu = neLow;
                else if (menu == "o")
                    tavernMenu = neLow;
                else
                    tavernMenu = nLow;
            }
            else if (tavernQuality < 13)
            {
                if (menu == "breakfast")
                    tavernMenu = breakMid;
                else if (menu == "s")
                    tavernMenu = sMid;
                else if (menu == "se")
                    tavernMenu = seMid;
                else if (menu == "ne")
                    tavernMenu = neMid;
                else if (menu == "b")
                    tavernMenu = neMid;
                else if (menu == "o")
                    tavernMenu = woMid;
                else
                    tavernMenu = nMid;
            }
            else
            {
                if (menu == "breakfast")
                    tavernMenu = breakHigh;
                else if (menu == "s")
                    tavernMenu = sHigh;
                else if (menu == "se")
                    tavernMenu = seHigh;
                else if (menu == "ne")
                    tavernMenu = neHigh;
                else if (menu == "b")
                    tavernMenu = balHigh;
                else if (menu == "o")
                    tavernMenu = neHigh;
                else
                    tavernMenu = nHigh;
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
            string menu = regionMenuDay();
            int price;

            if (tavernQuality < 5)
            {
                if (menu == "s")
                    price = sLowPrices[index];
                else if (menu == "se")
                    price = seLowPrices[index];
                else if (menu == "ne")
                    price = neLowPrices[index];
                else if (menu == "b")
                    price = neLowPrices[index];
                else if (menu == "o")
                    price = neLowPrices[index];
                else
                    price = nLowPrices[index];
            }
            else if (tavernQuality < 13)
            {
                if (menu == "s")
                    price = sMidPrices[index];
                else if (menu == "se")
                    price = seMidPrices[index];
                else if (menu == "ne")
                    price = neMidPrices[index];
                else if (menu == "b")
                    price = neMidPrices[index];
                else if (menu == "o")
                    price = woMidPrices[index];
                else
                    price = nMidPrices[index];
            }
            else
            {
                if (menu == "s")
                    price = sHighPrices[index];
                else if (menu == "se")
                    price = seHighPrices[index];
                else if (menu == "ne")
                    price = neHighPrices[index];
                else if (menu == "b")
                    price = balHighPrices[index];
                else if (menu == "o")
                    price = neHighPrices[index];
                else
                    price = nHighPrices[index];
            }

            if (breakfast)
                price -= 5;

            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            uint cal;
            cal = (uint)Mathf.Min(calories[index] * 10, 240);

            if (playerEntity.GetGoldAmount() < price)
            {
                DaggerfallUI.MessageBox("You do not have enough gold.");
            }
            else
            {
                playerEntity.DeductGoldAmount(price);
                TavernFood(cal);
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
            DaggerfallUI.AddHUDText("You feel invigorated by the meal.");
        }

        byte[] calories = { 80, 120, 150, 200, 240 };

        static readonly string[] breakLow =  {
            "1 gold          Leftovers"
        };

        static readonly string[] nLow =  {
            " 6 gold          Baked Apples",
            " 8 gold          Mystery Sausage",
            "10 gold          Grilled Hare"
        };
        byte[] nLowPrices = { 6, 8, 10 };


        static readonly string[] breakMid =  {
            "2 gold          Gruel",
            "5 gold          Bread and Cheese"
        };

        static readonly string[] nMid =  {
            " 7 gold          Breton Pork Sausage",
            "10 gold          Cheese Pork Schnitzel",
            "12 gold          Hare in Garlic Sauce",
            "15 gold          Highland Rabbit Stew"
        };
        byte[] nMidPrices = { 7, 10, 12, 15 };


        static readonly string[] breakHigh =  {
            " 5 gold          Oatmeal with Berries",
            " 8 gold          Artisinal Pastries",
            "10 gold          Royal Breakfast Plate"
        };

        static readonly string[] nHigh =  {
            "10 gold          Gorapple Cheesecake",
            "13 gold          Apple Cobbler Supreme",
            "15 gold          Peacock Pie",
            "18 gold          Rabbit Gnocchi Ragu",
            "22 gold          Salmon Steak Supreme"
        };
        byte[] nHighPrices = { 10, 13, 15, 18, 22 };



        static readonly string[] neLow =  {
            " 6 gold          Velothis Cabbage Soup",
            " 8 gold          Beetle-Cheese Poutine",
            "10 gold          Eidar Radish Salad"
        };
        byte[] neLowPrices = { 6, 8, 10 };

        static readonly string[] neMid =  {
            " 7 gold          Cabbage Biscuits",
            "10 gold          Potato Porridge",
            "12 gold          Dunmeri Jerked Horse Haunch",
            "15 gold          Solstheim Elk and Scuttle"
        };
        byte[] neMidPrices = { 7, 10, 12, 15 };

        static readonly string[] neHigh =  {
            "10 gold          Indoril Radish Tartlets",
            "13 gold          Vvardenfell Ash Yam Loaf",
            "15 gold          Kwama Egg Quiche",
            "18 gold          Millet-Stuffed Pork Loin",
            "22 gold          Akaviri Pork Fried Rice"
        };
        byte[] neHighPrices = { 10, 13, 15, 18, 22 };


        static readonly string[] sLow =  {
            " 6 gold          Cantaloupe Bread",
            " 8 gold          Fishy Stick",
            "10 gold          Roasted Corn"
        };
        byte[] sLowPrices = { 6, 8, 10 };

        static readonly string[] sMid =  {
            " 7 gold          Beets With Goat Cheese",
            "10 gold          Venison Pie",
            "12 gold          Antelope Stew",
            "15 gold          Parmesan Eels in Watermelon"
        };
        byte[] sMidPrices = { 7, 10, 12, 15 };

        static readonly string[] sHigh =  {
            "10 gold          Roast Anteloupe",
            "13 gold          Melon-Chevre Salad",
            "15 gold          Pork Fried Rice",
            "18 gold          Chili Cheese Corn",
            "22 gold          Supreme Jambalaya"
        };
        byte[] sHighPrices = { 10, 13, 15, 18, 22 };



        static readonly string[] seLow =  {
            " 6 gold          Banana Surprise",
            " 8 gold          Green Bananas With Garlic",
            "10 gold          Banana Cornbread"
        };
        byte[] seLowPrices = { 6, 8, 10 };

        static readonly string[] seMid =  {
            " 7 gold          Banana Millet Muffin",
            "10 gold          Baked Sole With Bananas",
            "12 gold          Chicken-and-Coconut Fried Rice",
            "15 gold          Mistral Banana-Bunny Hash"
        };
        byte[] seMidPrices = { 7, 10, 12, 15 };

        static readonly string[] seHigh =  {
            "10 gold          Clan Mother's Banana Pilaf",
            "13 gold          Stuffed Banana Leaves",
            "15 gold          Jungle Snake Curry",
            "18 gold          Banana-Radish Vichyssoise",
            "22 gold          Spicy Grilled Lizard"
        };
        byte[] seHighPrices = { 10, 13, 15, 18, 22 };


        static readonly string[] balHigh =  {
            "10 gold          Summerset Rainbow Pie",
            "13 gold          Old Aldmeri Gruel",
            "25 gold          Pickled Fish Bowl",
            "18 gold          Direnni Rabbit Bisque",
            "22 gold          Lillandril Summer Sausage"
        };
        byte[] balHighPrices = { 10, 13, 25, 18, 22 };


        static readonly string[] woMid =  {
            " 7 gold          Potato Porridge",
            "10 gold          Orcish Bratwurst On Bun",
            "12 gold          Jerall Carrot Cake",
            "15 gold          Bruma Jugged Rabbit"
        };
        byte[] woMidPrices = { 7, 10, 12, 15 };








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

                string menu = regionMenuDay();
                string[] tavernMenu;
                if (tavernQuality < 5)
                {
                    if (menu == "s")
                        tavernMenu = sLowDrinks;
                    else if (menu == "se")
                        tavernMenu = sLowDrinks;
                    else if (menu == "ne")
                        tavernMenu = neLowDrinks;
                    else if (menu == "b")
                        tavernMenu = neLowDrinks;
                    else if (menu == "o")
                        tavernMenu = woLowDrinks;
                    else
                        tavernMenu = nLowDrinks;
                }
                else if (tavernQuality < 13)
                {
                    if (menu == "s")
                        tavernMenu = sMidDrinks;
                    else if (menu == "se")
                        tavernMenu = sMidDrinks;
                    else if (menu == "ne")
                        tavernMenu = neMidDrinks;
                    else if (menu == "b")
                        tavernMenu = neMidDrinks;
                    else if (menu == "o")
                        tavernMenu = woMidDrinks;
                    else
                        tavernMenu = nMidDrinks;
                }
                else
                {
                    if (menu == "s")
                        tavernMenu = sHighDrinks;
                    else if (menu == "se")
                        tavernMenu = sHighDrinks;
                    else if (menu == "ne")
                        tavernMenu = neHighDrinks;
                    else if (menu == "b")
                        tavernMenu = neHighDrinks;
                    else if (menu == "o")
                        tavernMenu = neHighDrinks;
                    else
                        tavernMenu = nHighDrinks;
                }

                foreach (string menuItem in tavernMenu)
                    foodAndDrinkPicker.ListBox.AddItem(menuItem);

                uiManager.PushWindow(foodAndDrinkPicker);
            }
        }

        protected void Drinks_OnItemPicked(int index, string foodOrDrinkName)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseWindow();
            int price = drinkPrices[index];
            int tavernQuality = playerEnterExit.Interior.BuildingData.Quality;
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            int alcohol;

            if (tavernQuality < 5)
            {
                alcohol = alcoLow[index];
            }
            else if (tavernQuality < 13)
            {
                alcohol = alcoMid[index];
                price *= 2;
            }
            else
            {
                alcohol = alcoHigh[index];
                price *= 3;
            }

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
            PassTime(900);
            DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
            drunk += alcohol;
            Debug.Log("[Climates & Calories]  drunk = " + drunk.ToString());
            if (drunk > playerEntity.Stats.LiveEndurance)
                ShitFaced();
            else if (drunk > playerEntity.Stats.LiveEndurance / 2)
                DaggerfallUI.AddHUDText("You are getting drunk...");
            else if (alcohol > 0)
            {
                DaggerfallUI.AddHUDText("The drink fortifies you.");
                playerEntity.IncreaseFatigue(alcohol, true);
            }
            else
            {
                DaggerfallUI.AddHUDText("The drink refreshes you.");
                playerEntity.IncreaseFatigue(5, true);
            }
        }




        byte[] drinkPrices = { 1, 2, 2, 3, 4, 6, 7, 8, 10 };

        static readonly string[] nLowDrinks =  {
            " 1 gold          Goats Milk",
            " 2 gold          Spruce Tea",
            " 2 gold          Apple Cider",
            " 3 gold          Ale",
            " 4 gold          Moonshine"
        };

        static readonly string[] nMidDrinks =  {
            " 2 gold          Cows Milk",
            " 4 gold          Herbal Tea",
            " 4 gold          Ale",
            " 6 gold          Bitter",
            " 8 gold          Mulled Wine",
            "12 gold          Red Wine",
            "14 gold          Rye Liquor"
        };

        static readonly string[] nHighDrinks =  {
            " 3 gold          Berry Juice",
            " 6 gold          Herbal Tea",
            " 6 gold          Mint Tea",
            " 9 gold          Ale",
            "12 gold          Bitter",
            "18 gold          Port",
            "21 gold          Mulled Wine",
            "24 gold          Red Wine",
            "30 gold          Nereid Wine"
        };

        static readonly string[] neLowDrinks =  {
            " 1 gold          Goats Milk",
            " 2 gold          Berry Juice",
            " 2 gold          Pear Cider",
            " 3 gold          Ale",
            " 4 gold          Morrowind Mazte"
        };

        static readonly string[] neMidDrinks =  {
            " 2 gold          Fruit Juice",
            " 4 gold          Mint Tea",
            " 4 gold          Ale",
            " 6 gold          Weat Beer",
            " 8 gold          Bitter",
            "12 gold          Acai Mazte",
            "14 gold          Vvrdenfell Flin"
        };

        static readonly string[] neHighDrinks =  {
            " 3 gold          Fruit Juice",
            " 6 gold          Herbal Tea",
            " 6 gold          Mint Tea",
            " 9 gold          Golden Ale",
            "12 gold          Stout",
            "18 gold          Mulled Wine",
            "21 gold          Port Wine",
            "24 gold          Nereid Wine",
            "30 gold          Cyrodiil Brandy"
        };

        static readonly string[] sLowDrinks =  {
            " 1 gold          Camel Milk",
            " 2 gold          Coffee",
            " 2 gold          Beer",
            " 3 gold          Stout",
            " 4 gold          Rum"
        };

        static readonly string[] sMidDrinks =  {
            " 2 gold          Fruit Juice",
            " 4 gold          Coffee",
            " 4 gold          Beer",
            " 6 gold          Stout",
            " 8 gold          Bitter",
            "12 gold          Wine",
            "14 gold          Rum"
        };

        static readonly string[] sHighDrinks =  {
            " 3 gold          Fruit Juice",
            " 6 gold          Coffee",
            " 6 gold          Chai Tea",
            " 9 gold          Weat Beer",
            "12 gold          Beer",
            "18 gold          Stout",
            "21 gold          Bitter",
            "24 gold          Wine",
            "30 gold          Summerset Wine"
        };

        static readonly string[] woLowDrinks =  {
            " 1 gold          Goats Milk",
            " 2 gold          Berry Juice",
            " 2 gold          Ale",
            " 3 gold          Mead",
            " 4 gold          Orc Grog"
        };

        static readonly string[] woMidDrinks =  {
            " 2 gold          Berry Juice",
            " 4 gold          Mint Tea",
            " 4 gold          Ale",
            " 6 gold          Mead",
            "18 gold          Mulled Wine",
            "12 gold          Red Wine",
            "14 gold          Pine Rye"
        };

        static readonly string[] woHighDrinks =  {
            " 3 gold          Berry Juice",
            " 6 gold          Herbal Tea",
            " 6 gold          Mint Tea",
            " 9 gold          Ale",
            "12 gold          Meat",
            "18 gold          Stout",
            "21 gold          Mulled Wine",
            "24 gold          Red Wine",
            "30 gold          Cyrodiil Brandy"
        };

        

        byte[] alcoLow = { 0, 0, 10, 12, 25};
        byte[] alcoMid = { 0, 0, 10, 12, 15, 20, 30 };
        byte[] alcoHigh = { 0, 0, 0, 10, 12, 15, 20, 20, 40 };


        static string regionMenuDay()
        {
            //0 = Balfiera
            //1 = North
            //2 = NorthEast
            //3 = South
            //4 = SouthEast
            //5 = Orisium and Wrothgarian

            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            switch (playerGPS.CurrentRegionIndex)
            {
                case Regions.Anticlere:
                case Regions.Betony:
                case Regions.Bhoraine:
                case Regions.Daenia:
                case Regions.Daggerfall:
                case Regions.Dwynnen:
                case Regions.Glenpoint:
                case Regions.GlenumbraMoors:
                case Regions.IlessanHills:
                case Regions.Kambria:
                case Regions.Northmoor:
                case Regions.Phrygias:
                case Regions.Shalgora:
                case Regions.Tulune:
                case Regions.Urvaius:
                case Regions.Ykalon:
                    return "n";
                case Regions.Alcaire:
                case Regions.Gavaudon:
                case Regions.Koegria:
                case Regions.Menevia:
                case Regions.Wayrest:
                    return "ne";
                case Regions.Kozanset:
                case Regions.Lainlyn:
                case Regions.Mournoth:
                case Regions.Satakalaam:
                case Regions.Totambu:
                    return "se";
                case Regions.AbibonGora:
                case Regions.AlikrDesert:
                case Regions.Antipyllos:
                case Regions.Ayasofya:
                case Regions.Bergama:
                case Regions.Cybiades:
                case Regions.DakFron:
                case Regions.Dragontail:
                case Regions.Ephesus:
                case Regions.Kairou:
                case Regions.Myrkwasa:
                case Regions.Pothago:
                case Regions.Santaki:
                case Regions.Sentinel:
                case Regions.Tigonus:
                    return "s";
                case Regions.Balfiera:
                    return "b";
                case Regions.Orsinium:
                case Regions.Wrothgarian:
                    return "o";

            }

            switch (playerGPS.CurrentClimateIndex)
            {
                case (int)MapsFile.Climates.Desert2:
                case (int)MapsFile.Climates.Desert:
                case (int)MapsFile.Climates.Subtropical:
                    return "s";
                case (int)MapsFile.Climates.Rainforest:
                case (int)MapsFile.Climates.Swamp:
                    return "se";
                case (int)MapsFile.Climates.Woodlands:
                case (int)MapsFile.Climates.HauntedWoodlands:
                case (int)MapsFile.Climates.MountainWoods:
                case (int)MapsFile.Climates.Mountain:
                    return "n";
            }
            return "n";
        }

        public static void Drunk()
        {
            if (drunk > 0)
            {
                drunkCounter++;
                if (drunkCounter > 10)
                {
                    drunkCounter = 0;
                    drunk--;
                }
            }

            if(drunk > playerEntity.Stats.LiveEndurance / 2)
            {
                EntityEffectManager playerEffectManager = GameManager.Instance.PlayerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();

                int alcEffect = (drunk - (playerEntity.Stats.LiveEndurance/2)) / 10;
                int[] statMods = new int[DaggerfallStats.Count];
                statMods[(int)DFCareer.Stats.Agility] = -Mathf.Min(alcEffect, playerEntity.Stats.PermanentAgility - 5);
                statMods[(int)DFCareer.Stats.Intelligence] = -Mathf.Min(alcEffect, playerEntity.Stats.PermanentIntelligence - 5);
                statMods[(int)DFCareer.Stats.Willpower] = -Mathf.Min(alcEffect, playerEntity.Stats.PermanentWillpower - 5);
                statMods[(int)DFCareer.Stats.Personality] = 20 - Mathf.Min(alcEffect, playerEntity.Stats.PermanentPersonality - 5);
                statMods[(int)DFCareer.Stats.Speed] = -Mathf.Min(alcEffect, playerEntity.Stats.PermanentSpeed - 5);
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
            int playerGold = Mathf.Max(playerEntity.GoldPieces, 4);
            int goldPenalty = Random.Range(2, playerGold);

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
                    playerEntity.GoldPieces -= (Mathf.Max(playerGold / goldPenalty, 1));
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

    class Regions
    {
        public const int AlikrDesert = 0;
        public const int Dragontail = 1;
        public const int GlenpointF = 2;
        public const int DaggerfallBluffs = 3;
        public const int Yeorth = 4;
        public const int Dwynnen = 5;
        public const int Ravennian = 6;
        public const int Devilrock = 7;
        public const int Malekna = 8;
        public const int Balfiera = 9;
        public const int Bantha = 10;
        public const int DakFron = 11;
        public const int WesternIsles = 12;
        public const int Tamaril = 13;
        public const int LainlynC = 14;
        public const int Bjoulae = 15;
        public const int Wrothgarian = 16;
        public const int Daggerfall = 17;
        public const int Glenpoint = 18;
        public const int Betony = 19;
        public const int Sentinel = 20;
        public const int Anticlere = 21;
        public const int Lainlyn = 22;
        public const int Wayrest = 23;
        public const int GenTemHighRock = 24;
        public const int GenRaiHammerfell = 25;
        public const int Orsinium = 26;
        public const int SkeffingtonW = 27;
        public const int HammerfellBay = 28;
        public const int HammerfellCoast = 29;
        public const int HighRockBay = 30;
        public const int HighRockSea = 31;
        public const int Northmoor = 32;
        public const int Menevia = 33;
        public const int Alcaire = 34;
        public const int Koegria = 35;
        public const int Bhoraine = 36;
        public const int Kambria = 37;
        public const int Phrygias = 38;
        public const int Urvaius = 39;
        public const int Ykalon = 40;
        public const int Daenia = 41;
        public const int Shalgora = 42;
        public const int AbibonGora = 43;
        public const int Kairou = 44;
        public const int Pothago = 45;
        public const int Myrkwasa = 46;
        public const int Ayasofya = 47;
        public const int Tigonus = 48;
        public const int Kozanset = 49;
        public const int Satakalaam = 50;
        public const int Totambu = 51;
        public const int Mournoth = 52;
        public const int Ephesus = 53;
        public const int Santaki = 54;
        public const int Antipyllos = 55;
        public const int Bergama = 56;
        public const int Gavaudon = 57;
        public const int Tulune = 58;
        public const int GlenumbraMoors = 59;
        public const int IlessanHills = 60;
        public const int Cybiades = 61;
    }
}