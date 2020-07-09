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
        private readonly string modVersion = "1.04b";
        private readonly string modAuthor = "Stifflerstiv";
        private readonly bool debugMode = false;

        // ini keys
        Keys OpenTrunkKey;
        Keys PutWeaponKey;
        Keys TakeWeaponKey;
        Keys NavigateLeft;
        Keys NavigateRight;
        Keys NavigateUp;
        Keys NavigateDown;

        // ini parameters
        string ShowTrunkOpenHelpText = "1";
        // 

        // in-game current custom vehicles' list
        private List<CustomVehicle> CustomVehiclesList = new List<CustomVehicle>() { };

        // current in-game vehicle
        private Vehicle currentVehicle = null;

        // ----------------------------------- ini file parameters
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
        private Dictionary<string, string> IniModOtherSettings = new Dictionary<string, string>()
        {
            ["ShowTrunkOpenHelpText"] = "1",
        };


        //----------------------------------------------------------------------------------------------------------------------------------------------
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

            // ini parameters variables
            ShowTrunkOpenHelpText = IniModOtherSettings.ElementAt(0).Value;

            // loading of icons
            CustomWeapon.LoadWeaponIcons();

            KeyDown += OnKeyDown;
            Tick += OnTick;
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
            try
            {
                UI.ShowSubtitle($"ShowTrunkOpenHelpText = {ShowTrunkOpenHelpText}");
            }

            catch { }
        }

        void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (currentVehicle != null && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
            {
                Vector3 EngineCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "engine"));
                Vector3 HoodCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "bonnet"));
                Vector3 TrunkCoord = currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "boot"));

                CustomVehicle customV = CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle);

                // trunk or hood variable
                VehicleDoor currentTrunkDoor;

                if (Math.Abs(World.GetDistance(EngineCoord, HoodCoord)) < Math.Abs(World.GetDistance(EngineCoord, TrunkCoord)) || Math.Abs(World.GetDistance(EngineCoord, HoodCoord)) > 20)
                {
                    currentTrunkDoor = VehicleDoor.Trunk;
                }

                else 
                {
                    currentTrunkDoor = VehicleDoor.Hood;
                }


                if (e.KeyCode == OpenTrunkKey && !Game.Player.Character.IsInVehicle() && !Game.Player.Character.IsDead && !currentVehicle.IsDoorBroken(currentTrunkDoor))
                {
                    if (World.GetDistance(currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "engine")), currentVehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, currentVehicle, "boot"))) < 10)
                    {
                        if (currentVehicle.IsDoorOpen(currentTrunkDoor))
                        {
                            // playing close trunk anitation
                            Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "enter", 8f, 400, false, -1f);
                            currentVehicle.CloseDoor(currentTrunkDoor, false);
                        }

                        else
                        {
                            // playing open trunk anitation
                            Game.Player.Character.Task.PlayAnimation("anim@mp_player_intincarjazz_handsbodhi@ds@", "exit", 8f, 400, false, -1f);
                            currentVehicle.OpenDoor(currentTrunkDoor, false, false);
                        }
                    }
                }

                if (currentVehicle.IsDoorOpen(currentTrunkDoor) || currentVehicle.IsDoorBroken(currentTrunkDoor))
                {
                    if (e.KeyCode == PutWeaponKey && Game.Player.Character.Weapons.Current.Hash != WeaponHash.Unarmed)
                    {
                        customV.AddToVehicleInventory(Game.Player.Character.Weapons.Current, Game.Player.Character.Weapons.Current.Ammo);                        
                    }


                    if (e.KeyCode == TakeWeaponKey)
                    {
                        customV.RemoveFromVehicleInventory();
                    }


                    if (e.KeyCode == NavigateLeft)
                    {
                        customV.cursorPos[0]--;

                        if (customV.cursorPos[0] < 0)
                            customV.cursorPos[0] = CustomVehicle.invSizeByRequiredVehicleClass[currentVehicle.ClassType][0] - 1;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateRight)
                    {
                        customV.cursorPos[0]++;

                        if (customV.cursorPos[0] > CustomVehicle.invSizeByRequiredVehicleClass[currentVehicle.ClassType][0] - 1)
                            customV.cursorPos[0] = 0;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateUp)
                    {
                        customV.cursorPos[1]--;

                        if (customV.cursorPos[1] < 0)
                            customV.cursorPos[1] = CustomVehicle.invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateDown)
                    {
                        customV.cursorPos[1]++;

                        if (customV.cursorPos[1] > CustomVehicle.invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1)
                            customV.cursorPos[1] = 0;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }
                }


            }
        }

        private void MainInventory()
        {
            currentVehicle = null;

            if (!Game.Player.Character.IsInVehicle() && Game.Player.Character.IsAlive && !Game.Player.Character.IsSwimming)
            {
                Vehicle[] all_near_vehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 6f);
                        // current real trunk coord
                Vector3 TrunkNeonCoord;

                if (all_near_vehicles.Length == 0)
                    return;

                //get all entities near
                foreach (Vehicle car in all_near_vehicles)
                {
                    Vector3 EngineCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "engine"));
                    Vector3 HoodCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "bonnet"));
                    Vector3 TrunkCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "boot"));                

                    VehicleDoor vehDoor = VehicleDoor.Trunk;

                    // searching real trunk position (fixing R* bug with cars' dummys)
                    if (Math.Abs(World.GetDistance(EngineCoord, HoodCoord)) < Math.Abs(World.GetDistance(EngineCoord, TrunkCoord)))
                    {
                        float posTrunkX = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).X + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).X) / 2;
                        float posTrunkY = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).Y + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).Y) / 2;
                        float posTrunkZ = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).Z + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).Z) / 2;
                        //TrunkNeonCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, backPos));
                        Vector3 trunkPos = new Vector3(posTrunkX, posTrunkY, posTrunkZ);

                        float posHoodX = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).X + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).X) / 2;
                        float posHoodY = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).Y + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).Y) / 2;
                        float posHoodZ = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).Z + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).Z) / 2;
                        Vector3 hoodPos = new Vector3(posHoodX, posHoodY, posHoodZ);

                        if (Math.Abs(World.GetDistance(TrunkCoord, trunkPos)) < Math.Abs(World.GetDistance(TrunkCoord, hoodPos))) 
                            TrunkNeonCoord = trunkPos;

                        else
                            TrunkNeonCoord = hoodPos;

                        vehDoor = VehicleDoor.Trunk;
                    }

                    else
                    {
                        float posTrunkX = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).X + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).X) / 2;
                        float posTrunkY = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).Y + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).Y) / 2;
                        float posTrunkZ = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lr")).Z + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rr")).Z) / 2;
                        //TrunkNeonCoord = car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, backPos));
                        Vector3 trunkPos = new Vector3(posTrunkX, posTrunkY, posTrunkZ);

                        float posHoodX = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).X + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).X) / 2;
                        float posHoodY = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).Y + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).Y) / 2;
                        float posHoodZ = (car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_lf")).Z + car.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, car, "suspension_rf")).Z) / 2;
                        Vector3 hoodPos = new Vector3(posHoodX, posHoodY, posHoodZ);

                        if (Math.Abs(World.GetDistance(HoodCoord, hoodPos)) < Math.Abs(World.GetDistance(HoodCoord, trunkPos)))
                            TrunkNeonCoord = hoodPos;

                        else
                            TrunkNeonCoord = trunkPos;

                        if (Math.Abs(World.GetDistance(EngineCoord, HoodCoord)) > 25)
                            vehDoor = VehicleDoor.Trunk;

                        else
                            vehDoor = VehicleDoor.Hood;
                    }


                    if (car.IsOnScreen && World.GetDistance(Game.Player.Character.Position, TrunkNeonCoord) < 2.5f && World.GetDistance(EngineCoord, TrunkCoord) < 20)
                    {
                        Vector2 vec = World3DToScreen2d(car.Position);

                        if (vec.X > 0.4f && vec.X < 0.6f && vec.Y > 0.1f && vec.Y < 0.9f && CustomVehicle.invSizeByRequiredVehicleClass.ContainsKey(car.ClassType) && !car.IsDead)
                        {
                            currentVehicle = car;

                            if (!CustomVehiclesList.Contains(ContainsAVehicleCurrentCustomVehiclesList(currentVehicle)) && CustomVehicle.invSizeByRequiredVehicleClass.ContainsKey(currentVehicle.ClassType))
                            {
                                CustomVehiclesList.Add(new CustomVehicle(currentVehicle, CustomVehicle.invSizeByRequiredVehicleClass[currentVehicle.ClassType]));
                            }

                            // draw info text about open/close trunk
                            if (World.GetDistance(Game.Player.Character.Position, TrunkNeonCoord) < 2f && currentVehicle.LockStatus == VehicleLockStatus.Unlocked)
                            {
                                TrunkNeonCoord.Z += 1;
                                vec = World3DToScreen2d(TrunkNeonCoord);

                                if (!currentVehicle.IsDoorOpen(vehDoor) && !currentVehicle.IsDoorBroken(vehDoor))
                                {
                                    if (ShowTrunkOpenHelpText == "1")
                                        DrawHackPanelText($"Press {OpenTrunkKey} to open/close the trunk", vec.X, vec.Y + 0.1, 0.36f, Color.White, true);
                                }

                                else
                                    DrawInventoryPanel(TrunkNeonCoord);
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
            Function.Call(Hash.DRAW_RECT, 1, -0.12, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 50);
            //headline text

            // get custom vehicle from list
            CustomVehicle custVehicle= CustomVehiclesList.Find(cust => cust.CustomModel == currentVehicle);
            string weaponName;

            try
            {
                weaponName = custVehicle.CustomVehicleInventory[custVehicle.cursorPos[0], custVehicle.cursorPos[1]].CustomWeaponModel.Name;
            }

            catch
            {
                weaponName = "Empty";
            }

            DrawHackPanelText($"Selected: {weaponName}", 0, -0.133, 0.35, Color.White, true);

            //headline rect
            Function.Call(Hash.DRAW_RECT, 1, 0.12, 0.18, 0.02, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 50);
            //headline text
            DrawHackPanelText($"{PutWeaponKey} - put item, {TakeWeaponKey} - take item, {NavigateLeft} / {NavigateRight} / {NavigateUp} / {NavigateDown} - navigate", 0, 0.109, 0.243, Color.White, true);

            // draw bias
            double bias = 0.043;
            double x;
            double y;

            // draw inventory with 4x4 default size
            for (int i = 0; i < 4; i++)
            {
                x = -0.065 + bias * i;

                for (int j = 0; j < 4; j++)
                {
                    y = -0.08 + bias * j;

                    try
                    {
                        // draw cell
                        //Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);
                        if (custVehicle.CustomVehicleInventory[i, j] != null)
                        {
                            string testException = custVehicle.CustomVehicleInventory[i, j].Name;
                        }

                        Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "hud_outline_thin", x, y + 0.01 * j, 0.04, 0.05, 0.0, Color.White.R, Color.White.G, Color.White.B, 50);

                        //draw cursor
                        if (custVehicle.cursorPos[0] == i && custVehicle.cursorPos[1] == j) 
                        { 
                            Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "hud_lock", x, y + 0.01 * j, 0.04, 0.05, 0.0, Color.White.R, Color.White.G, Color.White.B, 255);
                            //Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 120);
                        }
                        
                        if (custVehicle.CustomVehicleInventory[i, j] != null)
                        {
                            //draw weapon icon
                            Function.Call(Hash.DRAW_SPRITE, "mpkillquota", custVehicle.CustomVehicleInventory[i, j].GetWeaponIconTextureName(), x, y + 0.01 * j, 0.035, 0.03, 0.0, Color.White.R, Color.White.G, Color.White.B, 255);

                            // draw weapon's ammo
                            if (custVehicle.CustomVehicleInventory[i, j].CustomWeaponModel.Group != WeaponGroup.Melee)
                                DrawHackPanelText($"{custVehicle.CustomVehicleInventory[i, j].CustomWeaponAmmo}", x + 0.01, y + 0.005 + 0.01 * j, 0.25, Color.White, true);
                        }
                    }

                    catch
                    {
                        // draw cell
                        //Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B, 30);
                        Function.Call(Hash.DRAW_SPRITE, "helicopterhud", "hud_block", x, y + 0.01 * j, 0.04, 0.05, 0.0, Color.White.R, Color.White.G, Color.White.B, 30);
                    }
                }
            }

            // end of draw of inventory
            Function.Call(Hash.CLEAR_DRAW_ORIGIN);
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

        private void DrawAllVehicleDummys(Vehicle veh) 
        {
            List<string> VehDummyes = new List<string>() 
            {   "chassis",
                "chassis_lowlod",
                "chassis_dummy",
                "seat_dside_f",
                "seat_dside_r",
                "seat_dside_r1",
                "seat_dside_r2",
                "seat_dside_r3",
                "seat_dside_r4",
                "seat_dside_r5",
                "seat_dside_r6",
                "seat_dside_r7",
                "seat_pside_f",
                "seat_pside_r",
                "seat_pside_r1",
                "seat_pside_r2",
                "seat_pside_r3",
                "seat_pside_r4",
                "seat_pside_r5",
                "seat_pside_r6",
                "seat_pside_r7",
                "window_lf1",
                "window_lf2",
                "window_lf3",
                "window_rf1",
                "window_rf2",
                "window_rf3",
                "window_lr1",
                "window_lr2",
                "window_lr3",
                "window_rr1",
                "window_rr2",
                "window_rr3",
                "door_dside_f",
                "door_dside_r",
                "door_pside_f",
                "door_pside_r",
                "handle_dside_f",
                "handle_dside_r",
                "handle_pside_f",
                "handle_pside_r",
                "wheel_lf",
                "wheel_rf",
                "wheel_lm1",
                "wheel_rm1",
                "wheel_lm2",
                "wheel_rm2",
                "wheel_lm3",
                "wheel_rm3",
                "wheel_lr",
                "wheel_rr",
                "suspension_lf",
                "suspension_rf",
                "suspension_lm",
                "suspension_rm",
                "suspension_lr",
                "suspension_rr",
                "spring_rf",
                "spring_lf",
                "spring_rr",
                "spring_lr",
                "transmission_f",
                "transmission_m",
                "transmission_r",
                "hub_lf",
                "hub_rf",
                "hub_lm1",
                "hub_rm1",
                "hub_lm2",
                "hub_rm2",
                "hub_lm3",
                "hub_rm3",
                "hub_lr",
                "hub_rr",
                "windscreen",
                "windscreen_r",
                "window_lf",
                "window_rf",
                "window_lr",
                "window_rr",
                "window_lm",
                "window_rm",
                "bodyshell",
                "bumper_f",
                "bumper_r",
                "wing_rf",
                "wing_lf",
                "bonnet",
                "boot",
                "exhaust",
                "exhaust_2",
                "exhaust_3",
                "exhaust_4",
                "exhaust_5",
                "exhaust_6",
                "exhaust_7",
                "exhaust_8",
                "exhaust_9",
                "exhaust_10",
                "exhaust_11",
                "exhaust_12",
                "exhaust_13",
                "exhaust_14",
                "exhaust_15",
                "exhaust_16",
                "engine",
                "overheat",
                "overheat_2",
                "petrolcap",
                "petroltank",
                "petroltank_l",
                "petroltank_r",
                "steering",
                "hbgrip_l",
                "hbgrip_r",
                "headlight_l",
                "headlight_r",
                "taillight_l",
                "taillight_r",
                "indicator_lf",
                "indicator_rf",
                "indicator_lr",
                "indicator_rr",
                "brakelight_l",
                "brakelight_r",
                "brakelight_m",
                "reversinglight_l",
                "reversinglight_r",
                "extralight_1",
                "extralight_2",
                "extralight_3",
                "extralight_4",
                "numberplate",
                "interiorlight",
                "siren1",
                "siren2",
                "siren3",
                "siren4",
                "siren5",
                "siren6",
                "siren7",
                "siren8",
                "siren9",
                "siren10",
                "siren11",
                "siren12",
                "siren13",
                "siren14",
                "siren15",
                "siren16",
                "siren17",
                "siren18",
                "siren19",
                "siren20",
                "siren_glass1",
                "siren_glass2",
                "siren_glass3",
                "siren_glass4",
                "siren_glass5",
                "siren_glass6",
                "siren_glass7",
                "siren_glass8",
                "siren_glass9",
                "siren_glass10",
                "siren_glass11",
                "siren_glass12",
                "siren_glass13",
                "siren_glass14",
                "siren_glass15",
                "siren_glass16",
                "siren_glass17",
                "siren_glass18",
                "siren_glass19",
                "siren_glass20",
                "spoiler",
                "struts",
                "misc_a",
                "misc_b",
                "misc_c",
                "misc_d",
                "misc_e",
                "misc_f",
                "misc_g",
                "misc_h",
                "misc_i",
                "misc_j",
                "misc_k",
                "misc_l",
                "misc_m",
                "misc_n",
                "misc_o",
                "misc_p",
                "misc_q",
                "misc_r",
                "misc_s",
                "misc_t",
                "misc_u",
                "misc_v",
                "misc_w",
                "misc_x",
                "misc_y",
                "misc_z",
                "misc_1",
                "misc_2",
                "weapon_1a",
                "weapon_1b",
                "weapon_1c",
                "weapon_1d",
                "weapon_1a_rot",
                "weapon_1b_rot",
                "weapon_1c_rot",
                "weapon_1d_rot",
                "weapon_2a",
                "weapon_2b",
                "weapon_2c",
                "weapon_2d",
                "weapon_2a_rot",
                "weapon_2b_rot",
                "weapon_2c_rot",
                "weapon_2d_rot",
                "weapon_3a",
                "weapon_3b",
                "weapon_3c",
                "weapon_3d",
                "weapon_3a_rot",
                "weapon_3b_rot",
                "weapon_3c_rot",
                "weapon_3d_rot",
                "weapon_4a",
                "weapon_4b",
                "weapon_4c",
                "weapon_4d",
                "weapon_4a_rot",
                "weapon_4b_rot",
                "weapon_4c_rot",
                "weapon_4d_rot",
                "turret_1base",
                "turret_1barrel",
                "turret_2base",
                "turret_2barrel",
                "turret_3base",
                "turret_3barrel",
                "ammobelt",
                "searchlight_base",
                "searchlight_light",
                "attach_female",
                "roof",
                "roof2",
                "soft_1",
                "soft_2",
                "soft_3",
                "soft_4",
                "soft_5",
                "soft_6",
                "soft_7",
                "soft_8",
                "soft_9",
                "soft_10",
                "soft_11",
                "soft_12",
                "soft_13",
                "forks",
                "mast",
                "carriage",
                "fork_l",
                "fork_r",
                "forks_attach",
                "frame_1",
                "frame_2",
                "frame_3",
                "frame_pickup_1",
                "frame_pickup_2",
                "frame_pickup_3",
                "frame_pickup_4",
                "freight_cont",
                "freight_bogey",
                "freightgrain_slidedoor",
                "door_hatch_r",
                "door_hatch_l",
                "tow_arm",
                "tow_mount_a",
                "tow_mount_b",
                "tipper",
                "combine_reel",
                "combine_auger",
                "slipstream_l",
                "slipstream_r",
                "arm_1",
                "arm_2",
                "arm_3",
                "arm_4",
                "scoop",
                "boom",
                "stick",
                "bucket",
                "shovel_2",
                "shovel_3",
                "Lookat_UpprPiston_head",
                "Lookat_LowrPiston_boom",
                "Boom_Driver",
                "cutter_driver",
                "vehicle_blocker",
                "extra_1",
                "extra_2",
                "extra_3",
                "extra_4",
                "extra_5",
                "extra_6",
                "extra_7",
                "extra_8",
                "extra_9",
                "extra_ten",
                "extra_11",
                "extra_12",
                "break_extra_1",
                "break_extra_2",
                "break_extra_3",
                "break_extra_4",
                "break_extra_5",
                "break_extra_6",
                "break_extra_7",
                "break_extra_8",
                "break_extra_9",
                "break_extra_10",
                "mod_col_1",
                "mod_col_2",
                "mod_col_3",
                "mod_col_4",
                "mod_col_5",
                "handlebars",
                "forks_u",
                "forks_l",
                "wheel_f",
                "swingarm",
                "wheel_r",
                "crank",
                "pedal_r",
                "pedal_l",
                "static_prop",
                "moving_prop",
                "static_prop2",
                "moving_prop2",
                "rudder",
                "rudder2",
                "wheel_rf1_dummy",
                "wheel_rf2_dummy",
                "wheel_rf3_dummy",
                "wheel_rb1_dummy",
                "wheel_rb2_dummy",
                "wheel_rb3_dummy",
                "wheel_lf1_dummy",
                "wheel_lf2_dummy",
                "wheel_lf3_dummy",
                "wheel_lb1_dummy",
                "wheel_lb2_dummy",
                "wheel_lb3_dummy",
                "bogie_front",
                "bogie_rear",
                "rotor_main",
                "rotor_rear",
                "rotor_main_2",
                "rotor_rear_2",
                "elevators",
                "tail",
                "outriggers_l",
                "outriggers_r",
                "rope_attach_a",
                "rope_attach_b",
                "prop_1",
                "prop_2",
                "elevator_l",
                "elevator_r",
                "rudder_l",
                "rudder_r",
                "prop_3",
                "prop_4",
                "prop_5",
                "prop_6",
                "prop_7",
                "prop_8",
                "rudder_2",
                "aileron_l",
                "aileron_r",
                "airbrake_l",
                "airbrake_r",
                "wing_l",
                "wing_r",
                "wing_lr",
                "wing_rr",
                "engine_l",
                "engine_r",
                "nozzles_f",
                "nozzles_r",
                "afterburner",
                "wingtip_1",
                "wingtip_2",
                "gear_door_fl",
                "gear_door_fr",
                "gear_door_rl1",
                "gear_door_rr1",
                "gear_door_rl2",
                "gear_door_rr2",
                "gear_door_rml",
                "gear_door_rmr",
                "gear_f",
                "gear_rl",
                "gear_lm1",
                "gear_rr",
                "gear_rm1",
                "gear_rm",
                "prop_left",
                "prop_right",
                "legs",
                "attach_male",
                "draft_animal_attach_lr",
                "draft_animal_attach_rr",
                "draft_animal_attach_lm",
                "draft_animal_attach_rm",
                "draft_animal_attach_lf",
                "draft_animal_attach_rf",
                "wheelcover_l",
                "wheelcover_r",
                "barracks",
                "pontoon_l",
                "pontoon_r",
                "no_ped_col_step_l",
                "no_ped_col_strut_1_l",
                "no_ped_col_strut_2_l",
                "no_ped_col_step_r",
                "no_ped_col_strut_1_r",
                "no_ped_col_strut_2_r",
                "light_cover",
                "emissives",
                "neon_l",
                "neon_r",
                "neon_f",
                "neon_b",
                "dashglow",
                "doorlight_lf",
                "doorlight_rf",
                "doorlight_lr",
                "doorlight_rr",
                "unknown_id",
                "dials",
                "engineblock",
                "bobble_head",
                "bobble_base",
                "bobble_hand",
                "chassis_Control",
                };

            foreach (var dummy in VehDummyes)
            {
                Vector3 dummyVehPos = veh.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, veh, dummy));
                //Vector3 dummyPlayer = Game.Player.Character.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, Game.Player.Character, "BONETAG_L_FINGER0"));

                if (World.GetDistance(veh.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, veh, "engine")), veh.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, veh, dummy))) < 10)
                {
                    Vector2 pos2DText = World3DToScreen2d(dummyVehPos);
                    DrawHackPanelText(dummy, pos2DText.X, pos2DText.Y, 0.25, Color.White, true);
                }

                //Function.Call(Hash.DRAW_LINE, dummyVehPos.X, dummyVehPos.Y, dummyVehPos.Z, dummyPlayer.X, dummyPlayer.Y, dummyPlayer.Z, Color.White.R, Color.White.G, Color.White.B, 180);
                //UI.ShowSubtitle($"dist = {World.GetDistance(vehNear.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, vehNear, "engine")), vehNear.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, vehNear, bummyStr)))}");

            }
        }

        private void DetachObjectTest()
        {
            try
            {
                Prop[] all_near_props = World.GetNearbyProps(Game.Player.Character.Position, 6);
                //Vehicle vehicle = all_near_vehicles[0];


                foreach(var prop in all_near_props) 
                {
                    //Vector3 dummyVehPos2 = vehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, vehicle, "boot"));
                    Function.Call(Hash.DRAW_LINE, prop.Position.X, prop.Position.Y, prop.Position.Z, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, Color.White.R, Color.White.G, Color.White.B, 180);
                }
                /*
                Vector3 dummyVehPos = vehicle.GetBoneCoord(Function.Call<int>(Hash._0xFB71170B7E76ACBA, vehicle, "boot"));

                Entity ent = World.GetNearbyProps(dummyVehPos, 1f)[0];

                Function.Call(Hash.DETACH_ENTITY, ent, false, false);
                Function.Call(Hash.DELETE_ENTITY, ent);
                */
            }

            catch(Exception ex) { UI.ShowSubtitle(ex.Message); };

        }

        // -------------------------------------------------------------------------------------------------------
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


        //-----------------------------------------------------------------
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

            // reading all setttings from file
            foreach(var param in IniModOtherSettings.Keys.ToList())
            {
                IniModOtherSettings[param] = myINI.Read("SETTINGS", param.ToString());
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
