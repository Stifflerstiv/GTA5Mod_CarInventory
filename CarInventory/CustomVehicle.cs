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

        public Dictionary<Weapon, Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>> VehicleInventory = new Dictionary<Weapon, Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>>() { };

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
                    //VehicleInventory.Add(weap, new Dictionary<int, List<WeaponComponent>>() { [ammo] = GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current) });
                    VehicleInventory.Add(weap, new Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>() 
                                                { 
                                                    [ammo] = new Dictionary<List<WeaponComponent>, WeaponTint>() 
                                                            { 
                                                                [GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current)] = Game.Player.Character.Weapons.Current.Tint                    
                                                            } 
                                                });
                }

                else
                {
                    int ammos = VehicleInventory[weap].ElementAt(0).Key + ammo;
                    VehicleInventory[weap] = new Dictionary<int, Dictionary<List<WeaponComponent>, WeaponTint>>()
                    {
                        [ammos] = new Dictionary<List<WeaponComponent>, WeaponTint>()
                        {
                            [GetAllWeaponComponentsList(Game.Player.Character.Weapons.Current)] = Game.Player.Character.Weapons.Current.Tint
                        }
                    };
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

                // get all weapon components
                List<WeaponComponent> GetWeaponComponentsList = VehicleInventory.ElementAt(pos).Value.ElementAt(0).Value.ElementAt(0).Key;

                foreach (WeaponComponent wea_comp in GetWeaponComponentsList)
                {
                    Game.Player.Character.Weapons.Current.SetComponent(wea_comp, true);
                }

                //set weapo tint
                Game.Player.Character.Weapons.Current.Tint = VehicleInventory.ElementAt(pos).Value.ElementAt(0).Value.ElementAt(0).Value;

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

            /*
            var allTintValues = (WeaponTint[])Enum.GetValues(typeof(WeaponTint));

            foreach (WeaponTint tint in allTintValues)
            {
                if (weap.IsComponentActive(tint))
                    weapon_components.Add(tint);
            }
            */

            return weapon_components;
        }
    }
}
