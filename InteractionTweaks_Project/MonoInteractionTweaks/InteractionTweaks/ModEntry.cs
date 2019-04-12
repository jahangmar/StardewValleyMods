//Copyright (c) 2019 Jahangmar

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU Lesser General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License
//along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

using StardewModdingAPI;
using StardewValley.Tools;

namespace InteractionTweaks
{
    public class ModEntry : Mod, IAssetEditor
    {
        //private bool eating_changes = true;
        private InteractionTweaksConfig config;

        public override void Entry(IModHelper helper)
        {
            //initializes mod features and reads config
            ModFeature.Init(this);

            if (config.EatingFeature || false/*config.WeaponBlockingFeature*/)
                EatingBlockingFeature.Enable();
            if (config.AdventurersGuildShopFeature || config.SlingshotFeature)
                AdventurersGuildFeature.Enable();
            if (config.CarpenterMenuFeature)
                CarpenterMenuFeature.Enable();
            //if (config.ToolsFeature)
            //     DontUseToolsFeature.Enable();

        }

        public InteractionTweaksConfig GetConfig() {
            if (config == null)
            {
                config = Helper.ReadConfig<InteractionTweaksConfig>();
            }
            return config;        
        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data/weapons");          
        }

        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/weapons"))
            {
                string[] dataArray = asset.AsDictionary<int, string>().Data[Slingshot.basicSlingshot].Split('/');
                dataArray[1] = Helper.Translation.Get("item.slingshotdescr");
                asset.AsDictionary<int, string>().Data[Slingshot.basicSlingshot] = string.Join("/", dataArray);

                dataArray = asset.AsDictionary<int, string>().Data[Slingshot.masterSlingshot].Split('/');
                dataArray[1] = Helper.Translation.Get("item.slingshotdescr");
                asset.AsDictionary<int, string>().Data[Slingshot.masterSlingshot] = string.Join("/", dataArray);
            }
        }

    }
}
