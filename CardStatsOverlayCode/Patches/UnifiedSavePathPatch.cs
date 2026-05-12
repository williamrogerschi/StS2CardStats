using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(UserDataPathProvider), nameof(UserDataPathProvider.GetProfileDir))]
public static class UnifiedSavePathPatch
{
    public static bool Prefix(int profileId, ref string __result)
    {
        __result = $"profile{profileId}";
        return false;
    }
}