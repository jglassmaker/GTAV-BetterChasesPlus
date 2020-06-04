using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BetterChasesPlus
{
    public class ArrestWarrants : Script
    {
        public static bool CopSearch;
        public static bool ShowSpottedMeter;
        public static double SpottedMeter;
        public static float VehicleMatch;
        public static float PedMatch;
        public static bool IsWarrantSearchActive;
        public static Warrant ActiveWarrant = new Warrant();
        public static Warrant PedWarrant = new Warrant();
        public static Warrant VehicleWarrant = new Warrant();
        public static List<Warrant> Warrants = new List<Warrant>();

        private bool IsWanted;
        private bool WasWanted;
        private int DescriptionCheckTime;
        private Ped ActiveCharacter = Game.Player.Character;
        private readonly int WarrantCheckFrequency = 59750;

        public class Warrant
        {
            public int WantedLevel = 0;
            public int WarrantCheckTime = 0;
            public DateTime WarrantIssueTime = new DateTime();
            public DateTime ChaseStartTime = new DateTime();
            public int VehicleCrashes = 0;
            public bool DeadlyForce = false;
            public bool PITAuthorized = false;
            public bool RecklessDriving = false;
            public bool ExcessiveSpeeding = false;
            public bool HitandRun = false;
            public int KilledPeds = 0;
            public int KilledCops = 0;
            public Model ped;
            public Model vehicle;
            public Vector3 LastKnownLocation;
            public VehicleDescription VehicleDescription = new VehicleDescription();
            public PedDescription PedDescription = new PedDescription();
        }

        public class VehicleDescription
        {
            public string plate;
            public VehicleColor primaryColor;
            public VehicleColor secondaryColor;
        }

        public class PedDescription
        {
            public int beardStyle;
            public int beardColor;
            public int hairStyle;
            public int hairColor;
            public int shirtStyle;
            public int shirtColor;
            public int pantStyle;
            public int pantColor;
            public int handStyle;
            public int handColor;
            public int footStyle;
            public int footColor;
            public int accessoryStyle;
            public int accessoryColor;
            public int accessory2Style;
            public int accessory2Color;
            public int hatExtraStyle;
            public int hatExtraColor;
            public int faceExtraStyle;
            public int faceExtraColor;
            public int earExtraStyle;
            public int earExtraColor;
        }

        public class VehicleColors
        {
            public VehicleColor primary;
            public VehicleColor secondary;
        }

        public ArrestWarrants()
        {
            Tick += OnTick;

            Interval = 250;
        }

        private void OnTick(object sender, EventArgs e)
        {
            //System.Diagnostics.Stopwatch Stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Stopwatch.Start();

            if (Options.ArrestWarrants.Enabled == false)
                return;

            int wantedLevel = Game.Player.WantedLevel;
            Ped character = Game.Player.Character;
            IsWanted = Game.Player.WantedLevel > 0 ? true : false;

            // Arrested -- Clear Warrant
            if (!IsWanted && (ActiveWarrant.ped.IsValid || ActiveWarrant.vehicle.IsValid) && Function.Call<int>(Hash.GET_TIME_SINCE_LAST_ARREST) >= 0 && Function.Call<int>(Hash.GET_TIME_SINCE_LAST_ARREST) < 10000)
            {
                ClearWarrant(ActiveWarrant);
            }

            // Died -- Clear Warrant
            if (!IsWanted && (ActiveWarrant.ped.IsValid || ActiveWarrant.vehicle.IsValid) && Function.Call<int>(Hash.GET_TIME_SINCE_LAST_DEATH) >= 0 && Function.Call<int>(Hash.GET_TIME_SINCE_LAST_DEATH) < 10000)
            {
                ClearWarrant(ActiveWarrant);
            }

            // Changed from not Wanted to Wanted
            if (IsWanted && !WasWanted)
            {
                // Clear recognition stuff
                SpottedMeter = 0;
                ShowSpottedMeter = false;
                Renderer.Markers.Clear();
            }
            // Changed from Wanted to not Wanted
            else if (!IsWanted && WasWanted)
            {
                // Issue Warrant
                if (ActiveWarrant.WantedLevel > 0 && (ActiveWarrant.ped.IsValid || ActiveWarrant.vehicle.IsValid))
                {
                    IssueWarrant(ActiveWarrant);
                }
            }
            else
            {
                // Record Highest Wanted Level for Warrant
                if (wantedLevel > ActiveWarrant.WantedLevel && (ActiveWarrant.ped.IsValid || ActiveWarrant.vehicle.IsValid))
                {
                    ActiveWarrant.WantedLevel = wantedLevel;
                }

                // Reference Ped & Vehicle warrants
                PedWarrant = ActiveWarrant.ped.IsValid ? ActiveWarrant : new Warrant();
                VehicleWarrant = ActiveWarrant.vehicle.IsValid ? ActiveWarrant : new Warrant();
                if (Warrants.Count > 0)
                {
                    foreach (Warrant warrant in Warrants)
                    {
                        if (warrant.ped.IsValid && warrant.ped.Hash == character.Model.Hash)
                        {
                            PedWarrant = warrant;
                        }

                        if (warrant.vehicle.IsValid && Helpers.IsValid(character.CurrentVehicle) && warrant.vehicle.Hash == character.CurrentVehicle.Model.Hash)
                        {
                            VehicleWarrant = warrant;
                        }
                    }
                }

                // Clear Warrant (Warrant time has expired)
                if (PedWarrant.WantedLevel > 0 && !IsWanted && PedWarrant.ped.IsValid && World.CurrentDate.CompareTo(WarrantExpireTime(PedWarrant)) == 1)
                {
                    ClearWarrant(PedWarrant);
                }
                else if (VehicleWarrant.WantedLevel > 0 && !IsWanted && VehicleWarrant.vehicle.IsValid && World.CurrentDate.CompareTo(WarrantExpireTime(VehicleWarrant)) == 1)
                {
                    ClearWarrant(VehicleWarrant);
                }

                // Give a UI update on Arrest Warrant
                if (Options.ArrestWarrants.ShowNotifications)
                {
                    if (PedWarrant.WantedLevel > 0 && !IsWanted && PedWarrant.ped.IsValid && PedWarrant.WarrantCheckTime + WarrantCheckFrequency < Game.GameTime)
                    {
                        CheckWarrant(PedWarrant);
                    }
                    else if (VehicleWarrant.WantedLevel > 0 && !IsWanted && VehicleWarrant.vehicle.IsValid && VehicleWarrant.WarrantCheckTime + WarrantCheckFrequency < Game.GameTime)
                    {
                        CheckWarrant(VehicleWarrant);
                    }
                    // When character changed show warrants
                    else if (PedWarrant.WantedLevel > 0 && !IsWanted && PedWarrant.ped.IsValid && character.Model.Hash != ActiveCharacter.Model.Hash)
                    {
                        CheckWarrant(PedWarrant);
                    }
                    else if (VehicleWarrant.WantedLevel > 0 && !IsWanted && VehicleWarrant.vehicle.IsValid && character.Model.Hash != ActiveCharacter.Model.Hash)
                    {
                        CheckWarrant(VehicleWarrant);
                    }
                }
            }

            // Identify Character
            if (IsWanted && !ActiveWarrant.ped.IsValid)
            {
                CopSearch = true;

                if (Witnesses.GetMaxRecognition(Witnesses.Cops) >= 100)
                {
                    CopSearch = false;

                    IdentifyPlayer();
                }
            }

            // Identify Vehicle
            if (IsWanted && Helpers.IsValid(character.CurrentVehicle) && (!ActiveWarrant.vehicle.IsValid || character.CurrentVehicle.Model.Hash != ActiveWarrant.vehicle.Hash))
            {
                CopSearch = true;

                if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 100)
                {
                    CopSearch = false;

                    IdentifyVehicle();
                }
            }

            // Warrent search
            if (!IsWanted && (PedWarrant.ped.IsValid || (VehicleWarrant.vehicle.IsValid && Helpers.IsValid(character.CurrentVehicle) && VehicleWarrant.vehicle.Hash == character.CurrentVehicle.Model.Hash)))
            {
                CopSearch = true;
                ShowSpottedMeter = false;
                IsWarrantSearchActive = true;

                double maxSpotted = 0;
                bool pedSpotted = false;
                Renderer.Markers.Clear();

                // Check Warrant Description
                if (Game.GameTime > DescriptionCheckTime + 10000)
                {
                    CompareWarrantDescription(VehicleWarrant, PedWarrant);
                    DescriptionCheckTime = Game.GameTime;
                }

                foreach (Witnesses.Witness cop in Witnesses.Cops)
                {
                    double spotted = 0;

                    if (PedWarrant.ped.IsValid && (VehicleWarrant.vehicle.IsValid && Helpers.IsValid(character.CurrentVehicle) && VehicleWarrant.vehicle.Hash == character.CurrentVehicle.Model.Hash))
                    {
                        spotted = Math.Max(cop.Spotted, cop.VehicleSpotted);
                        if (cop.Spotted > cop.VehicleSpotted)
                        {
                            pedSpotted = true;
                        }
                    }
                    else if (VehicleWarrant.vehicle.IsValid && Helpers.IsValid(character.CurrentVehicle) && VehicleWarrant.vehicle.Hash == character.CurrentVehicle.Model.Hash)
                    {
                        spotted = cop.VehicleSpotted;
                    }
                    else if (PedWarrant.ped.IsValid)
                    {
                        spotted = cop.Spotted;
                        pedSpotted = true;
                    }

                    if (cop.Recognition > 0 || cop.VehicleRecognition > 0)
                    {
                        ShowSpottedMeter = true;
                    }

                    if (spotted > 0)
                    {
                        // Witness tasks
                        // TODO add LOS via LastKnownLocation?
                        // TODO add tasks for when player in car
                        // TODO add tasks for when cop is in car (heli?)
                        if (cop.Ped.IsOnFoot && character.IsOnFoot)
                        {
                            if (cop.Spotted >= 30 && cop.distance < 30f)
                            {
                                cop.Ped.Task.GoTo(character);
                                cop.LastKnownLocation = character.Position;
                            }
                            else if (cop.Spotted >= 20 && cop.distance < 30f)
                            {
                                cop.Ped.Task.TurnTo(character);
                            }
                            else if (cop.Spotted >= 10 && cop.distance < 30f)
                            {
                                cop.Ped.Task.LookAt(character);
                            }
                            else if (cop.LastKnownLocation != null)
                            {
                                //TaskSequence tasks = new TaskSequence();
                                //tasks.AddTask.GoTo(cop.LastKnownLocation);
                                //tasks.AddTask.WanderAround(cop.LastKnownLocation, 25f);
                                //cop.Ped.Task.PerformSequence(tasks);
                                //cop.Ped.Task.WanderAround(cop.LastKnownLocation, 25f);
                                //Function.Call(Hash.TASK_WANDER_STANDARD, cop.Ped, 10f, 10);
                                //UI.Notify("Wander");
                                Function.Call(Hash.TASK_WANDER_IN_AREA, cop.Ped, cop.LastKnownLocation.X, cop.LastKnownLocation.Y, cop.LastKnownLocation.Z, 20f, 45f, 5f); // (param6) seconds to wander? (param7) seconds before change direction
                                cop.LastKnownLocation = new Vector3(); // wander gets called alot???
                            }
                            else if (cop.Spotted < 5)
                            {
                                cop.Ped.Task.ClearAll(); // not sure if this is needed but probably
                            }
                        }

                        Renderer.UIMarker Marker = new Renderer.UIMarker { Type = MarkerType.ChevronUpx1, Entity = cop.Ped };
                        if (spotted > 0 && spotted <= 5)
                        {
                            Marker.Color = Color.Green;
                        }
                        else if (spotted > 5 && spotted < 20)
                        {
                            Marker.Color = Color.Cyan;
                        }
                        else if (spotted >= 20 && spotted < 50)
                        {
                            Marker.Color = Color.Yellow;
                        }
                        else if (spotted >= 50 && spotted < 70)
                        {
                            Marker.Color = Color.Orange;
                        }
                        else if (spotted >= 70)
                        {
                            Marker.Color = Color.Red;
                        }

                        if (spotted > maxSpotted) maxSpotted = spotted;

                        if (Options.ArrestWarrants.ShowSpottedMeter) Renderer.Markers.Add(Marker);
                    }
                }

                SpottedMeter = maxSpotted;

                // Restore the chase
                if (SpottedMeter >= 100)
                {
                    CopSearch = false;

                    if (pedSpotted)
                    {
                        ResumeWarrent(PedWarrant, pedSpotted);
                    }
                    else
                    {
                        ResumeWarrent(VehicleWarrant, pedSpotted);
                    }
                }
            }
            else
            {
                IsWarrantSearchActive = false;

                // Clear recognition stuff
                SpottedMeter = 0;
                ShowSpottedMeter = false;
                Renderer.Markers.Clear();
            }

            // Stolen vehicle (needs to be at the bottom)
            if (Options.ArrestWarrants.EnableStolenVehicles && !IsWanted && Helpers.IsValid(character.CurrentVehicle) && character.CurrentVehicle.IsStolen && (!VehicleWarrant.vehicle.IsValid || VehicleWarrant.vehicle.Hash != character.CurrentVehicle.Model.Hash))
            {
                CopSearch = true;

                if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= Options.ArrestWarrants.StolenRecognitionThreshold)
                {
                    CopSearch = false;
                    Helpers.WantedLevel = 1;
                    ActiveWarrant.vehicle = character.CurrentVehicle.Model;

                    if (Options.ArrestWarrants.ShowNotifications)
                    {
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Model: ~y~" + character.CurrentVehicle.LocalizedName + "~w~~n~Color: ~y~" + SimplifyColorString(GetVehicleColors(character.CurrentVehicle).primary.ToString()) + "~w~~n~Plate: ~y~" + Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, character.CurrentVehicle));
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "STOLEN VEHICLE LOCATED", "~c~LSPD");
                        
                    }

                    if (Options.DisplayHints)
                    {
                        Renderer.ShowHelpMessage("The ~b~LSPD~w~ has identified your vehicle as stolen.");
                    }

                    if (Options.ArrestWarrants.ShowBigMessages)
                    {
                        Renderer.ShowBigMessage("STOLEN VEHICLE", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                    }

                    VehicleWarrant = ActiveWarrant;
                }
            }

            // Update last known position
            if (IsWanted && (Witnesses.GetMaxRecognition(Witnesses.Cops, true) > 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) > 0))
            {
                ActiveWarrant.LastKnownLocation = character.Position;
            }

            WasWanted = IsWanted;
            ActiveCharacter = character;
        }

        public static void IdentifyVehicle(bool showPlayerMessages = true)
        {
            Ped character = Game.Player.Character;

            // Look for any other warrants
            if (Warrants.Count > 0)
            {
                foreach (Warrant warrant in Warrants)
                {
                    if (warrant.vehicle.IsValid && warrant.vehicle.Hash == character.CurrentVehicle.Model.Hash)
                    {
                        // Merge warrants
                        ActiveWarrant.WantedLevel = ActiveWarrant.WantedLevel > warrant.WantedLevel ? ActiveWarrant.WantedLevel : warrant.WantedLevel;
                        ActiveWarrant.VehicleCrashes = ActiveWarrant.VehicleCrashes > warrant.VehicleCrashes ? ActiveWarrant.VehicleCrashes : warrant.VehicleCrashes;
                        ActiveWarrant.DeadlyForce = ActiveWarrant.DeadlyForce ? ActiveWarrant.DeadlyForce : warrant.DeadlyForce;
                        ActiveWarrant.PITAuthorized = ActiveWarrant.PITAuthorized ? ActiveWarrant.PITAuthorized : warrant.PITAuthorized;
                        ActiveWarrant.ped = ActiveWarrant.ped.IsValid ? ActiveWarrant.ped : warrant.ped;
                        ActiveWarrant.vehicle = warrant.vehicle;
                        Warrants.Remove(warrant);
                        Helpers.WantedLevel = ActiveWarrant.WantedLevel;
                        break;
                    }
                }
            }

            if (ActiveWarrant.vehicle.IsValid && character.CurrentVehicle.Model.Hash == ActiveWarrant.vehicle.Hash)
            {
                if (Options.ArrestWarrants.ShowNotifications && showPlayerMessages)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Model: ~y~" + character.CurrentVehicle.LocalizedName + "~w~~n~Color: ~y~" + SimplifyColorString(GetVehicleColors(character.CurrentVehicle).primary.ToString()) + "~w~~n~Plate: ~y~" + Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, character.CurrentVehicle));
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "VEHICLE IDENTIFIED", "~c~LSPD");
                }

                if (Options.DisplayHints && showPlayerMessages)
                {
                    Renderer.ShowHelpMessage("The ~b~LSPD~w~ has identified your vehicle as belonging to a previous suspect they were looking for.");
                    Renderer.ShowHelpMessage("Previous arrest warrant has been combined with the current one.");
                }
            }
            else
            {
                if (Options.ArrestWarrants.ShowNotifications && showPlayerMessages)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Model: ~y~" + character.CurrentVehicle.LocalizedName + "~w~~n~Color: ~y~" + SimplifyColorString(GetVehicleColors(character.CurrentVehicle).primary.ToString()) + "~w~~n~Plate: ~y~" + Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, character.CurrentVehicle));
                    if (ActiveWarrant.vehicle.IsValid)
                    {
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "NEW VEHICLE IDENTIFIED", "~c~LSPD");

                    }
                    else
                    {
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "VEHICLE IDENTIFIED", "~c~LSPD");
                    }
                }

                if (Options.DisplayHints && showPlayerMessages)
                {
                    Renderer.ShowHelpMessage("The ~b~LSPD~w~ has identified your vehicle. Cops will recognise it if you escape.");
                    Renderer.ShowHelpMessage("You can avoid this by acquiring a new vehicle while out of the cops' sight.");
                }

                ActiveWarrant.vehicle = character.CurrentVehicle.Model;
            }

            if (Options.ArrestWarrants.ShowBigMessages && showPlayerMessages)
            {
                Renderer.ShowBigMessage("VEHICLE IDENTIFIED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
            }

            // Vehicle Description
            ActiveWarrant.VehicleDescription.plate = Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, character.CurrentVehicle);
            ActiveWarrant.VehicleDescription.primaryColor = GetVehicleColors(character.CurrentVehicle).primary;
            ActiveWarrant.VehicleDescription.secondaryColor = GetVehicleColors(character.CurrentVehicle).secondary;

            VehicleWarrant = ActiveWarrant;
        }

        public static void IdentifyPlayer(bool showPlayerMessages = true)
        {
            Ped character = Game.Player.Character;

            // Look for any other warrants
            if (Warrants.Count > 0)
            {
                foreach (Warrant warrant in Warrants)
                {
                    if (warrant.ped.IsValid && warrant.ped.Hash == character.Model.Hash)
                    {
                        // Merge warrants
                        ActiveWarrant.WantedLevel = ActiveWarrant.WantedLevel > warrant.WantedLevel ? ActiveWarrant.WantedLevel : warrant.WantedLevel;
                        ActiveWarrant.VehicleCrashes = ActiveWarrant.VehicleCrashes > warrant.VehicleCrashes ? ActiveWarrant.VehicleCrashes : warrant.VehicleCrashes;
                        ActiveWarrant.DeadlyForce = ActiveWarrant.DeadlyForce ? ActiveWarrant.DeadlyForce : warrant.DeadlyForce;
                        ActiveWarrant.PITAuthorized = ActiveWarrant.PITAuthorized ? ActiveWarrant.PITAuthorized : warrant.PITAuthorized;
                        ActiveWarrant.vehicle = ActiveWarrant.vehicle.IsValid ? ActiveWarrant.vehicle : warrant.vehicle;
                        ActiveWarrant.ped = warrant.ped;
                        Warrants.Remove(warrant);
                        Helpers.WantedLevel = ActiveWarrant.WantedLevel;
                        break;
                    }
                }
            }

            if (ActiveWarrant.ped.IsValid)
            {
                if (Options.ArrestWarrants.ShowNotifications && showPlayerMessages)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Fleeing suspect has been identified as a previous suspect ~y~" + ((PedHash)ActiveWarrant.ped.Hash).ToString() + "~w~.");
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "SUSPECT IDENTIFIED", "~c~LSPD");
                }

                if (Options.DisplayHints && showPlayerMessages)
                {
                    Renderer.ShowHelpMessage(((PedHash)ActiveWarrant.ped.Hash).ToString() + " has been identified by the ~b~LSPD~w~ as a previous suspect they were looking for.");
                    Renderer.ShowHelpMessage("Previous arrest warrant has been combined with the current one.");
                }
            }
            else
            {
                ActiveWarrant.ped = character.Model;

                if (Options.ArrestWarrants.ShowNotifications && showPlayerMessages)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Fleeing suspect has been identified as ~y~" + ((PedHash)ActiveWarrant.ped.Hash).ToString() + "~w~.");
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "SUSPECT IDENTIFIED", "~c~LSPD");
                }

                if (Options.DisplayHints && showPlayerMessages)
                {
                    Renderer.ShowHelpMessage(((PedHash)ActiveWarrant.ped.Hash).ToString() + " has been identified by the ~b~LSPD~w~. Cops will be able to recognise you if you escape.");
                    Renderer.ShowHelpMessage("Change clothes while out of sight to avoid being recognised after escaping.");
                }
            }

            if (Options.ArrestWarrants.ShowBigMessages && showPlayerMessages)
            {
                Renderer.ShowBigMessage("IDENTIFIED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
            }

            // Ped Description
            ActiveWarrant.PedDescription.beardStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 1);
            ActiveWarrant.PedDescription.beardColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 1);
            ActiveWarrant.PedDescription.hairStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 2);
            ActiveWarrant.PedDescription.hairColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 2);
            ActiveWarrant.PedDescription.shirtStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 3);
            ActiveWarrant.PedDescription.shirtColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 3);
            ActiveWarrant.PedDescription.pantStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 4);
            ActiveWarrant.PedDescription.pantColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 4);
            ActiveWarrant.PedDescription.handStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 5);
            ActiveWarrant.PedDescription.handColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 5);
            ActiveWarrant.PedDescription.footStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 6);
            ActiveWarrant.PedDescription.footColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 6);
            ActiveWarrant.PedDescription.accessoryStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 8);
            ActiveWarrant.PedDescription.accessoryColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 8);
            ActiveWarrant.PedDescription.accessory2Style = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 9);
            ActiveWarrant.PedDescription.accessory2Color = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 9);
            ActiveWarrant.PedDescription.hatExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 0);
            ActiveWarrant.PedDescription.hatExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 0);
            ActiveWarrant.PedDescription.faceExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 1);
            ActiveWarrant.PedDescription.faceExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 1);
            ActiveWarrant.PedDescription.earExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 2);
            ActiveWarrant.PedDescription.earExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 2);

            PedWarrant = ActiveWarrant;
        }

        public static void ClearWarrant(Warrant warrant)
        {
            ActiveWarrant = new Warrant();
            Warrants.Remove(warrant);

            if (Options.ArrestWarrants.ShowNotifications)
            {
                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");

                if (warrant.ped.IsValid)
                {
                    if (warrant.vehicle.IsValid)
                    {
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant on ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~ and on the ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~ has been lifted.");
                    }
                    else
                    {
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant on ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~ has been lifted.");
                    }
                }
                else if (warrant.vehicle.IsValid)
                {
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant on the ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~ has been lifted.");
                }

                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "ARREST WARRANT LIFTED", "~c~LSPD");


                if (Options.DisplayHints)
                {
                    if (warrant.ped.IsValid)
                    {
                        Renderer.ShowHelpMessage(((PedHash)warrant.ped.Hash).ToString() + "'s Arrest Warrant has expired. You are now free to roam around without worries.");
                        Renderer.ShowHelpMessage("~g~The police have forgotten about you~w~.");
                    }
                    else
                    {
                        Renderer.ShowHelpMessage("The " + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "'s Arrest Warrant has expired. You are now free to roam around without worries.");
                        Renderer.ShowHelpMessage("~g~The police have forgotten about your vehicle~w~.");
                    }
                }
            }

            if (Options.ArrestWarrants.ShowBigMessages)
            {
                Renderer.ShowBigMessage("WARRANT LIFTED", "", Renderer.HudColor.BLUE, Renderer.HudColor.BLACK, 3000);
            }
        }

        public static void ClearWarrants()
        {
            ActiveWarrant = new Warrant();
            Warrants.Clear();
        }

        public static void IssuePlayerWarrant()
        {
            Ped character = Game.Player.Character;
            Warrant warrant = new Warrant();

            warrant.WantedLevel = 1;
            warrant.ped = character.Model;

            // Ped Description
            ActiveWarrant.PedDescription.beardStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 1);
            ActiveWarrant.PedDescription.beardColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 1);
            ActiveWarrant.PedDescription.hairStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 2);
            ActiveWarrant.PedDescription.hairColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 2);
            ActiveWarrant.PedDescription.shirtStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 3);
            ActiveWarrant.PedDescription.shirtColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 3);
            ActiveWarrant.PedDescription.pantStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 4);
            ActiveWarrant.PedDescription.pantColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 4);
            ActiveWarrant.PedDescription.handStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 5);
            ActiveWarrant.PedDescription.handColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 5);
            ActiveWarrant.PedDescription.footStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 6);
            ActiveWarrant.PedDescription.footColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 6);
            ActiveWarrant.PedDescription.accessoryStyle = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 8);
            ActiveWarrant.PedDescription.accessoryColor = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 8);
            ActiveWarrant.PedDescription.accessory2Style = Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 9);
            ActiveWarrant.PedDescription.accessory2Color = Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 9);
            ActiveWarrant.PedDescription.hatExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 0);
            ActiveWarrant.PedDescription.hatExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 0);
            ActiveWarrant.PedDescription.faceExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 1);
            ActiveWarrant.PedDescription.faceExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 1);
            ActiveWarrant.PedDescription.earExtraStyle = Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 2);
            ActiveWarrant.PedDescription.earExtraColor = Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 2);

            IssueWarrant(warrant);
        }

        public static Warrant IssueWarrant(Warrant warrant)
        {
            if (warrant.WantedLevel <= 0 || (!warrant.ped.IsValid && !warrant.vehicle.IsValid)) return warrant;

            warrant.WarrantIssueTime = World.CurrentDate;
            warrant.WarrantCheckTime = Game.GameTime;

            if (Options.ArrestWarrants.ShowNotifications)
            {
                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");

                if (warrant.ped.IsValid)
                {
                    if (warrant.vehicle.IsValid)
                    {
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Name: ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~~n~Model: ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~~n~Expiration: ~y~" + WarrantExpireFormatted(warrant) + "~w~");
                    }
                    else
                    {
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Name: ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~~n~Last Seen: ~y~" + World.GetStreetName(warrant.LastKnownLocation) + "~w~~n~Expiration: ~y~" + WarrantExpireFormatted(warrant) + "~w~");
                    }
                }
                else if (warrant.vehicle.IsValid)
                {
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Model: ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~~n~Last Seen: ~y~" + World.GetStreetName(warrant.LastKnownLocation) + "~w~~n~Expiration: ~y~" + WarrantExpireFormatted(warrant) + "~w~");
                }

                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "ARREST WARRANT ISSUED", "~c~LSPD");
            }

            if (Options.DisplayHints)
            {
                Renderer.ShowHelpMessage("You have escaped, but you're not safe yet.");
                if (warrant.ped.IsValid)
                {
                    Renderer.ShowHelpMessage("The Arrest Warrant has ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~'s appareance description.");
                    Renderer.ShowHelpMessage("You can change clothes to prevent the cops from being able to recognise you.");
                    if (warrant.vehicle.IsValid)
                    {
                        Renderer.ShowHelpMessage("The Arrest Warrant has your vehicle's description too.");
                        if (Helpers.IsValid(Game.Player.Character.CurrentVehicle) && Game.Player.Character.CurrentVehicle.Model.Hash == warrant.vehicle.Hash)
                        {
                            Renderer.ShowHelpMessage("It is best that you get rid of the vehicle, or modify it.");
                        }
                        else
                        {
                            Renderer.ShowHelpMessage("However, you are in a different vehicle, so you don't have to worry about the vehicle warrant.");
                        }
                    }
                    Renderer.ShowHelpMessage("You can also switch to another Character and wait for ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~'s Arrest Warrant to expire.");
                }
                else if (warrant.vehicle.IsValid)
                {
                    Renderer.ShowHelpMessage("~y~" + ((PedHash)Game.Player.Character.Model.Hash).ToString() + "~w~ hasn't been identfied, but ~y~" + ((PedHash)Game.Player.Character.Model.Hash).ToString() + "~w~'s vehicle has.");
                    if (Helpers.IsValid(Game.Player.Character.CurrentVehicle) && Game.Player.Character.CurrentVehicle.Model.Hash == warrant.vehicle.Hash)
                    {
                        Renderer.ShowHelpMessage("It is best that you get rid of the vehicle, or modify it.");
                    }
                    else
                    {
                        Renderer.ShowHelpMessage("However, you are in a different vehicle, so you don't have to worry about the vehicle warrant.");
                    }
                }
                Renderer.ShowHelpMessage("Cop's will be on full alert for a while, but they will eventually lose interest.");
                Renderer.ShowHelpMessage("The closer the Arrest Warrant is to expire, the less likely cops will recognise you.");
            }

            if (Options.ArrestWarrants.ShowBigMessages)
            {
                Renderer.ShowBigMessage("ESCAPED", "", Renderer.HudColor.BLUE, Renderer.HudColor.BLACK, 3000);
            }

            Warrants.Add(warrant);
            ActiveWarrant = new Warrant();
            return warrant;
        }

        private void CheckWarrant(Warrant warrant)
        {
            warrant.WarrantCheckTime = Game.GameTime;

            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
            if (warrant.ped.IsValid)
            {
                if (warrant.vehicle.IsValid)
                {
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant for ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~ and the ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~ will expire in ~y~" + WarrantExpireFormatted(warrant));
                }
                else
                {
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant for ~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~ will expire in ~y~" + WarrantExpireFormatted(warrant));
                }
            }
            else if (warrant.vehicle.IsValid)
            {
                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "The Arrest Warrant for the ~y~" + Function.Call<string>(Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, warrant.vehicle.Hash) + "~w~ will expire in ~y~" + WarrantExpireFormatted(warrant));
            }
            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "WARRANT EXPIRATION", "~c~LSPD");
        }

        private void ResumeWarrent(Warrant warrant, bool WasPedSpotted)
        {
            ActiveWarrant = warrant;

            // Set Wanted Level to Warrant Wanted Level
            if (Options.ArrestWarrants.RememberStarsWhenRecognised && warrant.WantedLevel > Game.Player.WantedLevel)
            {
                Helpers.WantedLevel = warrant.WantedLevel;
            }
            else
            {
                Helpers.WantedLevel = 2;
            }

            // Restore Better Chases
            BetterChases.DeadlyForce = warrant.DeadlyForce;
            BetterChases.PITAuthorized = warrant.PITAuthorized;
            BetterChases.VehicleCrashes = warrant.VehicleCrashes;
            BetterChases.RecklessDriving = warrant.RecklessDriving;
            BetterChases.ExcessiveSpeeding = warrant.ExcessiveSpeeding;
            BetterChases.HitandRun = warrant.HitandRun;
            BetterChases.KilledPeds = warrant.KilledPeds;
            BetterChases.KilledCops = warrant.KilledCops;
            BetterChases.ChaseStartTime = warrant.ChaseStartTime;

            if (WasPedSpotted)
            {
                if (Options.ArrestWarrants.ShowNotifications)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "~y~" + ((PedHash)warrant.ped.Hash).ToString() + "~w~ has been spotted on ~y~" + World.GetStreetName(Game.Player.Character.Position) + "~w.~");
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "WANTED PERSON SPOTTED", "~c~LSPD");
                }

                if (Options.DisplayHints)
                {
                    Renderer.ShowHelpMessage("You have been ~r~recognised~w~ by a cop. The chase will resume.");
                }

                if (Options.ArrestWarrants.ShowBigMessages)
                {
                    Renderer.ShowBigMessage("SPOTTED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                }
            }
            else
            {
                if (Options.ArrestWarrants.ShowNotifications)
                {
                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "A wanted vehicle has been spotted.");
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "WANTED VEHICLE SPOTTED", "~c~LSPD");
                }

                if (Options.DisplayHints)
                {
                    Renderer.ShowHelpMessage("Your vehicle has been ~r~recognised~w~ by a cop. The chase will resume and the Arrest Warrant has been updated.");
                }

                if (Options.ArrestWarrants.ShowBigMessages)
                {
                    Renderer.ShowBigMessage("VEHICLE SPOTTED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                }
            }
        }

        private static DateTime WarrantExpireTime(Warrant warrant)
        {
            int length = 0;

            switch (warrant.WantedLevel)
            {
                case 1:
                    {
                        length = Options.ArrestWarrants.OneStarWarrantTime;
                        break;
                    }
                case 2:
                    {
                        length = Options.ArrestWarrants.OneStarWarrantTime + Options.ArrestWarrants.TwoStarWarrantTime;
                        break;
                    }
                case 3:
                    {
                        length = Options.ArrestWarrants.OneStarWarrantTime + Options.ArrestWarrants.TwoStarWarrantTime + Options.ArrestWarrants.ThreeStarWarrantTime;
                        break;
                    }
                case 4:
                    {
                        length = Options.ArrestWarrants.OneStarWarrantTime + Options.ArrestWarrants.TwoStarWarrantTime + Options.ArrestWarrants.ThreeStarWarrantTime + Options.ArrestWarrants.FourStarWarrantTime;
                        break;
                    }
                case 5:
                    {
                        length = Options.ArrestWarrants.OneStarWarrantTime + Options.ArrestWarrants.TwoStarWarrantTime + Options.ArrestWarrants.ThreeStarWarrantTime + Options.ArrestWarrants.FourStarWarrantTime + Options.ArrestWarrants.FiveStarWarrantTime;
                        break;
                    }
            }

            return warrant.WarrantIssueTime.AddHours(length);
        }

        private static string WarrantExpireFormatted(Warrant warrant)
        {
            string output = "";
            TimeSpan diff = WarrantExpireTime(warrant).Subtract(World.CurrentDate);

            if (diff.Days > 0)
            {
                if (diff.Days > 1)
                {
                    output += diff.Days + " days ";
                }
                else
                {
                    output += diff.Days + " day ";
                }
            }

            if (diff.Hours > 0)
            {
                if (diff.Hours > 1)
                {
                    output += diff.Hours + " hours ";
                }
                else
                {
                    output += diff.Hours + " hour ";
                }
            }

            if (diff.Minutes > 0)
            {
                if (diff.Minutes > 1)
                {
                    output += diff.Minutes + " minutes";
                }
                else
                {
                    output += diff.Minutes + " minute";
                }
            }

            return output;
        }

        private void CompareWarrantDescription(Warrant vehicleWarrant, Warrant pedWarrant)
        {
            Vehicle vehicle = Game.Player.Character.CurrentVehicle;
            if (vehicleWarrant.vehicle.IsValid && Helpers.IsValid(vehicle) && vehicle.Model.Hash == vehicleWarrant.vehicle.Hash)
            {
                float match = 0f;
                //if (vehicle.PrimaryColor == vehicleWarrant.VehicleDescription.primaryColor)
                //{
                //    // if no secondary color - then primary color is even more important
                //    if (vehicle.SecondaryColor == VehicleColor.MetallicBlack)
                //    {
                //        match += 0.75f;
                //    }
                //    else
                //    {
                //        match += 0.5f;
                //    }
                //}

                //if (vehicle.SecondaryColor != VehicleColor.MetallicBlack && vehicle.SecondaryColor == vehicleWarrant.VehicleDescription.secondaryColor)
                //{
                //    match += 0.25f;
                //}

                if (Function.Call<string>(Hash.GET_VEHICLE_NUMBER_PLATE_TEXT, vehicle) == vehicleWarrant.VehicleDescription.plate)
                {
                    match += 0.25f;
                }

                VehicleMatch = match;
            }
            else
            {
                VehicleMatch = 0;
            }

            if (pedWarrant.ped.IsValid)
            {
                float match = 0f;
                Ped character = Game.Player.Character;

                if (Helpers.IsMasked(character) || Helpers.IsWearingHelmet(character))
                {
                    if (pedWarrant.PedDescription.hatExtraStyle == Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 0))
                    {
                        match += 0.20f;
                    }

                    if (pedWarrant.PedDescription.hatExtraColor == Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 0))
                    {
                        match += 0.15f;
                    }
                }
                else
                {
                    if (pedWarrant.PedDescription.beardStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 1))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.beardColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 1))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.hairStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 2))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.hairColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 2))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.hatExtraStyle == Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 0))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.hatExtraColor == Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 0))
                    {
                        match += 0.05f;
                    }

                    if (pedWarrant.PedDescription.faceExtraStyle == Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 1))
                    {
                        match += 0.0125f;
                    }

                    if (pedWarrant.PedDescription.faceExtraColor == Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 1))
                    {
                        match += 0.0125f;
                    }

                    if (pedWarrant.PedDescription.earExtraStyle == Function.Call<int>(Hash.GET_PED_PROP_INDEX, character, 2))
                    {
                        match += 0.0125f;
                    }

                    if (pedWarrant.PedDescription.earExtraColor == Function.Call<int>(Hash.GET_PED_PROP_TEXTURE_INDEX, character, 2))
                    {
                        match += 0.0125f;
                    }
                }

                if (pedWarrant.PedDescription.shirtStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 3))
                {
                    match += 0.1f;
                }

                if (pedWarrant.PedDescription.shirtColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 3))
                {
                    match += 0.1f;
                }

                if (pedWarrant.PedDescription.pantStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 4))
                {
                    match += 0.1f;
                }

                if (pedWarrant.PedDescription.pantColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 4))
                {
                    match += 0.1f;
                }

                if (pedWarrant.PedDescription.handStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 5))
                {
                    match += 0.0125f;
                }

                if (pedWarrant.PedDescription.handColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 5))
                {
                    match += 0.0125f;
                }

                if (pedWarrant.PedDescription.footStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 6))
                {
                    match += 0.0125f;
                }

                if (pedWarrant.PedDescription.footColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 6))
                {
                    match += 0.0125f;
                }

                if (pedWarrant.PedDescription.accessoryStyle == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 8))
                {
                    match += 0.05f;
                }

                if (pedWarrant.PedDescription.accessoryColor == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 8))
                {
                    match += 0.05f;
                }

                if (pedWarrant.PedDescription.accessory2Style == Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, character, 9))
                {
                    match += 0.05f;
                }

                if (pedWarrant.PedDescription.accessory2Color == Function.Call<int>(Hash.GET_PED_TEXTURE_VARIATION, character, 9))
                {
                    match += 0.05f;
                }

                PedMatch = match;
            }
            else
            {
                PedMatch = 0;
            }
        }

        public static string SimplifyColorString(string color)
        {
            string[] words = { "Straw", "Anthracite", "Graphite", "Pueblo", "Golden", "Metallic", "Matte", "Pure", "PoliceCar", "Util", "Worn", "Dark", "Light", "Taxi", "Desert", "Foliage", "Hot", "Hunter", "Midnight", "Marine", "Formula", "Frost", "Garnet", "Epsilon", "Moss", "Olive", "Util", "Ultra", "Salmon", "Gasoline" };
            foreach (string word in words)
            {
                color = color.Replace(word, string.Empty);
            }
            return color;
        }

        public static VehicleColors GetVehicleColors(Vehicle vehicle)
        {
            VehicleColors colors = new VehicleColors();
            OutputArgument color1 = new OutputArgument();
            OutputArgument color2 = new OutputArgument();

            Function.Call(Hash.GET_VEHICLE_COLOURS, vehicle, color1, color2);
            colors.primary = color1.GetResult<GTA.VehicleColor>();
            colors.secondary = color2.GetResult<GTA.VehicleColor>();

            return colors;
        }
    }
}