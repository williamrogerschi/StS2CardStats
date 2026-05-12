using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using System.Collections.Generic;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
public static class CardHoverPatch
{
    private static string Colorize(float value)
    {
        if (value >= 0.75f) return "#B44FE8"; // Purple
        if (value >= 0.50f) return "#4CAF50"; // Green
        if (value >= 0.20f) return "#FFC107"; // Yellow
        return "#F44336";                      // Red
    }

    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        string cardId = "CARD." + __instance.Id.Entry;
        if (!StatsTracker.Stats.TryGetValue(cardId, out var stats)) return;
        if (stats.Offered < 3) return;

        string pickColor = Colorize(stats.PickRate);
        string winColor = stats.RunsCompleted > 0 ? Colorize(stats.WinRate) : "#AAAAAA";

        string pickText = $"Pick: [color={pickColor}]{stats.PickRate:P0}[/color] ({stats.Picked}/{stats.Offered})";
        string winText = stats.RunsCompleted > 0
            ? $"Win: [color={winColor}]{stats.WinRate:P0}[/color] ({stats.RunsWon}/{stats.RunsCompleted})"
            : "Win: [color=#AAAAAA]N/A[/color]";

var tip = new HoverTip(
    new LocString("gameplay_ui", "CARD_RARITY.BASIC"),
    $"{pickText}\n{winText}",
    null
);

object boxed = tip;
typeof(HoverTip).GetProperty("Title")!.SetValue(boxed, "Card Stats");
tip = (HoverTip)boxed;

        var list = new List<IHoverTip>(__result) { tip };
        __result = list;
    }
}