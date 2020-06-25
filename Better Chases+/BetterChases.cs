using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace BetterChasesPlus
{
    public class BetterChases
    {
        public static bool CopSearch;
        public static Chase ActiveChase = new Chase();

        public static List<Ped> Peds = new List<Ped>();
        public static List<Vehicle> PedVehicles = new List<Vehicle>();
        public static List<Ped> Cops = new List<Ped>();
        public static List<Vehicle> CopVehicles = new List<Vehicle>();

        private static bool IsShooting = false;
        private static bool StopShooting = false;

        public class Chase
        {
            private TimeSpan _Duration = new TimeSpan();

            public bool DeadlyForce = false;
            public bool PITAuthorized = false;
            public DateTime StartTime = new DateTime();
            public Crimes Crimes = new Crimes();

            [XmlIgnore]
            public TimeSpan Duration
            {
                get { return _Duration; }
                set { _Duration = value; }
            }

            [XmlElement("Duration")]
            public long DuractionTicks
            {
                get { return _Duration.Ticks; }
                set { _Duration = new TimeSpan(value); }
            }
        }

        public class Crimes
        {
            public bool Fleeing1 = false;
            public bool Fleeing2 = false;
            public bool Fleeing3 = false;
            public bool Fleeing4 = false;
            public bool GrandTheftAuto = false;
            public bool Stolen = false;
            public bool Speeding = false;
            public bool Reckless = false;
            public bool Armed = false;
            public bool Aiming = false;
            public bool Assault = false;
            public bool PoliceAssault = false;
            public bool Shooting = false;
            public bool Murder = false;
            public bool PoliceMurder = false;
        }

        public static Chase MergeChases(Chase chase1, Chase chase2)
        {
            Chase chase = new Chase
            {
                DeadlyForce = chase1.DeadlyForce || chase2.DeadlyForce,
                PITAuthorized = chase1.PITAuthorized || chase2.PITAuthorized,
                Duration = chase1.Duration + chase2.Duration,
                StartTime = chase1.StartTime.CompareTo(chase2.StartTime) == -1 ? chase1.StartTime : chase2.StartTime
            };

            chase.Crimes.Fleeing1 = chase1.Crimes.Fleeing1 || chase2.Crimes.Fleeing1;
            chase.Crimes.Fleeing2 = chase1.Crimes.Fleeing2 || chase2.Crimes.Fleeing2;
            chase.Crimes.Fleeing3 = chase1.Crimes.Fleeing3 || chase2.Crimes.Fleeing3;
            chase.Crimes.Fleeing4 = chase1.Crimes.Fleeing4 || chase2.Crimes.Fleeing4;
            chase.Crimes.GrandTheftAuto = chase1.Crimes.GrandTheftAuto || chase2.Crimes.GrandTheftAuto;
            chase.Crimes.Stolen = chase1.Crimes.Stolen || chase2.Crimes.Stolen;
            chase.Crimes.Speeding = chase1.Crimes.Speeding || chase2.Crimes.Speeding;
            chase.Crimes.Reckless = chase1.Crimes.Reckless || chase2.Crimes.Reckless;
            chase.Crimes.Armed = chase1.Crimes.Armed || chase2.Crimes.Armed;
            chase.Crimes.Aiming = chase1.Crimes.Aiming || chase2.Crimes.Aiming;
            chase.Crimes.Assault = chase1.Crimes.Assault || chase2.Crimes.Assault;
            chase.Crimes.PoliceAssault = chase1.Crimes.PoliceAssault || chase2.Crimes.PoliceAssault;
            chase.Crimes.Shooting = chase1.Crimes.Shooting || chase2.Crimes.Shooting;
            chase.Crimes.Murder = chase1.Crimes.Murder || chase2.Crimes.Murder;
            chase.Crimes.PoliceMurder = chase1.Crimes.PoliceMurder || chase2.Crimes.PoliceMurder;

            return chase;
        }

        public class BetterChasesPassive : Script
        {
            private static bool IsWanted;
            private static bool WasWanted;
            private static int BustStars;
            private static bool AreHandsUp;
            private static bool IsGettingBusted;
            private static int AdditionalGroundUnits;
            private static int AdditionalAirUnits;
            private static DateTime ChaseTime = new DateTime();
            private static List<Ped> PedsKilled = new List<Ped>();
            private static List<Ped> CopsKilled = new List<Ped>();
            private static List<Ped> ModifiedCops = new List<Ped>();

            private static List<Ped> CopsMovingToCar = new List<Ped>();
            private static List<Ped> CopsAimingAtCar = new List<Ped>();

            public BetterChasesPassive()
            {
                Tick += OnTick;
                Interval = 250;
            }

            private void OnTick(object sender, EventArgs e)
            {
                if (Config.Options.BetterChases.Enabled == false)
                    return;

                Ped character = Game.Player.Character;

                IsWanted = Game.Player.WantedLevel > 0 || Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player) ? true : false;

                // Changed from Wanted to not Wanted - Reset for next chase
                if (WasWanted && !IsWanted)
                {
                    ActiveChase = new Chase();
                    AdditionalGroundUnits = 0;
                    AdditionalAirUnits = 0;
                    PedsKilled = new List<Ped>();
                    CopsKilled = new List<Ped>();
                }
                // Changed from not Wanted to Wanted
                else if (IsWanted && !WasWanted)
                {
                    // GTA.Native.Function.Call(GTA.Native.Hash.SET_CLOCK_DATE, 15, 6, 2009);
                    ChaseTime = World.CurrentDate;

                    // In case we are resuming a previous chase
                    if (ActiveChase.StartTime.CompareTo(new DateTime()) == 0)
                    {
                        ActiveChase.StartTime = World.CurrentDate;
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

                string crimeTitle = "";
                string crimeAnnouncment = "";
                int wantedLevel = Game.Player.WantedLevel;
                bool pitAuthorized = false;
                bool lethalAuthorized = false;
                int additionalGroundUnits = 0;
                int additionalAirUnits = 0;

                //System.Diagnostics.Stopwatch Stopwatch = System.Diagnostics.Stopwatch.StartNew();
                //Stopwatch.Start();

                //Vector3 pos = Player.Character.Position;
                //float radius = 100f;
                //bool FoundCop = Function.Call<bool>(Hash.IS_COP_PED_IN_AREA_3D, pos.X + radius, pos.Y + radius, pos.Z + radius, pos.X - radius, pos.Y - radius, pos.Z - radius);
                //bool FoundCop = Function.Call<bool>(Hash.IS_COP_VEHICLE_IN_AREA_3D, pos.X + radius, pos.Y + radius, pos.Z + radius, pos.X - radius, pos.Y - radius, pos.Z - radius);
                //Vehicle[] vehicles = World.GetAllVehicles();

                // Track All Peds & Active Vehicles
                Peds.Clear();
                PedVehicles.Clear();
                Cops.Clear();
                CopVehicles.Clear();
                foreach (Ped ped in World.GetAllPeds())
                {
                    if (ped.IsHuman)
                    {
                        if (Helpers.PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped))) // Helpers.PedCopHashes.Contains((PedHash)ped.Model.Hash
                        {
                            Cops.Add(ped);
                            

                            if (Helpers.IsValid(ped.CurrentVehicle) && Helpers.IsValid(ped.CurrentVehicle.Driver) && ped.CurrentVehicle.IsDriveable && ped.CurrentVehicle.Driver.Handle == ped.Handle)
                            {
                                CopVehicles.Add(ped.CurrentVehicle);
                            }
                            else if (Helpers.IsValid(ped.LastVehicle) && ped.LastVehicle.IsDriveable && !CopVehicles.Contains(ped.LastVehicle))
                            {
                                CopVehicles.Add(ped.LastVehicle);
                            }
                        }
                        else
                        {
                            Peds.Add(ped);

                            if (Helpers.IsValid(ped.CurrentVehicle) && Helpers.IsValid(ped.CurrentVehicle.Driver) && ped.CurrentVehicle.Driver.Handle == ped.Handle)
                            {
                                PedVehicles.Add(ped.CurrentVehicle);
                            }
                        }
                    }
                }

                // Prevent Cop Commandeering
                if (Config.Options.BetterChases.DisallowCopCommandeering && IsWanted)
                {
                    foreach (Ped cop in Cops)
                    {
                        Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, cop, 41, false); // BF_CanCommandeerVehicles in combatbehaviour.meta https://pastebin.com/MvE3Akam
                    }
                }

                // Lethal Force Authorization
                if (Config.Options.BetterChases.RequireLethalForceAuthorization)
                {
                    if (IsWanted && !ActiveChase.DeadlyForce)
                    {
                        foreach (Ped cop in Cops)
                        {
                            if (cop.Weapons.Current.Hash != WeaponHash.StunGun && cop.IsInCombatAgainst(character))
                            {
                                cop.Weapons.Give(WeaponHash.StunGun, 10, true, true);
                                cop.CanSwitchWeapons = false;
                                ModifiedCops.Add(cop);
                            }
                        }
                    }
                    else if (ModifiedCops.Count > 0)
                    {
                        foreach (Ped cop in ModifiedCops.ToList())
                        {
                            if (Helpers.IsValid(cop) && cop.Weapons.Current.Hash == WeaponHash.StunGun)
                            {
                                cop.CanSwitchWeapons = true;
                                cop.Weapons.Select(cop.Weapons.BestWeapon.Hash, true);
                                ModifiedCops.Remove(cop);
                            }
                            else
                            {
                                ModifiedCops.Remove(cop);
                            }
                        }
                    }
                }

                // Gather all cops & cop vehicles
                //Ped[] peds = World.GetAllPeds();

                //Cops.Clear();
                //CopVehicles.Clear();
                //foreach (Ped ped in peds)
                //{
                //    //ped.HasBeenDamagedBy
                //    //ped.IsInCombatAgainst
                //    //ped.GetMeleeTarget
                //    //Game.Player.GetTargetedEntity


                //    if (Helpers.IsValid(ped) && ped.IsAlive && Helpers.PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped)))
                //    {
                //        Cops.Add(ped);

                //        // Don't allow commandeering
                //        if (Config.Options.BetterChases.DisallowCopCommandeering)
                //        {
                //            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 41, false); // BF_CanCommandeerVehicles in combatbehaviour.meta https://pastebin.com/MvE3Akam
                //        }

                //        if (Helpers.IsValid(ped.CurrentVehicle) && Helpers.IsValid(ped.CurrentVehicle.Driver) && ped.CurrentVehicle.Driver.Handle == ped.Handle)
                //        {
                //            CopVehicles.Add(ped.CurrentVehicle);

                //            // driving style
                //            //Function.Call(Hash.SET_DRIVER_ABILITY, ped.CurrentVehicle.Driver, 100.0f);
                //            //Function.Call(Hash.SET_DRIVER_AGGRESSIVENESS, ped.CurrentVehicle.Driver, -100.0f);
                //            //ped.CurrentVehicle.Driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                //        }
                //    }
                //}
                ////Stopwatch.Stop();
                ////UI.ShowSubtitle("CPU: " + Stopwatch.Elapsed);
                ////UI.ShowSubtitle("" + peds.Length + " ~b~" + BetterChases.Cops.Count + " ~w~" + vehicles.Length + " ~b~" + BetterChases.CopVehicles.Count + "~w~ CPU: " + Stopwatch.Elapsed);

                //if (IsWanted)
                //{
                //    foreach (Ped cop in Cops)
                //    {
                //        // Lethal Force
                //        if (Config.Options.BetterChases.RequireLethalForceAuthorization && cop.IsInCombatAgainst(character))
                //        {
                //            if (!ActiveChase.DeadlyForce)
                //            {
                //                if (cop.Weapons.Current.Hash != WeaponHash.StunGun)
                //                {
                //                    cop.Weapons.Give(WeaponHash.StunGun, 1000, true, true);
                //                    cop.CanSwitchWeapons = false;
                //                    //Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, Cop, 0);
                //                    //Function.Call(Hash.SET_PED_COMBAT_RANGE, Cop, 0);
                //                    //RendererScript.DisplayHelpTextThisFrame("COP GIVEN SPECIAL GUN");
                //                }
                //                else
                //                {
                //                    // Don't shoot player when in a vehicle (doesnt work)
                //                    //if (Helpers.IsValid(character.CurrentVehicle) && cop.Weapons.Current.Hash == WeaponHash.StunGun)
                //                    //{
                //                    //    //function.call(hash.set_combat_float, cop, 0, 0);
                //                    //    //cop.Weapons.Current.Ammo = 0;
                //                    //    //cop.Weapons.Current.AmmoInClip = 0;
                //                    //    //cop.Weapons.Give(WeaponHash.StunGun, -1, true, false);
                //                    //    //UI.ShowSubtitle("No shoot");
                //                    //} else
                //                    //{
                //                    //    //cop.Weapons.Current.Ammo = 1000;
                //                    //}
                //                }
                //            }
                //            else
                //            {
                //                if (cop.Weapons.Current.Hash == WeaponHash.StunGun)
                //                {
                //                    //Cop.ShootRate = 500;
                //                    cop.CanSwitchWeapons = true;
                //                    cop.Weapons.Select(cop.Weapons.BestWeapon.Hash, true);
                //                    //Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, Cop, 2);
                //                    //Function.Call(Hash.SET_PED_COMBAT_RANGE, Cop, 1);
                //                }
                //            }
                //        }

                //        //if (Helpers.IsValid(Player.Character.CurrentVehicle) && Cop.Weapons.Current.Hash == WeaponHash.StunGun)
                //        //{
                //        //    Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, Cop, 1424, false);
                //        //    //Cop.Task.Wait(400);
                //        //    //Cop.Task.LookAt(Player.Character, 400);
                //        //    //Cop.ShootRate = -1;
                //        //    //Cop.Weapons.Current.Ammo = -1;
                //        //    //Cop.Weapons.Current.AmmoInClip = -1;
                //        //    //Cop.Weapons.RemoveAll();
                //        //    //RendererScript.DisplayHelpTextThisFrame("SHOOT 0");
                //        //} else if (Cop.Weapons.Current.Hash == WeaponHash.StunGun)
                //        //{
                //        //    //Cop.Weapons.Current.Ammo = 9000;
                //        //    //Cop.ShootRate = 500;
                //        //}
                //    }
                //}

                // Manage Cop Vehicles
                if (Config.Options.BetterChases.CopDispatch.Enabled && IsWanted)
                {
                    List<Vehicle> groundVehicles = new List<Vehicle>();
                    List<Vehicle> airVehicles = new List<Vehicle>();

                    foreach (Vehicle vehicle in CopVehicles)
                    {
                        if (vehicle.Model.IsCar || vehicle.Model.IsBike || vehicle.Model.IsQuadBike)
                        {
                            groundVehicles.Add(vehicle);
                        }
                        else if (vehicle.Model.IsPlane || vehicle.Model.IsHelicopter)
                        {
                            airVehicles.Add(vehicle);
                        }
                    }

                    if (groundVehicles.Count < Helpers.MinGroundUnits + AdditionalGroundUnits)
                    {
                        // spawn cops
                        //Function.Call(Hash.SET_DISPATCH_COPS_FOR_PLAYER, Game.Player, true);
                        //GTA.UI.Notification.Show("Debug " + ChaseGroundVehicles.Count);
                        //GTA.UI.Notification.Show("Car Units: " + (Helpers.MinGroundUnits + AdditionalGroundUnits));
                        //Function.Call(Hash.SET_DISPATCH_IDEAL_SPAWN_DISTANCE, 60f);
                        //Function.Call(Hash.SET_DISPATCH_SPAWN_LOCATION, character.Position.X, character.Position.Y, character.Position.Z);

                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 1, true);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 4, true);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 6, true);
                    }
                    else
                    {
                        //Function.Call(Hash.RESET_DISPATCH_IDEAL_SPAWN_DISTANCE);

                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 1, false);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 4, false);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 6, false);

                        //Function.Call(Hash.SET_DISPATCH_COPS_FOR_PLAYER, Game.Player, false);

                        // return GTA.Native.Function.Call<int>(GTA.Native.Hash._GET_NUM_DISPATCHED_UNITS_FOR_PLAYER, 1);
                        // GTA.Native.Function.Call(GTA.Native.Hash.ENABLE_DISPATCH_SERVICE, 1, false);
                        // GTA.Native.Function.Call(GTA.Native.Hash.BLOCK_DISPATCH_SERVICE_RESOURCE_CREATION, 1, false);
                        // GTA.Native.Function.Call(GTA.Native.Hash.SET_DISPATCH_COPS_FOR_PLAYER, GTA.Game.Player, false);
                    }

                    if (airVehicles.Count < Helpers.MinAirUnits + AdditionalAirUnits)
                    {
                        //GTA.UI.Notification.Show("Air Units: " + (Helpers.MinAirUnits + AdditionalAirUnits));
                        //Function.Call(Hash.SET_DISPATCH_IDEAL_SPAWN_DISTANCE, 120f);
                        //Function.Call(Hash.SET_DISPATCH_SPAWN_LOCATION, character.Position.X, character.Position.Y, character.Position.Z);

                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 2, true);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 12, true);
                    }
                    else
                    {
                        //Function.Call(Hash.RESET_DISPATCH_IDEAL_SPAWN_DISTANCE);

                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 2, false);
                        Function.Call(Hash.ENABLE_DISPATCH_SERVICE, 12, false);
                    }
                }

                // Crashed cars/motorcycles stop chasing
                if (Config.Options.BetterChases.WreckedCopsStopChasing && IsWanted)
                {
                    foreach (Vehicle vehicle in CopVehicles)
                    {
                        if (vehicle.EngineHealth > 0 && (vehicle.Model.IsCar || vehicle.Model.IsBike || vehicle.Model.IsBoat) && (vehicle.BodyHealth < 600f || vehicle.EngineHealth < 500f || Helpers.IsAnyTireBlown(vehicle)))
                        {
                            //vehicle.EngineHealth = -4000;
                            vehicle.IsDriveable = false;
                        }
                    }
                }

                // Don't shoot player in vehicle or on the ground
                if (Config.Options.BetterChases.RequireLethalForceAuthorization && IsWanted && !ActiveChase.DeadlyForce)
                {
                    if (character.IsInVehicle() || (character.IsBeingStunned || character.IsRagdoll || character.IsProne))
                    {
                        StopShooting = true;
                        //Renderer.ShowSubtitle("Stop shooting");
                    }
                    else if (StopShooting)
                    {
                        StopShooting = false;
                        //Renderer.ShowSubtitle("OK shooting");

                        foreach (Ped cop in Cops)
                        {
                            Function.Call(Hash.SET_PED_AMMO, cop, WeaponHash.StunGun, 10);
                            Function.Call(Hash.SET_AMMO_IN_CLIP, cop, WeaponHash.StunGun, 10);
                        }
                    }

                    // Trying some AI programming... FYI cops refuse to enter vehicles
                    //if (character.CurrentVehicle.Speed < 3f)
                    //{
                    //    foreach (Ped cop in Cops)
                    //    {
                    //        if (cop.IsAlive && cop.IsOnFoot && cop.IsInRange(character.Position, 20f) && !cop.IsInRange(character.Position, 6f) && !CopsMovingToCar.Contains(cop) && CopsMovingToCar.Count <= 2)
                    //        {
                    //            Renderer.ShowSubtitle("Go!");
                    //            cop.Task.ClearAllImmediately();
                    //            Function.Call(Hash.TASK_GO_TO_ENTITY_WHILE_AIMING_AT_ENTITY, cop, character.CurrentVehicle, character, 3f, false);
                    //            cop.AlwaysKeepTask = true;
                    //            CopsMovingToCar.Add(cop);
                    //        }
                    //        else if (cop.IsInRange(character.Position, 8f))
                    //        {
                    //            if (!CopsAimingAtCar.Contains(cop))
                    //            {
                    //                Renderer.ShowSubtitle("Arrest!");
                    //                cop.Task.ClearAllImmediately();
                    //                cop.Task.EnterVehicle(character.CurrentVehicle, VehicleSeat.Driver, 5000, 3f, EnterVehicleFlags.OnlyOpenDoor);
                    //                cop.AlwaysKeepTask = true;
                    //                CopsAimingAtCar.Add(cop);
                    //            }
                    //            else if (!Function.Call<bool>(Hash.GET_IS_TASK_ACTIVE, cop, 160))
                    //            {
                    //                Renderer.ShowSubtitle("Arrest NOW!");
                    //                cop.Task.EnterVehicle(character.CurrentVehicle, VehicleSeat.Driver, 5000, 3f, EnterVehicleFlags.OnlyOpenDoor);
                    //                cop.AlwaysKeepTask = true;
                    //            }
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    CopsMovingToCar.Clear();
                    //    CopsAimingAtCar.Clear();
                    //}
                }
                else if (StopShooting)
                {
                    StopShooting = false;

                    //CopsMovingToCar.Clear();
                    //CopsAimingAtCar.Clear();

                    //Renderer.ShowSubtitle("OK shooting2");

                    foreach (Ped cop in Cops)
                    {
                        Function.Call(Hash.SET_PED_AMMO, cop, WeaponHash.StunGun, 10);
                        Function.Call(Hash.SET_AMMO_IN_CLIP, cop, WeaponHash.StunGun, 10);
                    }
                }


                //if (Config.Options.BetterChases.RequireLethalForceAuthorization && IsWanted && character.IsInVehicle() && !ActiveChase.DeadlyForce)
                //{
                //    //Renderer.ShowSubtitle("Don't shoot");

                //    foreach (Ped cop in Cops)
                //    {
                //        Function.Call(Hash.SET_PED_AMMO, cop, cop.Weapons.Current.Hash, 0);
                //        Function.Call(Hash.SET_AMMO_IN_CLIP, cop, cop.Weapons.Current.Hash, 0);
                //        Function.Call(Hash.SET_PED_INFINITE_AMMO, cop, false, cop.Weapons.Current.Hash);
                //        Function.Call(Hash.SET_PED_INFINITE_AMMO_CLIP, cop, false);

                //        //if (cop.IsOnFoot && cop.IsInRange(character.Position, 20f))
                //        //{
                //        //    if ((character.CurrentVehicle.Speed > 2f || cop.IsInRange(character.Position, 6f)) && !CopsAimingAtCar.Contains(cop))
                //        //    {
                //        //        if (CopsMovingToCar.Contains(cop))
                //        //        {
                //        //            CopsMovingToCar.Remove(cop);
                //        //        }

                //        //        //cop.Task.AimAt(character, -1);
                //        //        Function.Call(Hash.TASK_AIM_GUN_AT_ENTITY, cop, character, 3000);
                //        //        //CopsAimingAtCar.Add(cop);
                //        //    }
                //        //    else if (!cop.IsInRange(character.Position, 6f) && !CopsMovingToCar.Contains(cop))
                //        //    {
                //        //        Function.Call(Hash.TASK_GO_TO_ENTITY_WHILE_AIMING_AT_ENTITY, cop, character, character, 3f, false);
                //        //        CopsMovingToCar.Add(cop);
                //        //    }
                //        //}
                //        //else if (CopsAimingAtCar.Contains(cop))
                //        //{
                //        //    CopsAimingAtCar.Remove(cop);
                //        //}
                //        //else if (CopsMovingToCar.Contains(cop))
                //        //{
                //        //    CopsMovingToCar.Remove(cop);
                //        //}
                //    }
                //}
                //else if (CopsAimingAtCar.Count > 0 || CopsMovingToCar.Count > 0)
                //{
                //    CopsAimingAtCar.Clear();
                //    CopsMovingToCar.Clear();
                //}

                // Allow extra bust opportunities (IsRunning is true when being stunned)
                if (Config.Options.BetterChases.AllowBustOpportunity && IsWanted && Helpers.WantedLevel < 5 && !character.IsInVehicle() && !character.IsSwimming && !character.IsSwimmingUnderWater && !character.IsFalling && !character.IsJumping && !character.IsWalking && character.Speed < 2f && !character.IsInCover && !Game.Player.IsAiming)
                {
                    // Bust Oportunity Enable/Disable
                    if ((Game.IsKeyPressed(Config.Options.SurrenderKey) || Game.IsControlPressed(Config.Options.SurrenderButton)) && Helpers.IsCopNearby(character.Position, 20f))
                    {
                        IsGettingBusted = true;
                    }
                    else if (IsGettingBusted)
                    {
                        IsGettingBusted = false;
                    }

                    // Set Wanted Level to 1 (only way police will arrest) and store wanted level in BustStars
                    if (IsGettingBusted && Game.Player.WantedLevel > 1)
                    {
                        BustStars = Game.Player.WantedLevel;
                        Helpers.WantedLevel = 1;
                        Helpers.MaxWantedLevel = 1;
                    }
                    else if (!IsGettingBusted && BustStars > 1)
                    {
                        Helpers.MaxWantedLevel = 5;
                        Helpers.WantedLevel = BustStars;
                        BustStars = 0;
                    }

                    // Hands up
                    if (IsGettingBusted && !character.IsBeingStunned && !character.IsRagdoll && !character.IsProne)
                    {
                        if (!AreHandsUp)
                        {
                            AreHandsUp = true;
                            character.Task.PlayAnimation("mp_am_hold_up", "handsup_base", 2f, 1f, -1, AnimationFlags.Loop, 0f);
                        }
                    }
                    else if (!IsGettingBusted && AreHandsUp)
                    {
                        AreHandsUp = false;
                        character.Task.ClearAnimation("mp_am_hold_up", "handsup_base");
                    }
                }

                // Crashing into cars
                //if (Config.Options.BetterChases.WantedLevelControl != "None" && IsWanted && VehicleCrashes < 4 && Game.Player.WantedLevel < 3 && Helpers.IsValid(character.CurrentVehicle) && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_VEHICLE, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_VEHICLE, Game.Player) < 500)
                //{
                //    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 50)
                //    {
                //        VehicleCrashes++;
                //        ArrestWarrants.ActiveWarrant.VehicleCrashes = VehicleCrashes;

                //        if (VehicleCrashes > 4)
                //        {
                //            Helpers.WantedLevel = Game.Player.WantedLevel + 1;

                //            if (Config.Options.BetterChases.ShowNotifications)
                //            {
                //                Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                //                Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect is reckless, backup required at ~y~" + World.GetStreetName(character.Position) + "~w~.");
                //                Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "RECKLESS DRIVING", "~c~LSPD");
                //            }

                //            if (Config.Options.DisplayHints)
                //            {
                //                Renderer.ShowHelpMessage("You have crashed into too many vehicles. The Wanted Level is now ~y~" + Helpers.WantedLevel + " Star(s)~w~.");
                //            }
                //        }
                //    }
                //}

                // Hit & Run
                //if (IsWanted && !HitandRun && Game.Player.WantedLevel <= 3 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_PED, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_HIT_PED, Game.Player) < 1000)
                //{
                //    if (Witnesses.GetMaxRecognition(Witnesses.Cops, true) >= 50)
                //    {
                //        HitandRun = true;
                //        ArrestWarrants.ActiveWarrant.HitandRun = true;

                //        if (Config.Options.BetterChases.WantedLevelControl != "None" && Game.Player.WantedLevel < 3)
                //        {
                //            Helpers.WantedLevel = 3;
                //        }

                //        if (Config.Options.BetterChases.RequirePITAuthorization && !PITAuthorized)
                //        {
                //            PITAuthorized = true;
                //            ArrestWarrants.ActiveWarrant.PITAuthorized = PITAuthorized;
                //        }

                //        if (Config.Options.BetterChases.ShowNotifications)
                //        {
                //            Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                //            Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, "Suspect involved in a hit & run, ~y~Tactical Vehicle Interventions~w~ are now authorized.");
                //            Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, "HIT & RUN", "~c~LSPD");
                //        }

                //        if (Config.Options.DisplayHints)
                //        {
                //            Renderer.ShowHelpMessage("You have run over a pedestrian, ~y~PIT~w~ has been authorized.");
                //            Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                //            Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                //        }

                //        if (Config.Options.BetterChases.ShowBigMessages)
                //        {
                //            Renderer.ShowBigMessage("PIT AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                //        }
                //    }
                //}

                // Want level rises over time
                if (Config.Options.BetterChases.ChaseEscalates.Enabled && IsWanted && !IsGettingBusted)
                {
                    if (Config.Options.BetterChases.ChaseEscalates.PhaseOne.Enabled && !ActiveChase.Crimes.Fleeing1 && ActiveChase.Duration.TotalMinutes > Config.Options.BetterChases.ChaseEscalates.PhaseOne.Length)
                    {
                        if (Config.Options.BetterChases.ChaseEscalates.PhaseOne.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.ChaseEscalates.PhaseOne.WantedLevel)
                        {
                            ActiveChase.Crimes.Fleeing1 = true;
                            wantedLevel = Config.Options.BetterChases.ChaseEscalates.PhaseOne.WantedLevel;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseOne.PITAuthorized && !ActiveChase.PITAuthorized)
                        {
                            ActiveChase.Crimes.Fleeing1 = true;
                            pitAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseOne.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                        {
                            ActiveChase.Crimes.Fleeing1 = true;
                            lethalAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseOne.RequestBackup)
                        {
                            ActiveChase.Crimes.Fleeing1 = true;
                            additionalGroundUnits += 1;
                        }

                        if (ActiveChase.Crimes.Fleeing1)
                        {
                            crimeTitle = "Fleeing";
                            crimeAnnouncment = "Suspect is refusing to stop";

                            if (Config.Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("The chase has gone on too long and is escalating.");
                            }
                        }
                    }
                    else if (Config.Options.BetterChases.ChaseEscalates.PhaseTwo.Enabled && !ActiveChase.Crimes.Fleeing2 && ActiveChase.Duration.TotalMinutes > Config.Options.BetterChases.ChaseEscalates.PhaseOne.Length + Config.Options.BetterChases.ChaseEscalates.PhaseTwo.Length)
                    {
                        if (Config.Options.BetterChases.ChaseEscalates.PhaseTwo.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.ChaseEscalates.PhaseTwo.WantedLevel)
                        {
                            ActiveChase.Crimes.Fleeing2 = true;
                            wantedLevel = Config.Options.BetterChases.ChaseEscalates.PhaseTwo.WantedLevel;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseTwo.PITAuthorized && !ActiveChase.PITAuthorized)
                        {
                            ActiveChase.Crimes.Fleeing2 = true;
                            pitAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseTwo.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                        {
                            ActiveChase.Crimes.Fleeing2 = true;
                            lethalAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseTwo.RequestBackup)
                        {
                            ActiveChase.Crimes.Fleeing2 = true;
                            additionalGroundUnits += 1;
                        }

                        if (ActiveChase.Crimes.Fleeing2)
                        {
                            crimeTitle = "Fleeing";
                            crimeAnnouncment = "Suspect is continuing to refuse to stop";

                            if (Config.Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("The chase has gone on too long and is continuing to escalate.");
                            }
                        }
                    }
                    else if (Config.Options.BetterChases.ChaseEscalates.PhaseThree.Enabled && !ActiveChase.Crimes.Fleeing3 && ActiveChase.Duration.TotalMinutes > Config.Options.BetterChases.ChaseEscalates.PhaseOne.Length + Config.Options.BetterChases.ChaseEscalates.PhaseTwo.Length + Config.Options.BetterChases.ChaseEscalates.PhaseThree.Length)
                    {
                        if (Config.Options.BetterChases.ChaseEscalates.PhaseThree.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.ChaseEscalates.PhaseThree.WantedLevel)
                        {
                            ActiveChase.Crimes.Fleeing3 = true;
                            wantedLevel = Config.Options.BetterChases.ChaseEscalates.PhaseThree.WantedLevel;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseThree.PITAuthorized && !ActiveChase.PITAuthorized)
                        {
                            ActiveChase.Crimes.Fleeing3 = true;
                            pitAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseThree.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                        {
                            ActiveChase.Crimes.Fleeing3 = true;
                            lethalAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseThree.RequestBackup)
                        {
                            ActiveChase.Crimes.Fleeing3 = true;
                            additionalGroundUnits += 1;
                        }

                        if (ActiveChase.Crimes.Fleeing3)
                        {
                            crimeTitle = "Fleeing";
                            crimeAnnouncment = "Suspect shows no sign of stopping";

                            if (Config.Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("You have continued to elude the police with no sign of stopping.");
                            }
                        }
                    }
                    else if (Config.Options.BetterChases.ChaseEscalates.PhaseFour.Enabled && !ActiveChase.Crimes.Fleeing4 && ActiveChase.Duration.TotalMinutes > Config.Options.BetterChases.ChaseEscalates.PhaseOne.Length + Config.Options.BetterChases.ChaseEscalates.PhaseTwo.Length + Config.Options.BetterChases.ChaseEscalates.PhaseThree.Length + Config.Options.BetterChases.ChaseEscalates.PhaseFour.Length)
                    {
                        if (Config.Options.BetterChases.ChaseEscalates.PhaseFour.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.ChaseEscalates.PhaseFour.WantedLevel)
                        {
                            ActiveChase.Crimes.Fleeing4 = true;
                            wantedLevel = Config.Options.BetterChases.ChaseEscalates.PhaseFour.WantedLevel;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseFour.PITAuthorized && !ActiveChase.PITAuthorized)
                        {
                            ActiveChase.Crimes.Fleeing4 = true;
                            pitAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseFour.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                        {
                            ActiveChase.Crimes.Fleeing4 = true;
                            lethalAuthorized = true;
                        }

                        if (Config.Options.BetterChases.ChaseEscalates.PhaseFour.RequestBackup)
                        {
                            ActiveChase.Crimes.Fleeing4 = true;
                            additionalGroundUnits += 1;
                        }

                        if (ActiveChase.Crimes.Fleeing4)
                        {
                            crimeTitle = "Fleeing";
                            crimeAnnouncment = "Suspect stil shows no sign of stopping";

                            if (Config.Options.DisplayHints)
                            {
                                Renderer.ShowHelpMessage("You have continued to elude the police with no sign of stopping.");
                            }
                        }
                    }
                }

                // Driving A Stolen Vehicle
                if (Config.Options.BetterChases.Crimes.Stolen.Enabled && IsWanted && !ActiveChase.Crimes.GrandTheftAuto && !ActiveChase.Crimes.Stolen && Helpers.IsValid(character.CurrentVehicle) && character.CurrentVehicle.IsStolen)
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Stolen.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Stolen.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Stolen.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Stolen.WantedLevel)
                            {
                                ActiveChase.Crimes.Stolen = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Stolen.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Stolen.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Stolen = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Stolen.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Stolen = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Stolen.RequestBackup)
                            {
                                ActiveChase.Crimes.Stolen = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Stolen)
                            {
                                crimeTitle = "Stolen Vehicle";
                                crimeAnnouncment = "Suspect is driving a stolen vehicle";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted driving a stolen vehicle during a chase.");
                                }
                            }
                        }
                    }
                }

                // Grand Theft Auto
                if (Config.Options.BetterChases.Crimes.GTA.Enabled && IsWanted && !ActiveChase.Crimes.GrandTheftAuto && (character.IsJacking || character.IsTryingToEnterALockedVehicle))
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.GTA.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.GTA.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.GTA.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.GTA.WantedLevel)
                            {
                                ActiveChase.Crimes.GrandTheftAuto = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.GTA.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.GTA.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.GrandTheftAuto = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.GTA.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.GrandTheftAuto = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.GTA.RequestBackup)
                            {
                                ActiveChase.Crimes.GrandTheftAuto = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.GrandTheftAuto)
                            {
                                crimeTitle = "Grand Theft Auto";
                                crimeAnnouncment = "Suspect is stealing a vehicle";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted stealing a vehicle during a chase.");
                                }
                            }
                        }
                    }
                }

                // Excessive Speeding
                if (Config.Options.BetterChases.Crimes.Speeding.Enabled && IsWanted && !ActiveChase.Crimes.Speeding && Helpers.IsValid(character.CurrentVehicle) && character.CurrentVehicle.Speed > Config.Options.BetterChases.Crimes.Speeding.Speed)
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Speeding.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Speeding.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Speeding.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Speeding.WantedLevel)
                            {
                                ActiveChase.Crimes.Speeding = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Speeding.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Speeding.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Speeding = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Speeding.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Speeding = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Speeding.RequestBackup)
                            {
                                ActiveChase.Crimes.Speeding = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Speeding)
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

                                crimeTitle = "Excessive Speeding";
                                crimeAnnouncment = "Suspect is speeding excessively at over ~y~" + speedText + "~w~";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted driving very fast during a chase.");
                                }
                            }
                        }
                    }
                }

                // Reckless Driving -- Driving against traffic OR Driving on sidewalks
                if (Config.Options.BetterChases.Crimes.Reckless.Enabled && IsWanted && !ActiveChase.Crimes.Reckless && Helpers.IsValid(character.CurrentVehicle) && !character.CurrentVehicle.Model.IsBicycle && character.CurrentVehicle.Speed > Config.Options.BetterChases.Crimes.Reckless.Speed && ((Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_AGAINST_TRAFFIC, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_AGAINST_TRAFFIC, Game.Player) < 1000) || (Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_ON_PAVEMENT, Game.Player) > -1 && Function.Call<int>(Hash.GET_TIME_SINCE_PLAYER_DROVE_ON_PAVEMENT, Game.Player) < 1000)))
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Reckless.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Reckless.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Reckless.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Reckless.WantedLevel)
                            {
                                ActiveChase.Crimes.Reckless = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Reckless.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Reckless.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Reckless = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Reckless.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Reckless = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Reckless.RequestBackup)
                            {
                                ActiveChase.Crimes.Reckless = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Reckless)
                            {
                                crimeTitle = "Reckless Driving";
                                crimeAnnouncment = "Suspect is driving recklessly";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You have been driving on sidewalks or the wrong way.");
                                }
                            }
                        }
                    }
                }

                // Carrying Weapon
                if (Config.Options.BetterChases.Crimes.Armed.Enabled && IsWanted && !ActiveChase.Crimes.Armed && !IsGettingBusted && Helpers.IsArmed && !character.IsSittingInVehicle())
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Armed.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Armed.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Armed.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Armed.WantedLevel)
                            {
                                ActiveChase.Crimes.Armed = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Armed.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Armed.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Armed = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Armed.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Armed = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Armed.RequestBackup)
                            {
                                ActiveChase.Crimes.Armed = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Armed)
                            {
                                crimeTitle = "Armed Suspect";
                                crimeAnnouncment = "Suspect is armed with a weapon";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted brandishing a weapon during a chase.");
                                }
                            }
                        }
                    }
                }

                // Aiming Weapon
                if (Config.Options.BetterChases.Crimes.Aiming.Enabled && IsWanted && !ActiveChase.Crimes.Aiming && Helpers.IsArmed && character.IsAiming)
                {
                    if (Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Stolen.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Aiming.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Aiming.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Aiming.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Aiming.WantedLevel)
                            {
                                ActiveChase.Crimes.Aiming = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Aiming.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Aiming.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Aiming = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Aiming.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Aiming = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Aiming.RequestBackup)
                            {
                                ActiveChase.Crimes.Aiming = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Aiming)
                            {
                                crimeTitle = "Raised Weapon";
                                crimeAnnouncment = "Suspect is aiming a weapon";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted aiming a weapon during a chase.");
                                }
                            }
                        }
                    }
                }

                // Assault
                if (IsWanted)
                {
                    if(Config.Options.BetterChases.Crimes.Assault.Enabled && !ActiveChase.Crimes.Assault)
                    {
                        foreach (Ped ped in Peds)
                        {
                            if (ped.HasBeenDamagedBy(character) || (Helpers.IsValid(character.CurrentVehicle) && ped.HasBeenDamagedBy(character.CurrentVehicle)))
                            {
                                if (Config.Options.BetterChases.Crimes.Assault.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Assault.MaxWantedLevel)
                                {
                                    if (Config.Options.BetterChases.Crimes.Assault.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Assault.PoliceWitnessThreshold)
                                    {
                                        if (Config.Options.BetterChases.Crimes.Assault.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Assault.WantedLevel)
                                        {
                                            ActiveChase.Crimes.Assault = true;
                                            wantedLevel = Config.Options.BetterChases.Crimes.Assault.WantedLevel;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Assault.PITAuthorized && !ActiveChase.PITAuthorized)
                                        {
                                            ActiveChase.Crimes.Assault = true;
                                            pitAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Assault.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                                        {
                                            ActiveChase.Crimes.Assault = true;
                                            lethalAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Assault.RequestBackup)
                                        {
                                            ActiveChase.Crimes.Assault = true;
                                            additionalGroundUnits += 1;
                                        }

                                        if (ActiveChase.Crimes.Assault)
                                        {
                                            crimeTitle = "Assault";
                                            crimeAnnouncment = "Suspect assaulted a civilian";

                                            if (Config.Options.DisplayHints)
                                            {
                                                Renderer.ShowHelpMessage("You were spotted harming a civilian during a chase.");
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Config.Options.BetterChases.Crimes.PoliceAssault.Enabled && !ActiveChase.Crimes.PoliceAssault)
                    {
                        foreach (Ped ped in Cops)
                        {
                            if (ped.HasBeenDamagedBy(character) || (Helpers.IsValid(character.CurrentVehicle) && ped.HasBeenDamagedBy(character.CurrentVehicle)))
                            {
                                if (Config.Options.BetterChases.Crimes.PoliceAssault.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.PoliceAssault.MaxWantedLevel)
                                {
                                    if (Config.Options.BetterChases.Crimes.PoliceAssault.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.PoliceAssault.PoliceWitnessThreshold)
                                    {
                                        if (Config.Options.BetterChases.Crimes.PoliceAssault.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.PoliceAssault.WantedLevel)
                                        {
                                            ActiveChase.Crimes.PoliceAssault = true;
                                            wantedLevel = Config.Options.BetterChases.Crimes.PoliceAssault.WantedLevel;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceAssault.PITAuthorized && !ActiveChase.PITAuthorized)
                                        {
                                            ActiveChase.Crimes.PoliceAssault = true;
                                            pitAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceAssault.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                                        {
                                            ActiveChase.Crimes.PoliceAssault = true;
                                            lethalAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceAssault.RequestBackup)
                                        {
                                            ActiveChase.Crimes.PoliceAssault = true;
                                            additionalGroundUnits += 1;
                                        }

                                        if (ActiveChase.Crimes.PoliceAssault)
                                        {
                                            crimeTitle = "Assaulting Police";
                                            crimeAnnouncment = "Suspect assaulted a police officer";

                                            if (Config.Options.DisplayHints)
                                            {
                                                Renderer.ShowHelpMessage("You were spotted harming a police officer during a chase.");
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Shooting Weapon
                if (Config.Options.BetterChases.Crimes.Shooting.Enabled && IsWanted && !ActiveChase.Crimes.Shooting && Helpers.IsArmed && IsShooting)
                {
                    if (Config.Options.BetterChases.Crimes.Shooting.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Shooting.MaxWantedLevel)
                    {
                        if (Config.Options.BetterChases.Crimes.Shooting.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Shooting.PoliceWitnessThreshold)
                        {
                            if (Config.Options.BetterChases.Crimes.Shooting.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Shooting.WantedLevel)
                            {
                                ActiveChase.Crimes.Shooting = true;
                                wantedLevel = Config.Options.BetterChases.Crimes.Shooting.WantedLevel;
                            }

                            if (Config.Options.BetterChases.Crimes.Shooting.PITAuthorized && !ActiveChase.PITAuthorized)
                            {
                                ActiveChase.Crimes.Shooting = true;
                                pitAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Shooting.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                            {
                                ActiveChase.Crimes.Shooting = true;
                                lethalAuthorized = true;
                            }

                            if (Config.Options.BetterChases.Crimes.Shooting.RequestBackup)
                            {
                                ActiveChase.Crimes.Shooting = true;
                                additionalGroundUnits += 1;
                            }

                            if (ActiveChase.Crimes.Shooting)
                            {
                                crimeTitle = "Shots fired";
                                crimeAnnouncment = "Shots have been fired";

                                if (Config.Options.DisplayHints)
                                {
                                    Renderer.ShowHelpMessage("You were spotted shooting a weapon during a chase.");
                                }
                            }
                        }
                    }
                }

                // Murder
                if (IsWanted)
                {
                    if (Config.Options.BetterChases.Crimes.Murder.Enabled && !ActiveChase.Crimes.Murder)
                    {
                        foreach (Ped ped in Peds)
                        {
                            if (!ped.IsAlive && !PedsKilled.Contains(ped) && Helpers.IsValid(ped.Killer) && (ped.Killer.Handle == character.Handle || (Helpers.IsValid(character.CurrentVehicle) && ped.Killer.Handle == character.CurrentVehicle.Handle)))
                            {
                                PedsKilled.Add(ped);
                                //GTA.UI.Notification.Show("Murder!");

                                if (Config.Options.BetterChases.Crimes.Murder.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.Murder.MaxWantedLevel)
                                {
                                    if (Config.Options.BetterChases.Crimes.Murder.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.Murder.PoliceWitnessThreshold)
                                    {
                                        if (Config.Options.BetterChases.Crimes.Murder.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.Murder.WantedLevel)
                                        {
                                            ActiveChase.Crimes.Murder = true;
                                            wantedLevel = Config.Options.BetterChases.Crimes.Murder.WantedLevel;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Murder.PITAuthorized && !ActiveChase.PITAuthorized)
                                        {
                                            ActiveChase.Crimes.Murder = true;
                                            pitAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Murder.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                                        {
                                            ActiveChase.Crimes.Murder = true;
                                            lethalAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.Murder.RequestBackup)
                                        {
                                            ActiveChase.Crimes.Murder = true;
                                            additionalGroundUnits += 1;
                                        }

                                        if (ActiveChase.Crimes.Murder)
                                        {
                                            crimeTitle = "Murder";
                                            crimeAnnouncment = "Suspect killed a civilian";

                                            if (Config.Options.DisplayHints)
                                            {
                                                Renderer.ShowHelpMessage("You were spotted killing a civilian during a chase.");
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Config.Options.BetterChases.Crimes.PoliceMurder.Enabled && !ActiveChase.Crimes.PoliceMurder)
                    {
                        foreach (Ped ped in Cops)
                        {
                            if (!ped.IsAlive && !CopsKilled.Contains(ped) && Helpers.IsValid(ped.Killer) && (ped.Killer.Handle == character.Handle || (Helpers.IsValid(character.CurrentVehicle) && ped.Killer.Handle == character.CurrentVehicle.Handle)))
                            {
                                CopsKilled.Add(ped);
                                //GTA.UI.Notification.Show("Murder!");

                                if (Config.Options.BetterChases.Crimes.PoliceMurder.MaxWantedLevel == 0 || wantedLevel <= Config.Options.BetterChases.Crimes.PoliceMurder.MaxWantedLevel)
                                {
                                    if (Config.Options.BetterChases.Crimes.PoliceMurder.PoliceWitnessThreshold == 0 || Witnesses.GetMaxRecognition(Witnesses.Cops) >= Config.Options.BetterChases.Crimes.PoliceMurder.PoliceWitnessThreshold)
                                    {
                                        if (Config.Options.BetterChases.Crimes.PoliceMurder.WantedLevel > 0 && wantedLevel < Config.Options.BetterChases.Crimes.PoliceMurder.WantedLevel)
                                        {
                                            ActiveChase.Crimes.PoliceMurder = true;
                                            wantedLevel = Config.Options.BetterChases.Crimes.PoliceMurder.WantedLevel;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceMurder.PITAuthorized && !ActiveChase.PITAuthorized)
                                        {
                                            ActiveChase.Crimes.PoliceMurder = true;
                                            pitAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceMurder.LethalForceAuthorized && !ActiveChase.DeadlyForce)
                                        {
                                            ActiveChase.Crimes.PoliceMurder = true;
                                            lethalAuthorized = true;
                                        }

                                        if (Config.Options.BetterChases.Crimes.PoliceMurder.RequestBackup)
                                        {
                                            ActiveChase.Crimes.PoliceMurder = true;
                                            additionalGroundUnits += 1;
                                        }

                                        if (ActiveChase.Crimes.PoliceMurder)
                                        {
                                            crimeTitle = "Police Murder";
                                            crimeAnnouncment = "Suspect killed a police officer";

                                            if (Config.Options.DisplayHints)
                                            {
                                                Renderer.ShowHelpMessage("You were spotted killing a police officer during a chase.");
                                            }

                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Wanted Level PIT & Lethal
                if (!ActiveChase.PITAuthorized && !pitAuthorized)
                {
                    switch (Game.Player.WantedLevel)
                    {
                        case 1:
                            pitAuthorized = Config.Options.BetterChases.CopDispatch.OneStar.PITAuthorized;
                            break;
                        case 2:
                            pitAuthorized = Config.Options.BetterChases.CopDispatch.TwoStar.PITAuthorized;
                            break;
                        case 3:
                            pitAuthorized = Config.Options.BetterChases.CopDispatch.ThreeStar.PITAuthorized;
                            break;
                        case 4:
                            pitAuthorized = Config.Options.BetterChases.CopDispatch.FourStar.PITAuthorized;
                            break;
                        case 5:
                            pitAuthorized = Config.Options.BetterChases.CopDispatch.FiveStar.PITAuthorized;
                            break;
                    }

                    if (pitAuthorized && crimeTitle == "")
                    {
                        crimeTitle = "Dangerous Suspect";
                        crimeAnnouncment = "Suspect is behaving dangerously";
                    }
                }
                else if (!ActiveChase.DeadlyForce && !lethalAuthorized)
                {
                    switch (Game.Player.WantedLevel)
                    {
                        case 1:
                            lethalAuthorized = Config.Options.BetterChases.CopDispatch.OneStar.LethalForceAuthorized;
                            break;
                        case 2:
                            lethalAuthorized = Config.Options.BetterChases.CopDispatch.TwoStar.LethalForceAuthorized;
                            break;
                        case 3:
                            lethalAuthorized = Config.Options.BetterChases.CopDispatch.ThreeStar.LethalForceAuthorized;
                            break;
                        case 4:
                            lethalAuthorized = Config.Options.BetterChases.CopDispatch.FourStar.LethalForceAuthorized;
                            break;
                        case 5:
                            lethalAuthorized = Config.Options.BetterChases.CopDispatch.FiveStar.LethalForceAuthorized;
                            break;
                    }

                    if (lethalAuthorized && crimeTitle == "")
                    {
                        crimeTitle = "Dangerous Suspect";
                        crimeAnnouncment = "Suspect is behaving dangerously";
                    }
                }

                if (crimeTitle != "")
                {
                    string output = crimeAnnouncment;

                    if (lethalAuthorized)
                    {
                        ActiveChase.DeadlyForce = true;
                        output += ", ~r~deadly force~w~ has been authorized";
                    }
                    
                    if (pitAuthorized)
                    {
                        ActiveChase.PITAuthorized = true;
                        output += ", ~y~PIT~w~ has been authorized";

                        if (Config.Options.DisplayHints)
                        {
                            Renderer.ShowHelpMessage("~y~PIT~w~ has been authorized.");
                            Renderer.ShowHelpMessage("Cops will now ~y~ram you~w~ at will.");
                            Renderer.ShowHelpMessage("However, they will still refrain from doing so in ~y~populated areas~w~.");
                        }

                        if (Config.Options.BetterChases.ShowBigMessages)
                        {
                            Renderer.ShowBigMessage("PIT AUTHORIZED", "", Renderer.HudColor.GOLD, Renderer.HudColor.BLACK, 3000);
                        }
                    }

                    if (additionalGroundUnits > 0 || additionalAirUnits > 0 || wantedLevel > Game.Player.WantedLevel)
                    {
                        int groundUnits = Helpers.MinGroundUnits + additionalGroundUnits + AdditionalGroundUnits;
                        int airUnits = Helpers.MinAirUnits + additionalAirUnits + AdditionalAirUnits;

                        if (wantedLevel > Game.Player.WantedLevel)
                        {
                            Helpers.WantedLevel = wantedLevel;
                            AdditionalGroundUnits = Math.Max(groundUnits - Helpers.MinGroundUnits, 0);
                            AdditionalAirUnits = Math.Max(airUnits - Helpers.MinAirUnits, 0);

                            if (AdditionalGroundUnits + Helpers.MinGroundUnits > groundUnits && AdditionalAirUnits + Helpers.MinAirUnits > airUnits)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~, requesting air unit same location";
                            }
                            else if (AdditionalGroundUnits + Helpers.MinGroundUnits > groundUnits)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                            else if (AdditionalAirUnits + Helpers.MinAirUnits > airUnits)
                            {
                                output += ", requesting air unit at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                        }
                        else if (Helpers.MinGroundUnits + additionalGroundUnits + AdditionalGroundUnits > Helpers.MaxGroundUnits)
                        {
                            Helpers.WantedLevel += 1;
                            AdditionalGroundUnits = Math.Max(groundUnits - Helpers.MinGroundUnits, 0);
                            AdditionalAirUnits = Math.Max(airUnits - Helpers.MinAirUnits, 0);

                            if (additionalGroundUnits > 0 && additionalAirUnits > 0)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~, requesting air unit same location";
                            }
                            else if (additionalGroundUnits > 0)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                            else if (additionalAirUnits > 0)
                            {
                                output += ", requesting air unit at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                        }
                        else
                        {
                            AdditionalGroundUnits += additionalGroundUnits;
                            AdditionalAirUnits += additionalAirUnits;

                            if (additionalGroundUnits > 0 && additionalAirUnits > 0)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~, requesting air unit same location";
                            }
                            else if (additionalGroundUnits > 0)
                            {
                                output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                            else if (additionalAirUnits > 0)
                            {
                                output += ", requesting air unit at ~y~" + World.GetStreetName(character.Position) + "~w~";
                            }
                        }
                    }

                    if (wantedLevel > Game.Player.WantedLevel)
                    {
                        Helpers.WantedLevel = wantedLevel;
                        output += ", backup required at ~y~" + World.GetStreetName(character.Position) + "~w~";
                    }

                    Function.Call(Hash.BEGIN_TEXT_COMMAND_THEFEED_POST, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, output + "~w~.");
                    Function.Call(Hash.END_TEXT_COMMAND_THEFEED_POST_MESSAGETEXT, "WEB_LOSSANTOSPOLICEDEPT", "WEB_LOSSANTOSPOLICEDEPT", true, 0, crimeTitle.ToUpper(), "~c~LSPD");
                }

                if (IsWanted)
                {
                    ActiveChase.Duration += World.CurrentDate - ChaseTime - ActiveChase.Duration;
                    ArrestWarrants.ActiveWarrant.Chase = ActiveChase;
                }

                //Renderer.ShowSubtitle(groundVehicles.Count + " | " + Helpers.MinGroundUnits + " " + additionalGroundUnits + " " + AdditionalGroundUnits);

                IsShooting = false;
                WasWanted = IsWanted;
            }
        }

        public class BetterChasesActive : Script
        {
            private static bool IsWanted;
            private static bool WasWanted;

            public BetterChasesActive()
            {
                Tick += OnTick;

                Interval = 1;
            }

            private void OnTick(object sender, EventArgs e)
            {
                if (Config.Options.BetterChases.Enabled == false)
                    return;

                Ped character = Game.Player.Character;

                IsWanted = Game.Player.WantedLevel > 0 || Function.Call<bool>(Hash.ARE_PLAYER_STARS_GREYED_OUT, Game.Player) ? true : false;

                // Manage wanted level
                if (Config.Options.BetterChases.WantedLevelControl == "Full")
                {
                    // Changed from not Wanted to Wanted
                    if (IsWanted && !WasWanted)
                    {
                        Helpers.WantedLevel = Game.Player.WantedLevel;
                        Helpers.MaxWantedLevel = Game.Player.WantedLevel;
                    }
                    // Changed from Wanted to not Wanted
                    else if (!IsWanted && WasWanted)
                    {
                        Helpers.WantedLevel = 0;
                        Helpers.MaxWantedLevel = 5;
                    }
                    else if (IsWanted && Function.Call<int>(Hash.GET_MAX_WANTED_LEVEL) != Helpers.MaxWantedLevel)
                    {
                        // Call set so game value is returned to mod's value
                        Helpers.MaxWantedLevel = Helpers.MaxWantedLevel;
                    }
                    else if (IsWanted && Game.Player.WantedLevel != Helpers.WantedLevel)
                    {
                        // Call set so game value is returned to mod's value
                        Helpers.WantedLevel = Helpers.WantedLevel;
                    }
                }

                if (IsWanted && character.IsShooting)
                {
                    IsShooting = true;
                }

                // Stop cops shooting stun guns
                if (Config.Options.BetterChases.RequireLethalForceAuthorization && !ActiveChase.DeadlyForce && StopShooting)
                {
                    foreach (Ped cop in Cops)
                    {
                        cop.Weapons.Select(WeaponHash.StunGun, true);
                        cop.CanSwitchWeapons = false;
                        Function.Call(Hash.SET_PED_INFINITE_AMMO, cop, false, WeaponHash.StunGun);
                        Function.Call(Hash.SET_PED_INFINITE_AMMO_CLIP, cop, false);
                        Function.Call(Hash.SET_PED_AMMO, cop, WeaponHash.StunGun, -1);
                        Function.Call(Hash.SET_AMMO_IN_CLIP, cop, WeaponHash.StunGun, -1);
                    }
                }

                //Function.Call(Hash.SET_WANTED_LEVEL_DIFFICULTY, Game.Player, 0.0f); // Not sure what this does
                //Function.Call(Hash.SET_DISPATCH_COPS_FOR_PLAYER, Game.Player, false); // Prevent game from adding police to chase

                // Pit and Manage Traffic
                foreach (Vehicle copVehicle in CopVehicles)
                {
                    if (copVehicle.IsOnAllWheels && (copVehicle.Model.IsCar || copVehicle.Model.IsBike || copVehicle.Model.IsBoat))
                    {
                        // PIT
                        if (Config.Options.BetterChases.RequirePITAuthorization && IsWanted && Helpers.IsValid(character.CurrentVehicle) && (!ActiveChase.PITAuthorized || Helpers.IsPopulatedArea(character.Position + character.Velocity, 40f)))
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
                        if (Config.Options.BetterChases.CopsManageTraffic && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, copVehicle, true).Y > 5f)
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
                        if (Config.Options.BetterChases.CopsManageTraffic && Function.Call<Vector3>(Hash.GET_ENTITY_SPEED_VECTOR, copVehicle, true).Y > 5f)
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