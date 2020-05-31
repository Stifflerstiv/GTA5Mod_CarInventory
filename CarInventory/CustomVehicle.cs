using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CarInventory
{
    class CustomVehicle : Script
    {
        public Vehicle CustomModel { get; set; }

        public List<CustomWeapon> CustomVehicleInventory = new List<CustomWeapon>() { };

        public CustomVehicle(Vehicle vehicle) 
        {
            CustomModel = vehicle;
        }

        public void AddToVehicleInventory(Weapon weap, int ammo)
        {       
            if (CustomVehicleInventory.Count < 8)
            {
                // take item anim
                Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                
                if (!CustomVehicleInventory.Exists(cus => cus.CustomWeaponModel == weap))
                {
                    CustomVehicleInventory.Add(new CustomWeapon(weap, GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current), Game.Player.Character.Weapons.Current.Tint, ammo));
                }

                else
                {
                    CustomVehicleInventory.Find(cus => cus.CustomWeaponModel == weap).CustomWeaponAmmo += ammo;
                }

                Game.Player.Character.Weapons.Current.Ammo = 0;
                Game.Player.Character.Weapons.Remove(weap.Hash);
            }
        }

        public void RemoveFromVehicleInventory(int pos)
        {
            try
            {
                Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                Game.Player.Character.Weapons.Give(CustomVehicleInventory.ElementAt(pos).CustomWeaponModel.Hash, CustomVehicleInventory.ElementAt(pos).CustomWeaponAmmo, true, true);

                foreach (WeaponComponent wea_comp in CustomVehicleInventory.ElementAt(pos).CustomWeaponComponentList)
                {
                    Game.Player.Character.Weapons.Current.SetComponent(wea_comp, true);
                }

                //set weapo tint
                Game.Player.Character.Weapons.Current.Tint = CustomVehicleInventory.ElementAt(pos).CustomWeaponTint;

                CustomVehicleInventory.Remove(CustomVehicleInventory.ElementAt(pos));
            }

            catch { }
        }

        public List<WeaponComponent> GetAllWeaponComponentsList(Weapon weap)
        {
            List<WeaponComponent> weapon_components = new List<WeaponComponent>() { };

            var allValues = (WeaponComponent[]) Enum.GetValues(typeof(WeaponComponent));

            foreach (WeaponComponent comp in allValues)
            {
                if (weap.IsComponentActive(comp))
                    weapon_components.Add(comp);
            }

            return weapon_components;
        }
    }
}
