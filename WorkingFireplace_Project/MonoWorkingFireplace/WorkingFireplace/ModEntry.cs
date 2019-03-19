//MIT License

//Copyright (c) 2019 Jahangmar

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.BellsAndWhistles;

using Netcode;

//using StardewValley.Menus;
//using System.Collections.Generic;

namespace WorkingFireplace
{
    public class ModEntry : Mod
    {
        private WorkingFireplaceConfig Config;

        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            Config = helper.ReadConfig<WorkingFireplaceConfig>();
        }

        void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            bool warmth = false;
            if (Game1.currentLocation is FarmHouse farmHouse)
            {
                foreach (Furniture furniture in farmHouse.furniture)
                {
                    if (furniture.furniture_type == Furniture.fireplace && furniture.isOn) {
                        Point tile = VectorToPoint(furniture.tileLocation.Get());
                        SetFireplace(farmHouse, tile.X, tile.Y, false, false);
                        warmth = true;
                    }
                }
            }
            if (Game1.IsWinter)
            {
                if (warmth)
                {
                    if (Config.showMessageOnStartOfDay)
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("msg.warm"), ""));
                }
                else
                {
                    if (Config.showMessageOnStartOfDay)
                        Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("msg.cold"), ""));
                    Game1.currentLocation.playSound("coldSpell");

                    if (Config.penalty)
                    {
                        Game1.player.health = CalcAttribute(Game1.player.health, Config.reduce_health, Game1.player.maxHealth);
                        Game1.player.stamina = CalcAttribute(Game1.player.stamina, Config.reduce_stamina, Game1.player.maxStamina);
                    }
                }
            }
        }


        void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            Point grabtile = VectorToPoint(e.Cursor.GrabTile);

            if (Game1.currentLocation is FarmHouse farmHouse1 &&
                e.Button.IsUseToolButton() && e.IsDown(e.Button))
            {
                //the fireplace is moved. We want to turn it off to avoid floating flames.
                Point tile = VectorToPoint(e.Cursor.Tile);
                SetFireplace(farmHouse1, tile.X, tile.Y, false, true);
            }
            else if (Game1.currentLocation is FarmHouse farmHouse &&
                e.Button.IsActionButton() && e.IsDown(e.Button) &&
                farmHouse.getObjectAtTile(grabtile.X, grabtile.Y) is Furniture furniture &&
                furniture.furniture_type == Furniture.fireplace)
            {
                Helper.Input.Suppress(e.Button);

                if (!furniture.isOn)
                {
                    Item item = Game1.player.CurrentItem;
                    if (item != null && item.Name == "Wood" && item.getStack() >= Config.wood_pieces)
                    {
                        Game1.player.removeItemsFromInventory(item.ParentSheetIndex, Config.wood_pieces);
                        SetFireplace(farmHouse, grabtile.X, grabtile.Y, true);
                        return;
                    }
                    Game1.showRedMessage(Helper.Translation.Get("msg.nowood", new { Config.wood_pieces }));
                }
            }
        }

        private int CalcAttribute(float value, double fac, int max)
        {
            int result = Convert.ToInt32(value - max * fac);

            if (result > max)
                return max;
            else if (result <= 0)
                return 1;
            else
                return result;
        }

        /// <summary>
        /// Checks if the given position matches a fireplace.
        /// Toggles the fireplace on or off if its state differs from <c>on</c>./// 
        /// </summary>
        /// <param name="farmHouse">Farm house.</param>
        /// <param name="X">X tile position of fireplace.</param>
        /// <param name="Y">Y tile position of fireplace.</param>
        /// <param name="on">new state of fireplace.</param>
        /// <param name="playsound">should a sound be played?</param>
        private void SetFireplace(FarmHouse farmHouse, int X, int Y, bool on, bool playsound = true)
        {
            if (farmHouse.getObjectAtTile(X, Y) is Furniture furniture && furniture.furniture_type == Furniture.fireplace)
            {
                //fireplaces are two tiles wide. The "FarmHouse.setFireplace" method needs the left tile so we set it to the left one.
                if (farmHouse.getObjectAtTile(X-1, Y) == furniture)
                {
                    X = X - 1;
                }
                if (furniture.isOn.Get() != on)
                {
                    furniture.isOn.Set(on);
                    farmHouse.setFireplace(on, X, Y, playsound);
                }
            }
        }

        private Point VectorToPoint(Vector2 vec)
        {
            return new Point(Convert.ToInt32(vec.X), Convert.ToInt32(vec.Y));
        }

    }
}
