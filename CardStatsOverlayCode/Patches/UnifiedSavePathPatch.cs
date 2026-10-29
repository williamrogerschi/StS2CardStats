using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(UserDataPathProvider), nameof(UserDataPathProvider.GetAccountDir))]
public static class UnifiedSavePathPatch
{
    public static bool Prefix(ref string __result)
    {
        __result = "";
        return false;
    }
}