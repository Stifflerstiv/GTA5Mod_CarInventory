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
        //public Dictionary<Weapon, int> VehicleInventory = new Dictionary<Weapon, int>() { };

        // key = Weapon, value = key=ammo, value=[components]
        //public Dictionary<Weapon, Dictionary<int, List<WeaponComponent>>> VehicleInventory = new Dictionary<Weapon, Dictionary<int, List<WeaponComponent>>>() { };

        //public Dictionary<Weapon, Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>> VehicleInventory = new Dictionary<Weapon, Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>>() { };
        
        //public Dictionary<Dictionary<Weapon, Dictionary<List<WeaponComponent>, WeaponTint>>, int> VehicleInventory = new Dictionary<Dictionary<Weapon, Dictionary<List<WeaponComponent>, WeaponTint>>, int>() { };
        
        //    0             1             2
        // Weapon, list<WeapComponents>, Tint
        public List<CustomWeapon> VehicleInventory = new List<CustomWeapon>() { };

        public CustomVehicle(Vehicle vehicle) 
        {
            CustomModel = vehicle;
        }

        public void AddToVehicleInventory(Weapon weap, int ammo)
        {       
            if (VehicleInventory.Count < 8)
            {
                // take item anim
                Game.Player.Character.Task.PlayAnimation("anim@heists@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                
                if (!VehicleInventory.Exists(cus => cus.CustomWeaponModel == weap))
                {
                    VehicleInventory.Add(new CustomWeapon(weap, GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current), Game.Player.Character.Weapons.Current.Tint, ammo));
                }

                else
                {
                    VehicleInventory.Find(cus => cus.CustomWeaponModel == weap).CustomWeaponAmmo += ammo;
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
                Game.Player.Character.Weapons.Give(VehicleInventory.ElementAt(pos).CustomWeaponModel.Hash, VehicleInventory.ElementAt(pos).CustomWeaponAmmo, true, true);

                foreach (WeaponComponent wea_comp in VehicleInventory.ElementAt(pos).CustomWeaponComponentList)
                {
                    Game.Player.Character.Weapons.Current.SetComponent(wea_comp, true);
                }

                //set weapo tint
                Game.Player.Character.Weapons.Current.Tint = VehicleInventory.ElementAt(pos).CustomWeaponTint;

                VehicleInventory.Remove(VehicleInventory.ElementAt(pos));
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
