using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using Godot;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory.Initialize))]
public static class ShopOfferPatch
{
    public static void Postfix(MerchantInventory inventory)
    {
        if (StatsTracker.IsMultiplayerRun()) return;
        foreach (var entry in inventory.CharacterCardEntries)
        {
            if (entry.CreationResult == null) continue;
            string cardId = "CARD." + entry.CreationResult.Card.Id.Entry;
            StatsTracker.OnCardOffered(cardId);
            GD.Print($"[CardStatsOverlay] SHOP offered: {cardId}");
        }
        foreach (var entry in inventory.ColorlessCardEntries)
        {
            if (entry.CreationResult == null) continue;
            string cardId = "CARD." + entry.CreationResult.Card.Id.Entry;
            StatsTracker.OnCardOffered(cardId);
            GD.Print($"[CardStatsOverlay] SHOP offered: {cardId}");
        }
    }
}

[HarmonyPatch(typeof(MerchantCardEntry), "OnTryPurchase")]
public static class ShopBuyPatch
{
    public static void Postfix(MerchantCardEntry __instance)
    {
        if (StatsTracker.IsMultiplayerRun()) return;
        if (__instance.CreationResult == null) return;
        string cardId = "CARD." + __instance.CreationResult.Card.Id.Entry;
        StatsTracker.OnCardPicked(cardId);
        GD.Print($"[CardStatsOverlay] SHOP bought: {cardId}");
    }
}