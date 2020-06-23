using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace BetterChasesPlus
{
    public class Config : Script
    {
        public static GlobalConfig Options = new GlobalConfig();

        public static List<dynamic> WantedLevelOptions = new List<dynamic> { 0, 1, 2, 3, 4, 5 };
        public static List<dynamic> WantedLevelControlOptions = new List<dynamic> { "Full", "Passive" };
        public static List<dynamic> SpeedingThresholdOptions = (dynamic)Enumerable.Range(1, 100).Cast<dynamic>().ToList();
        public static List<dynamic> PoliceWitnessThresholdOptions = (dynamic)Enumerable.Range(0, 100).Cast<dynamic>().ToList();
        public static List<dynamic> WarrantLengthOptions = (dynamic)Enumerable.Range(1, 672).Cast<dynamic>().ToList();
        public static List<dynamic> ChaseLengthOptions = (dynamic)Enumerable.Range(1, 359).Cast<dynamic>().ToList();
        public static List<dynamic> CopDispatchOptions = (dynamic)Enumerable.Range(-1, 21).Cast<dynamic>().ToList();
        public static List<dynamic> SpotSpeedOptions = (dynamic)Enumerable.Range(10, 300).Cast<dynamic>().ToList();
        public static List<dynamic> OffsetOptions = (dynamic)Enumerable.Range(-2000, 4000).Cast<dynamic>().ToList();

        public Config()
        {
            Tick += OnTick;
            KeyUp += OnKeyUp;

            Interval = 4;

            Load();
        }

        private void OnTick(object sender, EventArgs e)
        {
            Menu.MainPool.ProcessMenus();
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            //Toggle NativeUI Ingame Menu
            if (e.KeyCode == Menu.MenuKey && !Menu.MainPool.IsAnyMenuOpen())
            {
                Menu.MainMenu.Visible = !Menu.MainMenu.Visible;
            }
        }

        public static void Save()
        {
            XmlAttributes myXmlAttributes = new XmlAttributes();
            XmlRootAttribute myXmlRootAttribute = new XmlRootAttribute("BetterChasesPlus");
            myXmlAttributes.XmlRoot = myXmlRootAttribute;
            XmlAttributeOverrides myXmlAttributeOverrides = new XmlAttributeOverrides();
            myXmlAttributeOverrides.Add(Options.GetType(), myXmlAttributes);

            XmlSerializer serializer = new XmlSerializer(Options.GetType(), myXmlAttributeOverrides);
            TextWriter writer = new StreamWriter(@"scripts\\BetterChasesConfig.xml");
            serializer.Serialize(writer, Options);
            writer.Close();
        }

        public static void Load()
        {
            XmlAttributes myXmlAttributes = new XmlAttributes();
            XmlRootAttribute myXmlRootAttribute = new XmlRootAttribute("BetterChasesPlus");
            myXmlAttributes.XmlRoot = myXmlRootAttribute;
            XmlAttributeOverrides myXmlAttributeOverrides = new XmlAttributeOverrides();
            myXmlAttributeOverrides.Add(Options.GetType(), myXmlAttributes);

            XmlSerializer serializer = new XmlSerializer(Options.GetType(), myXmlAttributeOverrides);

            try
            {
                TextReader reader = new StreamReader(@"scripts\\BetterChasesConfig.xml");
                Options = (GlobalConfig)serializer.Deserialize(reader);
                reader.Close();

                ReloadMenu();
            }
            catch (Exception) { };
        }

        public static void ReloadMenu()
        {
            if (Menu == null)
            {
                return;
            }

            if (Menu.MainPool.IsAnyMenuOpen())
            {
                Menu.MainPool.CloseAllMenus();
            }

            Menu = new IngameMenu();
        }

        public static bool IsMenuOpen
        {
            get
            {
                return Menu.MainPool.IsAnyMenuOpen();
            }
        }

        public class GlobalConfig
        {
            public Keys MenuKey { get; set; } = Keys.F7;
            public Keys SurrenderKey { get; set; } = Keys.E;
            public GTA.Control SurrenderButton { get; set; } = GTA.Control.Cover;
            public bool DisplayHints { get; set; } = true;

            public BetterChasesConfig BetterChases = new BetterChasesConfig();
            public ArrestWarrantsConfig ArrestWarrants = new ArrestWarrantsConfig();
        }

        public class BetterChasesConfig
        {
            public bool Enabled { get; set; } = true;
            public string WantedLevelControl { get; set; } = "Full";
            public ChaseEscalatesConfig ChaseEscalates = new ChaseEscalatesConfig();
            public CopDispatchConfig CopDispatch = new CopDispatchConfig();
            public CrimesConfig Crimes = new CrimesConfig();
            public bool CopsManageTraffic { get; set; } = true;
            public bool WreckedCopsStopChasing { get; set; } = true;
            public bool DisallowCopCommandeering { get; set; } = true;
            public bool RequirePITAuthorization { get; set; } = true;
            public bool RequireLethalForceAuthorization { get; set; } = true;
            public bool AllowBustOpportunity { get; set; } = true;
            public bool ShowHUD { get; set; } = true;
            public bool ShowNotifications { get; set; } = true;
            public bool ShowBigMessages { get; set; } = true;
            public int IconOffsetX { get; set; } = 0;
            public int IconOffsetY { get; set; } = 0;
        }

        public class ArrestWarrantsConfig
        {
            public bool Enabled { get; set; } = true;
            public int SpotSpeed { get; set; } = 100;
            public WarrantLengthsConfig WarrantLenghts = new WarrantLengthsConfig();
            public bool RememberChase { get; set; } = true;
            public bool ShowSpottedMeter { get; set; } = true;
            public bool ShowSpottedIndicators { get; set; } = true;
            public bool ShowHUD { get; set; } = true;
            public bool ShowNotifications { get; set; } = true;
            public bool ShowBigMessages { get; set; } = true;
            public int IconOffsetX { get; set; } = 0;
            public int IconOffsetY { get; set; } = 0;
            public int TextOffsetX { get; set; } = 0;
            public int TextOffsetY { get; set; } = 0;
            public int GradientOffsetX { get; set; } = 0;
            public int GradientOffsetY { get; set; } = 0;
        }

        public class ChaseEscalatesConfig
        {
            public bool Enabled { get; set; } = true;

            public ChaseEscalatesPhaseConfig PhaseOne = new ChaseEscalatesPhaseConfig()
            {
                Enabled = true,
                Length = 30,
                RequestBackup = true
            };

            public ChaseEscalatesPhaseConfig PhaseTwo = new ChaseEscalatesPhaseConfig()
            {
                Enabled = true,
                Length = 30,
                RequestBackup = true
            };

            public ChaseEscalatesPhaseConfig PhaseThree = new ChaseEscalatesPhaseConfig()
            {
                Enabled = true,
                Length = 30,
                RequestBackup = true
            };

            public ChaseEscalatesPhaseConfig PhaseFour = new ChaseEscalatesPhaseConfig()
            {
                Enabled = true,
                Length = 30,
                WantedLevel = 3,
                PITAuthorized = true
            };
        }

        public class ChaseEscalatesPhaseConfig
        {
            public bool Enabled { get; set; }
            public int Length { get; set; }
            public int WantedLevel { get; set; }
            public bool PITAuthorized { get; set; }
            public bool LethalForceAuthorized { get; set; }
            public bool RequestBackup { get; set; }
        }

        public class WarrantLengthsConfig
        {
            public int OneStar { get; set; } = 6;
            public int TwoStar { get; set; } = 18;
            public int ThreeStar { get; set; } = 24;
            public int FourStar { get; set; } = 48;
            public int FiveStar { get; set; } = 72;
        }

        public class CopDispatchConfig
        {
            public bool Enabled { get; set; } = true;
            public CopDispatchStarConfig OneStar = new CopDispatchStarConfig
            {
                GroundMin = 1,
                GroundMax = 2
            };
            public CopDispatchStarConfig TwoStar = new CopDispatchStarConfig
            {
                GroundMin = 2,
                GroundMax = 3
            };
            public CopDispatchStarConfig ThreeStar = new CopDispatchStarConfig
            {
                GroundMin = 3,
                GroundMax = 4,
                AirMin = 0
            };
            public CopDispatchStarConfig FourStar = new CopDispatchStarConfig
            {
                GroundMin = 4,
                GroundMax = 6,
                AirMin = 1,
                PITAuthorized = true
            };
            public CopDispatchStarConfig FiveStar = new CopDispatchStarConfig
            {
                GroundMin = -1,
                AirMin = -1,
                PITAuthorized = true,
                LethalForceAuthorized = true
            };
        }

        public class CopDispatchStarConfig
        {
            public int GroundMin { get; set; }
            public int AirMin { get; set; }
            public int GroundMax { get; set; }
            public bool PITAuthorized { get; set; }
            public bool LethalForceAuthorized { get; set; }
        }

        public class CrimesConfig
        {
            public CrimeConfig GTA = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 2,
                PITAuthorized = false,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 20,
                RequestBackup = true
            };

            public CrimeConfig Stolen = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 2,
                PITAuthorized = false,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 40,
                RequestBackup = true
            };

            public CrimeConfig Speeding = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel= 2,
                PITAuthorized = false,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 40,
                RequestBackup = true,
                Speed = 35
            };

            public CrimeConfig Reckless = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 3,
                PITAuthorized = false,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 20,
                RequestBackup = true,
                Speed = 15
            };

            public CrimeConfig Armed = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 4,
                PITAuthorized = false,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 40,
                WantedLevel = 2,
                RequestBackup = true
            };

            public CrimeConfig Aiming = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 4,
                PITAuthorized = true,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 40,
                WantedLevel = 3,
                RequestBackup = true
            };

            public CrimeConfig Assault = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 4,
                PITAuthorized = true,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 30,
                WantedLevel = 3,
                RequestBackup = true
            };

            public CrimeConfig PoliceAssault = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 0,
                PITAuthorized = true,
                LethalForceAuthorized = false,
                PoliceWitnessThreshold = 10,
                WantedLevel = 3,
                RequestBackup = true
            };

            public CrimeConfig Shooting = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 4,
                PITAuthorized = true,
                LethalForceAuthorized = true,
                PoliceWitnessThreshold = 10,
                WantedLevel = 4
            };

            public CrimeConfig Murder = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 0,
                PITAuthorized = true,
                LethalForceAuthorized = true,
                PoliceWitnessThreshold = 40,
                WantedLevel = 4,
                RequestBackup = true
            };

            public CrimeConfig PoliceMurder = new CrimeConfig()
            {
                Enabled = true,
                MaxWantedLevel = 0,
                PITAuthorized = true,
                LethalForceAuthorized = true,
                PoliceWitnessThreshold = 10,
                WantedLevel = 5
            };
        }

        public class CrimeConfig
        {
            public bool Enabled { get; set; }
            public int MaxWantedLevel { get; set; }
            public bool PITAuthorized { get; set; }
            public bool LethalForceAuthorized { get; set; }
            public int PoliceWitnessThreshold { get; set; }
            public int WantedLevel { get; set; }
            public bool RequestBackup { get; set; }
            public int Speed { get; set; }
        }

        private static IngameMenu Menu = new IngameMenu();

        private class IngameMenu
        {
            public Keys MenuKey = Config.Options.MenuKey;
            public NativeUI.MenuPool MainPool = new NativeUI.MenuPool();
            public NativeUI.UIMenu MainMenu = new NativeUI.UIMenu("Better Chases+", "Configure the Global Options.");

            public IngameMenu()
            {
                // clear out any previous menus
                foreach (NativeUI.UIMenu menuItem in MainPool.ToList())
                {
                    menuItem.Clear();
                }

                MainPool = new NativeUI.MenuPool();

                MainPool.Add(MainMenu);

                AddMenus(MainPool, Menus, MainMenu);

                foreach (NativeUI.UIMenu menuItem in MainPool.ToList())
                {
                    menuItem.OnItemSelect += OnItemSelect;
                    menuItem.OnListChange += OnListChange;
                    //MenuItem.OnSliderChange += OnSliderChange;
                    menuItem.OnCheckboxChange += OnCheckboxChange;
                }

                MainMenu.OnMenuClose += OnMenuClose;
                MainPool.RefreshIndex();
            }

            private static void AddMenus(NativeUI.MenuPool mainPool, List<Menu> menus, NativeUI.UIMenu parent)
            {
                foreach (Menu menu in menus)
                {
                    if (menu.menu != null)
                    {
                        mainPool.Add(menu.menu);
                        parent.AddItem(menu.menuItem);
                        parent.BindMenuToItem(menu.menu, menu.menuItem);

                        if (menu.subMenus != null)
                        {
                            AddMenus(mainPool, menu.subMenus, menu.menu);
                        }
                    }
                    else
                    {
                        parent.AddItem(menu.menuItem);
                    }

                    // Toggle anyother menus on the same level
                    if (menu.toggle)
                    {
                        ToggleMenus(menus, menu.value);
                    }
                }
            }

            private static void ApplySetting(List<Menu> menus, NativeUI.UIMenuItem menuItem, dynamic value)
            {
                foreach (Menu menu in menus)
                {
                    if (menu.menuItem.GetHashCode() == menuItem.GetHashCode())
                    {
                        if (value != null)
                        {
                            menu.value = value;
                        }
                        else if (menu.command != null)
                        {
                            menu.command();
                        }

                        // Toggle anyother menus on the same level
                        if (menu.toggle)
                        {
                            ToggleMenus(menus, menu.value);
                        }
                    }
                    else if (menu.subMenus != null)
                    {
                        ApplySetting(menu.subMenus, menuItem, value);
                    }
                }
            }

            private static void ToggleMenus(List<Menu> menus, bool enabled)
            {
                foreach (Menu menu in menus)
                {
                    if (menu.toggle != true)
                    {
                        menu.menuItem.Enabled = enabled;
                    }
                }
            }

            private static void OnItemSelect(NativeUI.UIMenu sender, NativeUI.UIMenuItem menuItem, int index)
            {
                ApplySetting(Config.Menu.Menus, menuItem, null);
            }

            private static void OnCheckboxChange(NativeUI.UIMenu sender, NativeUI.UIMenuCheckboxItem checkboxItem, bool enabled)
            {
                ApplySetting(Config.Menu.Menus, checkboxItem, enabled);
            }

            private static void OnListChange(NativeUI.UIMenu sender, NativeUI.UIMenuListItem listItem, int index)
            {
                ApplySetting(Config.Menu.Menus, listItem, listItem.IndexToItem(index));
            }

            private static void OnMenuClose(NativeUI.UIMenu sender)
            {
                if (sender.GetHashCode() == Config.Menu.MainMenu.GetHashCode())
                {
                    Config.Save();
                }
            }

            private class Menu
            {
                public NativeUI.UIMenu menu;
                public NativeUI.UIMenuItem menuItem;
                public List<Menu> subMenus;
                public object parent;
                public string name;
                public dynamic value
                {
                    get
                    {
                        return parent.GetType().GetProperty(name).GetValue(parent, null);
                    }
                    set
                    {
                        parent.GetType().GetProperty(name).SetValue(parent, value);
                    }
                }
                public delegate void commandDelegate();
                public commandDelegate command;
                public bool toggle;

            }

            private List<Menu> Menus = new List<Menu>()
            {
                new Menu()
                {
                    menu = new NativeUI.UIMenu("Better Chases", "Configure Better Chases"),
                    menuItem = new NativeUI.UIMenuItem("Better Chases", "Configure Better Chases"),
                    subMenus = new List<Menu>()
                    {
                        new Menu()
                        {
                            parent = Options.BetterChases,
                            name = "Enabled",
                            toggle = true,
                            menuItem = new NativeUI.UIMenuCheckboxItem("Module Enabled", Options.BetterChases.Enabled, "Toggles the functionality of the entire module. If disabled, all features below will be disabled also.")
                        },
                        new Menu()
                        {
                            parent = Options.BetterChases,
                            name = "WantedLevelControl",
                            menuItem = new NativeUI.UIMenuListItem("Wanted Level Control", WantedLevelControlOptions, WantedLevelControlOptions.IndexOf(Options.BetterChases.WantedLevelControl), "Full if you want this mod to take full control, passive for mod compatibility.")
                        },
                        new Menu()
                        {
                            menu = new NativeUI.UIMenu("Chase Escalates", "Chase Escalates Over Time"),
                            menuItem = new NativeUI.UIMenuItem("Chase Escalates", "Chase Escalates Over Time"),
                            subMenus = new List<Menu>()
                            {
                                new Menu()
                                {
                                    parent = Config.Options.BetterChases.ChaseEscalates,
                                    name = "Enabled",
                                    toggle = true,
                                    menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Config.Options.BetterChases.ChaseEscalates.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Phase One", "Phase One Chase Escalation"),
                                    menuItem = new NativeUI.UIMenuItem("Phase One", "Phase One Chase Escalation"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.ChaseEscalates.PhaseOne.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "Length",
                                            menuItem = new NativeUI.UIMenuListItem("Chase Time", ChaseLengthOptions, ChaseLengthOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseOne.Length), "How long in in-game minutes until below take affect.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.ChaseEscalates.PhaseOne.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.ChaseEscalates.PhaseOne.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.ChaseEscalates.PhaseOne.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseOne,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseOne.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Phase Two", "Phase Two Chase Escalation"),
                                    menuItem = new NativeUI.UIMenuItem("Phase Two", "Phase Two Chase Escalation"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.ChaseEscalates.PhaseTwo.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "Length",
                                            menuItem = new NativeUI.UIMenuListItem("Chase Time", ChaseLengthOptions, ChaseLengthOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseTwo.Length), "How long in in-game minutes until below take affect.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.ChaseEscalates.PhaseTwo.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.ChaseEscalates.PhaseTwo.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.ChaseEscalates.PhaseTwo.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseTwo,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseTwo.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Phase Three", "Phase Three Chase Escalation"),
                                    menuItem = new NativeUI.UIMenuItem("Phase Three", "Phase Three Chase Escalation"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.ChaseEscalates.PhaseThree.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "Length",
                                            menuItem = new NativeUI.UIMenuListItem("Chase Time", ChaseLengthOptions, ChaseLengthOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseThree.Length), "How long in in-game minutes until below take affect.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.ChaseEscalates.PhaseThree.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.ChaseEscalates.PhaseThree.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.ChaseEscalates.PhaseThree.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseThree,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseThree.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Phase Four", "Phase Four Chase Escalation"),
                                    menuItem = new NativeUI.UIMenuItem("Phase Four", "Phase Four Chase Escalation"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.ChaseEscalates.PhaseFour.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "Length",
                                            menuItem = new NativeUI.UIMenuListItem("Chase Time", ChaseLengthOptions, ChaseLengthOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseFour.Length), "How long in in-game minutes until below take affect.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.ChaseEscalates.PhaseFour.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.ChaseEscalates.PhaseFour.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.ChaseEscalates.PhaseFour.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.ChaseEscalates.PhaseFour,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.ChaseEscalates.PhaseFour.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                            }
                        },
                        new Menu()
                        {
                            menu = new NativeUI.UIMenu("Dispatch Control", "Control Cop Vehicle Dispatching"),
                            menuItem = new NativeUI.UIMenuItem("Dispatch Control", "Control Cop Vehicle Dispatching"),
                            subMenus = new List<Menu>()
                            {
                                new Menu()
                                {
                                    parent = Config.Options.BetterChases.CopDispatch,
                                    name = "Enabled",
                                    toggle = true,
                                    menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Config.Options.BetterChases.CopDispatch.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("One Star", "One Star Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("One Star", "One Star Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.OneStar,
                                            name = "GroundMin",
                                            menuItem = new NativeUI.UIMenuListItem("One Star Ground Min Limit", new List<dynamic>() { 1, 2 }, new List<dynamic>() { 1, 2 }.IndexOf(Options.BetterChases.CopDispatch.OneStar.GroundMin), "Minimum ground cop vehicles that will respond to a one star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.OneStar,
                                            name = "GroundMax",
                                            menuItem = new NativeUI.UIMenuListItem("One Star Ground Max Limit", new List<dynamic>() { 1, 2, 3 }, new List<dynamic>() { 1, 2, 3 }.IndexOf(Options.BetterChases.CopDispatch.OneStar.GroundMax), "Maximum ground cop vehicles allowed to respond to a one star wanted level. Additional backup will increase the wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.OneStar,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.CopDispatch.OneStar.PITAuthorized, "Allow police to PIT once this wanted level is reached.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.OneStar,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.CopDispatch.OneStar.LethalForceAuthorized, "Allow police to use lethal force once this wanted level is reached.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Two Star", "TwoStar Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Two Star", "Two Star Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.TwoStar,
                                            name = "GroundMin",
                                            menuItem = new NativeUI.UIMenuListItem("Two Star Ground Min Limit", new List<dynamic>() { 2, 3 }, new List<dynamic>() { 2, 3 }.IndexOf(Options.BetterChases.CopDispatch.TwoStar.GroundMin), "Minimum ground cop vehicles that will respond to a two star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.TwoStar,
                                            name = "GroundMax",
                                            menuItem = new NativeUI.UIMenuListItem("Two Star Ground Max Limit", new List<dynamic>() { 2, 3, 4 }, new List<dynamic>() { 2, 3, 4 }.IndexOf(Options.BetterChases.CopDispatch.TwoStar.GroundMax), "Maximum ground cop vehicles allowed to respond to a two star wanted level. Additional backup will increase the wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.TwoStar,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.CopDispatch.TwoStar.PITAuthorized, "Allow police to PIT once this wanted level is reached.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.TwoStar,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.CopDispatch.TwoStar.LethalForceAuthorized, "Allow police to use lethal force once this wanted level is reached.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Three Star", "Three Star Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Three Star", "Three Star Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.ThreeStar,
                                            name = "GroundMin",
                                            menuItem = new NativeUI.UIMenuListItem("Three Star Ground Min Limit", new List<dynamic>() { 3, 4 }, new List<dynamic>() { 3, 4 }.IndexOf(Options.BetterChases.CopDispatch.ThreeStar.GroundMin), "Minimum ground cop vehicles that will respond to a three star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.ThreeStar,
                                            name = "GroundMax",
                                            menuItem = new NativeUI.UIMenuListItem("Three Star Ground Max Limit", new List<dynamic>() { 3, 4, 5 }, new List<dynamic>() { 3, 4, 5 }.IndexOf(Options.BetterChases.CopDispatch.ThreeStar.GroundMax), "Maximum ground cop vehicles allowed to respond to a three star wanted level. Additional backup will increase the wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.ThreeStar,
                                            name = "AirMin",
                                            menuItem = new NativeUI.UIMenuListItem("Three Star Air Min Limit", new List<dynamic>() { 0, 1 }, new List<dynamic>() { 0, 1 }.IndexOf(Options.BetterChases.CopDispatch.ThreeStar.AirMin), "Minimum air cop vehicles that will respond to a three star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.ThreeStar,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.CopDispatch.ThreeStar.PITAuthorized, "Allow police to PIT once this wanted level is reached.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.ThreeStar,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.CopDispatch.ThreeStar.LethalForceAuthorized, "Allow police to use lethal force once this wanted level is reached.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Four Star", "Four Star Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Four Star", "Four Star Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FourStar,
                                            name = "GroundMin",
                                            menuItem = new NativeUI.UIMenuListItem("Four Star Ground Min Limit", new List<dynamic>() { 4, 5, 6 }, new List<dynamic>() { 4, 5, 6 }.IndexOf(Options.BetterChases.CopDispatch.FourStar.GroundMin), "Minimum ground cop vehicles that will respond to a four star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FourStar,
                                            name = "GroundMax",
                                            menuItem = new NativeUI.UIMenuListItem("Four Star Ground Max Limit", new List<dynamic>() { 4, 5, 6, 7 }, new List<dynamic>() { 4, 5, 6, 7 }.IndexOf(Options.BetterChases.CopDispatch.FourStar.GroundMax), "Maximum ground cop vehicles allowed to respond to a four star wanted level. Additional backup will increase the wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FourStar,
                                            name = "AirMin",
                                            menuItem = new NativeUI.UIMenuListItem("Four Star Air Min Limit", new List<dynamic>() { 0, 1, 2 }, new List<dynamic>() { 0, 1, 2 }.IndexOf(Options.BetterChases.CopDispatch.FourStar.AirMin), "Minimum air cop vehicles that will respond to a four star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FourStar,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.CopDispatch.FourStar.PITAuthorized, "Allow police to PIT once this wanted level is reached.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FourStar,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.CopDispatch.FourStar.LethalForceAuthorized, "Allow police to use lethal force once this wanted level is reached.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Five Star", "Five Star Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Five Star", "Five Star Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FiveStar,
                                            name = "GroundMin",
                                            menuItem = new NativeUI.UIMenuListItem("Five Star Ground Min Limit", new List<dynamic>() { 5, 6, 7, 8 }, new List<dynamic>() { 5, 6, 7, 8 }.IndexOf(Options.BetterChases.CopDispatch.FiveStar.GroundMin), "Minimum ground cop vehicles that will respond to a five star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FiveStar,
                                            name = "AirMin",
                                            menuItem = new NativeUI.UIMenuListItem("Five Star Air Min Limit", new List<dynamic>() { 0, 1, 2, 3 }, new List<dynamic>() { 0, 1, 2, 3 }.IndexOf(Options.BetterChases.CopDispatch.FiveStar.AirMin), "Minimum air cop vehicles that will respond to a five star wanted level.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FiveStar,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.CopDispatch.FiveStar.PITAuthorized, "Allow police to PIT once this wanted level is reached.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.CopDispatch.FiveStar,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.CopDispatch.FiveStar.LethalForceAuthorized, "Allow police to use lethal force once this wanted level is reached.")
                                        }
                                    }
                                }
                            }
                        },
                        new Menu()
                        {
                            menu = new NativeUI.UIMenu("Crimes Control", "Control Crimes During Chases"),
                            menuItem = new NativeUI.UIMenuItem("Crimes Control", "Control Crimes During Chases"),
                            subMenus = new List<Menu>()
                            {
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Grand Theft Auto", "Grand Theft Auto Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Grand Theft Auto", "Grand Theft Auto Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.GTA.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.GTA.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.GTA.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.GTA.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.GTA.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.GTA.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.GTA,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.GTA.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Stolen Vehicle", "Stolen Vehicle Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Stolen Vehicle", "Stolen Vehicle Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Stolen.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Stolen.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Stolen.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Stolen.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Stolen.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Stolen.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Stolen,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Stolen.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Excessive Speeding", "Excessive Speeding Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Excessive Speeding", "Excessive Speeding Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Speeding.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Speeding.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Speeding.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Speeding.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Speeding.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Speeding.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Speeding.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Speeding,
                                            name = "Speed",
                                            menuItem = new NativeUI.UIMenuListItem("Speed Threshold", SpeedingThresholdOptions, SpeedingThresholdOptions.IndexOf(Options.BetterChases.Crimes.Speeding.Speed), "Speed in m/s needed to exceed.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Reckless Driving", "Reckless Driving Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Reckless Driving", "Reckless Driving Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Reckless.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Reckless.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Reckless.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Reckless.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Reckless.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Reckless.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Reckless.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Reckless,
                                            name = "Speed",
                                            menuItem = new NativeUI.UIMenuListItem("Speed Threshold", SpeedingThresholdOptions, SpeedingThresholdOptions.IndexOf(Options.BetterChases.Crimes.Reckless.Speed), "Speed in m/s needed to exceed.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Armed", "Armed Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Armed", "Armed Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Armed.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Armed.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Armed.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Armed.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Armed.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Armed.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Armed,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Armed.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Aiming Weapon", "Aiming Weapon Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Aiming Weapon", "Aiming Weapon Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Aiming.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Aiming.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Aiming.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Aiming.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Aiming.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Aiming.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Aiming,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Aiming.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Assaulting Civilian", "Assaulting Civilian Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Assaulting Civilian", "Assaulting Civilian Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Assault.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Assault.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Assault.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Assault.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Assault.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Assault.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Assault,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Assault.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Assaulting Police", "Assaulting Police Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Assaulting Police", "Assaulting Police Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.PoliceAssault.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.PoliceAssault.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.PoliceAssault.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.PoliceAssault.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.PoliceAssault.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.PoliceAssault.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceAssault,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.PoliceAssault.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Shooting Weapon", "Shooting Weapon Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Shooting Weapon", "Shooting Weapon Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Shooting.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Shooting.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Shooting.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Shooting.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Shooting.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Shooting.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Shooting,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Shooting.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Civilian Murder", "Civilian Murder Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Civilian Murder", "Civilian Murder Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.Murder.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Murder.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.Murder.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.Murder.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.Murder.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.Murder.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.Murder,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.Murder.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                },
                                new Menu()
                                {
                                    menu = new NativeUI.UIMenu("Police Murder", "Police Murder Configuration"),
                                    menuItem = new NativeUI.UIMenuItem("Police Murder", "Police Murder Configuration"),
                                    subMenus = new List<Menu>()
                                    {
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "Enabled",
                                            toggle = true,
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Feature Enabled", Options.BetterChases.Crimes.PoliceMurder.Enabled, "Toggles the functionality of the entire feature. If disabled, all options below will be disabled also.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "MaxWantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Max Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.PoliceMurder.MaxWantedLevel), "When wanted level is above this limit ignore this crime, 0 to disable.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "PoliceWitnessThreshold",
                                            menuItem = new NativeUI.UIMenuListItem("Police Witness Threshold", PoliceWitnessThresholdOptions, PoliceWitnessThresholdOptions.IndexOf(Options.BetterChases.Crimes.PoliceMurder.PoliceWitnessThreshold), "Lower value is easier to spot, 0 to not need LOS.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "PITAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("PIT Authorization", Options.BetterChases.Crimes.PoliceMurder.PITAuthorized, "Allow police to PIT.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "LethalForceAuthorized",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Lethal Force Authorization", Options.BetterChases.Crimes.PoliceMurder.LethalForceAuthorized, "Allow police to use lethal force.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "RequestBackup",
                                            menuItem = new NativeUI.UIMenuCheckboxItem("Request Backup", Options.BetterChases.Crimes.PoliceMurder.RequestBackup, "Request additional unit be dispatched.")
                                        },
                                        new Menu()
                                        {
                                            parent = Options.BetterChases.Crimes.PoliceMurder,
                                            name = "WantedLevel",
                                            menuItem = new NativeUI.UIMenuListItem("Set Wanted Level", WantedLevelOptions, WantedLevelOptions.IndexOf(Options.BetterChases.Crimes.PoliceMurder.WantedLevel), "Set wanted level to this if currently lower, 0 to disable.")
                                        }
                                    }
                                }
                            }
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "CopsManageTraffic",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Cops Manage Traffic", Config.Options.BetterChases.CopsManageTraffic, "If enabled, cops will try to avoid crashing into vehicles, pedestrians and other cops. They will also refrain from ramming you if there are people nearby.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "WreckedCopsStopChasing",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Wrecked Cops Give Up", Config.Options.BetterChases.WreckedCopsStopChasing, "If enabled, cops driving badly damaged vehicles will give up on pursuit.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "DisallowCopCommandeering",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Cops Won't Commandeer", Config.Options.BetterChases.DisallowCopCommandeering, "If enabled, cops will not commandeer civilian vehicles.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "RequirePITAuthorization",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Require PIT Authorization", Config.Options.BetterChases.RequirePITAuthorization, "If enabled, prevents cops from performing PITs/Ramming until they are allowed.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "RequireLethalForceAuthorization",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Require Lethal-Force Authorization", Config.Options.BetterChases.RequireLethalForceAuthorization, "If enabled, prevents cops from using lethal weapons until they are allowed.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "AllowBustOpportunity",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Allow Extra Bust Opportunity", Config.Options.BetterChases.AllowBustOpportunity, "If enabled, you can optionally give up when above 1 star by pressing ~y~" + Config.Options.SurrenderKey.ToString() + "~w~ or ~y~" + Config.Options.SurrenderButton.ToString() + "~w~.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "ShowHUD",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display PIT & Lethal Force HUD", Config.Options.BetterChases.ShowHUD, "If enabled, the icons representing PIT & Lethal Force authorization will display near the Wanted Level stars.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "ShowNotifications",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Notifications", Config.Options.BetterChases.ShowNotifications, "Toggles the notification system that keeps you informed of any changes in the police behavior.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "ShowBigMessages",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Big Messages", Config.Options.BetterChases.ShowBigMessages, "If enabled, the game will display messages similar to the online shards when something important happens.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "IconOffsetX",
                            menuItem = new NativeUI.UIMenuListItem("Icon Offset X", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.BetterChases.IconOffsetX), "Controls the horizontal UI offset of the icons")
                        },
                        new Menu()
                        {
                            parent = Config.Options.BetterChases,
                            name = "IconOffsetY",
                            menuItem = new NativeUI.UIMenuListItem("Icon Offset Y", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.BetterChases.IconOffsetY), "Controls the vertical UI offset of the icons")
                        }
                    }
                },
                new Menu()
                {
                    menu = new NativeUI.UIMenu("Arrest Warrants", "Configure Arrest Warrants"),
                    menuItem = new NativeUI.UIMenuItem("Arrest Warrants", "Configure Arrest Warrants"),
                    subMenus = new List<Menu>()
                    {
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "Enabled",
                            toggle = true,
                            menuItem = new NativeUI.UIMenuCheckboxItem("Module Enabled", Config.Options.ArrestWarrants.Enabled, "Toggles the functionality of the entire module. If disabled, all features below will be disabled also.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "SpotSpeed",
                            menuItem = new NativeUI.UIMenuListItem("Overall Spot Speed", Config.SpotSpeedOptions, Config.SpotSpeedOptions.IndexOf(Config.Options.ArrestWarrants.SpotSpeed), "How fast or slow police will spot you, this can be seen via the Spotted Meter. The default is 100%.")
                        },
                        new Menu()
                        {
                            menu = new NativeUI.UIMenu("Warrant Length", "Warrant Length Settings"),
                            menuItem = new NativeUI.UIMenuItem("Warrant Length", "Warrant Length Settings"),
                            subMenus = new List<Menu>()
                            {
                                new Menu()
                                {
                                    parent = Config.Options.ArrestWarrants.WarrantLenghts,
                                    name = "OneStar",
                                    menuItem = new NativeUI.UIMenuListItem("One Star Warrant Length", Config.WarrantLengthOptions, Config.WarrantLengthOptions.IndexOf(Config.Options.ArrestWarrants.WarrantLenghts.OneStar), "How many in-game hours a one star wanted level warrant expires.")
                                },
                                new Menu()
                                {
                                    parent = Config.Options.ArrestWarrants.WarrantLenghts,
                                    name = "TwoStar",
                                    menuItem = new NativeUI.UIMenuListItem("Two Star Warrant Length", Config.WarrantLengthOptions, Config.WarrantLengthOptions.IndexOf(Config.Options.ArrestWarrants.WarrantLenghts.TwoStar), "How many in-game hours a two star wanted level warrant expires. ~y~Note~w~: In addition to previous times.")
                                },
                                new Menu()
                                {
                                    parent = Config.Options.ArrestWarrants.WarrantLenghts,
                                    name = "ThreeStar",
                                    menuItem = new NativeUI.UIMenuListItem("Three Star Warrant Length", Config.WarrantLengthOptions, Config.WarrantLengthOptions.IndexOf(Config.Options.ArrestWarrants.WarrantLenghts.ThreeStar), "How many in-game hours a three star wanted level warrant expires. ~y~Note~w~: In addition to previous times.")
                                },
                                new Menu()
                                {
                                    parent = Config.Options.ArrestWarrants.WarrantLenghts,
                                    name = "FourStar",
                                    menuItem = new NativeUI.UIMenuListItem("Four Star Warrant Length", Config.WarrantLengthOptions, Config.WarrantLengthOptions.IndexOf(Config.Options.ArrestWarrants.WarrantLenghts.FourStar), "How many in-game hours a four star wanted level warrant expires. ~y~Note~w~: In addition to previous times.")
                                },
                                new Menu()
                                {
                                    parent = Config.Options.ArrestWarrants.WarrantLenghts,
                                    name = "FiveStar",
                                    menuItem = new NativeUI.UIMenuListItem("Five Star Warrant Length", Config.WarrantLengthOptions, Config.WarrantLengthOptions.IndexOf(Config.Options.ArrestWarrants.WarrantLenghts.FiveStar), "How many in-game hours a five star wanted level warrant expires. ~y~Note~w~: In addition to previous times.")
                                }
                            }
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "RememberChase",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Remember Last Chase", Config.Options.ArrestWarrants.RememberChase, "If enabled, you will resume the chase you escaped last time if you were identified. If disabled you will start instead with a 2 star level.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "ShowSpottedMeter",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Spotted Meter", Config.Options.ArrestWarrants.ShowSpottedMeter, "If enabled, the HUD showing how close nearby police are to spotting you will display near the bottom right corner of the screen.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "ShowSpottedIndicators",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Spotted Indicators", Config.Options.ArrestWarrants.ShowSpottedIndicators, "If enabled, color coded indicators showing police interest in you will display above the police.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "ShowHUD",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Arrest Warrants HUD", Config.Options.ArrestWarrants.ShowHUD, "If enabled, the current warrant status HUD will display near the bottom right corner of the screen.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "ShowNotifications",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Notifications", Config.Options.ArrestWarrants.ShowNotifications, "Toggles the notification system that keeps you informed of any changes in arrest warrants.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "ShowBigMessages",
                            menuItem = new NativeUI.UIMenuCheckboxItem("Display Big Messages", Config.Options.ArrestWarrants.ShowBigMessages, "If enabled, the game will display messages similar to the online shards when something important happens.")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "IconOffsetX",
                            menuItem = new NativeUI.UIMenuListItem("Icon Offset X", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.IconOffsetX), "Controls the horizontal UI offset of the icons")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "IconOffsetY",
                            menuItem = new NativeUI.UIMenuListItem("Icon Offset Y", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.IconOffsetY), "Controls the vertical UI offset of the icons")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "TextOffsetX",
                            menuItem = new NativeUI.UIMenuListItem("Text Offset X", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.TextOffsetX), "Controls the horizontal UI offset of the text")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "TextOffsetY",
                            menuItem = new NativeUI.UIMenuListItem("Text Offset Y", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.TextOffsetY), "Controls the vertical UI offset of the text")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "GradientOffsetX",
                            menuItem = new NativeUI.UIMenuListItem("Gradient Offset X", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.GradientOffsetX), "Controls the horizontal UI offset of the gradient")
                        },
                        new Menu()
                        {
                            parent = Config.Options.ArrestWarrants,
                            name = "GradientOffsetY",
                            menuItem = new NativeUI.UIMenuListItem("Gradient Offset Y", Config.OffsetOptions, Config.OffsetOptions.IndexOf(Config.Options.ArrestWarrants.GradientOffsetY), "Controls the vertical UI offset of the gradient")
                        }
                    }
                },
                new Menu()
                {
                    parent = Config.Options,
                    name = "DisplayHints",
                    menuItem = new NativeUI.UIMenuCheckboxItem("Display Hints", Config.Options.DisplayHints, "If enabled, the game will display some hints to help you understand how Better Chases and Arrest Warrants work.")
                },
                new Menu()
                {
                    menu = new NativeUI.UIMenu("Debug", "Mod commands & debugging"),
                    menuItem = new NativeUI.UIMenuItem("Debug", "Mod commands & debugging"),
                    subMenus = new List<Menu>()
                    {
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Issue Character Warrant"),
                            command = ArrestWarrants.IssuePlayerWarrant
                        },
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Clear All Warrants"),
                            command = ArrestWarrants.ClearWarrants
                        },
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Save Warrants"),
                            command = ArrestWarrants.SaveWarrants
                        },
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Load Warrants"),
                            command = ArrestWarrants.LoadWarrants
                        },
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Save mod settings to file"),
                            command = Config.Save
                        },
                        new Menu()
                        {
                            menuItem = new NativeUI.UIMenuItem("Load mod settings from file"),
                            command = Config.Load
                        }
                    }
                }
            };
        }
    }
}