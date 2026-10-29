using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class MasteryPatch
{
    private const int SparkleWins = 5;
    private const string MetaKey = "mastery_sparkle";

    private static readonly AccessTools.FieldRef<NCard, GpuParticles2D> Sparkles =
        AccessTools.FieldRefAccess<NCard, GpuParticles2D>("_sparkles");

    public static void Postfix(NCard __instance)
    {
        if (!__instance.IsNodeReady()) return;

        var model = __instance.Model;
        bool mastered = model != null
            && __instance.Visibility.ToString() == "Visible"
            && StatsTracker.Stats.TryGetValue("CARD." + model.Id.Entry, out var s)
            && s.RunsWon >= SparkleWins;

        if (mastered)
        {
            Sparkles(__instance).Visible = true;
            __instance.SetMeta(MetaKey, true);
        }
        else if (__instance.HasMeta(MetaKey))
        {
            Sparkles(__instance).Visible = false;
            __instance.RemoveMeta(MetaKey);
        }
    }
}