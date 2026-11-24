using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueLite;

public static class Assets
{
    public static Texture2D Pixel, Floor, Wall, Enemy;
    public static Texture2D[] Characters;

    public static void Init(GraphicsDevice gd)
    {
        Pixel = new Texture2D(gd, 1, 1);
        Pixel.SetData([Color.White]);

        Floor = Noise(gd, 16, new Color(60, 55, 70), 12, 1);
        Wall = Noise(gd, 16, new Color(120, 110, 130), 25, 2);
        Enemy = Character(gd, 900);

        Characters = new Texture2D[4];
        for (int i = 0; i < Characters.Length; i++)
            Characters[i] = Character(gd, 100 + i);
    }

    static Texture2D Noise(GraphicsDevice gd, int size, Color baseColor, int variance, int seed)
    {
        var r = new Random(seed);
        var data = new Color[size * size];
        for (int i = 0; i < data.Length; i++)
        {
            int d = r.Next(-variance, variance);
            data[i] = new Color(
                Math.Clamp(baseColor.R + d, 0, 255),
                Math.Clamp(baseColor.G + d, 0, 255),
                Math.Clamp(baseColor.B + d, 0, 255));
        }

        var t = new Texture2D(gd, size, size);
        t.SetData(data);
        return t;
    }

    // Random left-right symmetric sprite with a dark outline
    public static Texture2D Character(GraphicsDevice gd, int seed, int size = 10)
    {
        var r = new Random(seed);
        var filled = new bool[size, size];
        for (int y = 1; y < size - 1; y++)
            for (int x = 0; x < size / 2; x++)
            {
                bool on = r.NextDouble() < 0.6;
                filled[x, y] = on;
                filled[size - 1 - x, y] = on;
            }

        var main = new Color(r.Next(80, 256), r.Next(80, 256), r.Next(80, 256));
        var data = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                if (filled[x, y]) { data[y * size + x] = main; continue; }
                bool edge = false;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int nx = x + dx, ny = y + dy;
                        if (nx >= 0 && ny >= 0 && nx < size && ny < size && filled[nx, ny]) edge = true;
                    }

                data[y * size + x] = edge ? new Color(20, 20, 30) : Color.Transparent;
            }

        var t = new Texture2D(gd, size, size);
        t.SetData(data);
        return t;
    }
}