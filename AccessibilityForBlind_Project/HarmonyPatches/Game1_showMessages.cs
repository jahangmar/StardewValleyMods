// Copyright (c) 2020 Jahangmar
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using StardewValley;
using Harmony;

namespace AccessibilityForBlind.HarmonyPatches
{
    public class Game1_showMessages
    {
        public static void Patch(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.showRedMessage), new Type[] { typeof(string) }),
                new HarmonyMethod(AccessTools.Method(typeof(Game1_showMessages), nameof(showRedMessage_prefix))));

            harmony.Patch(AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.showGlobalMessage), new Type[] { typeof(string) }),
                new HarmonyMethod(AccessTools.Method(typeof(Game1_showMessages), nameof(showRedMessage_prefix))));

            harmony.Patch(AccessTools.Method(typeof(StardewValley.Game1), nameof(StardewValley.Game1.addHUDMessage), new Type[] { typeof(HUDMessage) }),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(Game1_showMessages), nameof(showHUDMessage_postfix))));
        }

        private static bool showRedMessage_prefix(string message)
        {
            TextToSpeech.Speak(message);
            return true;
        }

        private static void showHUDMessage_postfix()
        {
            if (Game1.hudMessages.Count < 0)
                return;
            bool first = false;
            foreach (HUDMessage message in Game1.hudMessages)
            {
                //HUDMessage message = Game1.hudMessages[Game1.hudMessages.Count-1];
                switch (message.whatType)
                {
                    case HUDMessage.achievement_type:
                        TextToSpeech.Speak("archievement: " + message.Message, first);
                        break;
                    case HUDMessage.error_type:
                        TextToSpeech.Speak("error: " + message.Message, first);
                        break;
                    case HUDMessage.health_type:
                        TextToSpeech.Speak("health: " + message.Message, first);
                        break;
                    case HUDMessage.stamina_type:
                        TextToSpeech.Speak("stamina: " + message.Message, first);
                        break;
                    case HUDMessage.newQuest_type:
                        TextToSpeech.Speak("quest: " + message.Message, first);
                        break;
                    case HUDMessage.screenshot_type:
                        TextToSpeech.Speak("screenshot: " + message.Message, first);
                        break;
                    default:
                        Item item = ModEntry.GetHelper().Reflection.GetField<Item>(message, "messageSubject").GetValue();
                        TextToSpeech.Speak("received " + TextToSpeech.ItemToSpeech(item), first);
                        break;
                }
                first = false;
            }
        }

    }
}
