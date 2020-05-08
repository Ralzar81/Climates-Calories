// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;

namespace ClimatesCalories
{
    public class AdviceText
    {
        static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
        static PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        static RaceTemplate playerRace = playerEntity.RaceTemplate;

        static private int wetCount = ClimateCalories.wetCount;
        static private int baseNatTemp = ClimateCalories.baseNatTemp;
        static private int natTemp = ClimateCalories.natTemp;
        static private int armorTemp = ClimateCalories.armorTemp;
        static private int pureClothTemp = ClimateCalories.pureClothTemp;
        static private int totalTemp = ClimateCalories.totalTemp;
        static private bool cloak = ClimateCalories.cloak;
        static private bool hood = ClimateCalories.hood;
        static private bool drink = ClimateCalories.gotDrink;
        static private uint hunger = FillingFood.hunger;
        static private bool starving = FillingFood.starving;
        static private bool rations = FillingFood.rations;

        public static void AdviceDataUpdate()
        {
            wetCount = ClimateCalories.wetCount;
            baseNatTemp = ClimateCalories.baseNatTemp;
            natTemp = ClimateCalories.natTemp;
            armorTemp = ClimateCalories.armorTemp;
            pureClothTemp = ClimateCalories.pureClothTemp;
            totalTemp = ClimateCalories.totalTemp;
            cloak = ClimateCalories.cloak;
            hood = ClimateCalories.hood;
            drink = ClimateCalories.gotDrink;
            hunger = FillingFood.hunger;
            starving = FillingFood.starving;
            rations = FillingFood.rations;
        }

        public static string TxtClimate()
        {
            string temperatureTxt = "mild ";
            string weatherTxt = "";
            string seasonTxt = " summer";
            string timeTxt = " in ";
            string climateTxt = "";
            string suitabilityTxt = " is suitable for you.";

            int climate = playerGPS.CurrentClimateIndex;

            bool isRaining = GameManager.Instance.WeatherManager.IsRaining;
            bool isOvercast = GameManager.Instance.WeatherManager.IsOvercast;
            bool isStorming = GameManager.Instance.WeatherManager.IsStorming;
            bool isSnowing = GameManager.Instance.WeatherManager.IsSnowing;

            if (baseNatTemp >= 10)
            {
                if (baseNatTemp >= 50)
                {
                    temperatureTxt = "scorching";
                }
                else if (baseNatTemp >= 30)
                {
                    temperatureTxt = "hot";
                }
                else
                {
                    temperatureTxt = "warm";
                }
            }
            else if (baseNatTemp <= -10)
            {
                if (baseNatTemp <= -50)
                {
                    temperatureTxt = "freezing";
                }
                else if (baseNatTemp <= -30)
                {
                    temperatureTxt = "cold";
                }
                else
                {
                    temperatureTxt = "cool";
                }
            }
            if (!GameManager.Instance.IsPlayerInsideDungeon)
            {
                if (isRaining)
                {
                    weatherTxt = " and rainy";
                }
                else if (isStorming)
                {
                    weatherTxt = " and stormy";
                }
                else if (isOvercast)
                {
                    weatherTxt = " and foggy";
                }
                else if (isSnowing)
                {
                    weatherTxt = " and snowy";
                }
                else if (playerEnterExit.IsPlayerInSunlight)
                {
                    weatherTxt = " and sunny";
                }
            }

            switch (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue)
            {
                //Spring
                case DaggerfallDateTime.Seasons.Fall:
                    seasonTxt = " fall";
                    break;
                case DaggerfallDateTime.Seasons.Spring:
                    seasonTxt = " spring";
                    break;
                case DaggerfallDateTime.Seasons.Winter:
                    seasonTxt = " winter";
                    break;
            }

            if (!GameManager.Instance.IsPlayerInsideDungeon)
            {
                int clock = DaggerfallUnity.Instance.WorldTime.Now.Hour;

                if (clock >= 4 && clock <= 7)
                {
                    timeTxt = " morning in ";
                }
                else if (clock >= 16 && clock <= 19)
                {
                    timeTxt = " evening in ";
                }
                else if (DaggerfallUnity.Instance.WorldTime.Now.IsNight)
                {
                    timeTxt = " night in ";
                }
                else
                {
                    timeTxt = " day in ";
                }
            }

            if (GameManager.Instance.IsPlayerInsideDungeon)
            {
                switch (climate)
                {
                    case (int)MapsFile.Climates.Desert2:
                    case (int)MapsFile.Climates.Desert:
                        climateTxt = "desert dungeon";
                        break;
                    case (int)MapsFile.Climates.Rainforest:
                    case (int)MapsFile.Climates.Subtropical:
                        climateTxt = "tropical dungeon";
                        break;
                    case (int)MapsFile.Climates.Swamp:
                        climateTxt = "swampy dungeon";
                        break;
                    case (int)MapsFile.Climates.Woodlands:
                    case (int)MapsFile.Climates.HauntedWoodlands:
                        climateTxt = "woodlands dungeon";
                        break;
                    case (int)MapsFile.Climates.MountainWoods:
                    case (int)MapsFile.Climates.Mountain:
                        climateTxt = "mountain dungeon";
                        break;
                }
            }
            else
            {
                switch (climate)
                {
                    case (int)MapsFile.Climates.Desert2:
                    case (int)MapsFile.Climates.Desert:
                        climateTxt = "the desert";
                        break;
                    case (int)MapsFile.Climates.Rainforest:
                    case (int)MapsFile.Climates.Subtropical:
                        climateTxt = "the tropics";
                        break;
                    case (int)MapsFile.Climates.Swamp:
                        climateTxt = "the swamps";
                        break;
                    case (int)MapsFile.Climates.Woodlands:
                    case (int)MapsFile.Climates.HauntedWoodlands:
                        climateTxt = "the woodlands";
                        break;
                    case (int)MapsFile.Climates.MountainWoods:
                    case (int)MapsFile.Climates.Mountain:
                        climateTxt = "the mountains";
                        break;
                }
            }


            if (playerRace.ID == (int)Races.Vampire && playerEnterExit.IsPlayerInSunlight)
            {
                if (natTemp > 0 && DaggerfallUnity.Instance.WorldTime.Now.IsDay && !hood)
                {
                    suitabilityTxt = " will burn you!";
                }
            }
            else if (natTemp < -60 || baseNatTemp > 50)
            {
                suitabilityTxt = " will be the death of you.";
            }
            else if (natTemp < -40 || baseNatTemp > 30)
            {
                suitabilityTxt = " will wear you down.";
            }
            else if (natTemp < -20)
            {
                suitabilityTxt = " makes you shiver.";
            }
            else if (natTemp > 10)
            {
                suitabilityTxt = " makes you sweat.";
            }

            if (GameManager.Instance.IsPlayerInsideDungeon)
            {
                return "The " + temperatureTxt.ToString() + " air in this " + climateTxt.ToString() + suitabilityTxt.ToString();
            }
            else
            {
                return "This " + temperatureTxt.ToString() + weatherTxt.ToString() + seasonTxt.ToString() + timeTxt.ToString() + climateTxt.ToString() + suitabilityTxt.ToString();
            }
        }

        public static string TxtClothing()
        {
            string clothTxt = "The way you are dressed provides no warmth";
            string wetTxt = ". ";
            string armorTxt = "";


            if (wetCount > 10)
            {
                if (wetCount > 200) { wetTxt = " and you are completely drenched."; }
                else if (wetCount > 100) { wetTxt = " and you are soaking wet."; }
                else if (wetCount > 50) { wetTxt = " and you are quite wet."; }
                else if (wetCount > 20) { wetTxt = " and you are somewhat wet."; }
                else { wetTxt = " and you are a bit wet."; }
            }

            if (pureClothTemp > 40)
            {
                clothTxt = "You are very warmly dressed";
                if (wetCount > 39)
                {
                    wetTxt = " but your clothes are soaked.";
                }
                else if (wetCount > 20)
                {
                    wetTxt = " but your clothes are damp.";
                }
            }
            else if (pureClothTemp > 20)
            {
                clothTxt = "You are warmly dressed";
                if (wetCount > 19)
                {
                    wetTxt = " but your clothes are soaked.";
                }
                else if (wetCount > 10)
                {
                    wetTxt = " but your clothes are damp.";
                }
            }
            else if (pureClothTemp > 10)
            {
                clothTxt = "You are moderately dressed";
                if (wetCount > 9)
                {
                    wetTxt = " but your clothes are soaked.";
                }
                else if (wetCount > 5)
                {
                    wetTxt = " but your clothes are damp.";
                }
            }
            else if (pureClothTemp > 5)
            {
                clothTxt = "You are lightly dressed";
                if (wetCount > 4)
                {
                    wetTxt = " and your clothes are wet.";
                }
                else if (wetCount > 2)
                {
                    wetTxt = " and your clothes are damp.";
                }
            }




            if (armorTemp > 20)
            {
                armorTxt = " Your armor is scorchingly hot.";
            }
            else if (armorTemp > 15)
            {
                armorTxt = " Your armor is very hot.";
            }
            else if (armorTemp > 11)
            {
                armorTxt = " Your armor is hot.";
            }
            else if (armorTemp > 5)
            {
                armorTxt = " Your armor is warm.";
            }
            else if (armorTemp > 0)
            {
                armorTxt = " Your armor is a bit stuffy.";
            }
            else if (armorTemp < -5)
            {
                armorTxt = " The metal of your armor is cold.";
            }
            else if (armorTemp < 0)
            {
                armorTxt = " The metal of your armor is cool.";
            }
            return clothTxt.ToString() + wetTxt.ToString() + armorTxt.ToString();
        }

        public static string TxtAdvice()
        {
            bool isDungeon = GameManager.Instance.IsPlayerInsideDungeon;
            bool isRaining = GameManager.Instance.WeatherManager.IsRaining;
            bool isStorming = GameManager.Instance.WeatherManager.IsStorming;
            bool isSnowing = GameManager.Instance.WeatherManager.IsSnowing;
            bool isWeather = isRaining || isStorming || isSnowing;
            bool isNight = DaggerfallUnity.Instance.WorldTime.Now.IsNight;
            bool isDesert = playerGPS.CurrentClimateIndex == (int)MapsFile.Climates.Desert || playerGPS.CurrentClimateIndex == (int)MapsFile.Climates.Desert2 || playerGPS.CurrentClimateIndex == (int)MapsFile.Climates.Subtropical;
            bool isMountain = playerGPS.CurrentClimateIndex == (int)MapsFile.Climates.Mountain || playerGPS.CurrentClimateIndex == (int)MapsFile.Climates.MountainWoods;
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);

            string adviceTxt = "You do not feel the need to make any adjustments.";

            if (totalTemp < -10)
            {
                if (!cloak && isWeather && !isDungeon)
                {
                    adviceTxt = "A cloak would protect you from getting wet.";
                }
                else if ((isRaining || isStorming) && !hood && !isDungeon)
                {
                    adviceTxt = "The rain is soaking your head and running down your neck.";
                }
                else if (wetCount > 19)
                {
                    adviceTxt = "Walking around cold and wet might be hazardous to your health.";
                }
                else if (pureClothTemp < 30)
                {
                    adviceTxt = "In weather like this, it is important to dress warm enough.";

                    if (cloak1 != null)
                    {
                        switch (cloak1.TemplateIndex)
                        {
                            case (int)MensClothing.Casual_cloak:
                            case (int)WomensClothing.Casual_cloak:
                                adviceTxt = "Your casual cloak offers little protection from this cold.";
                                break;
                        }
                        if (cloak2 == null)
                        {
                            adviceTxt = "In this cold, it might help to put on a second cloak.";
                        }
                    }
                    if (cloak2 != null)
                    {
                        switch (cloak2.TemplateIndex)
                        {
                            case (int)MensClothing.Casual_cloak:
                            case (int)WomensClothing.Casual_cloak:
                                adviceTxt = "Your casual cloak offers little protection from this cold.";
                                break;
                        }
                        if (cloak1 == null)
                        {
                            adviceTxt = "In this cold, it might help to put on a second cloak.";
                        }
                    }
                }
                else if (armorTemp < 0)
                {
                    adviceTxt = "The metal of your armor leeches the warmth from your body.";
                }
                else if (isNight && !isDungeon)
                {
                    adviceTxt = "Most adventurers know the dangers of traveling at night.";
                }
                else if (isDesert && isNight && !isDungeon)
                {
                    adviceTxt = "The desert nights are cold, but might be preferable to the heat of the day.";
                }
            }
            else if (totalTemp > 10)
            {
                if (armorTemp > 11 && playerEnterExit.IsPlayerInSunlight && !ClimateCalories.ArmorCovered())
                {
                    adviceTxt = "The sun is heating up your armor, perhaps you should cover it.";
                }
                else if (!cloak && baseNatTemp > 30 && playerEnterExit.IsPlayerInSunlight)
                {
                    adviceTxt = "The people of the deserts know to dress lightly and cover up in a casual cloak.";
                }
                else if (cloak && !hood && baseNatTemp > 30 && playerEnterExit.IsPlayerInSunlight)
                {
                    adviceTxt = "The hood of your cloak will protect your head from cooking.";
                }
                else if (pureClothTemp > 8 && baseNatTemp > 10)
                {
                    adviceTxt = "On a hot day like this, it is best to dress as lightly as possible.";
                }
                else if (pureClothTemp > 10)
                {
                    adviceTxt = "You might be more comfortable if you dressed lighter.";
                }
                else if (isMountain && !isNight && !isDungeon)
                {
                    adviceTxt = "Though it is slightly warm now, you know the mountains will be icy cold once night falls.";
                }
                else if (totalTemp > 10 && !drink)
                {
                    adviceTxt = "If you brought a waterskin you might be able to keep cool.";
                }
                else if (totalTemp > 30 && ClimateCalories.wetPen && playerGPS.IsPlayerInLocationRect)
                {
                    adviceTxt = "Perhaps there is a pool of water here you could cool off in.";
                }
                else if (isDesert && !isNight)
                {
                    adviceTxt = "Though monsters may roam the deserts at night, it might be preferable to this heat.";
                }
            }

            if (playerRace.ID == (int)Races.Vampire && playerEnterExit.IsPlayerInSunlight)
            {
                if (natTemp > 0 && DaggerfallUnity.Instance.WorldTime.Now.IsDay && !hood)
                {
                    if (cloak && !hood)
                    {
                        adviceTxt = "The rays of the sun burns your face and neck!";
                    }
                    adviceTxt = "Your exposed skin sizzles in the deadly sunlight!";
                }
            }

            return adviceTxt;
        }

        public static string TxtFood()
        {
            hunger = FillingFood.hunger;
            string foodString = "If you had a decent meal, you could go on for longer.";

            if (starving)
            {
                if (FillingFood.starvDays > 7)
                {
                    foodString = string.Format("You have not eaten properly in over a week. You feeling very weak.");
                }
                else if (FillingFood.starvDays == 1)
                {
                    foodString = string.Format("You have not eaten properly in a day. You are feeling weak.");
                }
                else
                {
                    foodString = string.Format("You have not eaten properly in {0} days. You are getting weaker.", FillingFood.starvDays.ToString());
                }
            }
            else if (playerGPS.IsPlayerInLocationRect && !rations)
            {
                foodString = "You might want to buy some rations while in town.";
            }
            else if (hunger < 180)
            {
                foodString = "You are still feeling envigorated from your last meal.";
            }
            else if (hunger < 240)
            {
                foodString = "You might get hungry again soon.";
            }
            
            return foodString;
        }

        public static string TxtEncumbrance()
        {
            float encPc = playerEntity.CarriedWeight / playerEntity.MaxEncumbrance;
            float encOver = Mathf.Max(encPc - 0.75f, 0f) * 2f;
            if (encOver > 0)
            {
                return "You are overburdened, which slows and exhausts you.";
            }
            else if (encPc > 0.6)
            {
                return "Your burdened is quite heavy.";
            }
            return "You are not overburdened.";
        }

        public static string TxtEncAdvice()
        {
            int goldWeight = playerEntity.GoldPieces / 400;
            int halfMaxEnc = playerEntity.MaxEncumbrance / 2;
            float encPc = playerEntity.CarriedWeight / playerEntity.MaxEncumbrance;
            if (playerEntity.Stats.LiveStrength < playerEntity.Stats.PermanentStrength)
            {
                return "Your strength is reduced, making you unable to carry as much.";
            }
            else if (goldWeight > halfMaxEnc)
            {
                return "You are carrying " + goldWeight.ToString() + " kg in gold pieces.";
            }
            else if (encPc >= 0.75)
            {
                return "Perhaps you should leave some items behind?";
            }
            return "You are still able to carry more.";
        }

    }
}