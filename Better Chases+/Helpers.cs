using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Linq;

namespace BetterChasesPlus
{
    public class Helpers
    {
        private static int wantedLevel = 0;
        private static int maxWantedLevel = 5;

        public static int WantedLevel
        {
            get
            {
                if (Config.Options.BetterChases.WantedLevelControl != "Full")
                {
                    return Game.Player.WantedLevel;
                }

                return wantedLevel;
            }
            set
            {
                if (Config.Options.BetterChases.WantedLevelControl != "Full")
                {
                    Game.Player.WantedLevel = value;
                }
                else
                {
                    if (value > maxWantedLevel)
                    {
                        MaxWantedLevel = value;
                    }

                    wantedLevel = value;
                    Game.Player.WantedLevel = value;
                }
            }
        }

        public static int MaxWantedLevel
        {
            get
            {
                return maxWantedLevel;
            }
            set
            {
                maxWantedLevel = value;
                Function.Call(Hash.SET_MAX_WANTED_LEVEL, value);
            }
        }

        public static int MinGroundUnits
        {
            get
            {
                int minUnits = 0;
                switch (wantedLevel)
                {
                    case 1:
                        minUnits = Config.Options.BetterChases.CopDispatch.OneStar.GroundMin;
                        break;
                    case 2:
                        minUnits = Config.Options.BetterChases.CopDispatch.TwoStar.GroundMin;
                        break;
                    case 3:
                        minUnits = Config.Options.BetterChases.CopDispatch.ThreeStar.GroundMin;
                        break;
                    case 4:
                        minUnits = Config.Options.BetterChases.CopDispatch.FourStar.GroundMin;
                        break;
                    case 5:
                        minUnits = Config.Options.BetterChases.CopDispatch.FiveStar.GroundMin;
                        break;
                }

                return minUnits;
            }
        }

        public static int MaxGroundUnits
        {
            get
            {
                int maxUnits = 0;
                switch (wantedLevel)
                {
                    case 1:
                        maxUnits = Config.Options.BetterChases.CopDispatch.OneStar.GroundMax;
                        break;
                    case 2:
                        maxUnits = Config.Options.BetterChases.CopDispatch.TwoStar.GroundMax;
                        break;
                    case 3:
                        maxUnits = Config.Options.BetterChases.CopDispatch.ThreeStar.GroundMax;
                        break;
                    case 4:
                        maxUnits = Config.Options.BetterChases.CopDispatch.FourStar.GroundMax;
                        break;
                    case 5:
                        maxUnits = Config.Options.BetterChases.CopDispatch.FiveStar.GroundMax;
                        break;
                }

                return maxUnits;
            }
        }

        public static int MinAirUnits
        {
            get
            {
                int minUnits = 0;
                switch (wantedLevel)
                {
                    case 1:
                        minUnits = Config.Options.BetterChases.CopDispatch.OneStar.AirMin;
                        break;
                    case 2:
                        minUnits = Config.Options.BetterChases.CopDispatch.TwoStar.AirMin;
                        break;
                    case 3:
                        minUnits = Config.Options.BetterChases.CopDispatch.ThreeStar.AirMin;
                        break;
                    case 4:
                        minUnits = Config.Options.BetterChases.CopDispatch.FourStar.AirMin;
                        break;
                    case 5:
                        minUnits = Config.Options.BetterChases.CopDispatch.FiveStar.AirMin;
                        break;
                }

                return minUnits;
            }
        }

        public static bool IsArmed
        {
            get
            {
                return Game.Player.Character.Weapons.Current != null && !NotWeapons.Contains(Game.Player.Character.Weapons.Current.Hash) && Function.Call<bool>(Hash.IS_PED_ARMED, Game.Player.Character, 7);
            }
        }

        public static int[] PedCopTypes = { 6, 27 }; // 6 Cop, 27 SWAT, 29 Army

        //public static List<PedHash> PedCopHashes = new List<PedHash>()
        //{
        //    PedHash.Cop01SFY,
        //    PedHash.Cop01SMY,
        //    PedHash.Hwaycop01SMY,
        //    PedHash.Sheriff01SFY,
        //    PedHash.Sheriff01SMY,
        //    PedHash.Snowcop01SMM,
        //    PedHash.Swat01SMY,
        //    PedHash.CiaSec01SMM,
        //    PedHash.FibSec01,
        //    PedHash.FibSec01SMM
        //};

        public static WeaponHash[] NotWeapons =
        {
            WeaponHash.Unarmed,
            WeaponHash.Bottle,
            WeaponHash.Ball,
            WeaponHash.Crowbar,
            WeaponHash.FireExtinguisher,
            WeaponHash.Firework,
            WeaponHash.Flare,
            WeaponHash.FlareGun,
            WeaponHash.Flashlight,
            WeaponHash.GolfClub,
            WeaponHash.Hammer,
            WeaponHash.KnuckleDuster,
            WeaponHash.Nightstick,
            WeaponHash.NightVision,
            WeaponHash.Parachute,
            WeaponHash.PetrolCan,
            WeaponHash.PoolCue,
            WeaponHash.SmokeGrenade,
            WeaponHash.Snowball,
            WeaponHash.Wrench
        };

        public static bool IsValid(Entity entity)
        {
            return entity != null && entity.Exists();
        }

        public static bool IsThisEntityAheadThatEntity(Entity ent1, Entity ent2)
        {
            Vector3 pos = ent1.Position;

            return Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_GIVEN_WORLD_COORDS, ent2, pos.X, pos.Y, pos.Z).Y > 0f;
        }

        public static bool IsAnyTireBlown(Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 0, false) || Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 1, false) || Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 2, false) || Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 3, false) || Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 4, false) || Function.Call<bool>(Hash.IS_VEHICLE_TYRE_BURST, vehicle, 5, false);
            //return vehicle.IsTireBurst(0) || vehicle.IsTireBurst(1) || vehicle.IsTireBurst(2) || vehicle.IsTireBurst(3) || vehicle.IsTireBurst(4) || vehicle.IsTireBurst(5) || vehicle.IsTireBurst(6);
        }

        /// <summary>Uses IS_COP_PED_IN_AREA_3D to determine if a cop ped is in the passed area within the passed radius.</summary>
        /// <param name="pos">A GTAV Vector3 Position</param>
        /// <param name="radius">Radius to check</param>
        /// <returns>Returns true if cop is in area</returns>
        public static bool IsCopNearby(Vector3 pos, float radius)
        {
            return Function.Call<bool>(Hash.IS_COP_PED_IN_AREA_3D, pos.X + radius, pos.Y + radius, pos.Z + radius, pos.X - radius, pos.Y - radius, pos.Z - radius);
        }

        public static bool IsPopulatedArea(Vector3 position, float range, int threshold = 8)
        {
            int population = 0;
            bool populated = false;

            Ped[] peds = World.GetNearbyPeds(position, range);
            foreach (Ped ped in peds)
            {
                if (population < threshold)
                {
                    if (!ped.IsPlayer && ped.IsHuman && ped.IsAlive && !PedCopTypes.Contains(Function.Call<int>(Hash.GET_PED_TYPE, ped))) population++;
                }
                else
                {
                    populated = true;
                    break;
                }
            }

            return populated;
        }

        public static bool IsSilenced(Weapon weapon)
        {
            bool silenced = false;

            //if (weapon.IsComponentActive(WeaponComponent.AtArSupp) || weapon.IsComponentActive(WeaponComponent.AtArSupp02) || weapon.IsComponentActive(WeaponComponent.AtPiSupp) || weapon.IsComponentActive(WeaponComponent.AtPiSupp02) || weapon.IsComponentActive(WeaponComponent.AtSrSupp) || weapon.IsComponentActive(WeaponComponent.AtSrSupp03))
            //{
            //    silenced = true;
            //}

            return silenced;
        }

        public static bool IsWearingHelmet(Ped ped)
        {
            if (Function.Call<bool>(Hash.IS_PED_WEARING_HELMET, ped)) return true;

            return false;
        }

        public static bool IsMasked(Ped ped)
        {
            List<int> MasksFranklin = new List<int> { 8, 9, 10, 11, 12, 13, 14 };
            List<int> MasksMichael = new List<int> { 14, 15, 16, 17, 18, 19, 20 };
            List<int> MasksTrevor = new List<int> { 14, 15, 16, 17, 18, 19, 20 };

            Model pedmodel = ped.Model;
            if ((pedmodel == "mp_f_freemode_01" || pedmodel == "mp_m_freemode_01") && Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, Game.Player.Character, 1) != 0) return true;
            if (pedmodel == PedHash.Franklin && MasksFranklin.Contains(Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped, 0))) return true;
            if (pedmodel == PedHash.Michael && MasksMichael.Contains(Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped, 0))) return true;
            if (pedmodel == PedHash.Trevor && MasksTrevor.Contains(Function.Call<int>(Hash.GET_PED_PROP_INDEX, ped, 0))) return true;
            return false;
        }

        public static Model GetModel(int hash)
        {
            return new Model(hash);
        }
    }
}