// Tools → Read of Mask → School Figures → Draw Masks
// Draws the school kids' masks: the player's own mask from the first-person mask sprites (a shield
// shape, almond eyes, a hooked nose line) seen from the front, in the same thick black marker line on
// white, with a few different mouths. Writes one PNG per face into Sprites/School/Masks - they're
// ordinary textures, so any of them can be redrawn by hand and SchoolFigureTools picks it up.

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class SchoolMaskArt
{
    public const string Folder = "Assets/_Project/Sprites/School/Masks";
    public static readonly string[] Faces = { "Smile", "Flat", "Sad", "Open" };

    private const int Width = 512, Height = 640;
    // line widths, as a fraction of the mask's width
    private const float Outline = 0.032f, Feature = 0.026f, Detail = 0.015f;

    public static string PathFor(string face) => $"{Folder}/SchoolMask_{face}.png";

    [MenuItem("Tools/Read of Mask/School Figures/Draw Masks")]
    public static void DrawAll()
    {
        System.IO.Directory.CreateDirectory(Folder);
        for (int i = 0; i < Faces.Length; i++)
        {
            string path = PathFor(Faces[i]);
            System.IO.File.WriteAllBytes(path, Draw(Faces[i], i).EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        Debug.Log($"[SchoolMaskArt] Drew {Faces.Length} masks into {Folder}.");
    }

    private struct Stroke
    {
        public List<Vector2> Points;
        public float Width;
    }

    private static Texture2D Draw(string face, int seed)
    {
        var random = new System.Random(17 + seed * 31);
        float Wobble() => (float)(random.NextDouble() - 0.5) * 0.008f;

        // the shield: a slightly bowed top edge running a little past both corners, as a marker
        // line does, and two sides sweeping down to the point
        var topLeft = new Vector2(0.06f + Wobble(), 0.92f);
        var topRight = new Vector2(0.94f + Wobble(), 0.915f);
        var point = new Vector2(0.515f + Wobble(), 0.03f);
        var right = Bezier(topRight, new Vector2(0.93f, 0.36f), point, 28);
        var left = Bezier(point, new Vector2(0.065f, 0.36f), topLeft, 28);
        var top = Bezier(topLeft + new Vector2(-0.02f, -0.012f), new Vector2(0.5f, 0.985f), topRight + new Vector2(0.022f, -0.016f), 24);

        var outline = new List<Vector2>();
        outline.AddRange(right);
        outline.AddRange(left);

        var strokes = new List<Stroke>
        {
            new Stroke { Points = right, Width = Outline },
            new Stroke { Points = left, Width = Outline },
            new Stroke { Points = top, Width = Outline },
        };

        // almond eyes, each with a lid line inside
        foreach (var (cx, cy, tilt) in new[] { (0.31f, 0.70f, 0.012f), (0.69f, 0.695f, -0.01f) })
        {
            float w = 0.25f, h = 0.105f;
            var a = new Vector2(cx - w * 0.5f, cy - tilt);
            var b = new Vector2(cx + w * 0.5f, cy + tilt);
            var eye = Bezier(a, new Vector2(cx + Wobble(), cy + h), b, 16);
            eye.AddRange(Bezier(b, new Vector2(cx + Wobble(), cy - h * 0.85f), a, 16));
            strokes.Add(new Stroke { Points = eye, Width = Feature });
            strokes.Add(new Stroke { Points = Bezier(new Vector2(cx - w * 0.28f, cy - 0.006f), new Vector2(cx, cy - 0.045f), new Vector2(cx + w * 0.3f, cy - 0.012f), 10), Width = Detail });
        }

        // the nose: one hooked line
        var nose = Bezier(new Vector2(0.535f, 0.615f), new Vector2(0.47f, 0.55f), new Vector2(0.44f, 0.505f), 10);
        nose.AddRange(Bezier(new Vector2(0.44f, 0.505f), new Vector2(0.43f, 0.47f), new Vector2(0.505f, 0.455f), 8));
        strokes.Add(new Stroke { Points = nose, Width = Feature });

        // the mouth
        switch (face)
        {
            case "Smile":
                strokes.Add(new Stroke { Points = Bezier(new Vector2(0.31f, 0.385f), new Vector2(0.46f, 0.23f), new Vector2(0.70f, 0.36f), 18), Width = Feature });
                break;
            case "Flat":
                strokes.Add(new Stroke { Points = Bezier(new Vector2(0.35f, 0.33f), new Vector2(0.5f, 0.315f), new Vector2(0.66f, 0.335f), 12), Width = Feature });
                break;
            case "Sad":
                strokes.Add(new Stroke { Points = Bezier(new Vector2(0.34f, 0.28f), new Vector2(0.5f, 0.40f), new Vector2(0.67f, 0.29f), 16), Width = Feature });
                break;
            default:
                strokes.Add(new Stroke { Points = Ellipse(new Vector2(0.5f, 0.32f), 0.055f, 0.05f, 24), Width = Feature });
                break;
        }

        // rasterise: white inside the shield, black where a line passes, clear outside
        var pixels = new Color32[Width * Height];
        float scale = Width;  // uv → pixels, by the width (the mask is drawn in a 1 x 1.25 box)
        var aspect = new Vector2(1f, (float)Height / Width);
        for (int y = 0; y < Height; y++)
        for (int x = 0; x < Width; x++)
        {
            var p = new Vector2((x + 0.5f) / Width, (y + 0.5f) / Height);
            var pa = Vector2.Scale(p, aspect);
            float line = 0f;
            foreach (var stroke in strokes)
            {
                float d = (Distance(pa, stroke.Points, aspect) - stroke.Width * 0.5f) * scale;
                line = Mathf.Max(line, Mathf.Clamp01(0.5f - d));
            }
            float fill = Inside(p, outline) ? 1f : 0f;
            byte grey = (byte)(255f * (1f - line));
            pixels[y * Width + x] = new Color32(grey, grey, grey, (byte)(255f * Mathf.Max(fill, line)));
        }

        var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    private static List<Vector2> Bezier(Vector2 a, Vector2 control, Vector2 b, int segments)
    {
        var points = new List<Vector2>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            points.Add((1 - t) * (1 - t) * a + 2 * (1 - t) * t * control + t * t * b);
        }
        return points;
    }

    private static List<Vector2> Ellipse(Vector2 centre, float rx, float ry, int segments)
    {
        var points = new List<Vector2>(segments + 1);
        for (int i = 0; i <= segments; i++)
        {
            float a = i / (float)segments * Mathf.PI * 2f;
            points.Add(centre + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry));
        }
        return points;
    }

    // distance from p to a polyline, both in the aspect-corrected space
    private static float Distance(Vector2 p, List<Vector2> points, Vector2 aspect)
    {
        float best = float.MaxValue;
        for (int i = 0; i < points.Count - 1; i++)
        {
            var a = Vector2.Scale(points[i], aspect);
            var b = Vector2.Scale(points[i + 1], aspect);
            var ab = b - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-8f));
            best = Mathf.Min(best, (p - (a + ab * t)).magnitude);
        }
        return best;
    }

    private static bool Inside(Vector2 p, List<Vector2> polygon)
    {
        bool inside = false;
        for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            if ((polygon[i].y > p.y) != (polygon[j].y > p.y)
                && p.x < (polygon[j].x - polygon[i].x) * (p.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x)
                inside = !inside;
        return inside;
    }
}
#endif
