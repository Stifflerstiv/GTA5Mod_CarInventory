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
        private readonly string modVersion = "1.04";
        private readonly string modAuthor = "Stifflerstiv";
        private readonly bool debugMode = true;

        // ini keys
        Keys OpenTrunkKey;
        Keys PutWeaponKey;
        Keys TakeWeaponKey;
        Keys NavigateLeft;
        Keys NavigateRight;
        Keys NavigateUp;
        Keys NavigateDown;
        // ini parameters

        private List<CustomVehicle> CustomVehiclesList = new List<CustomVehicle>() { };

        private Vector3 TrunkNeonCoord;
        private Vehicle currentVehicle = null;

        // inventory size by vehicle class
        private Dictionary<VehicleClass, int[]> invSizeByRequiredVehicleClass = new Dictionary<VehicleClass, int[]>()
        {
            [VehicleClass.Sports] = new int[] { 4, 4 },
            [VehicleClass.SportsClassics] = new int[] { 4, 4 },
            [VehicleClass.Sedans] = new int[] { 4, 4 },
            [VehicleClass.Super] = new int[] { 4, 4 },
            [VehicleClass.Coupes] = new int[] { 4, 4 },
            [VehicleClass.Compacts] = new int[] { 4, 4 },
            [VehicleClass.Muscle] = new int[] { 4, 4 },
            [VehicleClass.SUVs] = new int[] { 4, 4 },
        };

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

        // ini file parameters
        private IniFile myINI;
        // dict of mod keys
        private Dictionary<string, List<Keys>> IniModKeysSettings = new Dictionary<string, List<Keys>>()
        {
            ["OpenTrunkKey"] = new List<Keys>() { Keys.E, Keys.None },
            ["PutWeaponKey"] = new List<Keys>() { Keys.I, Keys.None },
            ["TakeWeaponKey"] = new List<Keys>() { Keys.O, Keys.None },
            ["NavigateLeft"] = new List<Keys>() { Keys.NumPad4, Keys.None },
            ["NavigateRight"] = new List<Keys>() { Keys.NumPad6, Keys.None },
            ["NavigateUp"] = new List<Keys>() { Keys.NumPad8, Keys.None },
            ["NavigateDown"] = new List<Keys>() { Keys.NumPad2, Keys.None },
        };
        //dict of mod settings
        private Dictionary<string, string> IniModOtherSettings = new Dictionary<string, string>() { };

        //----------------------------------------------------------------------------------------------------------------------------------------------

        public VehicleInventory()
        {
            // create ini file class object

            myINI = new IniFile(modName);

            // call initialization
            IniInitialization();

            // key variables
            OpenTrunkKey = IniModKeysSettings.ElementAt(0).Value[1];
            PutWeaponKey = IniModKeysSettings.ElementAt(1).Value[1];
            TakeWeaponKey = IniModKeysSettings.ElementAt(2).Value[1];
            NavigateLeft = IniModKeysSettings.ElementAt(3).Value[1];
            NavigateRight = IniModKeysSettings.ElementAt(4).Value[1];
            NavigateUp = IniModKeysSettings.ElementAt(5).Value[1];
            NavigateDown = IniModKeysSettings.ElementAt(6).Value[1];

            // end of initialization
            KeyDown += OnKeyDown;
            Tick += OnTick;

            // icons streaming
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

        private void DebugFunc()
        {
            //UI.ShowSubtitle($"key={openTrunkKey}, take={takeWeaponKey}, put={putWeaponKey}, count={CustomVehiclesList.Count}");

            UI.ShowSubtitle($"tint={Game.Player.Character.Weapons.Current.Tint}");
            try
            {
                UI.ShowSubtitle($"count={CustomVehiclesList.Count}");
            }

            catch { }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == OpenTrunkKey && currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked && !Game.Player.Character.IsInVehicle() && !Game.Player.Character.IsDead)
            {
                Vector3 EngineCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "engine"));
                Vector3 HoodCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "bonnet"));
                Vector3 TrunkCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "boot"));

                if (World.GetDistance(EngineCoord, HoodCoord) < World.GetDistance(EngineCoord, TrunkCoord))
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

                else
                {
                    if (currentVehicle.IsDoorOpen(VehicleDoor.Hood))
                    {
                        // playing close trunk anitation
                        Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "enter", 8f, 400, false, -1f);
                        currentVehicle.CloseDoor(VehicleDoor.Hood, false);
                    }

                    else
                    {
                        // playing open trunk anitation
                        Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "exit", 8f, 400, false, -1f);
                        currentVehicle.OpenDoor(VehicleDoor.Hood, false, false);
                    }
                }
            }


            if (e.KeyCode == PutWeaponKey && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (Game.Player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).AddToVehicleInventory(Game.Player.Character.Weapons.Current, Game.Player.Character.Weapons.Current.Ammo);
                }
            }


            if (e.KeyCode == TakeWeaponKey && currentVehicle != null && currentVehicle.IsDoorOpen(VehicleDoor.Trunk) && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {                 
                CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).RemoveFromVehicleInventory();
            }


            if (e.KeyCode == NavigateLeft && currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0]--;

                    if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0] < 0)
                        CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0] = invSizeByRequiredVehicleClass[currentVehicle.ClassType][0] - 1;
                }
            }


            if (e.KeyCode == NavigateRight && currentVehicle != null  && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                if (currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                {
                    //int rows = CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory.GetUpperBound(0) + 1;
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0]++;

                    if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0] > invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1)
                        CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0] = 0;
                }
            }


            if (e.KeyCode == NavigateUp && currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1]--;

                if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1] < 0)
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1] = invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1;


            }


            if (e.KeyCode == NavigateDown && currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1]++;

                if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1] > invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1)
                    CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1] = 0;


            }
        }

        private void MainInventory()
        {
            currentVehicle = null;

            if (!Game.Player.Character.IsInVehicle())
            {
                Vehicle[] all_near_vehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 6f);

                if (all_near_vehicles.Length == 0)
                    return;

                //get all entities near
                foreach (Vehicle car in all_near_vehicles)
                {
                    Vector3 EngineCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "engine"));
                    Vector3 HoodCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "bonnet"));
                    Vector3 TrunkCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "boot"));

                    if (World.GetDistance(EngineCoord, TrunkCoord) > World.GetDistance(EngineCoord, HoodCoord))
                        TrunkNeonCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "neon_b"));
                    else
                        TrunkNeonCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "neon_f"));

                    if (car.IsOnScreen && World.GetDistance(Game.Player.Character.Position, TrunkNeonCoord) < 1.5f)
                    {
                        Vector2 vec = World3DToScreen2d(car.Position);

                        if (vec.X > 0.4f && vec.X < 0.6f && vec.Y > 0.1f && vec.Y < 0.9f && invSizeByRequiredVehicleClass.ContainsKey(car.ClassType) && !car.IsDead)
                        {
                            currentVehicle = car;

                            if (!CustomVehiclesList.Contains(ContainsAVehicleCurrentCustomVehiclesList(currentVehicle)) && invSizeByRequiredVehicleClass.ContainsKey(currentVehicle.ClassType))
                            {
                                CustomVehiclesList.Add(new CustomVehicle(currentVehicle, invSizeByRequiredVehicleClass[currentVehicle.ClassType]));
                            }

                            // draw info text about open/close trunk
                            if (World.GetDistance(Game.Player.Character.Position, TrunkNeonCoord) < 2f && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
                            {
                                TrunkNeonCoord.Z += 1;
                                vec = World3DToScreen2d(TrunkNeonCoord);

                                if (World.GetDistance(EngineCoord, HoodCoord) < World.GetDistance(EngineCoord, TrunkCoord))
                                {
                                    if (!currentVehicle.IsDoorOpen(VehicleDoor.Trunk))
                                        DrawHackPanelText($"Press {OpenTrunkKey} to open/close the trunk", vec.X, vec.Y + 0.1, 0.36f, Color.White, true);

                                    else
                                        DrawInventoryPanel(TrunkNeonCoord);
                                }

                                else
                                {
                                    if (!currentVehicle.IsDoorOpen(VehicleDoor.Hood))
                                        DrawHackPanelText($"Press {OpenTrunkKey} to open/close the trunk", vec.X, vec.Y + 0.1, 0.36f, Color.White, true);

                                    else
                                        DrawInventoryPanel(TrunkNeonCoord);
                                }
                            }

                            break;
                        }
                    }
                }
            }
        }

        private void DrawInventoryPanel(Vector3 TrunkCoord)
        {
            Function.Call(Hash.SET_DRAW_ORIGIN, TrunkCoord.X, TrunkCoord.Y, TrunkCoord.Z, 0);

            //background rect
            Function.Call(Hash.DRAW_RECT, 1, 1, 0.18, 0.26, Color.Black.R, Color.Black.G, Color.Black.B, 160);

            //headline rect
            Function.Call(Hash.DRAW_RECT, 1, -0.12, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);
            //headline text

            string weaponName;

            try
            {
                weaponName = CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory[CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0], CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1]].CustomWeaponModel.Name;
            }

            catch
            {
                weaponName = "Empty";
            }

            DrawHackPanelText($"Selected: {weaponName}", 0, -0.133, 0.35, Color.White, true);

            //headline rect
            Function.Call(Hash.DRAW_RECT, 1, 0.12, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);
            //headline text
            DrawHackPanelText($"{PutWeaponKey} - put item, {TakeWeaponKey} - take item, {NavigateLeft} / {NavigateRight} / {NavigateUp} / {NavigateDown} - navigate", 0, 0.109, 0.24, Color.White, true);

            double bias = 0.043;
            double x;
            double y;

            int rows = invSizeByRequiredVehicleClass[currentVehicle.ClassType][0];
            int columns = invSizeByRequiredVehicleClass[currentVehicle.ClassType][1];

            UI.ShowSubtitle($"rows = {rows}, columns = {columns}");

            // draw inventory
            for (int i = 0; i < rows; i++)
            {
                x = -0.065 + bias * i;
                for (int j = 0; j < columns; j++)
                {                    
                    y = -0.08 + bias * j;
                    // draw cell
                    Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);

                    //draw cursor
                    if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[0] == i && CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).cursorPos[1] == j)
                        Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 120);

                    if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory[i, j] != null)
                    {
                        //draw weapon icon
                        Function.Call(Hash.DRAW_SPRITE, "mpkillquota", ReturnWeaponIconTextureName(CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory[i, j].CustomWeaponModel.Hash), x, y + 0.01 * j, 0.045, 0.04, 0.0, Color.White.R, Color.White.G, Color.White.B, 255);

                        // draw weapon's ammo
                        if (CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory[i, j].CustomWeaponModel.Group != WeaponGroup.Melee)
                            DrawHackPanelText($"{CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle).CustomVehicleInventory[i, j].CustomWeaponAmmo}", x + 0.01, y + 0.005 + 0.01 * j, 0.25, Color.White, true);
                    }
                }
            }

            // end of draw of inventory
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
                CustomVehiclesList.RemoveAll(cust => cust.CustomModel.Exists() == false);
            }

            catch (Exception ext) { WriteToLogFile(ext.ToString()); }
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
        private void WriteToLogFile(string text)
        {
            string path = @"Scripts\";
            string filename = $"{path}{modName}_Log.txt";

            try
            {
                if (!File.Exists(filename))
                    File.AppendAllText(filename, $"///// {modName} v{modVersion} Log, author {modAuthor}. This file contains main informations about mod errors!");

                File.AppendAllText(filename, $"\n{DateTime.Now}   {text}");
            }

            catch { }
        }








        //------------------------------ INI Section ----------------------
        private void IniInitialization()
        {
            // checking parameters exist, create if don't exist
            WriteToIniCongif();

            // reading parameters from checked config file
            ReadFromIniConfig();
        }

        private void ReadFromIniConfig()
        {
            // parsing control keys
            foreach (var param in IniModKeysSettings.Keys.ToList())
            {
                // sing key var
                Keys key;

                Enum.TryParse(myINI.Read("KEYS", param.ToString()), out key);

                IniModKeysSettings[param][1] = key;

                if (IniModKeysSettings[param][1] == Keys.None)
                {
                    myINI.Write("KEYS", IniModKeysSettings[param].ToString(), param.ToString());
                    IniModKeysSettings[param][1] = IniModKeysSettings[param][0];
                }
            }
        }

        private void WriteToIniCongif()
        {
            // writing of keys parameters
            for (int i = 0; i < IniModKeysSettings.Count; i++)
            {
                if (!myINI.KeyExists(IniModKeysSettings.ElementAt(i).Key, "KEYS"))
                {
                    myINI.Write("KEYS", IniModKeysSettings.ElementAt(i).Key, IniModKeysSettings.ElementAt(i).Value[0].ToString());
                }
            }

            for (int i = 0; i < IniModOtherSettings.Count; i++)
            {
                if (!myINI.KeyExists(IniModOtherSettings.ElementAt(i).Key, "SETTINGS"))
                {
                    myINI.Write("SETTINGS", IniModOtherSettings.ElementAt(i).Key, IniModOtherSettings.ElementAt(i).Value);
                }
            }
        }
    }
}
