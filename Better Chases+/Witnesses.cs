using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterChasesPlus
{
    public class Witnesses : Script
    {
        private static readonly bool ShowBlips = false;
        private static readonly int MaxCitizenWitnesses = 0;
        private static readonly int MaxCopWitnesses = 6;
        private static float MaxWitnessDistance = 0;
        private static float vehicleSize = 0;

        public static List<Witness> Citizens = new List<Witness>();
        public static List<Witness> Cops = new List<Witness>();

        public class Witness
        {
            public Ped Ped;
            public float distance;
            public double Spotted = 0;
            public double VehicleSpotted = 0;
            public double Recognition = 0;
            public double VehicleRecognition = 0;
            public Vector3 LastKnownLocation;
        }

        public Witnesses()
        {
            Tick += OnTick;
            Aborted += Shutdown;

            Interval = 200;
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!ArrestWarrants.CopSearch && !BetterChases.CopSearch)
            {
                Citizens.Clear();
                Cops.Clear();
                return;
            }

            Ped Character = Game.Player.Character;
            Vehicle CurrentVehicle = Game.Player.Character.CurrentVehicle;
            int WantedLevel = Game.Player.WantedLevel;

            vehicleSize = 0;
            MaxWitnessDistance = 60f;
            if (Helpers.IsValid(CurrentVehicle))
            {
                // Calculate vehicle size
                //Vector3 dimensions = CurrentVehicle.Model.Dimensions;
                vehicleSize = 500; // dimensions.X + dimensions.Y + dimensions.Z;

                // Calculate witness distance
                MaxWitnessDistance = Math.Min(60f, vehicleSize * MaxWitnessDistance / 4);
            }

            //System.Diagnostics.Stopwatch Stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //Stopwatch.Start();

            // Citizens
            for (int i = Citizens.Count - 1; i >= 0; i--)
            {
                Witness citizen = Citizens[i];

                if (!citizen.Ped.IsAlive)
                {
                    //citizen.Ped.CurrentBlip.Remove();
                    Citizens.Remove(citizen);
                    continue;
                }

                RaycastResult ray = new RaycastResult();
                if (Function.Call<bool>(Hash.IS_PED_FACING_PED, citizen.Ped, Character, MaxWitnessDistance))
                {
                    if (Helpers.IsValid(citizen.Ped.CurrentVehicle))
                    {
                        if (citizen.Ped.CurrentVehicle.Model.IsHelicopter)
                        {
                            ray = World.Raycast(citizen.Ped.CurrentVehicle.Position + new Vector3(0, 0, -10f), Character.Position, IntersectFlags.Everything);
                        }
                        else
                        {
                            ray = World.Raycast(citizen.Ped.CurrentVehicle.Position + new Vector3(0, 0, 10 * 1.1f), Character.Position, IntersectFlags.Everything); // citizen.Ped.CurrentVehicle.Model.GetDimensions().Z * 1.1f
                        }
                    }
                    else
                    {
                        ray = World.Raycast(citizen.Ped.Position + new Vector3(0, 0, 5), Character.Position, IntersectFlags.Everything); // citizen.Ped.Model.GetDimensions().Z
                    }
                }

                if (ray.HitEntity != null && ((Helpers.IsValid(Character.CurrentVehicle) && ray.HitEntity.Handle == Character.CurrentVehicle.Handle) || ray.HitEntity.Handle == Character.Handle))
                {
                    GetRecognitionPercent(citizen.Ped, Character, citizen);
                }
                else
                {
                    citizen.Recognition = 0;
                    citizen.VehicleRecognition = 0;
                }

                CalcSpotted(citizen);

                if (Math.Max(citizen.Spotted, citizen.VehicleSpotted) <= 0)
                {
                    //citizen.Ped.CurrentBlip.Remove();
                    Citizens.Remove(citizen);
                }
            }

            // Cops
            for (int i = Cops.Count - 1; i >= 0; i--)
            {
                Witness cop = Cops[i];

                if (!cop.Ped.IsAlive)
                {
                    //cop.Ped.CurrentBlip.Remove();
                    Cops.Remove(cop);
                    continue;
                }

                RaycastResult ray = new RaycastResult();
                if (Function.Call<bool>(Hash.IS_PED_FACING_PED, cop.Ped, Character, MaxWitnessDistance))
                {
                    if (Helpers.IsValid(cop.Ped.CurrentVehicle))
                    {
                        if (cop.Ped.CurrentVehicle.Model.IsHelicopter)
                        {
                            ray = World.Raycast(cop.Ped.CurrentVehicle.Position + new Vector3(0, 0, -10f), Character.Position, IntersectFlags.Everything);
                        }
                        else
                        {
                            ray = World.Raycast(cop.Ped.CurrentVehicle.Position + new Vector3(0, 0, 10 * 1.1f), Character.Position, IntersectFlags.Everything); // cop.Ped.CurrentVehicle.Model.GetDimensions().Z * 1.1f
                        }
                    }
                    else
                    {
                        ray = World.Raycast(cop.Ped.Position + new Vector3(0, 0, 5), Character.Position, IntersectFlags.Everything); // citizen.Ped.Model.GetDimensions().Z
                    }
                }

                if (ray.HitEntity != null && ((Helpers.IsValid(Character.CurrentVehicle) && ray.HitEntity.Handle == Character.CurrentVehicle.Handle) || ray.HitEntity.Handle == Character.Handle))
                {
                    GetRecognitionPercent(cop.Ped, Character, cop);
                }
                else
                {
                    cop.Recognition = 0;
                    cop.VehicleRecognition = 0;
                }

                CalcSpotted(cop);

                if (Math.Max(cop.Spotted, cop.VehicleSpotted) <= 0)
                {
                    //cop.Ped.CurrentBlip.Remove();
                    Cops.Remove(cop);
                }
            }

            List<Ped> NearbyPeds = new List<Ped>();
            if (Citizens.Count < MaxCitizenWitnesses || Cops.Count < MaxCopWitnesses)
            {
                NearbyPeds = new List<Ped>(World.GetNearbyPeds(Character, MaxWitnessDistance));
                NearbyPeds = NearbyPeds.OrderByDescending(Ped => (Character.Position - Ped.Position).Length()).ToList();

                foreach (Ped ped in NearbyPeds)
                {
                    // We have enough witnesses now
                    if (Citizens.Count >= MaxCitizenWitnesses && Cops.Count >= MaxCopWitnesses)
                    {
                        break;
                    }

                    if (!ped.IsAlive || !ped.IsHuman)
                    {
                        continue;
                    }

                    // Don't add a ped we are already tracking
                    if (Citizens.FindIndex(Citizen => Citizen.Ped.Handle == ped.Handle) > -1 || Cops.FindIndex(Cop => Cop.Ped.Handle == ped.Handle) > -1)
                    {
                        continue;
                    }

                    bool IsCop = Helpers.PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped));

                    if ((!IsCop && Citizens.Count >= MaxCitizenWitnesses) || (IsCop && Cops.Count >= MaxCopWitnesses))
                    {
                        continue;
                    }

                    // Only track cops right now
                    if (!IsCop)
                    {
                        continue;
                    }

                    RaycastResult ray = new RaycastResult();
                    if (Function.Call<bool>(Hash.IS_PED_FACING_PED, ped, Character, MaxWitnessDistance))
                    {
                        if (Helpers.IsValid(ped.CurrentVehicle))
                        {
                            if (ped.CurrentVehicle.Model.IsHelicopter)
                            {
                                ray = World.Raycast(ped.CurrentVehicle.Position + new Vector3(0, 0, -10f), Character.Position, IntersectFlags.Everything);
                            }
                            else
                            {
                                ray = World.Raycast(ped.CurrentVehicle.Position + new Vector3(0, 0, 10 * 1.1f), Character.Position, IntersectFlags.Everything); // cop.Ped.CurrentVehicle.Model.GetDimensions().Z * 1.1f
                            }
                        }
                        else
                        {
                            ray = World.Raycast(ped.Position + new Vector3(0, 0, 5), Character.Position, IntersectFlags.Everything); // ped.Model.GetDimensions().Z
                        }
                    }

                    if (ray.HitEntity != null && ((Helpers.IsValid(Character.CurrentVehicle) && ray.HitEntity.Handle == Character.CurrentVehicle.Handle) || ray.HitEntity.Handle == Character.Handle))
                    {
                        if (IsCop)
                        {
                            Witness NewWitness = new Witness { Ped = ped };
                            Cops.Add(NewWitness);
                            if (ShowBlips)
                            {
                                ped.AddBlip();
                                //ped.CurrentBlip.Color = BlipColor.Blue;
                            }
                        }
                        else
                        {
                            Witness NewWitness = new Witness { Ped = ped };
                            Citizens.Add(NewWitness);
                            if (ShowBlips)
                            {
                                ped.AddBlip();
                                //ped.CurrentBlip.Color = BlipColor.White;
                            }
                        }
                    }
                }
            }
            //Stopwatch.Stop();
            //UI.ShowSubtitle("Witnesses: " + Witnesses.Citizens.Count + " ~b~" + Witnesses.Cops.Count + "~w~ Peds: " + NearbyPeds.Count + " CPU: " + Stopwatch.Elapsed);
        }

        public static double GetMaxRecognition(List<Witness> witnesses, bool vehicle = false)
        {
            double recognition = 0;
            foreach (Witness witness in witnesses)
            {
                if (vehicle)
                {
                    recognition = Math.Max(recognition, witness.VehicleRecognition);
                }
                else
                {
                    recognition = Math.Max(recognition, witness.Recognition);
                }
            }

            return recognition;
        }

        private void GetRecognitionPercent(Ped ped, Ped ped2, Witness witness)
        {
            double distanceRecognitionPercent;
            double speedRecognitionPercent;

            double relativeSpeed = Math.Round(Math.Abs(ped2.Velocity.Length() - ped.Velocity.Length()), 0);
            float distance = (ped2.Position - ped.Position).Length();

            witness.distance = distance;
            witness.Recognition = 0;
            witness.VehicleRecognition = 0;

            if (Helpers.IsValid(ped2.CurrentVehicle))
            {
                distanceRecognitionPercent = distance <= MaxWitnessDistance / 6 ? 100.0 : distance < MaxWitnessDistance ? Math.Round(Math.Abs((((distance - 30) / (MaxWitnessDistance - (MaxWitnessDistance / 6))) - 1) * 100), 0) : 0; // 30-100 for car, 15-60 for ped
                speedRecognitionPercent = relativeSpeed <= 8 ? 100.0 : relativeSpeed < 50 ? Math.Round(Math.Abs((((relativeSpeed - 8) / 42) - 1) * 100), 0) : 0; // 8-50 for car, 4-15 for ped
                witness.VehicleRecognition = Math.Min(distanceRecognitionPercent, speedRecognitionPercent);

                if (CanBeIdentified(ped2) || distance <= 3f)
                {
                    distanceRecognitionPercent = distance <= 8 ? 100.0 : distance < 30 ? Math.Round(Math.Abs((((distance - 8) / 22) - 1) * 100), 0) : 0; // 30-100 for car, 15-60 for ped
                    speedRecognitionPercent = relativeSpeed <= 4 ? 100.0 : relativeSpeed < 15 ? Math.Round(Math.Abs((((relativeSpeed - 4) / 11) - 1) * 100), 0) : 0; // 8-50 for car, 4-15 for ped
                    witness.Recognition = Math.Min(distanceRecognitionPercent, speedRecognitionPercent);
                }
            }
            else if (CanBeIdentified(ped2))
            {
                distanceRecognitionPercent = distance <= 15 ? 100.0 : distance < 60 ? Math.Round(Math.Abs((((distance - 15) / 45) - 1) * 100), 0) : 0; // 30-100 for car, 15-60 for ped
                speedRecognitionPercent = relativeSpeed <= 4 ? 100.0 : relativeSpeed < 15 ? Math.Round(Math.Abs((((relativeSpeed - 4) / 11) - 1) * 100), 0) : 0; // 8-50 for car, 4-15 for ped
                witness.Recognition = Math.Min(distanceRecognitionPercent, speedRecognitionPercent);
            }

            //Renderer.DisplayHelpTextThisFrame("CanBeIdentified " + CanBeIdentified(ped2));
            //Renderer.DisplayHelpTextThisFrame("~y~Distance: " + distance + " " + distanceRecognitionPercent + "%~n~Relative Speed: " + relativeSpeed + " " + speedRecognitionPercent + "%~n~Recognition: " + witness.Recognition + "%~n~Vehical: " + witness.VehicleRecognition + "%");
        }

        private void CalcSpotted(Witness witness)
        {
            float spottedMultiplier = 1f;

            if (Config.Options.ArrestWarrants.Enabled)
            {
                spottedMultiplier = Config.Options.ArrestWarrants.SpotSpeed / 100f;
            }

            // Ped Spotted
            if (ArrestWarrants.IsWarrantSearchActive)
            {
                spottedMultiplier = ArrestWarrants.PedMatch * spottedMultiplier;
            }

            if (witness.Recognition >= 0 && witness.Recognition <= 25)
            {
                witness.Spotted += -1 * spottedMultiplier;
            }
            else if (witness.Recognition > 25 && witness.Recognition < 50)
            {
                witness.Spotted += -0.25 * spottedMultiplier;
            }
            else if (witness.Recognition >= 50 && witness.Recognition < 75)
            {
                if (witness.Spotted < 20) witness.Spotted += 0.25 * spottedMultiplier;
            }
            else if (witness.Recognition >= 75 && witness.Recognition < 100)
            {
                witness.Spotted += 0.5 * spottedMultiplier;
            }
            else if (witness.Recognition >= 100)
            {
                witness.Spotted += 5 * spottedMultiplier;
            }

            if (witness.Spotted > 100) witness.Spotted = 100;
            if (witness.Spotted < 0) witness.Spotted = 0;

            //// RESET
            spottedMultiplier = 1f;

            if (Config.Options.ArrestWarrants.Enabled)
            {
                spottedMultiplier = Config.Options.ArrestWarrants.SpotSpeed / 100f;
            }

            // Vehicle Spotted
            if (ArrestWarrants.IsWarrantSearchActive)
            {
                spottedMultiplier = ArrestWarrants.VehicleMatch * spottedMultiplier;
            }

            if (witness.VehicleRecognition >= 0 && witness.VehicleRecognition <= 25)
            {
                witness.VehicleSpotted += -1 * spottedMultiplier;
            }
            else if (witness.VehicleRecognition > 25 && witness.VehicleRecognition < 50)
            {
                witness.VehicleSpotted += -0.25 * spottedMultiplier;
            }
            else if (witness.VehicleRecognition >= 50 && witness.VehicleRecognition < 75)
            {
                if (witness.VehicleSpotted < 20) witness.VehicleSpotted += 0.25 * spottedMultiplier;
            }
            else if (witness.VehicleRecognition >= 75 && witness.VehicleRecognition < 100)
            {
                witness.VehicleSpotted += 0.5 * spottedMultiplier;
            }
            else if (witness.VehicleRecognition >= 100)
            {
                witness.VehicleSpotted += 5 * spottedMultiplier;
            }

            if (witness.VehicleSpotted > 100) witness.VehicleSpotted = 100;
            if (witness.VehicleSpotted < 0) witness.VehicleSpotted = 0;
        }

        private bool CanBeIdentified(Ped ped)
        {
            if (Helpers.IsValid(ped.CurrentVehicle))
            {
                Vehicle vehicle = ped.CurrentVehicle;

                if (vehicle.Model == VehicleHash.Rhino) return false;

                if (vehicle.Model.IsBike || vehicle.Model.IsBicycle || vehicle.Model.IsQuadBike || (vehicle.HasRoof && vehicle.RoofState == VehicleRoofState.Opened)) return true;

                if (vehicle.Windows.AllWindowsIntact && Function.Call<VehicleWindowTint>(Hash.GET_VEHICLE_WINDOW_TINT, vehicle) != VehicleWindowTint.None && Function.Call<VehicleWindowTint>(Hash.GET_VEHICLE_WINDOW_TINT, vehicle) != VehicleWindowTint.Stock && Function.Call<VehicleWindowTint>(Hash.GET_VEHICLE_WINDOW_TINT, vehicle) != (VehicleWindowTint)(-1)) return false;

                if (Game.IsControlPressed(Control.VehicleDuck)) return false;
            }

            return true;
        }

        public static Vector3 GetDimensions(Model model)
        {
            return Vector3.Subtract(model.Dimensions.frontTopRight, model.Dimensions.rearBottomLeft);
        }

        public static float GetVolume(Vector3 dimensions)
        {
            return dimensions.X * dimensions.Y * dimensions.Z;
        }

        private void Shutdown(object sender, EventArgs e)
        {
            //if (ShowBlips)
            //{
            //    foreach (Ped ped in World.GetAllPeds())
            //    {
            //        if (ped.CurrentBlip.Exists())
            //        {
            //            ped.CurrentBlip.Remove();
            //        }
            //    }
            //}
        }
    }
}