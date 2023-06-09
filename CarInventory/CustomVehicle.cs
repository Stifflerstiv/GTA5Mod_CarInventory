﻿using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarInventory
{
    class CustomVehicle : Script
    {
        public Vehicle CustomModel { get; set; }

        //public List<CustomWeapon> CustomVehicleInventory = new List<CustomWeapon>() { };
        public CustomWeapon[,] CustomVehicleInventory;

        public int[] cursorPos = new int[2] { 0, 0 };

        public CustomVehicle(Vehicle vehicle, int[] invSize)
        {
            CustomModel = vehicle;

            CustomVehicleInventory = new CustomWeapon[invSize[0], invSize[1]];

            for (int i = 0; i < invSize[0]; i++)
            {
                for (int j = 0; j < invSize[1]; j++)
                {
                    CustomVehicleInventory[i, j] = null;
                }
            }

        }

        public void AddToVehicleInventory(Weapon weap, int ammo)
        {
            int rows = CustomVehicleInventory.GetUpperBound(0) + 1;
            int columns = CustomVehicleInventory.Length / rows;

            if (!HasWeaponInInventory(weap))
            {
                CustomWeapon custWeap = new CustomWeapon(weap, GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current), Game.Player.Character.Weapons.Current.Tint, ammo);
                Game.Player.Character.Weapons.Current.Ammo = 0;                

                if (CustomVehicleInventory[cursorPos[0], cursorPos[1]] != null)
                {
                    RemoveFromVehicleInventory();
                }

                CustomVehicleInventory[cursorPos[0], cursorPos[1]] = custWeap;
                Game.Player.Character.Weapons.Remove(weap.Hash);

                Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
            }

            else
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (CustomVehicleInventory[i, j].CustomWeaponModel == weap)
                        {
                            CustomVehicleInventory[i, j].CustomWeaponAmmo += ammo;
                            Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                            Game.Player.Character.Weapons.Current.Ammo = 0;
                            Game.Player.Character.Weapons.Remove(weap.Hash);
                        }
                    }
                }
            }

            Game.PlaySound("Grab_Parachute", "BASEJUMPS_SOUNDS");
        }

        public bool HasWeaponInInventory(Weapon weap)
        {
            int rows = CustomVehicleInventory.GetLength(0);
            int columns = CustomVehicleInventory.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (CustomVehicleInventory[i, j] != null)
                    {
                        if (CustomVehicleInventory[i, j].CustomWeaponModel == weap)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool IsEmptyInventory() 
        {
            int rows = CustomVehicleInventory.GetUpperBound(0) + 1;
            int columns = CustomVehicleInventory.Length / rows;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (CustomVehicleInventory[i, j] != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void RemoveFromVehicleInventory()
        {
            if (CustomVehicleInventory[cursorPos[0], cursorPos[1]] != null)
            {
                Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                Game.Player.Character.Weapons.Give(CustomVehicleInventory[cursorPos[0], cursorPos[1]].CustomWeaponModel.Hash, CustomVehicleInventory[cursorPos[0], cursorPos[1]].CustomWeaponAmmo, true, true);
                foreach (WeaponComponent wea_comp in CustomVehicleInventory[cursorPos[0], cursorPos[1]].CustomWeaponComponentList)
                {
                    Game.Player.Character.Weapons.Current.SetComponent(wea_comp, true);
                }
                //set weapo tint
                Game.Player.Character.Weapons.Current.Tint = CustomVehicleInventory[cursorPos[0], cursorPos[1]].CustomWeaponTint;

                CustomVehicleInventory[cursorPos[0], cursorPos[1]] = null;
                Game.PlaySound("Grab_Parachute", "BASEJUMPS_SOUNDS");
            }
        }

        public List<WeaponComponent> GetAllWeaponComponentsList(Weapon weap)
        {
            List<WeaponComponent> weapon_components = new List<WeaponComponent>() { };

            var allValues = (WeaponComponent[])Enum.GetValues(typeof(WeaponComponent));

            foreach (WeaponComponent comp in allValues)
            {
                if (weap.IsComponentActive(comp))
                    weapon_components.Add(comp);
            }

            return weapon_components;
        }

        // inventory size by vehicle class
        public static Dictionary<VehicleClass, int[]> invSizeByRequiredVehicleClass = new Dictionary<VehicleClass, int[]>()
        {
            [VehicleClass.Sports] = new int[] { 3, 3 },
            [VehicleClass.SportsClassics] = new int[] { 3, 3 },
            [VehicleClass.Sedans] = new int[] { 4, 3 },
            [VehicleClass.Super] = new int[] { 3, 2 },
            [VehicleClass.Coupes] = new int[] { 4, 3 },
            [VehicleClass.Compacts] = new int[] { 2, 2 },
            [VehicleClass.Muscle] = new int[] { 4, 3 },
            [VehicleClass.SUVs] = new int[] { 4, 4 },
            [VehicleClass.Emergency] = new int[] { 4, 3 },
            [VehicleClass.Vans] = new int[] { 4, 4 },
        };
    }
}
