// Procedural textures for the Level 09 school, in the game's grays: the checkered terrazzo
// tiles of a post-Soviet school corridor, painted classroom floorboards, the gym's narrow strips and
// small ceramic toilet tiles; the glossy oil paint of the lower walls and their dark trim, whitewashed
// plaster for the upper walls and ceilings, speckled laminate and worn dark metal for the furniture.
// Generated once (SchoolBuilder calls this) and kept as PNGs, so they can be painted over or
// replaced like any other texture.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SchoolTextures
{
    private const string Folder = "Assets/_Project/Materials/School";
    private const int Size = 1024;

    private static readonly string[] Floors = { "Tiles", "Boards", "Gym", "SmallTiles" };

    // floors are School_Floor_*, everything else just School_*
    private static string AssetName(string name) => System.Array.IndexOf(Floors, name) >= 0 ? $"School_Floor_{name}" : $"School_{name}";

    /// <summary>Makes (or loads) the material. <paramref name="metresPerTexture"/> is how much surface one copy of the texture covers.</summary>
    public static Material Get(string name, float metresPerTexture, float smoothness)
    {
        string matPath = $"{Folder}/{AssetName(name)}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null)
            return mat;

        var texture = Texture(name);
        mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = AssetName(name) };
        mat.SetTexture("_BaseMap", texture);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetTextureScale("_BaseMap", Vector2.one / metresPerTexture);
        mat.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(mat, matPath);
        return mat;
    }

    private static Texture2D Texture(string name)
    {
        string path = $"{Folder}/Textures/{AssetName(name)}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null)
            return existing;

        var pixels = name switch
        {
            "Tiles" => TerrazzoTiles(),
            "Boards" => SimpleBoards(planks: 6, baseShade: 0.58f, seam: 0.34f, variation: 0.06f, seed: 3),
            "Gym" => SimpleBoards(planks: 10, baseShade: 0.76f, seam: 0.56f, variation: 0.04f, seed: 9),
            "WallPaint" => OilPaint(baseShade: 0.86f, streaks: 0.035f, wear: 0.03f, chips: 0f, seed: 31),
            "Trim" => OilPaint(baseShade: 0.33f, streaks: 0.05f, wear: 0.06f, chips: 0.012f, seed: 41),
            "Plaster" => Plaster(baseShade: 0.95f, mottle: 0.026f, seed: 51),
            "Laminate" => Laminate(baseShade: 0.86f, seed: 61),
            "Metal" => OilPaint(baseShade: 0.44f, streaks: 0.025f, wear: 0.05f, chips: 0.008f, seed: 71),
            _ => SmallTiles(),
        };

        if (!AssetDatabase.IsValidFolder(Folder + "/Textures"))
            AssetDatabase.CreateFolder(Folder, "Textures");
        var tex = new Texture2D(Size, Size, TextureFormat.RGB24, true);
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.anisoLevel = 4;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    // 4 x 4 big tiles, light and dark in a checker, speckled with chips like terrazzo, a bit worn
    private static Color[] TerrazzoTiles()
    {
        const int tile = Size / 4, grout = 3;
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            int tx = x / tile, ty = y / tile, lx = x % tile, ly = y % tile;
            float v = ((tx + ty) & 1) == 0 ? 0.80f : 0.56f;
            v += (Hash(tx, ty, 1) - 0.5f) * 0.05f;
            v += (Noise(x, y, 96, 2) - 0.5f) * 0.07f;                       // wear
            if (Hash(x / 3, y / 3, 7) > 0.9f)                                 // terrazzo chips
                v += (Hash(x / 3, y / 3, 8) - 0.5f) * 0.32f;
            v += (Hash(x, y, 5) - 0.5f) * 0.03f;

            int edge = Mathf.Min(Mathf.Min(lx, tile - 1 - lx), Mathf.Min(ly, tile - 1 - ly));
            if (edge < grout)
                v = 0.33f + (Hash(x, y, 11) - 0.5f) * 0.06f;
            else if (edge < grout + 3)
                v *= 0.93f;                                                   // worn tile edge
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    // plain, drawn-looking boards: each a flat shade with a little variation, thin seams, staggered joints - no grain
    private static Color[] SimpleBoards(int planks, float baseShade, float seam, float variation, int seed)
    {
        int width = Size / planks, length = Size / 2;
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            int plank = x / width, lx = x % width;
            int along = (y + (plank % 2) * (length / 2) + (int)(Hash(plank, 0, seed) * 3) * (length / 6)) % Size;
            int board = along / length, ly = along % length;
            float v = baseShade + (Hash(plank, board, seed + 1) - 0.5f) * variation * 2f;
            v += (Noise(x, y, 256, seed + 2) - 0.5f) * 0.02f;
            if (lx < 3 || ly < 3)
                v = seam;
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    // oil paint rolled on in horizontal strokes, a little uneven, with grime and (for the dark trim) chips
    private static Color[] OilPaint(float baseShade, float streaks, float wear, float chips, int seed)
    {
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            float v = baseShade;
            // long horizontal roller strokes: noise stretched along x
            v += (Noise(x / 8, y, 24, seed) - 0.5f) * streaks;
            v += (Noise(x / 3, y, 8, seed + 1) - 0.5f) * streaks * 0.5f;
            v += (Noise(x, y, 160, seed + 2) - 0.5f) * wear;                  // grime, uneven coats
            v += (Hash(x, y, seed + 3) - 0.5f) * 0.015f;                      // fine grain
            if (chips > 0f && Noise(x, y, 6, seed + 4) > 1f - chips * 20f && Hash(x / 5, y / 5, seed + 5) > 0.6f)
                v += 0.16f;                                                    // chipped paint shows the lighter coat under it
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    // whitewashed plaster: soft blotches at a few sizes, faint trowel marks, fine grain
    private static Color[] Plaster(float baseShade, float mottle, int seed)
    {
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            float v = baseShade;
            v += (Noise(x, y, 256, seed) - 0.5f) * mottle;
            v += (Noise(x, y, 64, seed + 1) - 0.5f) * mottle * 0.6f;
            v += (Noise(x, y, 16, seed + 2) - 0.5f) * mottle * 0.3f;
            v += (Noise((x + y) / 6, (x - y + Size) / 2, 32, seed + 3) - 0.5f) * mottle * 0.4f;   // diagonal trowel sweeps
            v += (Hash(x, y, seed + 4) - 0.5f) * 0.02f;
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    // speckled canteen laminate (formica)
    private static Color[] Laminate(float baseShade, int seed)
    {
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            float v = baseShade + (Noise(x, y, 128, seed) - 0.5f) * 0.04f + (Hash(x, y, seed + 1) - 0.5f) * 0.03f;
            if (Hash(x / 2, y / 2, seed + 2) > 0.93f)
                v -= 0.12f + Hash(x / 2, y / 2, seed + 3) * 0.12f;
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    // 8 x 8 small white tiles with gray grout
    private static Color[] SmallTiles()
    {
        const int tile = Size / 8, grout = 3;
        var px = new Color[Size * Size];
        for (int y = 0; y < Size; y++)
        for (int x = 0; x < Size; x++)
        {
            int tx = x / tile, ty = y / tile, lx = x % tile, ly = y % tile;
            float v = 0.9f + (Hash(tx, ty, 21) - 0.5f) * 0.04f + (Noise(x, y, 128, 22) - 0.5f) * 0.04f;
            int edge = Mathf.Min(Mathf.Min(lx, tile - 1 - lx), Mathf.Min(ly, tile - 1 - ly));
            if (edge < grout)
                v = 0.58f + (Hash(x, y, 23) - 0.5f) * 0.05f;
            px[y * Size + x] = Gray(v);
        }
        return px;
    }

    private static Color Gray(float v)
    {
        v = Mathf.Clamp01(v);
        return new Color(v, v, v, 1f);
    }

    private static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
            h = (h ^ (h >> 13)) * 1274126177u;
            return ((h ^ (h >> 16)) & 0xffffff) / 16777215f;
        }
    }

    // smooth value noise that tiles across the texture (cell = pixels per lattice step)
    private static float Noise(int x, int y, int cell, int seed)
    {
        int cells = Mathf.Max(1, Size / cell);
        float fx = (float)x / cell, fy = (float)y / cell;
        int ix = Mathf.FloorToInt(fx), iy = Mathf.FloorToInt(fy);
        float tx = fx - ix, ty = fy - iy;
        tx = tx * tx * (3f - 2f * tx);
        ty = ty * ty * (3f - 2f * ty);
        int Wrap(int i) => ((i % cells) + cells) % cells;
        float a = Hash(Wrap(ix), Wrap(iy), seed), b = Hash(Wrap(ix + 1), Wrap(iy), seed);
        float c = Hash(Wrap(ix), Wrap(iy + 1), seed), d = Hash(Wrap(ix + 1), Wrap(iy + 1), seed);
        return Mathf.Lerp(Mathf.Lerp(a, b, tx), Mathf.Lerp(c, d, tx), ty);
    }
}
#endif
