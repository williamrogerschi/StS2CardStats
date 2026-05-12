using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Saves;

namespace CardStatsOverlay.CardStatsOverlayCode;

public class CardStats
{
    public int Offered { get; set; } = 0;
    public int Picked { get; set; } = 0;
    public int RunsCompleted { get; set; } = 0;
    public int RunsWon { get; set; } = 0;

    public float PickRate => Offered == 0 ? 0f : (float)Picked / Offered;
    public float WinRate => RunsCompleted == 0 ? 0f : (float)RunsWon / RunsCompleted;
}

public class SaveData
{
    public Dictionary<string, CardStats> Stats { get; set; } = new();
    public HashSet<string> PickedThisRun { get; set; } = new();
}

public static class StatsTracker
{
    private static string GetSavePath()
    {
        int profileId = SaveManager.Instance?.CurrentProfileId ?? 1;
        return Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "SlayTheSpire2", $"card_stats_overlay_profile{profileId}.json"
        );
    }

    public static Dictionary<string, CardStats> Stats { get; private set; } = new();
    private static HashSet<string> PickedThisRun = new();

    public static void Load()
    {
        Stats = new();
        PickedThisRun = new();

        string savePath = GetSavePath();
        if (!File.Exists(savePath))
        {
            GD.Print($"[CardStatsOverlay] No stats file found for profile {SaveManager.Instance?.CurrentProfileId ?? 1}, starting fresh.");
            return;
        }
        try
        {
            string json = File.ReadAllText(savePath);

            var saveData = JsonSerializer.Deserialize<SaveData>(json);
            if (saveData?.Stats != null)
            {
                Stats = saveData.Stats;
                PickedThisRun = saveData.PickedThisRun ?? new();
                GD.Print($"[CardStatsOverlay] Loaded stats for {Stats.Count} cards, {PickedThisRun.Count} cards picked this run.");
                return;
            }

            var oldStats = JsonSerializer.Deserialize<Dictionary<string, CardStats>>(json);
            if (oldStats != null)
            {
                Stats = oldStats;
                GD.Print($"[CardStatsOverlay] Loaded stats (old format) for {Stats.Count} cards.");
            }
        }
        catch (System.Exception e)
        {
            GD.Print($"[CardStatsOverlay] Failed to load stats: {e.Message}");
            Stats = new();
            PickedThisRun = new();
        }
    }

    public static void Save()
    {
        try
        {
            var saveData = new SaveData
            {
                Stats = Stats,
                PickedThisRun = PickedThisRun
            };
            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(GetSavePath(), json);
        }
        catch (System.Exception e)
        {
            GD.Print($"[CardStatsOverlay] Failed to save stats: {e.Message}");
        }
    }

    public static CardStats GetOrCreate(string cardId)
    {
        if (!Stats.TryGetValue(cardId, out var stats))
        {
            stats = new CardStats();
            Stats[cardId] = stats;
        }
        return stats;
    }

    public static void OnCardOffered(string cardId)
    {
        GetOrCreate(cardId).Offered++;
        Save();
    }

    public static void OnCardPicked(string cardId)
    {
        GetOrCreate(cardId).Picked++;
        PickedThisRun.Add(cardId);
        Save();
    }

    public static void OnRunEnded(bool won)
    {
        foreach (string cardId in PickedThisRun)
        {
            var stats = GetOrCreate(cardId);
            stats.RunsCompleted++;
            if (won) stats.RunsWon++;
        }
        PickedThisRun.Clear();
        Save();
    }
}