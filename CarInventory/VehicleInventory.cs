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
    public class VehicleInventory : Script
    {
        private readonly string modName = "CarInventory";
        private readonly string modVersion = "1.02";
        private readonly string modAuthor = "Stifflerstiv";
        private readonly bool debugMode = true;

        private Keys openTrunkKey = Keys.E;
        private Keys putWeaponKey = Keys.I;
        private Keys takeWeaponKey = Keys.O;
        readonly string CurrentFileDirectory = Environment.CurrentDirectory.ToString() + @"\Scripts\";

        private List<CustomVehicle> CustomVehiclesList = new List<CustomVehicle>() { };

        private Vector3 TrunkCoord;
        private Vehicle currentVehicle = null;

        private int cursorPos = 0;        

        private readonly Dictionary<WeaponHash, string> WeaponsIconsDict = new Dictionary<WeaponHash, string>() 
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
        public VehicleInventory()
        {
            IniInitialization();

            KeyDown += OnKeyDown;
            Tick += OnTick;

            Function.Call(Hash.REQUEST_STREAMED_TEXTURE_DICT, "mpkillquota", false);
            foreach (string texture in WeaponsIconsDict.Values)
            {
                Function.Call(Hash.HAS_STREAMED_TEXTURE_DICT_LOADED, texture);
            }
        }
        void OnTick(object sender, EventArgs e)
        {
            MainInventory();
            RemoveDontExistCustomVehicles();

            if (debugMode)
                DebugFunc();
        }

        void DebugFunc()
        {
            UI.ShowSubtitle($"key={openTrunkKey}, take={takeWeaponKey}, put={putWeaponKey}, count={CustomVehiclesList.Count}");
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == openTrunkKey && currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked && !currentVehicle.IsDoorBroken(VehicleDoor.Trunk))
            {
                if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    // playing close trunk anitation
                    Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "enter", 8f, 400, false, -1f);
                    currentVehicle.CloseDoor(VehicleDoor.Trunk, false);
                }

                else
                {
                    // playing open trunk anitation
                    Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "exit", 8f, 400, false, -1f);
                    currentVehicle.OpenDoor(VehicleDoor.Trunk, false, false);
                }
            }

            if (e.KeyCode == putWeaponKey && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (Game.Player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    if (!CustomVehiclesList.Contains(ContainsAVehicleCurrentCustomVehiclesList(currentVehicle)))
                    {
                        CustomVehiclesList.Add(new CustomVehicle(currentVehicle));
                    }

                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).AddToVehicleInventory(Game.Player.Character.Weapons.Current, Game.Player.Character.Weapons.Current.Ammo);
                }
            }

            if (e.KeyCode == takeWeaponKey && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                try
                {
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).RemoveFromVehicleInventory(cursorPos);
                }

                catch (Exception ext) { WriteToLogFile(ext.Message); }
            }

            if (e.KeyCode == Keys.Left && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    cursorPos--;

                    if (cursorPos < 0)
                        cursorPos = 7;
                }
            }

            if (e.KeyCode == Keys.Right && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    cursorPos++;

                    if (cursorPos > 7)
                        cursorPos = 0;
                }
            }
        }

        private void MainInventory() 
        {
            currentVehicle = null;

            Vehicle[] all_near_vehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 6f);

            if (all_near_vehicles.Length == 0)
                return;

            //get all entities near
            foreach (Vehicle car in all_near_vehicles)
            {
                TrunkCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "neon_b"));

                if (car.IsOnScreen && World.GetDistance(Game.Player.Character.Position, TrunkCoord) < 1.5f)
                {
                    Vector2 vec = World3DToScreen2d(car.Position);

                    if (vec.X > 0.4f && vec.X < 0.6f && vec.Y > 0.1f && vec.Y < 0.9f && HasCarRequieredVehicleClass(car) && !car.IsDead)
                    {
                        currentVehicle = car;
                        break;
                    }
                }
            }

            if (currentVehicle != null)
            {
                if (World.GetDistance(Game.Player.Character.Position, TrunkCoord) < 2f && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
                {
                    TrunkCoord.Z += 1;
                    Vector2 vec = World3DToScreen2d(TrunkCoord);

                    if (!currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                        DrawHackPanelText($"Press {openTrunkKey} to open/close the trunk", vec.X, vec.Y + 0.1, 0.36f, Color.White, true);

                    if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                        DrawInventoryPanel(TrunkCoord);
                }
            }
        }

        private void DrawInventoryPanel(Vector3 TrunkCoord) 
        {
            Function.Call(Hash.SET_DRAW_ORIGIN, TrunkCoord.X, TrunkCoord.Y, TrunkCoord.Z, 0);

            //background rect
            Function.Call(Hash.DRAW_RECT, 1, 1, 0.18, 0.2, Color.Black.R, Color.Black.G, Color.Black.B, 160);

            //headline rect
            Function.Call(Hash.DRAW_RECT, 1, -0.09, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);
            //headline text

            string weaponName;

            try
            {
                weaponName = ContainsAVehicleCurrentCustomVehiclesList(currentVehicle).VehicleInventory.ElementAt(cursorPos).Key.Name;
            }

            catch 
            { 
                weaponName = "Empty"; 
            }

            DrawHackPanelText($"Selected: {weaponName}", 0, -0.103, 0.35, Color.White, true);

            //headline rect
            Function.Call(Hash.DRAW_RECT, 1, 0.09, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);
            //headline text
            DrawHackPanelText($"{putWeaponKey} - put item, {takeWeaponKey} - take item, left/right - select", 0, 0.078, 0.35, Color.White, true);

            double bias = 0.043;
            double x;
            double y;

            //draw inventory cells
            for (int i = 0; i < 8; i++)
            {
                if (i < 4)
                {
                    x = -0.065 + bias * i;
                    y = -0.04;
                }

                else
                {
                    x = -0.065 + bias * Math.Abs(4 - i);
                    y = 0.03;
                }
                // draw cell
                Function.Call(Hash.DRAW_RECT, x, y, 0.035, 0.05, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);

                //draw cursor
                if (cursorPos == i)
                    Function.Call(Hash.DRAW_RECT, x, y, 0.035, 0.05, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 120);
            }

            //draw inventory items icons
            foreach (CustomVehicle cust in CustomVehiclesList)
            {
                if (cust.CustomModel == currentVehicle)
                {
                    for (int i = 0; i < cust.VehicleInventory.Count; i++)
                    {
                        try
                        {
                            if (cust.VehicleInventory.ContainsKey(cust.VehicleInventory.ElementAt(i).Key))
                            {
                                if (i < 4)
                                {
                                    x = -0.065 + bias * i;
                                    y = -0.04;
                                }

                                else
                                {
                                    x = -0.065 + bias * Math.Abs(4 - i);
                                    y = 0.03;
                                }
                                //draw weapon icon
                                Function.Call(Hash.DRAW_SPRITE, "mpkillquota", ReturnWeaponIconTextureName(cust.VehicleInventory.ElementAt(i).Key.Hash), x, y, 0.035, 0.03, 0.0, Color.White.R, Color.White.G, Color.White.B, 255);
                                
                                // draw weapon's ammo
                                if (cust.VehicleInventory.ElementAt(i).Key.Group != WeaponGroup.Melee)
                                    DrawHackPanelText($"{cust.VehicleInventory.ElementAt(i).Value}", x + 0.01, y + 0.005, 0.25, Color.White, true);
                            }
                        }

                        catch { continue; }
                    }

                    break;
                }
            }

            Function.Call(Hash.CLEAR_DRAW_ORIGIN);
        }

        private string ReturnWeaponIconTextureName(WeaponHash hash) 
        {
            if (WeaponsIconsDict.ContainsKey(hash))
                return WeaponsIconsDict[hash];

            else
                return "vehicle_weapon_player_buzzard";
        }
        private CustomVehicle ContainsAVehicleCurrentCustomVehiclesList(Vehicle car) 
        {
            try
            {
                return CustomVehiclesList.First(veh => veh.CustomModel == car);
            }

            catch 
            { 
                return null; 
            }
        }
        private void RemoveDontExistCustomVehicles()
        {
            try
            {
                CustomVehiclesList.RemoveAll(cust => cust.CustomModel.Exists() == false || cust.VehicleInventory.Count == 0);
            }

            catch (Exception ext) { WriteToLogFile(ext.Message); }
        }
        private bool HasCarRequieredVehicleClass(Vehicle car)
        {
            var listOfVehicleClasses = new List<VehicleClass> 
            { 
                VehicleClass.Sports, VehicleClass.SportsClassics, VehicleClass.Sedans, VehicleClass.Super, VehicleClass.Coupes, VehicleClass.Compacts, VehicleClass.Muscle, VehicleClass.SUVs
            };

            return listOfVehicleClasses.Contains(car.ClassType);
        }
        // get convert 3d coord to 2d coord for screen
        Vector2 World3DToScreen2d(Vector3 pos)
        {
            var x2dp = new OutputArgument();
            var y2dp = new OutputArgument();

            Function.Call<bool>(Hash._WORLD3D_TO_SCREEN2D, pos.X, pos.Y, pos.Z, x2dp, y2dp);
            return new Vector2(x2dp.GetResult<float>(), y2dp.GetResult<float>());
        }
        public void DrawHackPanelText(string message, double x, double y, double scale, Color color, bool centre)
        {
            Function.Call(Hash.SET_TEXT_FONT, 4);
            Function.Call(Hash.SET_TEXT_SCALE, scale, scale);
            Function.Call(Hash.SET_TEXT_COLOUR, color.R, color.G, color.B, 220);
            Function.Call(Hash.SET_TEXT_WRAP, 0.0, 1.0);
            Function.Call(Hash.SET_TEXT_CENTRE, centre);
            Function.Call(Hash.SET_TEXT_DROPSHADOW, 2, 2, 0, 0, 0);
            Function.Call(Hash.SET_TEXT_EDGE, 1, 1, 1, 1, 205);
            Function.Call(Hash._SET_TEXT_ENTRY, "STRING");
            Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, message);
            Function.Call(Hash._DRAW_TEXT, x, y);
        }

        // --------------------------------------- LOG Section --------------------------------------------------
        void WriteToLogFile(string text)
        {
            string path = @"Scripts\";
            string filename = $"{path}{modName}_Log.txt";

            try
            {
                if (!File.Exists(filename))
                    File.WriteAllText(filename, $"///// {modName} v{modVersion} Log, author {modAuthor}. This file contains main informations about mod errors!");

                File.AppendAllText(filename, $"\n{DateTime.Now}           {text}");
            }
            catch { }
        }

        //------------------------------ INI Section ----------------------
        void IniInitialization()
        {
            if (System.IO.File.Exists(CurrentFileDirectory + "CarInventory.ini"))
            {
                // ini file exist checker
                var myINI = new IniFile(CurrentFileDirectory);

                if (!myINI.KeyExists("OpenTrunkKey", "SETTINGS") || (openTrunkKey == Keys.None))
                {
                    myINI.Write("SETTINGS", "OpenTrunkKey", "E");
                }

                if (!myINI.KeyExists("PutWeaponKey", "SETTINGS") || (putWeaponKey == Keys.None))
                {
                    myINI.Write("SETTINGS", "PutWeaponKey", "I");
                }

                if (!myINI.KeyExists("TakeWeaponKey", "SETTINGS") || (takeWeaponKey == Keys.None))
                {
                    myINI.Write("SETTINGS", "TakeWeaponKey", "O");
                }

                ReadFromIniConfig();
            }

            else
            {
                WriteToIniCongif();
                ReadFromIniConfig();
            }
        }
        //read parameters from ini file
        private void ReadFromIniConfig()
        {
            var myINI = new IniFile(CurrentFileDirectory);
            Enum.TryParse(myINI.Read("SETTINGS", "OpenTrunkKey"), out openTrunkKey);
            Enum.TryParse(myINI.Read("SETTINGS", "PutWeaponKey"), out putWeaponKey);
            Enum.TryParse(myINI.Read("SETTINGS", "TakeWeaponKey"), out takeWeaponKey);

            if (openTrunkKey == Keys.None)
            {
                myINI.Write("SETTINGS", "OpenTrunkKey", "E");
                openTrunkKey = Keys.E;
            }

            if (putWeaponKey == Keys.None)
            {
                myINI.Write("SETTINGS", "PutWeaponKey", "I");
                putWeaponKey = Keys.I;
            }

            if (takeWeaponKey == Keys.None)
            {
                myINI.Write("SETTINGS", "TakeWeaponKey", "O");
                takeWeaponKey = Keys.O;
            }
        }
        //write to ini file
        private void WriteToIniCongif()
        {
            var myINI = new IniFile(CurrentFileDirectory);
            myINI.Write("SETTINGS", "OpenTrunkKey", "E");
            myINI.Write("SETTINGS", "PutWeaponKey", "I");
            myINI.Write("SETTINGS", "TakeWeaponKey", "O");
        }
    }
}
