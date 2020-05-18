using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Drawing;
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
                if (!VehicleInventory.ContainsKey(Game.Player.Character.Weapons.Current))
                {
                    // take item anim
                    Game.Player.Character.Task.PlayAnimation("anim@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                    VehicleInventory.Add(weap, ammo);
                    Game.Player.Character.Weapons.Current.Ammo = 0;
                    Game.Player.Character.Weapons.Remove(weap.Hash);                    
                }

                else
                {
                    // take item anim
                    Game.Player.Character.Task.PlayAnimation("anim@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                    VehicleInventory[weap] += ammo;
                    Game.Player.Character.Weapons.Current.Ammo = 0;
                    Game.Player.Character.Weapons.Remove(weap.Hash);
                }
            }
        }

        public void RemoveFromVehicleInventory(int pos)
        {
            try
            {
                Game.Player.Character.Task.PlayAnimation("anim@narcotics@trash", "drop_front", 8f, 1500, false, -80f);
                Game.Player.Character.Weapons.Give(VehicleInventory.ElementAt(pos).Key.Hash, VehicleInventory.ElementAt(pos).Value, false, true);
                VehicleInventory.Remove(VehicleInventory.ElementAt(pos).Key);
            }

            catch  
            { 

            }
        }
    }
}
