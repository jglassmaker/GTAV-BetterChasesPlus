using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BetterChasesPlus
{
    public class Renderer : Script
    {
        private Scaleform Scaleform = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
        private static List<string> HelpMessages = new List<string>();
        private static List<BigMessage> BigMessages = new List<BigMessage>();
        public static List<UIMarker> Markers = new List<UIMarker>();

        private int HelpMessageTime = 0;
        private int HelpMessageInterval = 5000;

        private int BigMessageTime = 0;
        private int BigMessageInterval = 5000;

        private int BetterChasesIconOffsetX;
        private int BetterChasesIconOffsetY;
        private int ArrestWarrantsIconOffsetX;
        private int ArrestWarrantsIconOffsetY;
        private int ArrestWarrantsTextOffsetX;
        private int ArrestWarrantsTextOffsetY;
        private int ArrestWarrantsGradientOffsetX;
        private int ArrestWarrantsGradientOffsetY;

        private int ShowBetterChasesHUDUntil = 0;
        private int ShowArrestWarrantsHUDUntil = 0;
        private int OffsetChangeDisplayLength = 5000;

        private readonly GTA.UI.Sprite CopsRamSprite = new GTA.UI.Sprite("mpinventory", "vehicle_deathmatch", new SizeF(18, 18), new PointF(GTA.UI.Screen.Width - 265, 5));
        private readonly GTA.UI.Sprite CopsShootSprite = new GTA.UI.Sprite("mpinventory", "survival", new SizeF(18, 18), new PointF(GTA.UI.Screen.Width - 280, 5));

        private readonly GTA.UI.Sprite PedWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_ped", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 130, GTA.UI.Screen.Height - 19));
        private readonly GTA.UI.Sprite CarWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_car", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 144, GTA.UI.Screen.Height - 19));
        private readonly GTA.UI.Sprite PlaneWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_plane", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 144, GTA.UI.Screen.Height - 19));
        private readonly GTA.UI.Sprite HeliWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_heli", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 144, GTA.UI.Screen.Height - 19));
        private readonly GTA.UI.Sprite BoatWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_boat", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 144, GTA.UI.Screen.Height - 19));
        private readonly GTA.UI.Sprite BikeWantedSprite = new GTA.UI.Sprite("mpinventory", "mp_specitem_bike", new SizeF(16, 16), new PointF(GTA.UI.Screen.Width - 144, GTA.UI.Screen.Height - 19));

        private readonly GTA.UI.Sprite WantedGradient = new GTA.UI.Sprite("timerbars", "all_white_bg", new SizeF(500, 22), new PointF(GTA.UI.Screen.Width - 490, GTA.UI.Screen.Height - 22), Color.DarkRed);
        private readonly GTA.UI.Sprite MichaelGradient = new GTA.UI.Sprite("timerbars", "all_white_bg", new SizeF(500, 22), new PointF(GTA.UI.Screen.Width - 490, GTA.UI.Screen.Height - 22), Color.CornflowerBlue);
        private readonly GTA.UI.Sprite FranklinGradient = new GTA.UI.Sprite("timerbars", "all_white_bg", new SizeF(500, 22), new PointF(GTA.UI.Screen.Width - 490, GTA.UI.Screen.Height - 22), Color.ForestGreen);
        private readonly GTA.UI.Sprite TrevorGradient = new GTA.UI.Sprite("timerbars", "all_white_bg", new SizeF(500, 22), new PointF(GTA.UI.Screen.Width - 490, GTA.UI.Screen.Height - 22), Color.DarkOrange);
        private readonly GTA.UI.Sprite DimGradient = new GTA.UI.Sprite("timerbars", "all_white_bg", new SizeF(500, 22), new PointF(GTA.UI.Screen.Width - 490, GTA.UI.Screen.Height - 22), Color.DimGray);

        private readonly GTA.UI.ContainerElement SpottedMeterBG = new GTA.UI.ContainerElement(new PointF(GTA.UI.Screen.Width - 145, GTA.UI.Screen.Height - 55), new SizeF(112, 30), Color.FromArgb(220, Color.Black));

        private GTA.UI.TextElement SuspectWanted = new GTA.UI.TextElement("~w~SUSPECT WANTED", new PointF(GTA.UI.Screen.Width - 150, GTA.UI.Screen.Height - 20), 0.4f, Color.White, GTA.UI.Font.ChaletLondon);
        private GTA.UI.TextElement WarrantActive = new GTA.UI.TextElement("~w~WARRANT ACTIVE", new PointF(GTA.UI.Screen.Width - 150, GTA.UI.Screen.Height - 20), 0.4f, Color.White, GTA.UI.Font.ChaletLondon);
        private GTA.UI.TextElement SpottedMeterText = new GTA.UI.TextElement("", new PointF(GTA.UI.Screen.Width - 90, GTA.UI.Screen.Height - 55), 0.3f, Color.White, GTA.UI.Font.ChaletLondon, GTA.UI.Alignment.Center);

        public class UIMarker
        {
            public MarkerType Type;
            public Entity Entity;
            public Color Color;
        }

        public Renderer()
        {
            Tick += OnTick;

            Interval = 4;

            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "WEB_LOSSANTOSPOLICEDEPT", true);
        }

        private void OnTick(object sender, EventArgs e)
        {
            //System.Diagnostics.Stopwatch Stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Stopwatch.Start();
            //float ar = (float)Game.ScreenResolution.Width / Game.ScreenResolution.Height;
            //UI.ShowSubtitle("" + Game.ScreenResolution.Height+ " " + UIScreen.Height + " " + UI.HEIGHT + " " + SafeZoneBounds.Y);
            //UI.ShowSubtitle("" + Game.ScreenResolution.Width + " " + UIScreen.Width + " " + UI.WIDTH + " " + SafeZoneBounds.X);
            //UI.ShowSubtitle("" + UISafeZone.Height + " " + UI.HEIGHT + " " + SafeZoneBounds);
            //UI.ShowSubtitle("" + UIScreen.Width + " " + UI.WIDTH + " " + SafeZoneBounds);
            //UI.ShowSubtitle("" + Game.ScreenResolution.Width + " " + Game.ScreenResolution.Height + " " + ar);
            //UI.ShowSubtitle("" + Function.Call<float>(Hash.GET_SAFE_ZONE_SIZE) + " " + SafeZone.X + " " + UI.WIDTH + " " + (float)(Game.ScreenResolution.Width / Game.ScreenResolution.Height));

            if (Function.Call<bool>(Hash.IS_HUD_HIDDEN) || Function.Call<bool>(Hash.IS_PLAYER_SWITCH_IN_PROGRESS))
                return;

            Ped Character = Game.Player.Character;
            int WantedLevel = Game.Player.WantedLevel;

            //DateTime expireTest = World.CurrentDate.AddHours(-1);
            //UI.ShowSubtitle("Time: " + World.CurrentDate.ToString() + " " + expireTest.ToString() + " " + World.CurrentDate.CompareTo(expireTest));

            // Option Offsets
            if ((Options.BetterChases.IconOffsetX != 0 && Options.BetterChases.IconOffsetX != BetterChasesIconOffsetX) || (Options.BetterChases.IconOffsetY != 0 && Options.BetterChases.IconOffsetY != BetterChasesIconOffsetY))
            {
                CopsRamSprite.Position = new PointF(GTA.UI.Screen.Width - 265 + Options.BetterChases.IconOffsetX, 5 + Options.BetterChases.IconOffsetY);
                CopsShootSprite.Position = new PointF(GTA.UI.Screen.Width - 280 + Options.BetterChases.IconOffsetX, 5 + Options.BetterChases.IconOffsetY);
                
                ShowBetterChasesHUDUntil = Game.GameTime;
                BetterChasesIconOffsetX = Options.BetterChases.IconOffsetX;
                BetterChasesIconOffsetY = Options.BetterChases.IconOffsetY;
            }
            else if ((Options.ArrestWarrants.IconOffsetX != 0 && Options.ArrestWarrants.IconOffsetX != ArrestWarrantsIconOffsetX) || (Options.ArrestWarrants.IconOffsetY != 0 && Options.ArrestWarrants.IconOffsetY != ArrestWarrantsIconOffsetY))
            {
                PedWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 130 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);
                CarWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 144 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);
                PlaneWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 144 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);
                HeliWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 144 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);
                BoatWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 144 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);
                BikeWantedSprite.Position = new PointF(GTA.UI.Screen.Width - 144 + Options.ArrestWarrants.IconOffsetX, GTA.UI.Screen.Height - 19 + Options.ArrestWarrants.IconOffsetY);

                ShowArrestWarrantsHUDUntil = Game.GameTime;
                ArrestWarrantsIconOffsetX = Options.ArrestWarrants.IconOffsetX;
                ArrestWarrantsIconOffsetY = Options.ArrestWarrants.IconOffsetY;
            }
            else if ((Options.ArrestWarrants.TextOffsetX != 0 && Options.ArrestWarrants.TextOffsetX != ArrestWarrantsTextOffsetX) || (Options.ArrestWarrants.TextOffsetY != 0 && Options.ArrestWarrants.TextOffsetY != ArrestWarrantsTextOffsetY))
            {
                SuspectWanted = new GTA.UI.TextElement("~w~SUSPECT WANTED", new PointF(GTA.UI.Screen.Width - 150 + Options.ArrestWarrants.TextOffsetX, GTA.UI.Screen.Height - 20 + Options.ArrestWarrants.TextOffsetY), 0.4f, Color.White, GTA.UI.Font.ChaletLondon);
                WarrantActive = new GTA.UI.TextElement("~w~WARRANT ACTIVE", new PointF(GTA.UI.Screen.Width - 150 + Options.ArrestWarrants.TextOffsetX, GTA.UI.Screen.Height - 20 + Options.ArrestWarrants.TextOffsetY), 0.4f, Color.White, GTA.UI.Font.ChaletLondon);
                
                ShowArrestWarrantsHUDUntil = Game.GameTime;
                ArrestWarrantsTextOffsetX = Options.ArrestWarrants.TextOffsetX;
                ArrestWarrantsTextOffsetY = Options.ArrestWarrants.TextOffsetY;
            }
            else if ((Options.ArrestWarrants.GradientOffsetX != 0 && Options.ArrestWarrants.GradientOffsetX != ArrestWarrantsGradientOffsetX) || (Options.ArrestWarrants.GradientOffsetY != 0 && Options.ArrestWarrants.GradientOffsetY != ArrestWarrantsGradientOffsetY))
            {
                WantedGradient.Position = new PointF(GTA.UI.Screen.Width - 490 + Options.ArrestWarrants.GradientOffsetX, GTA.UI.Screen.Height - 22 + Options.ArrestWarrants.GradientOffsetY);
                MichaelGradient.Position = new PointF(GTA.UI.Screen.Width - 490 + Options.ArrestWarrants.GradientOffsetX, GTA.UI.Screen.Height - 22 + Options.ArrestWarrants.GradientOffsetY);
                FranklinGradient.Position = new PointF(GTA.UI.Screen.Width - 490 + Options.ArrestWarrants.GradientOffsetX, GTA.UI.Screen.Height - 22 + Options.ArrestWarrants.GradientOffsetY);
                TrevorGradient.Position = new PointF(GTA.UI.Screen.Width - 490 + Options.ArrestWarrants.GradientOffsetX, GTA.UI.Screen.Height - 22 + Options.ArrestWarrants.GradientOffsetY);
                DimGradient.Position = new PointF(GTA.UI.Screen.Width - 490 + Options.ArrestWarrants.GradientOffsetX, GTA.UI.Screen.Height - 22 + Options.ArrestWarrants.GradientOffsetY);

                ShowArrestWarrantsHUDUntil = Game.GameTime;
                ArrestWarrantsGradientOffsetX = Options.ArrestWarrants.GradientOffsetX;
                ArrestWarrantsGradientOffsetY = Options.ArrestWarrants.GradientOffsetY;
            }

            if (Options.Menu.MainPool.IsAnyMenuOpen() && ShowBetterChasesHUDUntil != 0 && ShowBetterChasesHUDUntil + OffsetChangeDisplayLength > Game.GameTime)
            {
                CopsRamSprite.Draw();
                CopsShootSprite.Draw();
            }
            else if (Options.Menu.MainPool.IsAnyMenuOpen() && ShowArrestWarrantsHUDUntil != 0 && ShowArrestWarrantsHUDUntil + OffsetChangeDisplayLength > Game.GameTime)
            {
                WantedGradient.Draw();
                PedWantedSprite.Draw();
                CarWantedSprite.Draw();
                SuspectWanted.Draw();
            }
            else
            {
                if (Options.BetterChases.ShowHUD && BetterChases.PITAuthorized && WantedLevel > 0)
                {
                    CopsRamSprite.Draw();
                }

                if (Options.BetterChases.ShowHUD && BetterChases.DeadlyForce && WantedLevel > 0)
                {
                    CopsShootSprite.Draw();
                }
            
                if (Options.ArrestWarrants.ShowHUD && (ArrestWarrants.PedWarrant.ped.IsValid || (ArrestWarrants.VehicleWarrant.vehicle.IsValid && Helpers.IsValid(Character.CurrentVehicle) && Character.CurrentVehicle.Model.Hash == ArrestWarrants.VehicleWarrant.vehicle.Hash)))
                {
                    if (WantedLevel > 0 || Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player))
                    {
                        SuspectWanted.Draw();
                        WantedGradient.Draw();
                    }
                    else
                    {
                        WarrantActive.Draw();

                        if (Character.Model == PedHash.Michael)
                        {
                            MichaelGradient.Draw();
                        }
                        else if (Character.Model == PedHash.Franklin)
                        {
                            FranklinGradient.Draw();
                        }
                        else if (Character.Model == PedHash.Trevor)
                        {
                            TrevorGradient.Draw();
                        }
                        else
                        {
                            DimGradient.Draw();
                        }
                    }

                    if (ArrestWarrants.PedWarrant.ped.IsValid)
                    {
                        PedWantedSprite.Draw();
                    }

                    if (ArrestWarrants.VehicleWarrant.vehicle.IsValid && Helpers.IsValid(Character.CurrentVehicle) && Character.CurrentVehicle.Model.Hash == ArrestWarrants.VehicleWarrant.vehicle.Hash)
                    {
                        if (ArrestWarrants.VehicleWarrant.vehicle.IsCar)
                        {
                            CarWantedSprite.Draw();
                        }
                        else if (ArrestWarrants.VehicleWarrant.vehicle.IsBicycle || ArrestWarrants.VehicleWarrant.vehicle.IsBike)
                        {
                            BikeWantedSprite.Draw();
                        }
                        else if (ArrestWarrants.VehicleWarrant.vehicle.IsBoat)
                        {
                            BoatWantedSprite.Draw();
                        }
                        else if (ArrestWarrants.VehicleWarrant.vehicle.IsPlane)
                        {
                            PlaneWantedSprite.Draw();
                        }
                        else if (ArrestWarrants.VehicleWarrant.vehicle.IsHelicopter)
                        {
                            HeliWantedSprite.Draw();
                        }
                    }
                }
            }


            // Recognition Markers
            if (Markers.Count > 0)
            {
                foreach (UIMarker marker in Markers)
                {
                    World.DrawMarker(marker.Type, marker.Entity.Position + new Vector3(0f, 0f, 1.5f), new Vector3(0, 0, 0), new Vector3(0f, 180f, 0f), new Vector3(0.5f, 0.5f, 0.5f), marker.Color, true, false, true, "", "", false);
                }
            }

            // Recognition meter
            if (Options.ArrestWarrants.ShowSpottedMeter && Markers.Count > 0 && ArrestWarrants.ShowSpottedMeter)
            {
                SpottedMeterBG.Draw();
                SpottedMeterText.Caption = "A cop is watching you.~n~~y~Recognition process: ~r~" + Math.Round(ArrestWarrants.SpottedMeter, 0) + "%";
                SpottedMeterText.Draw();
            }

            // Display Help Messages
            if (HelpMessages.Count > 0 && Game.GameTime > HelpMessageTime + HelpMessageInterval)
            {
                Function.Call(Hash.BEGIN_TEXT_COMMAND_DISPLAY_HELP, "STRING");
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, HelpMessages[0]);
                Function.Call(Hash.END_TEXT_COMMAND_DISPLAY_HELP, 0, 0, 0, HelpMessageInterval);

                HelpMessageTime = Game.GameTime;
                HelpMessages.RemoveAt(0);
            }

            // Display Big Messages
            if (BigMessages.Count > 0 && BigMessageTime == 0 && Game.GameTime > BigMessageTime + BigMessageInterval)
            {
                BigMessage message = BigMessages[0];
                Scaleform.CallFunction("SHOW_SHARD_CENTERED_MP_MESSAGE", message.text, message.description, (int)message.color, (int)message.background);
                Function.Call(Hash.PLAY_SOUND_FRONTEND, 0, "CHECKPOINT_NORMAL", "HUD_MINI_GAME_SOUNDSET");

                BigMessageTime = Game.GameTime;
                BigMessageInterval = message.duration;
            }
            else if (BigMessageTime > 0 && Game.GameTime < BigMessageTime + BigMessageInterval)
            {
                Scaleform.Render2D();
            }
            else if (BigMessageTime > 0 && Game.GameTime > BigMessageTime + BigMessageInterval)
            {
                Scaleform.CallFunction("TRANSITION_OUT");
                BigMessageTime = 0;
                BigMessages.RemoveAt(0);
            }

            //Stopwatch.Stop();
            //UI.ShowSubtitle("Debug: " + "" + " CPU: " + Stopwatch.Elapsed);
        }

        public static void ShowHelpMessage(string text)
        {
            HelpMessages.Add(text);
        }

        public static void ShowBigMessage(string text, string description, HudColor color, HudColor background, int duration = 3000)
        {
            BigMessage message = new BigMessage
            {
                text = text,
                description = description,
                color = color,
                background = background,
                duration = duration
            };
            BigMessages.Add(message);
        }

        public static void TestBig(string text)
        {
            BigMessage message = new BigMessage
            {
                text = text,
                color = HudColor.RED,
                background = HudColor.BLACK
            };
            BigMessages.Add(message);
        }

        private class BigMessage
        {
            public string text = "";
            public string description = "";
            public HudColor color = HudColor.BLACK;
            public HudColor background = HudColor.WHITE;
            public int duration = 3000;
        }

        public enum HudColor
        {
            PURE_WHITE = 0,
            WHITE = 1,
            BLACK = 2,
            GREY = 3,
            GREYLIGHT = 4,
            GREYDARK = 5,
            RED = 6,
            REDLIGHT = 7,
            REDDARK = 8,
            BLUE = 9,
            BLUELIGHT = 10,
            BLUEDARK = 11,
            YELLOW = 12,
            YELLOWLIGHT = 13,
            YELLOWDARK = 14,
            ORANGE = 15,
            ORANGELIGHT = 16,
            ORANGEDARK = 17,
            GREEN = 18,
            GREENLIGHT = 19,
            GREENDARK = 20,
            PURPLE = 21,
            PURPLELIGHT = 22,
            PURPLEDARK = 23,
            PINK = 24,
            RADAR_HEALTH = 25,
            RADAR_ARMOUR = 26,
            RADAR_DAMAGE = 27,
            NET_PLAYER1 = 28,
            NET_PLAYER2 = 29,
            NET_PLAYER3 = 30,
            NET_PLAYER4 = 31,
            NET_PLAYER5 = 32,
            NET_PLAYER6 = 33,
            NET_PLAYER7 = 34,
            NET_PLAYER8 = 35,
            NET_PLAYER9 = 36,
            NET_PLAYER10 = 37,
            NET_PLAYER11 = 38,
            NET_PLAYER12 = 39,
            NET_PLAYER13 = 40,
            NET_PLAYER14 = 41,
            NET_PLAYER15 = 42,
            NET_PLAYER16 = 43,
            NET_PLAYER17 = 44,
            NET_PLAYER18 = 45,
            NET_PLAYER19 = 46,
            NET_PLAYER20 = 47,
            NET_PLAYER21 = 48,
            NET_PLAYER22 = 49,
            NET_PLAYER23 = 50,
            NET_PLAYER24 = 51,
            NET_PLAYER25 = 52,
            NET_PLAYER26 = 53,
            NET_PLAYER27 = 54,
            NET_PLAYER28 = 55,
            NET_PLAYER29 = 56,
            NET_PLAYER30 = 57,
            NET_PLAYER31 = 58,
            NET_PLAYER32 = 59,
            SIMPLEBLIP_DEFAULT = 60,
            MENU_BLUE = 61,
            MENU_GREY_LIGHT = 62,
            MENU_BLUE_EXTRA_DARK = 63,
            MENU_YELLOW = 64,
            MENU_YELLOW_DARK = 65,
            MENU_GREEN = 66,
            MENU_GREY = 67,
            MENU_GREY_DARK = 68,
            MENU_HIGHLIGHT = 69,
            MENU_STANDARD = 70,
            MENU_DIMMED = 71,
            MENU_EXTRA_DIMMED = 72,
            BRIEF_TITLE = 73,
            MID_GREY_MP = 74,
            NET_PLAYER1_DARK = 75,
            NET_PLAYER2_DARK = 76,
            NET_PLAYER3_DARK = 77,
            NET_PLAYER4_DARK = 78,
            NET_PLAYER5_DARK = 79,
            NET_PLAYER6_DARK = 80,
            NET_PLAYER7_DARK = 81,
            NET_PLAYER8_DARK = 82,
            NET_PLAYER9_DARK = 83,
            NET_PLAYER10_DARK = 84,
            NET_PLAYER11_DARK = 85,
            NET_PLAYER12_DARK = 86,
            NET_PLAYER13_DARK = 87,
            NET_PLAYER14_DARK = 88,
            NET_PLAYER15_DARK = 89,
            NET_PLAYER16_DARK = 90,
            NET_PLAYER17_DARK = 91,
            NET_PLAYER18_DARK = 92,
            NET_PLAYER19_DARK = 93,
            NET_PLAYER20_DARK = 94,
            NET_PLAYER21_DARK = 95,
            NET_PLAYER22_DARK = 96,
            NET_PLAYER23_DARK = 97,
            NET_PLAYER24_DARK = 98,
            NET_PLAYER25_DARK = 99,
            NET_PLAYER26_DARK = 100,
            NET_PLAYER27_DARK = 101,
            NET_PLAYER28_DARK = 102,
            NET_PLAYER29_DARK = 103,
            NET_PLAYER30_DARK = 104,
            NET_PLAYER31_DARK = 105,
            NET_PLAYER32_DARK = 106,
            BRONZE = 107,
            SILVER = 108,
            GOLD = 109,
            PLATINUM = 110,
            GANG1 = 111,
            GANG2 = 112,
            GANG3 = 113,
            GANG4 = 114,
            SAME_CREW = 115,
            FREEMODE = 116,
            PAUSE_BG = 117,
            FRIENDLY = 118,
            ENEMY = 119,
            LOCATION = 120,
            PICKUP = 121,
            PAUSE_SINGLEPLAYER = 122,
            FREEMODE_DARK = 123,
            INACTIVE_MISSION = 124,
            DAMAGE = 125,
            PINKLIGHT = 126,
            PM_MITEM_HIGHLIGHT = 127,
            SCRIPT_VARIABLE = 128,
            YOGA = 129,
            TENNIS = 130,
            GOLF = 131,
            SHOOTING_RANGE = 132,
            FLIGHT_SCHOOL = 133,
            NORTH_BLUE = 134,
            SOCIAL_CLUB = 135,
            PLATFORM_BLUE = 136,
            PLATFORM_GREEN = 137,
            PLATFORM_GREY = 138,
            FACEBOOK_BLUE = 139,
            INGAME_BG = 140,
            DARTS = 141,
            WAYPOINT = 142,
            MICHAEL = 143,
            FRANKLIN = 144,
            TREVOR = 145,
            GOLF_P1 = 146,
            GOLF_P2 = 147,
            GOLF_P3 = 148,
            GOLF_P4 = 149,
            WAYPOINTLIGHT = 150,
            WAYPOINTDARK = 151,
            PANEL_LIGHT = 152,
            MICHAEL_DARK = 153,
            FRANKLIN_DARK = 154,
            TREVOR_DARK = 155,
            OBJECTIVE_ROUTE = 156,
            PAUSEMAP_TINT = 157,
            PAUSE_DESELECT = 158,
            PM_WEAPONS_PURCHASABLE = 159,
            PM_WEAPONS_LOCKED = 160,
            END_SCREEN_BG = 161,
            CHOP = 162,
            PAUSEMAP_TINT_HALF = 163,
            NORTH_BLUE_OFFICIAL = 164,
            SCRIPT_VARIABLE_2 = 165,
            H = 166,
            HDARK = 167,
            T = 168,
            TDARK = 169,
            HSHARD = 170,
            CONTROLLER_MICHAEL = 171,
            CONTROLLER_FRANKLIN = 172,
            CONTROLLER_TREVOR = 173,
            CONTROLLER_CHOP = 174,
            VIDEO_EDITOR_VIDEO = 175,
            VIDEO_EDITOR_AUDIO = 176,
            VIDEO_EDITOR_TEXT = 177,
            HB_BLUE = 178,
            HB_YELLOW = 179,
        }
    }
}