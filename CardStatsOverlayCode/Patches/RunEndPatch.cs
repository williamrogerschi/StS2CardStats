using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NGameOverScreen), nameof(NGameOverScreen.Create))]
public static class RunEndPatch
{
    public static void Postfix(RunState runState, SerializableRun serializableRun)
    {
        bool won = runState.CurrentRoom?.IsVictoryRoom ?? false;
        StatsTracker.OnRunEnded(won);
    }
}