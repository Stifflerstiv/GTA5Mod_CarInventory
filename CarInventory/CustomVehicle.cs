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
        public Dictionary<Weapon, Dictionary<int, List<WeaponComponent>>> VehicleInventory = new Dictionary<Weapon, Dictionary<int, List<WeaponComponent>>>() { };

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

                if (!VehicleInventory.ContainsKey(Game.Player.Character.Weapons.Current))
                {
                    VehicleInventory.Add(weap, new Dictionary<int, List<WeaponComponent>>() { [ammo] = GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current) });
                }

                else
                {
                    int ammos = VehicleInventory[weap].ElementAt(0).Key + ammo;
                    VehicleInventory[weap] = new Dictionary<int, List<WeaponComponent>>() { [ammos] = GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current) };
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
                Game.Player.Character.Weapons.Give(VehicleInventory.ElementAt(pos).Key.Hash, VehicleInventory.ElementAt(pos).Value.ElementAt(0).Key, true, true);                

                List<WeaponComponent> GetWeaponComponentsList = VehicleInventory.ElementAt(pos).Value.ElementAt(0).Value;
                UI.Notify(GetWeaponComponentsList.Count.ToString());

                foreach (WeaponComponent wea_comp in GetWeaponComponentsList)
                {
                    Game.Player.Character.Weapons.Current.SetComponent(wea_comp, true);
                }                              
                               
                VehicleInventory.Remove(VehicleInventory.ElementAt(pos).Key);
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
