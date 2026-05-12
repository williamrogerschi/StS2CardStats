using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using System.Collections.Generic;
using Godot;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NCardRewardSelectionScreen), nameof(NCardRewardSelectionScreen.RefreshOptions))]
public static class CardOfferPatch
{
    public static void Postfix(IReadOnlyList<CardCreationResult> options)
    {
        if (StatsTracker.IsMultiplayerRun()) return;
        foreach (var option in options)
        {
            string cardId = "CARD." + option.Card.Id.Entry;
            StatsTracker.OnCardOffered(cardId);
            GD.Print($"[CardStatsOverlay] DRAFT offered: {cardId}");
        }
    }
}