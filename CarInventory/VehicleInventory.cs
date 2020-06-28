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

        private List<CustomVehicle> CustomVehiclesList = new List<CustomVehicle>() { };

        private Vector3 TrunkNeonCoord;
        private Vehicle currentVehicle = null;

        // inventory size by vehicle class
        private Dictionary<VehicleClass, int[]> invSizeByRequiredVehicleClass = new Dictionary<VehicleClass, int[]>()
        {
            [VehicleClass.Sports] = new int[] { 3, 3 },
            [VehicleClass.SportsClassics] = new int[] { 3, 3 },
            [VehicleClass.Sedans] = new int[] { 4, 3 },
            [VehicleClass.Super] = new int[] { 3, 2 },
            [VehicleClass.Coupes] = new int[] { 4, 3 },
            [VehicleClass.Compacts] = new int[] { 2, 2 },
            [VehicleClass.Muscle] = new int[] { 4, 3 },
            [VehicleClass.SUVs] = new int[] { 4, 4 },
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
                UI.ShowSubtitle($"count={CustomVehiclesList.Count}");
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

                if (World.GetDistance(EngineCoord, HoodCoord) < World.GetDistance(EngineCoord, TrunkCoord))
                {
                    currentTrunkDoor = VehicleDoor.Trunk;
                }

                else 
                {
                    currentTrunkDoor = VehicleDoor.Hood;
                }


                if (e.KeyCode == OpenTrunkKey && !Game.Player.Character.IsInVehicle() && !Game.Player.Character.IsDead)
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

                if (currentVehicle.IsDoorOpen(currentTrunkDoor))
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
                            customV.cursorPos[0] = invSizeByRequiredVehicleClass[currentVehicle.ClassType][0] - 1;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateRight)
                    {
                        customV.cursorPos[0]++;

                        if (customV.cursorPos[0] > invSizeByRequiredVehicleClass[currentVehicle.ClassType][0] - 1)
                            customV.cursorPos[0] = 0;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateUp)
                    {
                        customV.cursorPos[1]--;

                        if (customV.cursorPos[1] < 0)
                            customV.cursorPos[1] = invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1;

                        Game.PlaySound("NAV_UP_DOWN", "HUD_MINI_GAME_SOUNDSET");
                    }


                    if (e.KeyCode == NavigateDown)
                    {
                        customV.cursorPos[1]++;

                        if (customV.cursorPos[1] > invSizeByRequiredVehicleClass[currentVehicle.ClassType][1] - 1)
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
            for (int i = 0; i < custVehicle.CustomVehicleInventory.GetLength(0); i++)
            {
                x = -0.065 + bias * i;

                for (int j = 0; j < custVehicle.CustomVehicleInventory.GetLength(1); j++)
                {
                    y = -0.08 + bias * j;

                    try
                    {                      
                        // draw cell
                        Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 30);

                        //draw cursor
                        if (custVehicle.cursorPos[0] == i && custVehicle.cursorPos[1] == j)
                            Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkGray.R, Color.DarkGray.G, Color.DarkGray.B, 120);

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
                        //Function.Call(Hash.DRAW_RECT, x, y + 0.01 * j, 0.035, 0.045, Color.DarkRed.R, Color.DarkRed.G, Color.DarkRed.B, 120);
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
