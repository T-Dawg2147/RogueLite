using Javax.Sql;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueLite;

public static class Font
{
    private static readonly Dictionary<char, string[]> Glyphs = [];

    static Font()
    {
        Add('A', ".#.|#.#|###|#.#|#.#");
        Add('B', "##.|#.#|##.|#.#|##.");
        Add('C', ".##|#..|#..|#..|.##");
        Add('D', "##.|#.#|#.#|#.#|##.");
        Add('E', "###|#..|##.|#..|###");
        Add('F', "###|#..|##.|#..|#..");
        Add('G', ".##|#..|#.#|#.#|.##");
        Add('H', "#.#|#.#|###|#.#|#.#");
        Add('I', "###|.#.|.#.|.#.|###");
        Add('J', "..#|..#|..#|#.#|.#.");
        Add('K', "#.#|#.#|##.|#.#|#.#");
        Add('L', "#..|#..|#..|#..|###");
        Add('M', "#.#|###|###|#.#|#.#");
        Add('N', "##.|#.#|#.#|#.#|#.#");
        Add('O', ".#.|#.#|#.#|#.#|.#.");
        Add('P', "##.|#.#|##.|#..|#..");
        Add('Q', ".#.|#.#|#.#|##.|.##");
        Add('R', "##.|#.#|##.|#.#|#.#");
        Add('S', ".##|#..|.#.|..#|##.");
        Add('T', "###|.#.|.#.|.#.|.#.");
        Add('U', "#.#|#.#|#.#|#.#|###");
        Add('V', "#.#|#.#|#.#|#.#|.#.");
        Add('W', "#.#|#.#|###|###|#.#");
        Add('X', "#.#|#.#|.#.|#.#|#.#");
        Add('Y', "#.#|#.#|.#.|.#.|.#.");
        Add('Z', "###|..#|.#.|#..|###");
        Add('0', "###|#.#|#.#|#.#|###");
        Add('1', ".#.|##.|.#.|.#.|###");
        Add('2', "##.|..#|.#.|#..|###");
        Add('3', "##.|..#|.#.|..#|##.");
        Add('4', "#.#|#.#|###|..#|..#");
        Add('5', "###|#..|##.|..#|##.");
        Add('6', ".##|#..|###|#.#|###");
        Add('7', "###|..#|.#.|.#.|.#.");
        Add('8', "###|#.#|###|#.#|###");
        Add('9', "###|#.#|###|..#|##.");
        Add(':', "...|.#.|...|.#.|...");
        Add('/', "..#|..#|.#.|#..|#..");
        Add('-', "...|...|###|...|...");
        Add('.', "...|...|...|...|.#.");
        Add('!', ".#.|.#.|.#.|...|.#.");
        Add('<', "..#|.#.|#..|.#.|..#");
        Add('>', "#..|.#.|..#|.#.|#..");
    }

    static void Add(char c, string rows) => Glyphs[c] = rows.Split('|');

    public static int Width(string text, int scale = 1) => text.Length * 4 * scale;

    public static void Draw(SpriteBatch sb, string text, Vector2 pos, Color color, int scale = 1)
    {
        int x = (int)pos.X;
        foreach (char ch in text.ToUpperInvariant())
        {
            if (Glyphs.TryGetValue(ch, out var rows))
                for (int y = 0; y < 5; y++)
                    for (int cx = 0; cx < 3; cx++)
                        if (rows[y][cx] == '#')
                            sb.Draw(Assets.Pixel,
                                new Rectangle(x + cx * scale, (int)pos.Y + y * scale, scale, scale), color);
            x += 4 * scale;
        }
    }

    public static void DrawCentered(SpriteBatch sb, string text, int centerX, int y, Color color, int scale = 1)
        => Draw(sb, text, new Vector2(centerX - Width(text, scale) / 2f, y), color, scale);
}