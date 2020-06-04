using GTA;
using NativeUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace BetterChasesPlus
{
    public class Options : Script
    {
        private static List<dynamic> StarOptions = new List<dynamic> { 0, 1, 2, 3, 4, 5, 6, 7, 8, -1 };
        private static List<dynamic> ChaseTimeOptions = (dynamic)Enumerable.Range(1, 120).Cast<dynamic>().ToList();
        private static List<dynamic> WarrantTimeOptions = (dynamic)Enumerable.Range(1, 336).Cast<dynamic>().ToList();
        private static List<dynamic> SpotSpeedOptions = new List<dynamic> { 25, 50, 75, 100, 125, 150, 175, 200, 250, 300 };
        private static List<dynamic> StolenRecognitionOptions = new List<dynamic> { 10, 20, 40, 60, 80, 90, 100 };
        private static List<dynamic> WantedLevelControlOptions = new List<dynamic> { "Full", "Partial", "None" };
        private static List<dynamic> offsetOptions = (dynamic)Enumerable.Range(-2000, 4000).Cast<dynamic>().ToList();

    // Global
        public static bool DisplayHints { get => GetSetting(Settings, "DisplayHints"); }
        public static Keys MenuKey = Keys.F8;

        public static class BetterChases
        {
            public static bool Enabled { get => GetSetting(Settings[0].settings, "Enabled"); }
            public static string WantedLevelControl { get => GetSetting(Settings[0].settings, "WantedLevelControl"); }
            public static bool CopsManageTraffic { get => GetSetting(Settings[0].settings, "CopsManageTraffic"); }
            public static bool WreckedCopsStopChasing { get => GetSetting(Settings[0].settings, "WreckedCopsStopChasing"); }
            public static bool DisallowCopCommandeering { get => GetSetting(Settings[0].settings, "DisallowCopCommandeering");  }

            public static bool EnableCopVehicleControl { get => GetSetting(Settings[0].settings, "EnableCopVehicleControl"); }
            public static int OneStarCopCars { get => GetSetting(Settings[0].settings, "OneStarCopCars"); }
            public static int OneStarCopHelis { get => GetSetting(Settings[0].settings, "OneStarCopHelis"); }
            public static int TwoStarCopCars { get => GetSetting(Settings[0].settings, "TwoStarCopCars"); }
            public static int TwoStarCopHelis { get => GetSetting(Settings[0].settings, "TwoStarCopHelis"); }
            public static int ThreeStarCopCars { get => GetSetting(Settings[0].settings, "ThreeStarCopCars"); }
            public static int ThreeStarCopHelis { get => GetSetting(Settings[0].settings, "ThreeStarCopHelis"); }
            public static int FourStarCopCars { get => GetSetting(Settings[0].settings, "FourStarCopCars"); }
            public static int FourStarCopHelis { get => GetSetting(Settings[0].settings, "FourStarCopHelis"); }
            public static int FiveStarCopCars { get => GetSetting(Settings[0].settings, "FiveStarCopCars"); }
            public static int FiveStarCopHelis { get => GetSetting(Settings[0].settings, "FiveStarCopHelis"); }

            public static bool EnableChaseTimes { get => GetSetting(Settings[0].settings, "EnableChaseTimes"); }
            public static int OneStarChaseTime { get => GetSetting(Settings[0].settings, "OneStarChaseTime"); }
            public static int TwoStarChaseTime { get => GetSetting(Settings[0].settings, "TwoStarChaseTime"); }
            public static int ThreeStarChaseTime { get => GetSetting(Settings[0].settings, "ThreeStarChaseTime"); }

            public static bool RequirePITAuthorization { get => GetSetting(Settings[0].settings, "RequirePITAuthorization"); }
            public static bool RequireLethalForceAuthorization { get => GetSetting(Settings[0].settings, "RequireLethalForceAuthorization"); }
            public static bool LethalForceOnAim { get => GetSetting(Settings[0].settings, "LethalForceOnAim"); }
            public static bool AllowBustOpportunity { get => GetSetting(Settings[0].settings, "AllowBustOpportunity"); }
            public static bool ShowHUD { get => GetSetting(Settings[0].settings, "ShowHUD"); }
            public static bool ShowNotifications { get => GetSetting(Settings[0].settings, "ShowNotifications"); }
            public static bool ShowBigMessages { get => GetSetting(Settings[0].settings, "ShowBigMessages"); }
            public static int IconOffsetX { get => GetSetting(Settings[0].settings, "IconOffsetX"); }
            public static int IconOffsetY { get => GetSetting(Settings[0].settings, "IconOffsetY"); }
        }

        public static class ArrestWarrants
        {
            public static bool Enabled { get => GetSetting(Settings[1].settings, "Enabled"); }
            public static bool EnableStolenVehicles { get => GetSetting(Settings[1].settings, "EnableStolenVehicles"); }
            public static bool RememberStarsWhenRecognised { get => GetSetting(Settings[1].settings, "RememberStarsWhenRecognised"); }
            public static int OneStarWarrantTime { get => GetSetting(Settings[1].settings, "OneStarWarrantTime"); }
            public static int TwoStarWarrantTime { get => GetSetting(Settings[1].settings, "TwoStarWarrantTime"); }
            public static int ThreeStarWarrantTime { get => GetSetting(Settings[1].settings, "ThreeStarWarrantTime"); }
            public static int FourStarWarrantTime { get => GetSetting(Settings[1].settings, "FourStarWarrantTime"); }
            public static int FiveStarWarrantTime { get => GetSetting(Settings[1].settings, "FiveStarWarrantTime"); }
            public static float OverallSpotSpeed { get => GetSetting(Settings[1].settings, "OverallSpotSpeed"); }
            public static int StolenRecognitionThreshold { get => GetSetting(Settings[1].settings, "StolenRecognitionThreshold"); }
            public static bool ShowSpottedMeter { get => GetSetting(Settings[1].settings, "ShowSpottedMeter"); }
            public static bool ShowHUD { get => GetSetting(Settings[1].settings, "ShowHUD"); }
            public static bool ShowNotifications { get => GetSetting(Settings[1].settings, "ShowNotifications"); }
            public static bool ShowBigMessages { get => GetSetting(Settings[1].settings, "ShowBigMessages"); }
            public static int IconOffsetX { get => GetSetting(Settings[1].settings, "IconOffsetX"); }
            public static int IconOffsetY { get => GetSetting(Settings[1].settings, "IconOffsetY"); }
            public static int TextOffsetX { get => GetSetting(Settings[1].settings, "TextOffsetX"); }
            public static int TextOffsetY { get => GetSetting(Settings[1].settings, "TextOffsetY"); }
            public static int GradientOffsetX { get => GetSetting(Settings[1].settings, "GradientOffsetX"); }
            public static int GradientOffsetY { get => GetSetting(Settings[1].settings, "GradientOffsetY"); }
        }

        public new static List<Setting> Settings = new List<Setting>()
        {
            new Setting()
            {
                name = "BetterChases",
                menu = new UIMenu("Better Chases", "Configure Better Chases"),
                menuItem = new UIMenuItem("Better Chases", "Configure Better Chases"),
                settings = new List<Setting>()
                {
                    new Setting()
                    {
                        name = "Enabled",
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Module Enabled", true, "Toggles the functionality of the entire module. If disabled, all features below will be disabled also.")
                    },
                    new Setting()
                    {
                        name = "WantedLevelControl",
                        enableToggle = true,
                        value = "Full",
                        options = WantedLevelControlOptions,
                        menuItem = new UIMenuListItem("Wanted Level Control", WantedLevelControlOptions, 0, "Full if you want this mod to take full control, partial for mod compatibility, none for vanilla GTAV.")
                    },
                    new Setting()
                    {
                        name = "CopsManageTraffic",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Cops Manage Traffic", true, "If enabled, cops will try to avoid crashing into vehicles, pedestrians and other cops. They will also refrain from ramming you if there are people nearby.")
                    },
                    new Setting()
                    {
                        name = "WreckedCopsStopChasing",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Wrecked cops give up chase", true, "If enabled, cops driving badly damaged vehicles will give up on pursuit.")
                    },
                    new Setting()
                    {
                        name = "DisallowCopCommandeering",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Cops won't commandeer", true, "If enabled, cops will not be able to commandeer civilian vehicles.")
                    },
                    new Setting()
                    {
                        name = "CopVehicleControls",
                        enableToggle = true,
                        menu = new UIMenu("Cop Vehicle Control", "Control cop vehicle counts"),
                        menuItem = new UIMenuItem("Cop Vehicle Control", "Control cop vehicle"),
                        settings = new List<Setting>()
                        {
                            new Setting()
                            {
                                name = "EnableCopVehicleControl",
                                subMenu = true,
                                value = true,
                                menuItem = new UIMenuCheckboxItem("Feature Enabled", true, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                            },
                            new Setting()
                            {
                                name = "OneStarCopCars",
                                enableToggle = true,
                                subMenu = true,
                                value = 1,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("One Star Cop Cars", StarOptions, 0, "How many cop cars will respond to a one star wanted level.")
                            },
                            new Setting()
                            {
                                name = "OneStarCopHelis",
                                enableToggle = true,
                                subMenu = true,
                                value = 0,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("One Star Cop Helis", StarOptions, 0, "How many cop helicopters will respond to a one star wanted level.")
                            },
                            new Setting()
                            {
                                name = "TwoStarCopCars",
                                enableToggle = true,
                                subMenu = true,
                                value = 2,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Two Star Cop Cars", StarOptions, 2, "How many cop cars will respond to a two star wanted level.")
                            },
                            new Setting()
                            {
                                name = "TwoStarCopHelis",
                                enableToggle = true,
                                subMenu = true,
                                value = 0,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Two Star Cop Helis", StarOptions, 0, "How many cop helicopters will respond to a two star wanted level.")
                            },
                            new Setting()
                            {
                                name = "ThreeStarCopCars",
                                enableToggle = true,
                                subMenu = true,
                                value = 4,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Three Star Cop Cars", StarOptions, 4, "How many cop cars will respond to a three star wanted level.")
                            },
                            new Setting()
                            {
                                name = "ThreeStarCopHelis",
                                enableToggle = true,
                                subMenu = true,
                                value = 1,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Three Star Cop Helis", StarOptions, 1, "How many cop helicopters will respond to a three star wanted level.")
                            },
                            new Setting()
                            {
                                name = "FourStarCopCars",
                                enableToggle = true,
                                subMenu = true,
                                value = 7,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Four Star Cop Cars", StarOptions, 7, "How many cop cars will respond to a four star wanted level.")
                            },
                            new Setting()
                            {
                                name = "FourStarCopHelis",
                                enableToggle = true,
                                subMenu = true,
                                value = 2,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Four Star Cop Helis", StarOptions, 2, "How many cop helicopters will respond to a four star wanted level.")
                            },
                            new Setting()
                            {
                                name = "FiveStarCopCars",
                                enableToggle = true,
                                subMenu = true,
                                value = -1,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Five Star Cop Cars", StarOptions, -1, "How many cop cars will respond to a five star wanted level.")
                            },
                            new Setting()
                            {
                                name = "FiveStarCopHelis",
                                enableToggle = true,
                                subMenu = true,
                                value = -1,
                                options = StarOptions,
                                menuItem = new UIMenuListItem("Five Star Cop Helis", StarOptions, -1, "How many cop helicopters will respond to a five star wanted level.")
                            }
                        }
                    },
                    new Setting()
                    {
                        name = "ChaseTimes",
                        enableToggle = true,
                        menu = new UIMenu("Chase Times", "Wanted level rises over time"),
                        menuItem = new UIMenuItem("Chase Times", "Wanted level rises over time"),
                        settings = new List<Setting>()
                        {
                            new Setting()
                            {
                                name = "EnableChaseTimes",
                                subMenu = true,
                                value = true,
                                menuItem = new UIMenuCheckboxItem("Feature Enabled", true, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                            },
                            new Setting()
                            {
                                name = "OneStarChaseTime",
                                enableToggle = true,
                                subMenu = true,
                                value = 5,
                                options = ChaseTimeOptions,
                                menuItem = new UIMenuListItem("One Star Chase Time", ChaseTimeOptions, 4, "How long in minutes until wanted level rises to 2 stars.")
                            },
                            new Setting()
                            {
                                name = "TwoStarChaseTime",
                                enableToggle = true,
                                subMenu = true,
                                value = 5,
                                options = ChaseTimeOptions,
                                menuItem = new UIMenuListItem("Two Star Chase Time", ChaseTimeOptions, 4, "How long in minutes until wanted level rises to 3 stars.")
                            },
                            new Setting()
                            {
                                name = "ThreeStarChaseTime",
                                enableToggle = true,
                                subMenu = true,
                                value = 5,
                                options = ChaseTimeOptions,
                                menuItem = new UIMenuListItem("Three Star Chase Time", ChaseTimeOptions, 4, "How long in minutes until ~y~PIT~w~ is allowed.")
                            }
                        }
                    },
                    new Setting()
                    {
                        name = "RequirePITAuthorization",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Require PIT authorization", true, "If enabled, prevents cops from performing PITs/Ramming. PIT Authorization is given if they see you driving recklessly.")
                    },
                    new Setting()
                    {
                        name = "RequireLethalForceAuthorization",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Require Lethal-Force authorization", true, "If enabled, prevents cops from using lethal weapons. Lethal Force Authorization is given if the cops see you shooting, or aiming a gun at them.")
                    },
                    new Setting()
                    {
                        name = "LethalForceOnAim",
                        enableToggle = true,
                        value = false,
                        menuItem = new UIMenuCheckboxItem("Lethal-Force on aiming any weapon", false, "If enabled, Lethal Force Authorization is given the moment you aim any weapon at all.")
                    },
                    new Setting()
                    {
                        name = "AllowBustOpportunity",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Allow extra bust opportunity", true, "If enabled, you can optionally give up when above 1 star by pressing ~y~E~w~ or ~y~Cover~w~ (Controller). Also cops will bust you while stunned or prone.")
                    },
                    new Setting()
                    {
                        name = "ShowHUD",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display PIT/Lethal Force HUD", true, "If enabled, the icons representing PIT/Lethal Force authorization will display near the Wanted Level stars.")
                    },
                    new Setting()
                    {
                        name = "ShowNotifications",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Notifications", true, "Toggles the notification system that keeps you informed of any changes in the police behavior.")
                    },
                    new Setting()
                    {
                        name = "ShowBigMessages",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Big Messages", true, "If enabled, the game will display messages similar to the Online shards when something important happens.")
                    },
                    new Setting()
                    {
                        name = "IconOffsetX",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Icon Offset X", offsetOptions, 0, "Controls the UI offset of the icons")
                    },
                    new Setting()
                    {
                        name = "IconOffsetY",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Icon Offset Y", offsetOptions, 0, "Controls the UI offset of the icons")
                    }
                }
            },
            new Setting()
            {
                name = "ArrestWarrants",
                menu = new UIMenu("Arrest Warrants", "Configure Arrest Warrants"),
                menuItem = new UIMenuItem("Arrest Warrants", "Configure Arrest Warrants"),
                settings = new List<Setting>()
                {
                    new Setting()
                    {
                        name = "Enabled",
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Module Enabled", true, "Toggles the functionality of the entire module. If disabled, all features below will be disabled also.")
                    },
                    new Setting()
                    {
                        name = "EnableStolenVehicles",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Enable stolen vehicles", true, "If enabled, driving stolen vehicles will get you a one star wanted level. ~n~~y~Note~w~: requires an extra mod to work.")
                    },
                    new Setting()
                    {
                        name = "RememberStarsWhenRecognised",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Remember last wanted level", true, "If enabled, you will be given the Wanted Level at which you escaped last time. Otherwise, you will recieve a standard 2 Star Wanted Level.")
                    },
                    new Setting()
                    {
                        name = "WarrantLengths",
                        enableToggle = true,
                        menu = new UIMenu("Warrant Length", "Warrant length settings based on the wanted stars"),
                        menuItem = new UIMenuItem("Warrant Length", "Warrant length settings based on the wanted stars"),
                        settings = new List<Setting>()
                        {
                            new Setting()
                            {
                                name = "OneStarWarrantTime",
                                subMenu = true,
                                value = 5,
                                options = WarrantTimeOptions,
                                menuItem = new UIMenuListItem("One Star Warrant Length", WarrantTimeOptions, 2, "How many minutes after escaping a one star wanted level before the warrant expires.")
                            },
                            new Setting()
                            {
                                name = "TwoStarWarrantTime",
                                subMenu = true,
                                value = 5,
                                options = WarrantTimeOptions,
                                menuItem = new UIMenuListItem("Two Star Warrant Length", WarrantTimeOptions, 2, "How many minutes after escaping a two star wanted level before the warrant expires. ~n~~y~Note~w~: In addition to previous minutes.")
                            },
                            new Setting()
                            {
                                name = "ThreeStarWarrantTime",
                                subMenu = true,
                                value = 5,
                                options = WarrantTimeOptions,
                                menuItem = new UIMenuListItem("Three Star Warrant Length", WarrantTimeOptions, 2, "How many minutes after escaping a three star wanted level before the warrant expires. ~n~~y~Note~w~: In addition to previous minutes.")
                            },
                            new Setting()
                            {
                                name = "FourStarWarrantTime",
                                subMenu = true,
                                value = 5,
                                options = WarrantTimeOptions,
                                menuItem = new UIMenuListItem("Four Star Warrant Length", WarrantTimeOptions, 2, "How many minutes after escaping a four star wanted level before the warrant expires. ~n~~y~Note~w~: In addition to previous minutes.")
                            },
                            new Setting()
                            {
                                name = "FiveStarWarrantTime",
                                subMenu = true,
                                value = 10,
                                options = WarrantTimeOptions,
                                menuItem = new UIMenuListItem("Five Star Warrant Length", WarrantTimeOptions, 4, "How many minutes after escaping a five star wanted level before the warrant expires. ~n~~y~Note~w~: In addition to previous minutes.")
                            }
                        }
                    },
                    new Setting()
                    {
                        name = "RecognitionSettings",
                        enableToggle = true,
                        menu = new UIMenu("Recognition Settings", "Police recognition settings"),
                        menuItem = new UIMenuItem("Recognition Settings", "Police recognition settings"),
                        settings = new List<Setting>()
                        {
                            new Setting()
                            {
                                name = "OverallSpotSpeed",
                                subMenu = true,
                                value = 100,
                                options = SpotSpeedOptions,
                                menuItem = new UIMenuListItem("Overall Spot Speed", SpotSpeedOptions, 3, "How fast or slow police will spot you, this can be seen via the Spotted Meter recognition % - default is 100%.")
                            },
                            new Setting()
                            {
                                name = "StolenRecognitionThreshold",
                                subMenu = true,
                                value = 90,
                                options = StolenRecognitionOptions,
                                menuItem = new UIMenuListItem("Stolen Vehicle Recognition Threshold", StolenRecognitionOptions, 5, "How close police must be to spot a stolen vehicle, lower values make it easier for them. default is 90%.")
                            }
                        }
                    },
                    new Setting()
                    {
                        name = "ShowSpottedMeter",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Spotted meter", true, "If enabled, you will see a HUD showing how close nearby police are to spotting you.")
                    },
                    new Setting()
                    {
                        name = "ShowHUD",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Arrest Warrants HUD", true, "If enabled, the Arrest Warrant HUD will be shown when appropiate. Disable it if you want to take shots from the cinematic camera or something.")
                    },
                    new Setting()
                    {
                        name = "ShowNotifications",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Notifications", true, "Toggles the notification system that keeps you informed of any changes in arrest warrants.")
                    },
                    new Setting()
                    {
                        name = "ShowBigMessages",
                        enableToggle = true,
                        value = true,
                        menuItem = new UIMenuCheckboxItem("Display Big Messages", true, "If enabled, the game will display messages similar to the Online shards when something important happens.")
                    },
                    new Setting()
                    {
                        name = "IconOffsetX",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Icon Offset X", offsetOptions, 0, "Controls the UI offset of the icons")
                    },
                    new Setting()
                    {
                        name = "IconOffsetY",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Icon Offset Y", offsetOptions, 0, "Controls the UI offset of the icons")
                    },
                    new Setting()
                    {
                        name = "TextOffsetX",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Text Offset X", offsetOptions, 0, "Controls the UI offset of the text")
                    },
                    new Setting()
                    {
                        name = "TextOffsetY",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Text Offset Y", offsetOptions, 0, "Controls the UI offset of the text")
                    },
                    new Setting()
                    {
                        name = "GradientOffsetX",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Gradient Offset X", offsetOptions, 0, "Controls the UI offset of the gradient")
                    },
                    new Setting()
                    {
                        name = "GradientOffsetY",
                        enableToggle = true,
                        value = 0,
                        options = offsetOptions,
                        menuItem = new UIMenuListItem("Gradient Offset Y", offsetOptions, 0, "Controls the UI offset of the gradient")
                    }
                }
            },
            new Setting()
            {
                name = "DisplayHints",
                value = true,
                menuItem = new UIMenuCheckboxItem("Display Hints", true, "If enabled, the game will display some hints to help you understand how Better Chases and Arrest Warrants work.")
            },
            new Setting()
            {
                menu = new UIMenu("Debug", "Mod commands & debugging"),
                menuItem = new UIMenuItem("Debug", "Mod commands & debugging"),
                settings = new List<Setting>()
                {
                    new Setting()
                    {
                        menuItem = new UIMenuItem("Issue Character Warrant"),
                        command = BetterChasesPlus.ArrestWarrants.IssuePlayerWarrant
                    },
                    new Setting()
                    {
                        menuItem = new UIMenuItem("Clear All Warrants"),
                        command = BetterChasesPlus.ArrestWarrants.ClearWarrants
                    }
                }
            }
        };

        public class Setting
        {
            public string name;
            public bool enableToggle;
            public dynamic value;
            public UIMenu menu;
            public dynamic menuItem;
            public bool subMenu;
            public List<dynamic> options;
            public delegate void commandDelegate();
            public commandDelegate command;

            public List<Setting> settings;
        }

        private static dynamic GetSetting(List<Setting> settings, string settingName)
        {
            dynamic value = null;
            foreach (Setting setting in settings)
            {
                if (setting.name == settingName)
                {
                    value = setting.value;
                }
                else if (value == null && setting.settings != null)
                {
                    value = GetSetting(setting.settings, settingName);
                }
            }

            return value;
        }

        public Options()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;

            Menu.Init();
            Load();

            Interval = 4;
        }

        private void OnTick(object sender, EventArgs e)
        {
            Menu.MainPool.ProcessMenus();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //Toggle script NativeUI menu
            if (e.KeyCode == MenuKey && !Menu.MainPool.IsAnyMenuOpen())
            {
                Menu.MainMenu.Visible = !Menu.MainMenu.Visible;
            }
        }

        private static void SaveSettings(List<Setting> settings, ref XmlDocument xml, string parentName = null)
        {
            foreach(Setting setting in settings)
            {
                if (setting.settings != null)
                {
                    if (parentName != null)
                    {
                        AddXPathNodeToXml(xml, setting.name, null, parentName);
                    }
                    SaveSettings(setting.settings, ref xml, setting.name);
                }

                if (setting.value != null)
                {
                    if (setting.subMenu)
                    {
                        AddXPathNodeToXml(xml, setting.name, setting.value.ToString(), "*/" + parentName);
                    }
                    else
                    {
                        AddXPathNodeToXml(xml, setting.name, setting.value.ToString(), parentName);
                    }
                }
            }
        }

        public static void Save()
        {
            XmlDocument xml = new XmlDocument();
            if (!File.Exists(@"scripts\\BetterChasesConfig.xml"))
            {
                try
                {
                    File.Create(@"scripts\\BetterChasesConfig.xml").Close();
                }
                catch (Exception exception)
                {
                    if (exception is UnauthorizedAccessException || exception is IOException)
                        GTA.UI.Notification.Show("~r~File write permission error!");

                    return;
                }
            }

            xml.CreateXmlDeclaration("1.0", "utf-8", "yes");
            xml.AppendChild(xml.CreateNode(XmlNodeType.Element, "BetterChasesPlus", null));

            AddXPathNodeToXml(xml, "MenuKey", MenuKey.ToString());

            SaveSettings(Settings, ref xml);

            try
            {
                xml.Save(@"scripts\\BetterChasesConfig.xml");
            }
            catch (Exception)
            {
                GTA.UI.Notification.Show("~r~Error saving config!");
            }
        }

        private static void LoadSetting(List<Setting> settings, XmlDocument xml, string parentName = null)
        {
            foreach (Setting setting in settings)
            {
                if (setting.name != null && setting.value != null)
                {
                    string parentString = parentName;
                    if (setting.subMenu)
                    {
                        parentString = "*/" + parentName;
                    }

                    string fileValue = GetXPathNodeFromXml(xml, setting.name, setting.value.ToString(), parentString);
                    if (fileValue != null && fileValue != "")
                    {
                        if (setting.value.GetType() == typeof(bool))
                        {
                            setting.value = bool.Parse(fileValue);
                        }
                        else if (setting.value.GetType() == typeof(int))
                        {
                            setting.value = int.Parse(fileValue);
                        }
                        else
                        {
                            setting.value = fileValue;
                        }

                        if (setting.menuItem.GetType() == typeof(UIMenuCheckboxItem))
                        {
                            setting.menuItem.Checked = setting.value;
                        }
                        //else if (setting.menuItem.GetType() == typeof(UIMenuSliderItem))
                        //{
                        //    setting.menuItem.Value = setting.value;
                        //}
                        else if (setting.menuItem.GetType() == typeof(UIMenuListItem))
                        {
                            //setting.menuItem.Index = setting.menuItem.Items.IndexOf(setting.value);
                            //setting.menuItem.Index = setting.menuItem.ItemToIndex(setting.value);
                            setting.menuItem.Index = setting.options.FindIndex(item => String.Equals(item, setting.value));
                        }
                    }
                }

                if (setting.settings != null)
                {
                    LoadSetting(setting.settings, xml, setting.name);
                }

                if (setting.enableToggle)
                {
                    setting.menuItem.Enabled = settings[0].value;
                }
            }
        }

        public static void Load()
        {
            XmlDocument xml = new XmlDocument();
            if (!File.Exists(@"scripts\\BetterChasesConfig.xml"))
            {
                GTA.UI.Notification.Show("No config file found");
                return;
            }

            try
            {
                xml.Load(@"scripts\\BetterChasesConfig.xml");
            }
            catch (Exception exception)
            {
                if (exception is UnauthorizedAccessException || exception is IOException)
                    GTA.UI.Notification.Show("~r~File read permission error!");

                return;
            }

            if (xml != null)
            {
                LoadSetting(Settings, xml);

                //DisplayHints = bool.Parse(GetXPathNodeFromXml(ScriptSettings, "//DisplayHints", DisplayHints.ToString()));
                MenuKey = (Keys)Enum.Parse(typeof(Keys), GetXPathNodeFromXml(xml, "//MenuKey", MenuKey.ToString()));
            }
            else
            {
                GTA.UI.Notification.Show("~r~Failed to load config file!");
            }
        }

        private static void AddMenus(List<Setting> settings, UIMenu menu)
        {
            foreach (Setting setting in settings)
            {
                if (setting.menu != null)
                {
                    Menu.MainPool.Add(setting.menu);

                    menu.AddItem(setting.menuItem);
                    menu.BindMenuToItem(setting.menu, setting.menuItem);

                    AddMenus(setting.settings, setting.menu);
                }
                else
                {
                    menu.AddItem(setting.menuItem);
                }
            }
        }

        public class Menu
        {
            public static MenuPool MainPool = new MenuPool();
            public static UIMenu MainMenu = new UIMenu("Better Chases+", "Configure the Global Options.");

            public static void Init()
            {
                MainPool.Add(MainMenu);

                AddMenus(Settings, MainMenu);

                foreach (UIMenu MenuItem in MainPool.ToList())
                {
                    //MenuItem.RefreshIndex();
                    MenuItem.OnItemSelect += OnItemSelect;
                    MenuItem.OnListChange += OnListChange;
                    //MenuItem.OnSliderChange += OnSliderChange;
                    MenuItem.OnCheckboxChange += OnCheckboxChange;
                }

                MainMenu.OnMenuClose += OnMenuClose;
            }

            public static void ApplySetting(List<Setting> settings, dynamic menuItem, dynamic value)
            {
                foreach (Setting setting in settings)
                {
                    if (menuItem.GetHashCode() == setting.menuItem.GetHashCode())
                    {
                        if (setting.menuItem.GetType() == typeof(UIMenuCheckboxItem))
                        {
                            setting.value = value;
                        }
                        else if (setting.menuItem.GetType() == typeof(UIMenuListItem))
                        {
                            setting.value = setting.options[value];
                        }
                        else if (setting.command != null)
                        {
                            setting.command();
                        }
                    }
                    else if (setting.settings != null)
                    {
                        ApplySetting(setting.settings, menuItem, value);
                    }
                    
                    if (setting.enableToggle)
                    {
                        setting.menuItem.Enabled = settings[0].value;
                    }
                }
            }

            public static void OnMenuClose(UIMenu Sender)
            {
                if (Sender.GetHashCode() == MainMenu.GetHashCode())
                {
                    Save();
                }
            }

            public static void OnItemSelect(UIMenu Sender, UIMenuItem MenuItem, int Index)
            {
                ApplySetting(Settings, MenuItem, Index);
            }

            public static void OnListChange(UIMenu Sender, UIMenuListItem List, int Index)
            {
                ApplySetting(Settings, List, Index);
            }

            //public static void OnSliderChange(UIMenu Sender, UIMenuSliderItem SliderItem, int Value)
            //{
            //    ApplySetting(Settings, SliderItem, Value);
            //}

            public static void OnCheckboxChange(UIMenu Sender, UIMenuCheckboxItem CheckboxItem, bool Checked)
            {
                ApplySetting(Settings, CheckboxItem, Checked);
            }
        }

        internal static string GetXPathNodeFromXml(XmlDocument XmlDoc, String SearchString, String DefaultValue, String ParentString = null)
        {
            try
            {
                if (ParentString != null)
                {
                    XmlNode parent = XmlDoc.DocumentElement.SelectSingleNode(ParentString);
                    return parent.SelectSingleNode(SearchString).InnerText;
                }
                else
                {
                    return XmlDoc.DocumentElement.SelectSingleNode(SearchString).InnerText;
                }
            }
            catch (Exception)
            {
                GTA.UI.Notification.Show("~y~Error Loading Setting " + SearchString + ". Possibly a new setting.");
                return DefaultValue;
            }
        }

        internal static void SetXPathNodeFromXml(XmlDocument XmlDoc, String SearchString, String Value)
        {
            try
            {
                XmlDoc.DocumentElement.SelectSingleNode(SearchString).InnerText = Value;
            }
            catch (Exception) { }
        }

        internal static void RemoveXPathNodeFromXml(XmlDocument XmlDoc, String SearchString)
        {
            try
            {
                XmlNode Node = XmlDoc.DocumentElement.SelectSingleNode(SearchString);
                XmlDoc.DocumentElement.RemoveChild(Node);
            }
            catch (Exception) { }
        }

        internal static void AddXPathNodeToXml(XmlDocument XmlDoc, String SearchString, String Value, String ParentString = null)
        {
            try
            {
                if (ParentString != null)
                {
                    XmlNode parent = XmlDoc.DocumentElement.SelectSingleNode(ParentString);
                    if (parent == null)
                    {
                        parent = XmlDoc.CreateNode(XmlNodeType.Element, ParentString, null);
                        XmlDoc.DocumentElement.AppendChild(parent);
                    }

                    XmlNode Node = XmlDoc.CreateNode(XmlNodeType.Element, SearchString, null);
                    Node.InnerText = Value;
                    parent.AppendChild(Node);
                }
                else
                {
                    XmlNode Node = XmlDoc.CreateNode(XmlNodeType.Element, SearchString, null);
                    Node.InnerText = Value;
                    XmlDoc.DocumentElement.AppendChild(Node);
                }
            }
            catch (Exception exception)
            {
                GTA.UI.Notification.Show("~r~Error Saving Setting~n~" + exception.ToString());
            }
        }
    }
}