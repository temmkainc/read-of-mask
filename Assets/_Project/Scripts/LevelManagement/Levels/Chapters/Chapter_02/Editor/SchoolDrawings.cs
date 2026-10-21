// The paper things on the school's walls, drawn in the game's black marker on off-white paper:
// faceless portraits for above the blackboards and for the honor board, wall-newspaper sheets of
// scribbled writing, a periodic table, a map, a geometry poster, a flag and emblem, a clock face.
//
// Every one is a PNG in Sprites/School/Props, drawn only if it isn't there yet - so paint over any of
// them, or replace it with your own drawing (any size; it's fitted into its frame keeping its
// proportions), and the school keeps it through every rebuild. Each placement has a file of its own,
// named kind@where (e.g. Photo@HonorBoard_03, Sheet_2@SchoolNews_1, Portrait_3@Literature_2), so each
// photo on the honor board, each sheet on each stand, each portrait in each room can be different.
// Only Tools → Read of Mask → School Props → Redraw Wall Drawings overwrites them (it asks first).

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SchoolDrawings
{
    public const string Folder = "Assets/_Project/Sprites/School/Props";

    public static readonly string[] Names =
    {
        "Portrait_1", "Portrait_2", "Portrait_3", "Portrait_4", "Photo",
        "Sheet_1", "Sheet_2", "Sheet_3", "PeriodicTable", "Map", "Geometry", "Clock", "Flag", "Emblem",
        "Heart", "Doodle",
    };

    // the copies of one kind of drawing differ a little, like things drawn one by one
    private static int _variant;

    /// <summary>The drawing as a sprite, drawing it first if it isn't there yet. <paramref name="name"/> is
    /// a kind (one of <see cref="Names"/>), or kind@where for a placement's own copy.</summary>
    public static Sprite Get(string name)
    {
        string path = $"{Folder}/{name}.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
            Draw(name, path);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    [MenuItem("Tools/Read of Mask/School Props/Redraw Wall Drawings")]
    public static void RedrawAll()
    {
        if (!EditorUtility.DisplayDialog("Redraw wall drawings",
                $"Redraw every drawing in {Folder}? Anything you painted there will be replaced.", "Redraw", "Cancel"))
            return;
        var names = new HashSet<string>(Names);
        foreach (var file in System.IO.Directory.GetFiles(Folder, "*.png"))
            names.Add(System.IO.Path.GetFileNameWithoutExtension(file));
        foreach (var name in names)
            Draw(name, $"{Folder}/{name}.png");
        Debug.Log($"[SchoolDrawings] Drew {names.Count} drawings into {Folder}.");
    }

    private static void Draw(string name, string path)
    {
        System.IO.Directory.CreateDirectory(Folder);
        int at = name.IndexOf('@');
        string kind = at >= 0 ? name.Substring(0, at) : name;
        _variant = at >= 0 ? StableHash(name) : 0;
        var canvas = kind switch
        {
            "Portrait_1" => Portrait(0), "Portrait_2" => Portrait(1), "Portrait_3" => Portrait(2), "Portrait_4" => Portrait(3),
            "Photo" => Photo(),
            "Sheet_1" => Sheet(1), "Sheet_2" => Sheet(2), "Sheet_3" => Sheet(3),
            "PeriodicTable" => PeriodicTable(),
            "Map" => Map(),
            "Geometry" => Geometry(),
            "Flag" => Flag(),
            "Emblem" => Emblem(),
            "Heart" => Heart(),
            "Doodle" => Doodle(),
            _ => Clock(),
        };
        System.IO.File.WriteAllBytes(path, canvas.ToTexture().EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    private static int StableHash(string s)
    {
        unchecked
        {
            int h = 17;
            foreach (char ch in s)
                h = h * 31 + ch;
            return h & 0xffff;
        }
    }

    private const float Ink = 0.07f, Paper = 0.93f;

    // a faceless bust: an oval with no face, a neck, shoulders and a jacket - with hair, glasses, a beard
    private static Canvas Portrait(int variant)
    {
        var c = new Canvas(384, 480, Paper, 11 + variant);
        float cx = 192;
        c.Ellipse(cx, 300, 70, 92, 6);                                         // head
        c.Line(6, (cx - 30, 212), (cx - 34, 170));                              // neck
        c.Line(6, (cx + 30, 212), (cx + 34, 170));
        c.Line(7, (40, 0), (60, 120), (cx - 34, 170));                           // shoulders
        c.Line(7, (344, 0), (324, 120), (cx + 34, 170));
        c.Line(5, (cx - 34, 170), (cx, 90), (cx + 34, 170));                     // collar and lapels
        c.Line(5, (cx - 50, 150), (cx - 12, 40));
        c.Line(5, (cx + 50, 150), (cx + 12, 40));
        switch (variant)
        {
            case 0:  // swept-back hair
                c.Line(6, (cx - 66, 330), (cx - 40, 388), (cx + 10, 396), (cx + 60, 372), (cx + 70, 320));
                break;
            case 1:  // round glasses, a bald crown
                c.Ellipse(cx - 30, 312, 22, 18, 5);
                c.Ellipse(cx + 30, 312, 22, 18, 5);
                c.Line(4, (cx - 8, 314), (cx + 8, 314));
                break;
            case 2:  // a full beard
                for (int i = 0; i < 9; i++)
                {
                    float a = -60f + i * 15f;
                    float x0 = cx + Mathf.Sin(a * Mathf.Deg2Rad) * 64, y0 = 290 - Mathf.Cos(a * Mathf.Deg2Rad) * 40;
                    c.Line(4, (x0, y0), (x0 + Mathf.Sin(a * Mathf.Deg2Rad) * 14, y0 - 70 + Mathf.Abs(a) * 0.4f));
                }
                c.Line(6, (cx - 64, 330), (cx, 400), (cx + 64, 330));
                break;
            default: // a high collar and a bow tie
                c.Line(5, (cx - 26, 172), (cx - 4, 160), (cx - 26, 148), (cx - 26, 172));
                c.Line(5, (cx + 26, 172), (cx + 4, 160), (cx + 26, 148), (cx + 26, 172));
                c.Line(6, (cx - 64, 340), (cx - 20, 392), (cx + 40, 390), (cx + 66, 340));
                break;
        }
        c.Frame(10);
        return c;
    }

    // an honor-board photo: the same faceless bust, smaller, on a gray studio background
    private static Canvas Photo()
    {
        var c = new Canvas(240, 300, 0.62f, 21);
        c.Ellipse(120, 190, 46, 58, 4, fill: Paper);
        c.Line(5, (24, 0), (40, 72), (120, 110), (200, 72), (216, 0));
        c.Line(4, (120, 110), (100, 30));
        c.Line(4, (120, 110), (140, 30));
        c.Frame(6);
        return c;
    }

    // a wall-newspaper sheet: a fat heading, a drawn picture, columns of scribbled writing
    private static Canvas Sheet(int variant)
    {
        var c = new Canvas(360, 480, Paper, 31 + variant);
        c.Scribble(30, 430, 300, 26, 7);                                       // heading
        float pictureTop = 400, pictureBottom = 250;
        c.Rect(30, pictureBottom, 170, pictureTop, 4);
        switch (variant)
        {
            case 1:  // a rocket going up
                c.Line(5, (100, 270), (100, 360), (115, 390), (130, 360), (130, 270), (100, 270));
                c.Line(4, (100, 280), (85, 262)); c.Line(4, (130, 280), (145, 262));
                c.Ellipse(115, 330, 8, 8, 3);
                break;
            case 2:  // a house and the sun
                c.Line(5, (60, 260), (60, 320), (100, 350), (140, 320), (140, 260), (60, 260));
                c.Ellipse(160, 375, 14, 14, 4);
                break;
            default: // a flower in a pot
                c.Line(5, (100, 260), (100, 340));
                c.Ellipse(100, 355, 18, 18, 4);
                c.Line(5, (80, 252), (120, 252), (114, 270), (86, 270), (80, 252));
                break;
        }
        for (int i = 0; i < 6; i++)
            c.Scribble(190, 390 - i * 26, 140, 10, 3);
        for (int i = 0; i < 8; i++)
            c.Scribble(30, 220 - i * 26, i == 7 ? 160 : 300, 10, 3);
        c.Frame(6);
        return c;
    }

    // Mendeleev's table: two tall columns, the transition block, the gap, the lanthanides below
    private static Canvas PeriodicTable()
    {
        var c = new Canvas(720, 420, Paper, 41);
        c.Scribble(220, 392, 280, 18, 6);
        float cell = 36, x0 = 36, y0 = 340;
        for (int period = 0; period < 7; period++)
        for (int group = 0; group < 18; group++)
        {
            bool exists = period == 0 ? group == 0 || group == 17
                : period < 3 ? group < 2 || group > 11
                : true;
            if (!exists)
                continue;
            float x = x0 + group * cell, y = y0 - period * cell;
            c.Rect(x, y - cell, x + cell, y, 2.5f);
            c.Scribble(x + 8, y - 14, 14, 6, 2);
        }
        for (int row = 0; row < 2; row++)
        for (int i = 0; i < 14; i++)
        {
            float x = x0 + (i + 3) * cell, y = 70 - row * cell;
            c.Rect(x, y - cell, x + cell, y, 2.5f);
        }
        c.Frame(8);
        return c;
    }

    // a wall map: one big wobbly landmass, a sea, a few rivers and towns, a grid
    private static Canvas Map()
    {
        var c = new Canvas(720, 480, Paper, 51);
        for (int i = 1; i < 6; i++)
            c.Line(1.5f, (i * 120, 20), (i * 120, 460));
        for (int i = 1; i < 4; i++)
            c.Line(1.5f, (20, i * 120), (700, i * 120));
        var coast = new List<(float, float)>();
        var random = new System.Random(52);
        for (int i = 0; i <= 40; i++)
        {
            float a = i / 40f * Mathf.PI * 2f;
            float r = 1f + (float)(random.NextDouble() - 0.5) * 0.25f;
            coast.Add((360 + Mathf.Cos(a) * 260 * r, 240 + Mathf.Sin(a) * 170 * r));
        }
        c.Line(5, coast.ToArray());
        c.Line(3, (300, 120), (330, 200), (310, 260), (350, 330));
        c.Line(3, (460, 150), (430, 230), (470, 300));
        foreach (var (x, y) in new[] { (330f, 200f), (470f, 300f), (250f, 280f), (420f, 360f) })
            c.Ellipse(x, y, 7, 7, 4, fill: Ink);
        c.Frame(8);
        return c;
    }

    // a geometry poster: a right triangle, a circle with its radius, a cube, a formula
    private static Canvas Geometry()
    {
        var c = new Canvas(480, 600, Paper, 61);
        c.Scribble(120, 560, 240, 22, 6);
        c.Line(5, (60, 330), (260, 330), (60, 500), (60, 330));
        c.Line(3, (60, 360), (90, 360), (90, 330));
        c.Ellipse(360, 420, 80, 80, 5);
        c.Line(4, (360, 420), (436, 444));
        c.Ellipse(360, 420, 5, 5, 3, fill: Ink);
        c.Line(5, (90, 90), (250, 90), (250, 250), (90, 250), (90, 90));
        c.Line(5, (90, 250), (150, 300), (310, 300), (250, 250));
        c.Line(5, (310, 300), (310, 140), (250, 90));
        c.Scribble(300, 200, 140, 14, 4);
        c.Scribble(300, 160, 120, 14, 4);
        c.Frame(8);
        return c;
    }

    // the state flag on its pole: two bands, the lower one hatched - no particular country's
    private static Canvas Flag()
    {
        var c = new Canvas(360, 480, Paper, 81);
        c.Line(8, (60, 30), (60, 450));
        c.Ellipse(60, 456, 9, 9, 4, fill: Ink);
        var top = new List<(float, float)>();
        var middle = new List<(float, float)>();
        var bottom = new List<(float, float)>();
        for (int i = 0; i <= 12; i++)
        {
            float x = 60 + i * 22f, wave = Mathf.Sin(i * 0.55f) * 14f;
            top.Add((x, 430 + wave));
            middle.Add((x, 340 + wave));
            bottom.Add((x, 250 + wave));
        }
        c.Line(5, top.ToArray());
        c.Line(4, middle.ToArray());
        c.Line(5, bottom.ToArray());
        c.Line(5, (324, 430 + Mathf.Sin(6.6f) * 14f), (324, 250 + Mathf.Sin(6.6f) * 14f));
        for (int i = 0; i < 14; i++)
        {
            float x = 70 + i * 18f, wave = Mathf.Sin((x - 60f) / 22f * 0.55f) * 14f;
            c.Line(2.5f, (x, 258 + wave), (x + 14, 332 + wave));
        }
        c.Frame(6);
        return c;
    }

    // the state emblem: a shield with a star in it, wreathed in leaves
    private static Canvas Emblem()
    {
        var c = new Canvas(360, 420, Paper, 91);
        float cx = 180;
        c.Line(6, (cx - 90, 330), (cx + 90, 330), (cx + 90, 210), (cx, 110), (cx - 90, 210), (cx - 90, 330));
        var star = new (float, float)[11];
        for (int i = 0; i <= 10; i++)
        {
            float a = (i * 36f + 90f) * Mathf.Deg2Rad, r = i % 2 == 0 ? 60f : 25f;
            star[i] = (cx + Mathf.Cos(a) * r, 240 + Mathf.Sin(a) * r);
        }
        c.Line(5, star);
        foreach (float side in new[] { -1f, 1f })
            for (int i = 0; i < 7; i++)
            {
                float a = (200f + i * 22f) * Mathf.Deg2Rad;
                float x = cx + side * Mathf.Cos(a) * -135f, y = 230 + Mathf.Sin(a) * 135f;
                c.Ellipse(x, y, 16, 9, 3);
            }
        c.Scribble(cx - 80, 50, 160, 14, 4);
        c.Frame(6);
        return c;
    }

    // toilet-wall graffiti, on nothing (clear): a heart with an arrow through it
    private static Canvas Heart()
    {
        var c = new Canvas(320, 280, Paper, 101, clear: true);
        var heart = new List<(float, float)>();
        for (int i = 0; i <= 40; i++)
        {
            float t = i / 40f * Mathf.PI * 2f;
            float x = 16f * Mathf.Pow(Mathf.Sin(t), 3f);
            float y = 13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t);
            heart.Add((160 + x * 6f, 150 + y * 6f));
        }
        c.Line(7, heart.ToArray());
        c.Line(5, (30, 60), (290, 230));
        c.Line(5, (290, 230), (262, 228)); c.Line(5, (290, 230), (282, 204));
        c.Line(4, (30, 60), (44, 82)); c.Line(4, (30, 60), (52, 64));
        return c;
    }

    // toilet-wall graffiti, on nothing (clear): a stick figure with a big round head, and a sun
    private static Canvas Doodle()
    {
        var c = new Canvas(300, 320, Paper, 111, clear: true);
        c.Ellipse(110, 230, 40, 40, 6);
        c.Line(6, (110, 190), (110, 90));
        c.Line(6, (60, 160), (110, 150), (160, 175));
        c.Line(6, (70, 20), (110, 90), (150, 20));
        c.Ellipse(240, 270, 24, 24, 5);
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            c.Line(4, (240 + Mathf.Cos(a) * 32, 270 + Mathf.Sin(a) * 32), (240 + Mathf.Cos(a) * 46, 270 + Mathf.Sin(a) * 46));
        }
        return c;
    }

    // a round wall clock's face: twelve marks, the hands at twenty to four
    private static Canvas Clock()
    {
        var c = new Canvas(400, 400, Paper, 71, clear: true);
        c.Ellipse(200, 200, 190, 190, 1, fill: Paper);
        c.Ellipse(200, 200, 184, 184, 10);
        for (int i = 0; i < 12; i++)
        {
            float a = i * 30f * Mathf.Deg2Rad;
            float r0 = i % 3 == 0 ? 130 : 150;
            c.Line(i % 3 == 0 ? 10 : 6, (200 + Mathf.Sin(a) * r0, 200 + Mathf.Cos(a) * r0), (200 + Mathf.Sin(a) * 170, 200 + Mathf.Cos(a) * 170));
        }
        float hour = (3f + 40f / 60f) * 30f * Mathf.Deg2Rad, minute = 40f * 6f * Mathf.Deg2Rad;
        c.Line(12, (200, 200), (200 + Mathf.Sin(hour) * 90, 200 + Mathf.Cos(hour) * 90));
        c.Line(7, (200, 200), (200 + Mathf.Sin(minute) * 140, 200 + Mathf.Cos(minute) * 140));
        c.Ellipse(200, 200, 10, 10, 4, fill: Ink);
        return c;
    }

    // A small raster for marker drawings: strokes are drawn as round-capped thick lines, each one
    // rasterised only over its own bounding box, a little wobbly like a hand.
    private sealed class Canvas
    {
        private readonly int _w, _h;
        private readonly float[] _shade, _alpha;
        private readonly System.Random _random;

        public Canvas(int w, int h, float paper, int seed, bool clear = false)
        {
            _w = w; _h = h;
            _shade = new float[w * h];
            _alpha = new float[w * h];
            _random = new System.Random(seed + _variant);
            for (int i = 0; i < _shade.Length; i++)
            {
                _shade[i] = paper;
                _alpha[i] = clear ? 0f : 1f;
            }
        }

        private float Wobble(float amount) => (float)(_random.NextDouble() - 0.5) * amount;

        public void Line(float width, params (float x, float y)[] points)
        {
            for (int i = 0; i < points.Length - 1; i++)
                Segment(points[i].x + Wobble(2), points[i].y + Wobble(2), points[i + 1].x + Wobble(2), points[i + 1].y + Wobble(2), width);
        }

        public void Ellipse(float cx, float cy, float rx, float ry, float width, float fill = -1f)
        {
            if (fill >= 0f)
                for (int y = Mathf.Max(0, (int)(cy - ry)); y < Mathf.Min(_h, (int)(cy + ry) + 1); y++)
                for (int x = Mathf.Max(0, (int)(cx - rx)); x < Mathf.Min(_w, (int)(cx + rx) + 1); x++)
                {
                    float dx = (x - cx) / rx, dy = (y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        _shade[y * _w + x] = fill;
                        _alpha[y * _w + x] = 1f;
                    }
                }
            const int steps = 48;
            var pts = new (float, float)[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float a = i / (float)steps * Mathf.PI * 2f;
                pts[i] = (cx + Mathf.Cos(a) * rx, cy + Mathf.Sin(a) * ry);
            }
            for (int i = 0; i < steps; i++)
                Segment(pts[i].Item1, pts[i].Item2, pts[i + 1].Item1, pts[i + 1].Item2, width);
        }

        public void Rect(float x0, float y0, float x1, float y1, float width) =>
            Line(width, (x0, y0), (x1, y0), (x1, y1), (x0, y1), (x0, y0));

        // handwriting seen from a distance: a run of small loops along a line
        public void Scribble(float x, float y, float length, float height, float width)
        {
            var pts = new List<(float, float)>();
            for (float t = 0; t <= length; t += height * 0.45f)
                pts.Add((x + t + Wobble(height * 0.3f), y + (pts.Count % 2 == 0 ? 0f : height) + Wobble(height * 0.25f)));
            for (int i = 0; i < pts.Count - 1; i++)
                Segment(pts[i].Item1, pts[i].Item2, pts[i + 1].Item1, pts[i + 1].Item2, width);
        }

        public void Frame(float width) => Rect(width * 0.5f, width * 0.5f, _w - width * 0.5f, _h - width * 0.5f, width);

        private void Segment(float ax, float ay, float bx, float by, float width)
        {
            float r = width * 0.5f;
            int x0 = Mathf.Max(0, (int)(Mathf.Min(ax, bx) - r - 1)), x1 = Mathf.Min(_w - 1, (int)(Mathf.Max(ax, bx) + r + 1));
            int y0 = Mathf.Max(0, (int)(Mathf.Min(ay, by) - r - 1)), y1 = Mathf.Min(_h - 1, (int)(Mathf.Max(ay, by) + r + 1));
            float abx = bx - ax, aby = by - ay, len2 = Mathf.Max(abx * abx + aby * aby, 1e-6f);
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float px = x + 0.5f - ax, py = y + 0.5f - ay;
                float t = Mathf.Clamp01((px * abx + py * aby) / len2);
                float dx = px - abx * t, dy = py - aby * t;
                float coverage = Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                if (coverage <= 0f)
                    continue;
                int i = y * _w + x;
                _shade[i] = Mathf.Lerp(_shade[i], Ink, coverage);
                _alpha[i] = Mathf.Max(_alpha[i], coverage);
            }
        }

        public Texture2D ToTexture()
        {
            var pixels = new Color32[_w * _h];
            for (int i = 0; i < pixels.Length; i++)
            {
                byte g = (byte)(Mathf.Clamp01(_shade[i]) * 255f);
                pixels[i] = new Color32(g, g, g, (byte)(Mathf.Clamp01(_alpha[i]) * 255f));
            }
            var texture = new Texture2D(_w, _h, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }
    }
}
#endif
