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

using StardewValley.Menus;
using System.Collections.Generic;

namespace SeedsAreRare
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Display.MenuChanged += this.MenuChanged;
        }

        /// <summary>Raised after a menu was opened.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        void MenuChanged(object sender, MenuChangedEventArgs e)
        {

            //check if we are in a shop menu
            if (e.NewMenu is ShopMenu shopMenu)
            {
                List<Item> shopInventory = this.Helper.Reflection.GetField<List<Item>>(shopMenu, "forSale").GetValue();

                //remove all seeds but do not remove the tree saplings (those are also categorized as seeds)
                shopInventory.RemoveAll((Item item) => item.category.Get() == StardewValley.Object.SeedsCategory && !item.Name.EndsWith("Sapling", StringComparison.Ordinal));
                    
            }           
        }

    }
}