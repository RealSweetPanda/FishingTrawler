﻿using FishingTrawler.Framework.Objects.Items.Rewards;
using FishingTrawler.Framework.Objects.Items.Tools;
using FishingTrawler.Framework.Utilities;
using FishingTrawler.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Objects.BedFurniture;

namespace FishingTrawler.Framework.Patches.Objects
{
    internal class FurniturePatch : PatchTemplate
    {
        private readonly Type _object = typeof(Furniture);

        public FurniturePatch(IMonitor modMonitor, IModHelper modHelper) : base(modMonitor, modHelper)
        {

        }

        internal override void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(StardewValley.Object), "get_DisplayName", null), postfix: new HarmonyMethod(GetType(), nameof(GetNamePostfix)));
            harmony.Patch(AccessTools.Method(_object, "get_description", null), postfix: new HarmonyMethod(GetType(), nameof(GetDescriptionPostfix)));

            harmony.Patch(AccessTools.Method(_object, nameof(Furniture.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }), prefix: new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix)));
            harmony.Patch(AccessTools.Method(typeof(StardewValley.Object), "drawPlacementBounds", new[] { typeof(SpriteBatch), typeof(GameLocation) }), prefix: new HarmonyMethod(GetType(), nameof(DrawPlacementBoundsPrefix)));
            harmony.Patch(AccessTools.Method(_object, nameof(Furniture.placementAction), new[] { typeof(GameLocation), typeof(int), typeof(int), typeof(Farmer) }), prefix: new HarmonyMethod(GetType(), nameof(PlacementActionPrefix)));
        }

        private static void GetNamePostfix(StardewValley.Object __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.ANCIENT_FLAG_KEY))
            {
                __result = AncientFlag.GetFlagName(AncientFlag.GetFlagType(__instance));
                return;
            }
        }

        private static void GetDescriptionPostfix(Furniture __instance, ref string __result)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.ANCIENT_FLAG_KEY))
            {
                __result = AncientFlag.GetFlagDescription(AncientFlag.GetFlagType(__instance));
                return;
            }
        }

        private static bool DrawInMenuPrefix(Furniture __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.ANCIENT_FLAG_KEY))
            {
                var flagType = AncientFlag.GetFlagType(__instance);
                var flagTexture = FishingTrawler.assetManager.ancientFlagsTexture;
                var sourceRectangle = new Rectangle(32 * (int)flagType % flagTexture.Width, 32 * (int)flagType / flagTexture.Width * 32, 32, 32); ;

                spriteBatch.Draw(flagTexture, location + new Vector2(32f, 32f), sourceRectangle, color * transparency, 0f, new Vector2(sourceRectangle.Width / 2, sourceRectangle.Height / 2), 2f * scaleSize, SpriteEffects.None, layerDepth);
                if (((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && __instance.Stack != int.MaxValue)
                {
                    Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
                }
                return false;
            }

            return true;
        }

        private static bool DrawPlacementBoundsPrefix(StardewValley.Object __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.ANCIENT_FLAG_KEY) && location is Beach)
            {
                // Draw nothing to avoid covering up Murphy when attempting to give him an ancient flag
                return false;
            }

            return true;
        }

        private static bool PlacementActionPrefix(Furniture __instance, GameLocation location, int x, int y, Farmer who = null)
        {
            if (__instance.modData.ContainsKey(ModDataKeys.ANCIENT_FLAG_KEY))
            {
                var flagType = AncientFlag.GetFlagType(__instance);
                if (flagType is FlagType.Unknown)
                {
                    Game1.showRedMessage(_helper.Translation.Get("game_message.identify_flag_first"));
                    return false;
                }
            }

            return true;
        }
    }
}
