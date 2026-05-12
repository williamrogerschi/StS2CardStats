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
        // Skip multiplayer runs
        if (serializableRun.Players.Count > 1)
        {
            StatsTracker.ClearRun();
            return;
        }

        var history = RunManager.Instance?.History;
        if (history?.WasAbandoned ?? false)
        {
            StatsTracker.ClearRun();
            return;
        }

        bool won = runState.CurrentRoom?.IsVictoryRoom ?? false;
        StatsTracker.OnRunEnded(won);
    }
}