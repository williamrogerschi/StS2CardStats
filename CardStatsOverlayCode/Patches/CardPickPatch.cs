using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using Godot;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NCardRewardSelectionScreen), "SelectCard")]
public static class CardPickPatch
{
    public static void Postfix(NCardHolder cardHolder)
    {
        if (StatsTracker.IsMultiplayerRun()) return;
        var cardModel = cardHolder.CardModel;
        if (cardModel != null)
        {
            string cardId = "CARD." + cardModel.Id.Entry;
            StatsTracker.OnCardPicked(cardId);
            GD.Print($"[CardStatsOverlay] DRAFT picked: {cardId}");
        }
    }
}