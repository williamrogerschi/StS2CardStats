using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;
using Godot;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.InitProfileId))]
public static class ProfileInitPatch
{
    public static void Postfix()
    {
        GD.Print($"[CardStatsOverlay] Profile initialized: {SaveManager.Instance?.CurrentProfileId}");
        StatsTracker.Load();
        Mastery.LogSummary();
    }
}

[HarmonyPatch(typeof(SaveManager), nameof(SaveManager.SwitchProfileId))]
public static class ProfileSwitchPatch
{
    public static void Postfix()
    {
        GD.Print($"[CardStatsOverlay] Profile switched to: {SaveManager.Instance?.CurrentProfileId}");
        StatsTracker.Load();
        Mastery.LogSummary();
    }
}