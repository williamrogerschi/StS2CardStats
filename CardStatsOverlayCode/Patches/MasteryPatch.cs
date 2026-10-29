using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace CardStatsOverlay.CardStatsOverlayCode.Patches;

public static class Mastery
{
    // Thresholds
    public const int GradientWins = 10;       // gradient: total wins with the card
    public const float SparkleRate = 0.60f;   // sparkles: win rate...
    public const int SparkleMinRuns = 5;      // ...over at least this many completed runs

    public static bool HasGradient(CardStats s) => s.RunsWon >= GradientWins;
    public static bool HasSparkle(CardStats s) => s.RunsCompleted >= SparkleMinRuns && s.WinRate >= SparkleRate;

    public static CardStats? GetStats(NCard card)
    {
        var model = card.Model;
        if (model == null || card.Visibility.ToString() != "Visible") return null;
        return StatsTracker.Stats.TryGetValue("CARD." + model.Id.Entry, out var s) ? s : null;
    }

    public static void LogSummary()
    {
        var stats = StatsTracker.Stats;
        var grad = stats.Where(kv => HasGradient(kv.Value)).Select(kv => kv.Key).ToHashSet();
        var spark = stats.Where(kv => HasSparkle(kv.Value)).Select(kv => kv.Key).ToHashSet();
        var both = grad.Intersect(spark).ToList();
        GD.Print($"[CardStatsOverlay] Mastery: gradient {grad.Count}, sparkles {spark.Count}, both {both.Count}" +
                 (both.Count > 0 ? $" ({string.Join(", ", both)})" : ""));
    }
}

public static class MasteryHaze
{
    private const string NodeName = "MasteryHaze";

    // Knobs
    private const float Dim = 0.60f;           // overall darkness so cream text stays readable (1 = full bright)
    private const float GlowStrength = 0.25f;  // soft light spot in the middle (0 = off)
    private const float GlowSize = 0.35f;      // how wide that light spot is
    private const float BodyTop = 0.45f;       // where the text box starts (0 = card top, 1 = card bottom)
    private const float ChromaLo = 0.03f;      // body detection: less saturated than this = body
    private const float ChromaHi = 0.08f;      // more saturated than this = border (left alone)
    private const float LumLo = 0.10f;         // darker than this = outline (left alone)
    private const float LumHi = 0.18f;

    // Four corners of the text box + a center glow color
    private record Palette(Color TopLeft, Color TopRight, Color BottomLeft, Color BottomRight, Color Glow);

    private static readonly Dictionary<string, Palette> Palettes = new()
    {
        ["orange"] = new( // Regent: amber / salmon pink / gold / lavender
            new(0.85f, 0.55f, 0.15f), new(0.95f, 0.45f, 0.50f),
            new(0.95f, 0.75f, 0.25f), new(0.65f, 0.55f, 0.90f), new(1.00f, 0.92f, 0.85f)),
        ["pink"] = new(   // Necrobinder: magenta / violet / plum / periwinkle
            new(0.85f, 0.30f, 0.70f), new(0.50f, 0.30f, 0.85f),
            new(0.55f, 0.20f, 0.55f), new(0.55f, 0.65f, 0.95f), new(0.98f, 0.85f, 0.95f)),
        ["red"] = new(    // Ironclad: crimson / rust orange / wine / amber
            new(0.80f, 0.15f, 0.20f), new(0.90f, 0.45f, 0.15f),
            new(0.60f, 0.15f, 0.30f), new(0.95f, 0.70f, 0.30f), new(1.00f, 0.85f, 0.75f)),
        ["green"] = new(  // Silent: teal / sage lime / deep green / pale yellow
            new(0.15f, 0.65f, 0.60f), new(0.55f, 0.80f, 0.35f),
            new(0.15f, 0.45f, 0.30f), new(0.90f, 0.88f, 0.50f), new(0.85f, 1.00f, 0.90f)),
        ["blue"] = new(   // Defect: cyan / indigo / deep blue / lavender
            new(0.20f, 0.80f, 0.90f), new(0.35f, 0.30f, 0.85f),
            new(0.15f, 0.30f, 0.65f), new(0.70f, 0.60f, 0.95f), new(0.88f, 0.95f, 1.00f)),
        ["default"] = new( // Colorless / anything else: pearl
            new(0.85f, 0.80f, 0.70f), new(0.70f, 0.78f, 0.90f),
            new(0.75f, 0.70f, 0.80f), new(0.75f, 0.88f, 0.82f), new(1.00f, 1.00f, 1.00f)),
    };

    private const string ShaderCode = @"
shader_type canvas_item;
uniform vec4 c_tl : source_color;
uniform vec4 c_tr : source_color;
uniform vec4 c_bl : source_color;
uniform vec4 c_br : source_color;
uniform vec4 c_glow : source_color;
uniform vec2 rect_size;
uniform float dim = 0.6;
uniform float glow_strength = 0.25;
uniform float glow_size = 0.35;
uniform float body_top = 0.45;
uniform float chroma_lo = 0.03;
uniform float chroma_hi = 0.08;
uniform float lum_lo = 0.10;
uniform float lum_hi = 0.18;

// Pixel position inside the card, independent of how the texture is stored
varying vec2 local_pos;

void vertex() {
    local_pos = VERTEX;
}

void fragment() {
    vec4 tex = texture(TEXTURE, UV);

    // Body = grey pixels: low color (border has color) and not near-black (outlines)
    float i = 0.5959 * tex.r - 0.2746 * tex.g - 0.3213 * tex.b;
    float q = 0.2115 * tex.r - 0.5227 * tex.g + 0.3112 * tex.b;
    float lum = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    float body = (1.0 - smoothstep(chroma_lo, chroma_hi, length(vec2(i, q))))
               * smoothstep(lum_lo, lum_hi, lum);

    // 0-1 position inside the text box
    vec2 n = local_pos / rect_size;
    vec2 p = vec2(clamp(n.x, 0.0, 1.0), clamp((n.y - body_top) / (1.0 - body_top), 0.0, 1.0));
    vec2 s = smoothstep(0.0, 1.0, p);

    // Corner blend: mix left-right along the top and bottom, then top-bottom
    vec3 top = mix(c_tl.rgb, c_tr.rgb, s.x);
    vec3 bot = mix(c_bl.rgb, c_br.rgb, s.x);
    vec3 col = mix(top, bot, s.y);

    // Soft light spot
    float d = distance(p, vec2(0.5, 0.45)) / glow_size;
    col = mix(col, c_glow.rgb, exp(-d * d) * glow_strength);

    COLOR = vec4(col * dim, tex.a * body);
}";

    private static readonly AccessTools.FieldRef<NCard, TextureRect> Frame =
        AccessTools.FieldRefAccess<NCard, TextureRect>("_frame");

    private static Shader? _shader;
    private static readonly Dictionary<string, ShaderMaterial> _materials = new();
    private static readonly HashSet<string> _loggedPaths = new();

    private static string PaletteKey(string framePath)
    {
        foreach (var key in new[] { "orange", "pink", "red", "green", "blue" })
            if (framePath.Contains(key)) return key;
        return "default";
    }

    private static ShaderMaterial GetMaterial(string key, Vector2 size)
    {
        if (_materials.TryGetValue(key, out var m)) return m;
        _shader ??= new Shader { Code = ShaderCode };

        var p = Palettes[key];
        m = new ShaderMaterial { Shader = _shader };
        m.SetShaderParameter("c_tl", p.TopLeft);
        m.SetShaderParameter("c_tr", p.TopRight);
        m.SetShaderParameter("c_bl", p.BottomLeft);
        m.SetShaderParameter("c_br", p.BottomRight);
        m.SetShaderParameter("c_glow", p.Glow);
        m.SetShaderParameter("rect_size", size);
        m.SetShaderParameter("dim", Dim);
        m.SetShaderParameter("glow_strength", GlowStrength);
        m.SetShaderParameter("glow_size", GlowSize);
        m.SetShaderParameter("body_top", BodyTop);
        m.SetShaderParameter("chroma_lo", ChromaLo);
        m.SetShaderParameter("chroma_hi", ChromaHi);
        m.SetShaderParameter("lum_lo", LumLo);
        m.SetShaderParameter("lum_hi", LumHi);
        _materials[key] = m;
        return m;
    }

    public static void Apply(NCard card, bool on)
    {
        var frame = Frame(card);
        var overlay = frame.GetNodeOrNull<TextureRect>(NodeName);

        if (!on || !frame.Visible || frame.Texture == null)
        {
            if (overlay != null)
            {
                frame.RemoveChild(overlay);
                overlay.QueueFree();
            }
            return;
        }

        string path = frame.Material?.ResourcePath ?? "";
        string key = PaletteKey(path);
        if (_loggedPaths.Add(path))
            GD.Print($"[CardStatsOverlay] Haze palette: {path} -> {key}");

        if (overlay == null)
        {
            overlay = new TextureRect { Name = NodeName, MouseFilter = Control.MouseFilterEnum.Ignore };
            frame.AddChild(overlay);
        }

        // Mirror the frame exactly. Size set explicitly (not anchors) so it's correct
        // even when the card is set up off-screen while being reused from the pool.
        var size = frame.Size == Vector2.Zero ? NCard.defaultSize : frame.Size;
        overlay.Texture = frame.Texture;
        overlay.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        overlay.StretchMode = frame.StretchMode;
        overlay.FlipH = frame.FlipH;
        overlay.FlipV = frame.FlipV;
        overlay.Position = Vector2.Zero;
        overlay.Size = size;
        overlay.Material = GetMaterial(key, size);
    }
}

[HarmonyPatch(typeof(NCard), "Reload")]
public static class MasteryPatch
{
    private const string SparkleMeta = "mastery_sparkle";

    private static readonly AccessTools.FieldRef<NCard, GpuParticles2D> Sparkles =
        AccessTools.FieldRefAccess<NCard, GpuParticles2D>("_sparkles");

    public static void Postfix(NCard __instance)
    {
        if (!__instance.IsNodeReady()) return;

        var stats = Mastery.GetStats(__instance);
        bool gradient = stats != null && Mastery.HasGradient(stats);
        bool sparkle = stats != null && Mastery.HasSparkle(stats);

        try { MasteryHaze.Apply(__instance, gradient); }
        catch (Exception e) { GD.PrintErr($"[CardStatsOverlay] MasteryHaze failed: {e}"); }

        if (sparkle)
        {
            Sparkles(__instance).Visible = true;
            __instance.SetMeta(SparkleMeta, true);
        }
        else if (__instance.HasMeta(SparkleMeta))
        {
            Sparkles(__instance).Visible = false;
            __instance.RemoveMeta(SparkleMeta);
        }
    }
}