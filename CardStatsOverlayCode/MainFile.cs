using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace CardStatsOverlay.CardStatsOverlayCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "CardStatsOverlay";
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        try
        {
            StatsTracker.Load();
            GD.Print("[CardStatsOverlay] Stats loaded!");

            Harmony harmony = new(ModId);
            harmony.PatchAll();
            GD.Print("[CardStatsOverlay] Harmony patches applied!");
        }
        catch (System.Exception e)
        {
            GD.PrintErr($"[CardStatsOverlay] Error during initialization: {e}");
        }
    }
}