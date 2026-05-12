using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NCardRewardSelectionScreen), "SelectCard")]
public static class CardPickPatch
{
    public static void Postfix(NCardHolder cardHolder)
    {
        var cardModel = cardHolder.CardModel;
        if (cardModel != null)
        {
            StatsTracker.OnCardPicked("CARD." + cardModel.Id.Entry);
        }
    }
}