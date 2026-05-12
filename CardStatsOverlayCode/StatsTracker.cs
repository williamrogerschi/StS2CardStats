using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

public static class StatsTracker
{
    private static readonly string SavePath = Path.Combine(
        System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
        "SlayTheSpire2", "card_stats_overlay.json"
    );

    public static Dictionary<string, CardStats> Stats { get; private set; } = new();
    private static readonly HashSet<string> PickedThisRun = new();

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            MainFile.Logger.Info("No stats file found, starting fresh.");
            return;
        }
        try
        {
            string json = File.ReadAllText(SavePath);
            Stats = JsonSerializer.Deserialize<Dictionary<string, CardStats>>(json) ?? new();
            MainFile.Logger.Info($"Loaded stats for {Stats.Count} cards.");
        }
        catch (System.Exception e)
        {
            MainFile.Logger.Info($"Failed to load stats: {e.Message}");
            Stats = new();
        }
    }

    public static void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(Stats, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception e)
        {
            MainFile.Logger.Info($"Failed to save stats: {e.Message}");
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