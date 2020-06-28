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

        public static readonly Dictionary<WeaponHash, string> WeaponsIconsDict = new Dictionary<WeaponHash, string>()
        {
            [WeaponHash.SniperRifle] = "weapon_sniper",
            [WeaponHash.FireExtinguisher] = "weapon_thrown_bz_gas",
            [WeaponHash.CompactGrenadeLauncher] = "weapon_compact_grenade_launcher",
            [WeaponHash.Snowball] = "weapon_ball",
            [WeaponHash.VintagePistol] = "weapon_vintagepistol",
            [WeaponHash.CombatPDW] = "weapon_combatpdw",
            [WeaponHash.HeavySniperMk2] = "weapon_sniper_heavy",
            [WeaponHash.HeavySniper] = "weapon_sniper_heavy",
            [WeaponHash.SweeperShotgun] = "weapon_automatic_shotgun",
            [WeaponHash.MicroSMG] = "weapon_smg_micro",
            [WeaponHash.Wrench] = "weapon_wrench",
            [WeaponHash.Pistol] = "weapon_pistol",
            [WeaponHash.PumpShotgun] = "weapon_shotgun_pump",
            [WeaponHash.APPistol] = "weapon_pistol_ap",
            [WeaponHash.Ball] = "weapon_ball",
            [WeaponHash.Molotov] = "weapon_molotov",
            [WeaponHash.SMG] = "weapon_smg",
            [WeaponHash.StickyBomb] = "weapon_thrown_sticky",
            [WeaponHash.PetrolCan] = "weapon_jerry_can",
            [WeaponHash.StunGun] = "weapon_stungun",
            [WeaponHash.AssaultrifleMk2] = "weapon_rifle_assault",
            [WeaponHash.HeavyShotgun] = "weapon_shotgun_assault",
            [WeaponHash.Minigun] = "weapon_heavy_minigun",
            [WeaponHash.GolfClub] = "weapon_golfclub",
            [WeaponHash.UnholyHellbringer] = "weapon_programmable_ar",
            [WeaponHash.FlareGun] = "weapon_flare_gun",
            [WeaponHash.Flare] = "weapon_flare",
            [WeaponHash.GrenadeLauncherSmoke] = "weapon_heavy_grenade_launcher",
            [WeaponHash.Hammer] = "weapon_hammer",
            [WeaponHash.PumpShotgunMk2] = "weapon_shotgun_pump",
            [WeaponHash.CombatPistol] = "weapon_pistol_combat",
            [WeaponHash.Gusenberg] = "weapon_gusenberg",
            [WeaponHash.CompactRifle] = "weapon_compactrifle",
            [WeaponHash.HomingLauncher] = "weapon_hominglauncher",
            [WeaponHash.Nightstick] = "weapon_nightstick",
            [WeaponHash.MarksmanRifleMk2] = "weapon_marksmanrifle",
            [WeaponHash.Railgun] = "weapon_railgun",
            [WeaponHash.SawnOffShotgun] = "weapon_shotgun_sawnoff",
            [WeaponHash.SMGMk2] = "weapon_smg",
            [WeaponHash.BullpupRifle] = "weapon_bullpuprifle",
            [WeaponHash.Firework] = "weapon_firework",
            [WeaponHash.CombatMG] = "weapon_lmg_combat",
            [WeaponHash.CarbineRifle] = "weapon_rifle_carbine",
            [WeaponHash.Crowbar] = "weapon_crowbar",
            [WeaponHash.BullpupRifleMk2] = "weapon_bullpuprifle",
            [WeaponHash.SNSPistolMk2] = "weapon_snspistol",
            [WeaponHash.Flashlight] = "weapon_flashlight",
            [WeaponHash.Dagger] = "weapon_dagger",
            [WeaponHash.Grenade] = "weapon_thrown_grenade",
            [WeaponHash.PoolCue] = "weapon_pool_cue",
            [WeaponHash.Bat] = "weapon_bat",
            [WeaponHash.SpecialCarbineMk2] = "weapon_rifle_carbine",
            [WeaponHash.DoubleActionRevolver] = "weapon_revolver",
            [WeaponHash.Pistol50] = "weapon_pistol_50",
            [WeaponHash.Knife] = "weapon_knife",
            [WeaponHash.MG] = "weapon_assault_mg",
            [WeaponHash.BullpupShotgun] = "weapon_shotgun_bullpup",
            [WeaponHash.BZGas] = "weapon_thrown_bz_gas",
            [WeaponHash.Unarmed] = "weapon_unarmed",
            [WeaponHash.GrenadeLauncher] = "weapon_heavy_grenade_launcher",
            [WeaponHash.NightVision] = "weapon_thermalcharge",
            [WeaponHash.Musket] = "weapon_musket",
            [WeaponHash.ProximityMine] = "weapon_thrown_sticky",
            [WeaponHash.AdvancedRifle] = "weapon_rifle_advanced",
            [WeaponHash.UpNAtomizer] = "weapon_stungun",
            [WeaponHash.RPG] = "weapon_heavy_rpg",
            [WeaponHash.Widowmaker] = "weapon_firework",
            [WeaponHash.PipeBomb] = "weapon_pipebomb",
            [WeaponHash.MiniSMG] = "weapon_mini_smg",
            [WeaponHash.SNSPistol] = "weapon_snspistol",
            [WeaponHash.PistolMk2] = "weapon_pistol",
            [WeaponHash.AssaultRifle] = "weapon_rifle_assault",
            [WeaponHash.SpecialCarbine] = "weapon_rifle_carbine",
            [WeaponHash.Revolver] = "weapon_revolver",
            [WeaponHash.MarksmanRifle] = "weapon_marksmanrifle",
            [WeaponHash.RevolverMk2] = "weapon_revolver",
            [WeaponHash.BattleAxe] = "weapon_battle_axe",
            [WeaponHash.HeavyPistol] = "weapon_heavypistol",
            [WeaponHash.KnuckleDuster] = "weapon_knuckle",
            [WeaponHash.MachinePistol] = "weapon_machinepistol",
            [WeaponHash.CombatMGMk2] = "weapon_lmg_combat",
            [WeaponHash.MarksmanPistol] = "weapon_marksmanpistol",
            [WeaponHash.Machete] = "weapon_machete",
            [WeaponHash.SwitchBlade] = "weapon_switchblade",
            [WeaponHash.AssaultShotgun] = "weapon_shotgun_assault",
            [WeaponHash.DoubleBarrelShotgun] = "weapon_shotgun_sawnoff",
            [WeaponHash.AssaultSMG] = "weapon_smg_assault",
            [WeaponHash.Hatchet] = "weapon_hatchet",
            [WeaponHash.Bottle] = "weapon_bottle",
            [WeaponHash.CarbineRifleMk2] = "weapon_rifle_carbine",
            [WeaponHash.Parachute] = "vehicle_weapon_player_buzzard",
            [WeaponHash.SmokeGrenade] = "weapon_thrown_bz_gas",
        };
        //weapons icons dict
        public CustomWeapon(Weapon weapon, List<WeaponComponent> weapCompList, WeaponTint customTint, int ammo) 
        {
            CustomWeaponModel = weapon;
            CustomWeaponComponentList = weapCompList;
            CustomWeaponTint = customTint;
            CustomWeaponAmmo = ammo;
        }
        public static void LoadWeaponIcons() 
        {
            // icons directory loading
            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "mpkillquota", false);

            // icons loading
            foreach (string texture in WeaponsIconsDict.Values)
            {
                Function.Call(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, texture);
            }
        }
        public string GetWeaponIconTextureName()
        {
            if (WeaponsIconsDict.ContainsKey(CustomWeaponModel.Hash))
                return WeaponsIconsDict[CustomWeaponModel.Hash];

            else
                return "vehicle_weapon_player_buzzard";
        }
    }
}
