using GTA;
using System.Collections.Generic;
using System.Linq;

namespace CarInventory
{
    class CustomVehicle : Script
    {
        public Vehicle CustomModel { get; set; }
        public Dictionary<Weapon, int> VehicleInventory = new Dictionary<Weapon, int>() { };


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
                    VehicleInventory.Add(weap, ammo);                
                }

                else
                {               
                    VehicleInventory[weap] += ammo;                    
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
                Game.Player.Character.Weapons.Give(VehicleInventory.ElementAt(pos).Key.Hash, VehicleInventory.ElementAt(pos).Value, true, true);
                VehicleInventory.Remove(VehicleInventory.ElementAt(pos).Key);
            }

            catch { }
        }
    }
}
