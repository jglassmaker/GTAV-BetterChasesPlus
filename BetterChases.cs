using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BetterChasesPlus
{
    public class BetterChases
    {
        public static bool CopSearch;
        public static bool DeadlyForce;
        public static bool PITAuthorized;
        public static int VehicleCrashes;
        public static bool RecklessDriving;
        public static bool ExcessiveSpeeding;
        public static bool HitandRun;
        public static int KilledPeds;
        public static int KilledCops;
        public static DateTime ChaseStartTime;

        public static List<Ped> Cops = new List<Ped>();
        public static List<Vehicle> CopVehicles = new List<Vehicle>();

        public class BetterChasesPassive : Script
        {
            private static bool IsWanted;
            private static bool WasWanted;
            private static int BustStars;
            private static bool AreHandsUp;
            private static bool IsGettingBusted;
            private static List<Ped> PedsKilled = new List<Ped>();

            public BetterChasesPassive()
            {
                Tick += OnTick;
                Interval = 250;
            }

            private void OnTick(object sender, EventArgs e)
            {
                if (Options.BetterChases.Enabled == false)
                    return;

                Ped character = Game.Player.Character;

                IsWanted = Game.Player.WantedLevel > 0 || Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player) ? true : false;

                // Reset for next chase
                if (WasWanted && !IsWanted)
                {
                    DeadlyForce = false;
                    PITAuthorized = false;
                    VehicleCrashes = 0;
                    RecklessDriving = false;
                    ExcessiveSpeeding = false;
                    HitandRun = false;
                    KilledPeds = 0;
                    KilledCops = 0;
                    ChaseStartTime = new DateTime();
                }
                else if (IsWanted && !WasWanted)
                {
                    if (ChaseStartTime == new DateTime())
                    {
                        ChaseStartTime = World.CurrentDate;
                        ArrestWarrants.ActiveWarrant.ChaseStartTime = World.CurrentDate;
                    }
                }

                if (IsWanted)
                {
                    CopSearch = true;
                }
                else
                {
                    CopSearch = false;
                }

                //System.Diagnostics.Stopwatch Stopwatch = System.Diagnostics.Stopwatch.StartNew();
                //Stopwatch.Start();

                //Vector3 pos = Player.Character.Position;
                //float radius = 100f;
                //bool FoundCop = Function.Call<bool>(Hash.IS_COP_PED_IN_AREA_3D, pos.X + radius, pos.Y + radius, pos.Z + radius, pos.X - radius, pos.Y - radius, pos.Z - radius);
                //bool FoundCop = Function.Call<bool>(Hash.IS_COP_VEHICLE_IN_AREA_3D, pos.X + radius, pos.Y + radius, pos.Z + radius, pos.X - radius, pos.Y - radius, pos.Z - radius);
                //Vehicle[] vehicles = World.GetAllVehicles();


                // Gather all cops & cop vehicles
                Ped[] peds = World.GetAllPeds();

                Cops.Clear();
                CopVehicles.Clear();
                foreach (Ped ped in peds)
                {
                    //ped.HasBeenDamagedBy
                    //ped.IsInCombatAgainst
                    //ped.GetMeleeTarget
                    //Game.Player.GetTargetedEntity


                    if (Helpers.IsValid(ped) && ped.IsAlive && Helpers.PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped)))
                    {
                        Cops.Add(ped);

                        // Don't allow commandeering
                        if (Options.BetterChases.DisallowCopCommandeering)
                        {
                            if (Function.Call<int>(Hash.GET_PED_TYPE, ped) == 6)
                            {
                                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 10, false); // BF_CanCommandeerVehicles in combatbehaviour.meta
                            }
                            else if (Function.Call<int>(Hash.GET_PED_TYPE, ped) == 27)
                            {
                                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 12, false); // BF_CanCommandeerVehicles in combatbehaviour.meta
                            }
                        }
                        
                        if (Helpers.IsValid(ped.CurrentVehicle) && Helpers.IsValid(ped.CurrentVehicle.Driver) && ped.CurrentVehicle.Driver.Handle == ped.Handle)
                        {
                            CopVehicles.Add(ped.CurrentVehicle);

                            // driving style
                            //Function.Call(Hash.SET_DRIVER_ABILITY, ped.CurrentVehicle.Driver, 100.0f);
                            //Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, ped.CurrentVehicle.Driver, -100.0f);
                            //ped.CurrentVehicle.Driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                        }
                    }
                }
                //Stopwatch.Stop();
                //UI.ShowSubtitle("CPU: " + Stopwatch.Elapsed);
                //UI.ShowSubtitle("" + peds.Length + " ~b~" + BetterChases.Cops.Count + " ~w~" + vehicles.Length + " ~b~" + BetterChases.CopVehicles.Count + "~w~ CPU: " + Stopwatch.Elapsed);

                if (IsWanted)
                {
                    foreach (Ped cop in Cops)
                    {
                        // Lethal Force
                        if (Options.BetterChases.RequireLethalForceAuthorization && cop.IsInCombatAgainst(character))
                        {
                            if (!DeadlyForce)
                            {
                                if (cop.Weapons.Current.Hash != WeaponHash.StunGun)
                                {
                                    cop.Weapons.Give(WeaponHash.StunGun, 1000, true, true);
                                    cop.CanSwitchWeapons = false;
                                    //Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, Cop, 0);
                                    //Function.Call(Hash.SET_PED_COMBAT_RANGE, Cop, 0);
                                    //RendererScript.DisplayHelpTextThisFrame("COP GIVEN SPECIAL GUN");
                                }
                                else
                                {
                                    // Don't shoot player when in a vehicle (doesnt work)
                                    //if (Helpers.IsValid(character.CurrentVehicle) && cop.Weapons.Current.Hash == WeaponHash.StunGun)
                                    //{
                                    //    //function.call(hash.set_combat_float, cop, 0, 0);
                                    //    //cop.Weapons.Current.Ammo = 0;
                                    //    //cop.Weapons.Current.AmmoInClip = 0;
                                    //    //cop.Weapons.Give(WeaponHash.StunGun, -1, true, false);
                                    //    //UI.ShowSubtitle("No shoot");
                                    //} else
                                    //{
                                    //    //cop.Weapons.Current.Ammo = 1000;
                                    //}
                                }
                            }
                            else
                            {
                                if (cop.Weapons.Current.Hash == WeaponHash.StunGun)
                                {
                                    //Cop.ShootRate = 500;
                                    cop.CanSwitchWeapons = true;
                                    cop.Weapons.Select(cop.Weapons.BestWeapon.Hash, true);
                                    //Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, Cop, 2);
                                    //Function.Call(Hash.SET_PED_COMBAT_RANGE, Cop, 1);
                                }
                            }
                        }

                        //if (Helpers.IsValid(Player.Character.CurrentVehicle) && Cop.Weapons.Current.Hash == WeaponHash.StunGun)
                        //{
                        //    Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, Cop, 1424, false);
                        //    //Cop.Task.Wait(400);
                        //    //Cop.Task.LookAt(Player.Character, 400);
                        //    //Cop.ShootRate = -1;
                        //    //Cop.Weapons.Current.Ammo = -1;
                        //    //Cop.Weapons.Current.AmmoInClip = -1;
                        //    //Cop.Weapons.RemoveAll();
                        //    //RendererScript.DisplayHelpTextThisFrame("SHOOT 0");
                        //} else if (Cop.Weapons.Current.Hash == WeaponHash.StunGun)
                        //{
                        //    //Cop.Weapons.Current.Ammo = 9000;
                        //    //Cop.ShootRate = 500;
                        //}
                    }
                }

                // Crashed cars/motorcycles stop chasing
                if (Options.BetterChases.WreckedCopsStopChasing && IsWanted)
                {
                    foreach (Vehicle vehicle in BetterChases.CopVehicles)
                    {
                        if (Helpers.IsValid(vehicle) && vehicle.EngineHealth > 0 && (vehicle.Model.IsCar || vehicle.Model.IsBike || vehicle.Model.IsBoat) && (vehicle.BodyHealth < 600f || vehicle.EngineHealth < 500f || Helpers.IsAnyTireBlown(vehicle)))
                        {
                            vehicle.EngineHealth = -4000;
                        }
                    }
                }

                // Allow extra bust opportunities
                if (Options.BetterChases.AllowBustOpportunity && IsWanted && character.IsOnFoot && !character.IsSwimming && !character.IsFalling && Game.Player.WantedLevel < 5)
                {
                    // Hands up
                    if (IsGettingBusted && !character.IsBeingStunned && !character.IsRagdoll && !character.IsProne && (Game.IsKeyPressed(Keys.E) || Game.IsControlPressed(GTA.Control.Cover)))
                    {
                        if (!AreHandsUp)
                        {
                            AreHandsUp = true;
                            character.Task.PlayAnimation("missminuteman_1ig_2", "handsup_enter", 8.0f, 8.0f, -1, (AnimationFlags)2, 0f);
                        }

                        if (AreHandsUp)
                        {
                            // Force player out of stealth stance
                            Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, character, false);
                            // Force player out of "action" stance/mode (combat mode) -- doesn't work when cops are after you, game forces action mode
                            //Function.Call(Hash.SET_PED_USING_ACTION_MODE, Game.Player.Character, false, -1, "DEFAULT_ACTION");
                        }
                    }
                    else if (AreHandsUp)
                    {
                        AreHandsUp = false;
                        character.Task.ClearAnimation("missminuteman_1ig_2", "handsup_enter");
                    }

                    // Bust Oportunity
                    if (!IsGettingBusted && (character.IsBeingStunned || character.IsRagdoll || character.IsProne || Game.IsKeyPressed(Keys.E) || Game.IsControlPressed(GTA.Control.Cover)) && character.Velocity.Length() <= 4f && !character.IsJumping && !character.IsRunning && !character.IsInCover && !Game.Player.IsAiming && Helpers.IsCopNearby(character.Position, 20f))
                    {
                        IsGettingBusted = true;
                        BustStars = Game.Player.WantedLevel;
                        Helpers.WantedLevel = 1;
                        Helpers.MaxWantedLevel = 1;
                    }
                    else if (IsGettingBusted && !AreHandsUp && !character.IsBeingStunned && !character.IsRagdoll && !character.IsProne)
                    {
                        IsGettingBusted = false;
                        Helpers.MaxWantedLevel = 5;
                        Helpers.WantedLevel = BustStars;
                    }
                }

                // 4+ stars
                if (((Options.BetterChases.RequireLethalForceAuthorization && !DeadlyForce) || (Options.BetterChases.RequirePITAuthorization && !PITAuthorized)) && IsWanted && Game.Player.WantedLevel >= 4)
                {
                    DeadlyForce = true;
                    PITAuthorized = true;
                    ArrestWarrants.ActiveWarrant.DeadlyForce = DeadlyForce;
                    ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;

                    if (Options.BetterChases.ShowNotifications)
                    {
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is dangerous, ~y~Tactical Vehicle Interventions~w~ and use of ~r~deadly force~w~ are now authorized.");
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "DANGEROUS", "~c~LSPD");
                    }

                    if (Options.DisplayHints)
                    {
                        Renderer.ShowHelpMessage("You have reached 4 stars and considered dangerous, ~y~PIT~w~ and ~r~deadly force~w~ have been authorized.");
                        Renderer.ShowHelpMessage("Cops will now ~r~shoot you~w~.");
                        Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                        Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                    }

                    if (Options.BetterChases.ShowBigMessages)
                    {
                        Renderer.ShowBigMessage("PIT & DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                    }
                }

                // Reckless Driving + Crashing + Excessive Speeding
                if (Options.BetterChases.RequirePITAuthorization && IsWanted && RecklessDriving && VehicleCrashes > 4 && ExcessiveSpeeding && !PITAuthorized)
                {
                    PITAuthorized = true;
                    ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;

                    if (Options.BetterChases.ShowNotifications)
                    {
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is driving dangerously, ~y~Tactical Vehicle Interventions~w~ are now authorized.");
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "DRIVING DANGEROUSLY", "~c~LSPD");
                    }

                    if (Options.DisplayHints)
                    {
                        Renderer.ShowHelpMessage("You have been crashing into cars, driving recklessly, and speeding excessively. ~y~PIT~w~ has been authorized.");
                        Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                        Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                    }

                    if (Options.BetterChases.ShowBigMessages)
                    {
                        Renderer.ShowBigMessage("PIT AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                    }
                }

                // Reckless Driving -- Driving against traffic OR Driving on sidewalks
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && !RecklessDriving && Game.Player.WantedLevel < 3 && Helpers.IsValid(character.CurrentVehicle) && character.CurrentVehicle.Speed > 14 && ((Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_AGAINST_TRAFFIC, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_AGAINST_TRAFFIC, Game.Player) < 1000) || (Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_ON_PAVEMENT, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_ON_PAVEMENT, Game.Player) < 1000)))
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 50)
                    {
                        RecklessDriving = true;
                        Helpers.WantedLevel = Game.Player.WantedLevel + 1;
                        ArrestWarrants.ActiveWarrant.RecklessDriving = true;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is driving recklessly, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "RECKLESS DRIVING", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You have been driving on sidewalks or the wrong way. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                }

                // Crashing into cars
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && VehicleCrashes < 4 && Game.Player.WantedLevel < 3 && Helpers.IsValid(character.CurrentVehicle) && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_VEHICLE, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_VEHICLE, Game.Player) < 500)
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 50)
                    {
                        VehicleCrashes++;
                        ArrestWarrants.ActiveWarrant.VehicleCrashes = VehicleCrashes;

                        if (VehicleCrashes > 4)
                        {
                            Helpers.WantedLevel = Game.Player.WantedLevel + 1;

                            if (Options.BetterChases.ShowNotifications)
                            {
                                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is reckless, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "RECKLESS DRIVING", "~c~LSPD");
                            }

                            if (Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("You have crashed into too many vehicles. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                            }
                        }
                    }
                }

                // Excessive Speeding (90 MPH)
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && !ExcessiveSpeeding && Game.Player.WantedLevel < 3 && Helpers.IsValid(character.CurrentVehicle) && character.CurrentVehicle.Speed > 40)
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) > 0)
                    {
                        ExcessiveSpeeding = true;
                        Helpers.WantedLevel = Game.Player.WantedLevel + 1;
                        ArrestWarrants.ActiveWarrant.ExcessiveSpeeding = true;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            string speedText;
                            if (Function.Call<bool>(Hash.SHOULD_USE_METRIC_MEASUREMENTS)) // Check game settings - using metric system
                            {
                                speedText = Math.Round((character.CurrentVehicle.Speed * 3.6), 0) + "KPH";
                            }
                            else
                            {
                                speedText = Math.Round((character.CurrentVehicle.Speed * 2.237), 0) + "MPH";
                            }
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is speeding excessively at over ~y~" + speedText + "~w~, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "EXCESSIVE SPEEDING", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You have been speeding excessively. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                }

                // Hit & Run
                if (IsWanted && !HitandRun && Game.Player.WantedLevel <= 3 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_PED, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_PED, Game.Player) < 1000)
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 50)
                    {
                        HitandRun = true;
                        ArrestWarrants.ActiveWarrant.HitandRun = true;

                        if (Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3)
                        {
                            Helpers.WantedLevel = 3;
                        }

                        if (Options.BetterChases.RequirePITAuthorization && !PITAuthorized)
                        {
                            PITAuthorized = true;
                            ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;
                        }

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect involved in a hit & run, ~y~Tactical Vehicle Interventions~w~ are now authorized.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "HIT & RUN", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You have run over a pedestrian, ~y~PIT~w~ has been authorized.");
                            Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                            Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                        }

                        if (Options.BetterChases.ShowBigMessages)
                        {
                            Renderer.ShowBigMessage("PIT AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                        }
                    }
                }

                // Killed Ped
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && Game.Player.WantedLevel < 5)
                {
                    bool killedCop = false;
                    bool killedPed = false;
                    //Ped[] nearbyPeds = World.GetNearbyPeds(character, 40f);
                    foreach (Ped ped in peds)
                    {
                        if (!ped.IsAlive && !PedsKilled.Contains(ped))
                        {
                            if ((Helpers.IsValid(ped.Killer) && ped.Killer.Handle == character.Handle) || (Helpers.IsValid(ped.Killer) &&  Helpers.IsValid(character.CurrentVehicle) && ped.Killer.Handle == character.CurrentVehicle.Handle))
                            {
                                if (Helpers.PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped)))
                                {
                                    killedCop = true;
                                    PedsKilled.Add(ped);
                                }
                                else
                                {
                                    killedPed = true;
                                    PedsKilled.Add(ped);
                                }
                            }
                        }
                    }

                    if (killedCop || killedPed)
                    {
                        if (Witnesses.GetMaxRecognition(Witnesses.Cops) >= 20)
                        {
                            if (killedPed)
                            {
                                KilledPeds += 1;
                                ArrestWarrants.ActiveWarrant.KilledPeds = KilledPeds;

                                if (KilledPeds < 4 && Game.Player.WantedLevel < 4)
                                {
                                    if (Options.BetterChases.ShowNotifications && DeadlyForce)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed a civilian, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED CIVILIAN", "~c~LSPD");
                                    }
                                    else if (Options.BetterChases.ShowNotifications)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed civilian, use of ~r~deadly force~w~ is now authorized.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED CIVILIAN", "~c~LSPD");
                                    }

                                    if (Options.DisplayHints)
                                    {
                                        Renderer.ShowHelpMessage("You have killed a civilian, ~r~deadly force~w~ has been authorized.");
                                    }

                                    if (Options.BetterChases.ShowBigMessages)
                                    {
                                        Renderer.ShowBigMessage("DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                                    }

                                    Helpers.WantedLevel = 4;
                                    DeadlyForce = true;
                                    PITAuthorized = true;
                                    ArrestWarrants.ActiveWarrant.DeadlyForce = true;
                                    ArrestWarrants.ActiveWarrant.PITAuthorized = true;
                                }
                                else if (Game.Player.WantedLevel < 5)
                                {
                                    Helpers.WantedLevel = 5;

                                    if (Options.BetterChases.ShowNotifications)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed multiple civilians, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED CIVILIANS", "~c~LSPD");
                                    }

                                    if (Options.DisplayHints)
                                    {
                                        Renderer.ShowHelpMessage("You have killed multiple civilians, The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                                    }
                                }
                            }
                            else if (killedCop)
                            {
                                KilledCops += 1;
                                ArrestWarrants.ActiveWarrant.KilledCops = KilledCops;

                                if (KilledCops < 3 && Game.Player.WantedLevel < 4)
                                {
                                    if (Options.BetterChases.ShowNotifications && DeadlyForce)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed a police officer, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED POLICE", "~c~LSPD");
                                    }
                                    else if (Options.BetterChases.ShowNotifications)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed police officer, use of ~r~deadly force~w~ is now authorized.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED POLICE", "~c~LSPD");
                                    }

                                    if (Options.DisplayHints)
                                    {
                                        Renderer.ShowHelpMessage("You have killed a police officer, ~r~deadly force~w~ has been authorized.");
                                    }

                                    if (Options.BetterChases.ShowBigMessages)
                                    {
                                        Renderer.ShowBigMessage("DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                                    }

                                    Helpers.WantedLevel = 4;
                                    DeadlyForce = true;
                                    PITAuthorized = true;
                                    ArrestWarrants.ActiveWarrant.DeadlyForce = true;
                                    ArrestWarrants.ActiveWarrant.PITAuthorized = true;
                                }
                                else if (Game.Player.WantedLevel < 5)
                                {
                                    Helpers.WantedLevel = 5;

                                    if (Options.BetterChases.ShowNotifications)
                                    {
                                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect killed multiple police officers, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "KILLED POLICE", "~c~LSPD");
                                    }

                                    if (Options.DisplayHints)
                                    {
                                        Renderer.ShowHelpMessage("You have killed multiple police officers, The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                                    }
                                }
                            }
                        }
                    }
                }

                // Grand Theft Auto
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && Game.Player.WantedLevel == 1 && (character.IsJacking || character.IsTryingToEnterALockedVehicle))
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops) >= 20)
                    {
                        Helpers.WantedLevel = 2;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is stealing a vehicle, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "GRAND THEFT AUTO", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You were seen stealing a vehicle. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                }

                // Carrying Weapon
                if (Options.BetterChases.WantedLevelControl != "None" && IsWanted && Game.Player.WantedLevel == 1 && !IsGettingBusted && character.IsOnFoot && Helpers.IsArmed)
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops) >= 20)
                    {
                        Helpers.WantedLevel = 2;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is armed, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "ARMED", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("A cop has spotted you carrying a weapon. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                }

                // Assaulting police -- lethal force
                if (Options.BetterChases.RequireLethalForceAuthorization && IsWanted && !DeadlyForce)
                {
                    foreach (Ped cop in Cops)
                    {
                        if (cop.HasBeenDamagedBy(character) || (Helpers.IsValid(character.CurrentVehicle) && cop.HasBeenDamagedBy(character.CurrentVehicle))) {
                            DeadlyForce = true;
                            PITAuthorized = true;
                            ArrestWarrants.ActiveWarrant.DeadlyForce = true;
                            ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;

                            if (Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3)
                            {
                                Helpers.WantedLevel = 3;
                            }

                            if (Options.BetterChases.ShowNotifications)
                            {
                                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect has assaulted police, use of ~r~deadly force~w~ is now authorized.");
                                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "DEADLY THREAT", "~c~LSPD");
                            }

                            if (Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("You have harmed a cop, ~r~deadly force~w~ has been authorized.");
                                Renderer.ShowHelpMessage("Cops will now ~r~shoot you~w~.");
                            }

                            if (Options.BetterChases.ShowBigMessages)
                            {
                                Renderer.ShowBigMessage("DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                            }
                        }
                    }
                }

                // Aiming at police -- lethal force
                if (Options.BetterChases.RequireLethalForceAuthorization && IsWanted && !DeadlyForce && Game.Player.IsAiming && Helpers.IsArmed && Helpers.IsValid(Game.Player.TargetedEntity))
                {
                    Entity target = Game.Player.TargetedEntity;
                    if (Helpers.IsValid(target) && Witnesses.GetMaxRecognition(Witnesses.Cops) >= 0)
                    {
                        if (Cops.Contains(target) || peds.Contains(target))
                        {
                            DeadlyForce = true;
                            PITAuthorized = true;
                            ArrestWarrants.ActiveWarrant.DeadlyForce = true;
                            ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;

                            if (Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3)
                            {
                                Helpers.WantedLevel = 3;
                            }

                            if (Options.BetterChases.ShowNotifications)
                            {
                                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect aimed a weapon at someone, use of ~r~deadly force~w~ is now authorized.");
                                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "DEADLY THREAT", "~c~LSPD");
                            }

                            if (Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("You have aimed your weapon at someone, ~r~deadly force~w~ has been authorized.");
                                Renderer.ShowHelpMessage("Cops will now ~r~shoot you~w~.");
                            }

                            if (Options.BetterChases.ShowBigMessages)
                            {
                                Renderer.ShowBigMessage("DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                            }
                        }
                    }
                }

                // Aiming Weapon
                if (IsWanted && Game.Player.IsAiming && Helpers.IsArmed && ((Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3) || (Options.BetterChases.LethalForceOnAim && !DeadlyForce)))
                {
                    if (Witnesses.GetMaxRecognition(Witnesses.Cops) >= 20)
                    {
                        Helpers.WantedLevel = 3;

                        if (Options.BetterChases.LethalForceOnAim && !DeadlyForce)
                        {
                            DeadlyForce = true;
                            PITAuthorized = true;
                            ArrestWarrants.ActiveWarrant.DeadlyForce = true;
                            ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;
                        }

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is aiming a weapon, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "RAISED WEAPON", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You have aimed a weapon. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                }

                // Want level rises over time
                if (Options.BetterChases.EnableChaseTimes && Options.BetterChases.WantedLevelControl != "None" && IsWanted && !IsGettingBusted)
                {
                    if (Game.Player.WantedLevel == 1 && World.CurrentDate > ChaseStartTime.AddMinutes(Options.BetterChases.OneStarChaseTime))
                    {
                        Helpers.WantedLevel = 2;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is resisting arrest, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "FLEEING", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("The chase has gone on too long. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                    else if (Game.Player.WantedLevel == 2 && World.CurrentDate > ChaseStartTime.AddMinutes(Options.BetterChases.OneStarChaseTime + Options.BetterChases.TwoStarChaseTime))
                    {
                        Helpers.WantedLevel = 3;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is continuing to resisting arrest, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "FLEEING", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("The chase has gone on too long. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                        }
                    }
                    else if (Game.Player.WantedLevel == 3 && Options.BetterChases.RequirePITAuthorization && World.CurrentDate > ChaseStartTime.AddMinutes(Options.BetterChases.OneStarChaseTime + Options.BetterChases.TwoStarChaseTime + Options.BetterChases.ThreeStarChaseTime) && !PITAuthorized && Helpers.IsValid(character.CurrentVehicle))
                    {
                        PITAuthorized = true;
                        ArrestWarrants.ActiveWarrant.PITAuthorized = true;

                        if (Options.BetterChases.ShowNotifications)
                        {
                            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect shows no sign of stopping, ~y~PIT~w~ has been authorized.");
                            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "FLEEING", "~c~LSPD");
                        }

                        if (Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("You have continued to elude the police with no sign of stopping, ~y~PIT~w~ has been authorized.");
                            Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                            Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                        }

                        if (Options.BetterChases.ShowBigMessages)
                        {
                            Renderer.ShowBigMessage("PIT AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                        }
                    }
                }

                WasWanted = IsWanted;
            }
        }

        public class BetterChasesActive : Script
        {
            private static bool IsWanted;
            private static bool WasWanted;
            private static int MaxCopCars;
            private static int MaxCopHelis;
            private static List<Vehicle> ChaseCopCars = new List<Vehicle>();
            private static List<Vehicle> ChaseCopHelis = new List<Vehicle>();

            public BetterChasesActive()
            {
                Tick += OnTick;

                Interval = 1;
            }

            private void OnTick(object sender, EventArgs e)
            {
                if (Options.BetterChases.Enabled == false)
                    return;

                Ped character = Game.Player.Character;

                IsWanted = Game.Player.WantedLevel > 0 || Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player) ? true : false;

                // Manage wanted level
                if (Options.BetterChases.WantedLevelControl == "Full")
                {
                    // Changed from not Wanted to Wanted
                    if (IsWanted && !WasWanted)
                    {
                        Helpers.WantedLevel = Game.Player.WantedLevel;
                        Helpers.MaxWantedLevel = Game.Player.WantedLevel;

                        ChaseCopCars.Clear();
                        ChaseCopHelis.Clear();
                    }
                    // Changed from Wanted to not Wanted
                    else if (!IsWanted && WasWanted)
                    {
                        Helpers.WantedLevel = 0;
                        Helpers.MaxWantedLevel = 5;
                    }
                    else if (IsWanted && Function.Call<int>(Hash.GET_MAX_WANTED_LEVEL) != Helpers.MaxWantedLevel)
                    {
                        Helpers.MaxWantedLevel = Helpers.MaxWantedLevel;
                    }
                    else if (IsWanted && Game.Player.WantedLevel != Helpers.WantedLevel)
                    {
                        Helpers.WantedLevel = Helpers.WantedLevel;
                    }
                }

                // Manage Cop Vehicles
                if (Options.BetterChases.EnableCopVehicleControl && IsWanted && !Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player))
                {
                    switch (Game.Player.WantedLevel)
                    {
                        case 1:
                            MaxCopCars = Options.BetterChases.OneStarCopCars;
                            MaxCopHelis = Options.BetterChases.OneStarCopHelis;
                            break;
                        case 2:
                            MaxCopCars = Options.BetterChases.TwoStarCopCars;
                            MaxCopHelis = Options.BetterChases.TwoStarCopHelis;
                            break;
                        case 3:
                            MaxCopCars = Options.BetterChases.ThreeStarCopCars;
                            MaxCopHelis = Options.BetterChases.ThreeStarCopHelis;
                            break;
                        case 4:
                            MaxCopCars = Options.BetterChases.FourStarCopCars;
                            MaxCopHelis = Options.BetterChases.FourStarCopHelis;
                            break;
                        case 5:
                            MaxCopCars = Options.BetterChases.FiveStarCopCars;
                            MaxCopHelis = Options.BetterChases.FiveStarCopHelis;
                            break;
                    }

                    if (MaxCopCars != -1 && MaxCopHelis != -1)
                    {
                        for (int i = CopVehicles.Count - 1; i >= 0; i--)
                        {
                            Vehicle vehicle = CopVehicles[i];

                            if (vehicle.Model.IsCar || vehicle.Model.IsBike || vehicle.Model.IsBoat)
                            {
                                // if the cop isn't in the chase - remove it from the list and just ignore it
                                if (vehicle.IsDead || !vehicle.IsDriveable || vehicle.IsUpsideDown || !vehicle.Exists())
                                {
                                    if (ChaseCopCars.Contains(vehicle))
                                    {
                                        ChaseCopCars.Remove(vehicle);
                                    }
                                }
                                // if the cop was in the chase but is too far away, remote it from the list and give another cop a chance
                                else if (ChaseCopCars.Contains(vehicle) && !vehicle.IsInRange(character.Position, 130f))
                                {
                                    ChaseCopCars.Remove(vehicle);
                                }
                                // remove extra cop cars the game is trying to add to the chase
                                else if (ChaseCopCars.Count >= MaxCopCars && !ChaseCopCars.Contains(vehicle) && !vehicle.IsInRange(character.Position, 120f))
                                {
                                    for (int x = vehicle.PassengerCount - 1; x >= 0; x--)
                                    {
                                        vehicle.Passengers[x].Delete();
                                    }

                                    ChaseCopCars.Remove(vehicle);
                                    vehicle.Delete();
                                }
                                else if (ChaseCopCars.Count < MaxCopCars && !ChaseCopCars.Contains(vehicle) && vehicle.IsInRange(character.Position, 65f))
                                {
                                    ChaseCopCars.Add(vehicle);
                                }
                            }
                            else if (vehicle.Model.IsHelicopter)
                            {
                                // if the cop isn't in the chase - remove it from the list and just ignore it
                                if (vehicle.IsDead || !vehicle.IsDriveable || vehicle.IsUpsideDown || !vehicle.Exists())
                                {
                                    if (ChaseCopHelis.Contains(vehicle))
                                    {
                                        ChaseCopHelis.Remove(vehicle);
                                    }
                                }
                                // if the cop was in the chase but is too far away, remote it from the list and give another cop a chance
                                else if (ChaseCopHelis.Contains(vehicle) && !vehicle.IsInRange(character.Position, 200f))
                                {
                                    ChaseCopHelis.Remove(vehicle);
                                }
                                // remove extra cop helis the game is trying to add to the chase
                                else if (ChaseCopHelis.Count >= MaxCopHelis && !ChaseCopHelis.Contains(vehicle) && !vehicle.IsInRange(character.Position, 190f))
                                {
                                    for (int x = vehicle.PassengerCount - 1; x >= 0; x--)
                                    {
                                        vehicle.Passengers[x].Delete();
                                    }

                                    ChaseCopHelis.Remove(vehicle);
                                    vehicle.Delete();
                                }
                                else if (ChaseCopHelis.Count < MaxCopHelis && !ChaseCopHelis.Contains(vehicle) && !vehicle.IsInRange(character.Position, 100f))
                                {
                                    ChaseCopHelis.Add(vehicle);
                                }
                            }
                        }
                    }
                    else
                    {
                        ChaseCopCars.Clear();
                        ChaseCopHelis.Clear();
                    }
                }

                //Function.Call(Hash.SET_WANTED_LEVEL_DIFFICULTY, Game.Player, 0.0f); // Not sure what this does
                //Function.Call(Hash.SET_DISPATCH_COPS_FOR_PLAYER, Game.Player, false); // Prevent game from adding police to chase

                // Shooting weapon check
                if (Options.BetterChases.RequireLethalForceAuthorization && IsWanted && !DeadlyForce && character.IsShooting && Helpers.IsArmed && Witnesses.GetMaxRecognition(Witnesses.Cops) >= 0 && (Helpers.IsSilenced(character.Weapons.Current) && Helpers.IsCopNearby(character.Position, 35f) || Helpers.IsCopNearby(character.Position, 100f)))
                {
                    DeadlyForce = true;
                    PITAuthorized = true;
                    ArrestWarrants.ActiveWarrant.DeadlyForce = DeadlyForce;
                    ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;

                    if (Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3)
                    {
                        Helpers.WantedLevel = 3;
                    }

                    if (Options.BetterChases.ShowNotifications)
                    {
                        Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                        Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Shots have been fired, use of ~r~deadly force~w~ is now authorized.");
                        Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "web_lossantospolicedept", "web_lossantospolicedept", true, 0, "SHOTS FIRED", "~c~LSPD");
                    }

                    if (Options.DisplayHints)
                    {
                        Renderer.ShowHelpMessage("You have fired your weapon near a cop, ~r~deadly force~w~ has been authorized.");
                        Renderer.ShowHelpMessage("Cops will now ~r~shoot you~w~.");
                    }

                    if (Options.BetterChases.ShowBigMessages)
                    {
                        Renderer.ShowBigMessage("DEADLY FORCE AUTHORIZED", "", Renderer.HudColor.RED, Renderer.HudColor.BLACK, 3000);
                    }
                }

                // Pit and Manage Traffic
                foreach (Vehicle copVehicle in CopVehicles)
                {
                    if (copVehicle.IsOnAllWheels && (copVehicle.Model.IsCar || copVehicle.Model.IsBike || copVehicle.Model.IsBoat))
                    {
                        // PIT
                        if (Options.BetterChases.RequirePITAuthorization && IsWanted && Helpers.IsValid(character.CurrentVehicle) && (!PITAuthorized || Helpers.IsPopulatedArea(character.Position + character.Velocity, 40f)))
                        {
                            if (copVehicle.IsInRange(character.CurrentVehicle.Position, 14f) && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, copVehicle, true).Y > 0f && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, character.CurrentVehicle, true).Y > 0f)
                            {
                                Vector3 pos = character.CurrentVehicle.Position;
                                Vector3 offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, copVehicle, pos.X, pos.Y, pos.Z);
                                float relativeSpeed = copVehicle.Speed - character.CurrentVehicle.Speed;
                                if (relativeSpeed > 0f && offset.Y > 0f && offset.Z < 3f && offset.Z > -3f && offset.X < 2f && offset.X > -2f)
                                {
                                    float force = relativeSpeed < 0.6f ? relativeSpeed * -1 : -0.6f;
                                    Function.Call(Hash.APPLY_FORCE_TO_ENTITY_CENTER_OF_MASS, copVehicle, 1, 0f, force, 0f, true, true, true, true);
                                }
                            }
                        }

                        // Mange Traffic -- Avoid Peds
                        if (Options.BetterChases.CopsManageTraffic && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, copVehicle, true).Y > 5f)
                        {
                            Ped[] peds = World.GetNearbyPeds(copVehicle.Position, 14f);
                            foreach (Ped ped in peds)
                            {
                                if (ped.Handle != character.Handle && !Helpers.IsValid(ped.CurrentVehicle))
                                {
                                    Vector3 pos = ped.Position;
                                    Vector3 offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, copVehicle, pos.X, pos.Y, pos.Z);
                                    float relativeSpeed = copVehicle.Speed - Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, ped, true).Y;
                                    if (relativeSpeed > 0f && offset.Y > 0f && offset.Z < 3f && offset.Z > -3f && offset.X < 2f && offset.X > -2f)
                                    {
                                        float force = relativeSpeed < 0.8f ? relativeSpeed * -1 : -0.8f;
                                        Function.Call(Hash.APPLY_FORCE_TO_ENTITY_CENTER_OF_MASS, copVehicle, 1, 0f, force, 0f, true, true, true, true);
                                    }
                                }
                            }
                        }

                        // Manage Traffic -- Avoid TA
                        if (Options.BetterChases.CopsManageTraffic && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, copVehicle, true).Y > 5f)
                        {
                            Vehicle[] vehicles = World.GetNearbyVehicles(copVehicle.Position, 14f);
                            foreach (Vehicle vehicle in vehicles)
                            {
                                if (vehicle.Handle != copVehicle.Handle && (!Helpers.IsValid(character.CurrentVehicle) || character.CurrentVehicle.Handle != vehicle.Handle))
                                {
                                    Vector3 pos = vehicle.Position;
                                    Vector3 offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, copVehicle, pos.X, pos.Y, pos.Z);
                                    float relativeSpeed = copVehicle.Speed - Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, vehicle, true).Y;
                                    if (relativeSpeed > 0f && offset.Y > 0f && offset.Z < 3f && offset.Z > -3f && offset.X < 2f && offset.X > -2f)
                                    {
                                        float force = relativeSpeed < 0.8f ? relativeSpeed * -1 : -0.8f;
                                        Function.Call(Hash.APPLY_FORCE_TO_ENTITY_CENTER_OF_MASS, copVehicle, 1, 0f, force, 0f, true, true, true, true);
                                    }
                                }
                            }
                        }
                    }
                }

                WasWanted = IsWanted;
            }
        }
    }
}