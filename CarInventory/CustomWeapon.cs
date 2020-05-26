using GTA;
using GTA.Native;
using GTA.Math;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.IO;

namespace CarInventory
{
    class CustomWeapon : Script
    {
        public Weapon CustomWeaponModel { get; set; }
        public List<WeaponComponent> CustomWeaponComponentList { get; set; }
        public WeaponTint CustomWeaponTint { get; set; }
        public int CustomWeaponAmmo { get; set; }

        public CustomWeapon(Weapon weapon, List<WeaponComponent> weapCompList, WeaponTint customTint, int ammo) 
        {
            CustomWeaponModel = weapon;
            CustomWeaponComponentList = weapCompList;
            CustomWeaponTint = customTint;
            CustomWeaponAmmo = ammo;
        }
    }
}
