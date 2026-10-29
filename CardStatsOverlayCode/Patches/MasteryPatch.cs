using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

public enum MasteryTier { None, Bronze, Silver, Gold }

public static class Mastery
{
    public const int BronzeWins = 7;
    public const int SilverWins = 10;
    public const int GoldWins = 15;

    public static MasteryTier GetTier(int wins) => wins switch
    {
        >= GoldWins => MasteryTier.Gold,
        >= SilverWins => MasteryTier.Silver,
        >= BronzeWins => MasteryTier.Bronze,
        _ => MasteryTier.None
    };

    public static MasteryTier GetTier(string cardId) =>
        StatsTracker.Stats.TryGetValue(cardId, out var s) ? GetTier(s.RunsWon) : MasteryTier.None;

    public static void LogSummary()
    {
        var mastered = StatsTracker.Stats
            .Select(kv => (Id: kv.Key, Wins: kv.Value.RunsWon, Tier: GetTier(kv.Value.RunsWon)))
            .Where(x => x.Tier != MasteryTier.None)
            .OrderByDescending(x => x.Wins)
            .ToList();

        foreach (var x in mastered)
            GD.Print($"[CardStatsOverlay] Mastery {x.Tier}: {x.Id} ({x.Wins} wins)");

        var counts = mastered.GroupBy(x => x.Tier).ToDictionary(g => g.Key, g => g.Count());
        GD.Print($"[CardStatsOverlay] Mastery totals: " +
                 $"Gold {counts.GetValueOrDefault(MasteryTier.Gold)}, " +
                 $"Silver {counts.GetValueOrDefault(MasteryTier.Silver)}, " +
                 $"Bronze {counts.GetValueOrDefault(MasteryTier.Bronze)}");
    }
}

[HarmonyPatch(typeof(NCard), "Reload")]
public static class MasteryPatch
{
    private const string MetaKey = "mastery_sparkle";

    private static readonly AccessTools.FieldRef<NCard, GpuParticles2D> Sparkles =
        AccessTools.FieldRefAccess<NCard, GpuParticles2D>("_sparkles");

    public static void Postfix(NCard __instance)
    {
        if (!__instance.IsNodeReady()) return;

        var model = __instance.Model;
        bool mastered = model != null
            && __instance.Visibility.ToString() == "Visible"
            && Mastery.GetTier("CARD." + model.Id.Entry) != MasteryTier.None;

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