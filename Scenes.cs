using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueLite;

public interface IScene
{
    void Update(GameTime gt);
    void Draw(SpriteBatch sb);
}

public static class Ui
{
    public static bool Hit(Rectangle r, Vector2? p) => p.HasValue && r.Contains(p.Value.ToPoint());
}

public class HubScene : IScene
{
    readonly Game1 _game;
    readonly Rectangle _left = new(100, 110, 40, 50);
    readonly Rectangle _right = new(340, 110, 40, 50);
    readonly Rectangle _start = new(170, 200, 140, 50);
    readonly Rectangle _upgrade = new(330, 200, 130, 50);

    static int UpgradeCost => 10 * (Save.Data.HealthLevel + 1);

    public HubScene(Game1 game) => _game = game;

    public void Update(GameTime gt)
    {
        int count = Assets.Characters.Length;

        if (Ui.Hit(_left, Input.Tap))
        {
            Save.Data.Character = (Save.Data.Character + count - 1) % count;
            Save.Write();
        }
        if (Ui.Hit(_right, Input.Tap))
        {
            Save.Data.Character = (Save.Data.Character + 1) % count;
            Save.Write();
        }
        if (Ui.Hit(_upgrade, Input.Tap) && Save.Data.Gold >= UpgradeCost)
        {
            Save.Data.Gold -= UpgradeCost;
            Save.Data.HealthLevel++;
            Save.Write();
        }
        if (Ui.Hit(_start, Input.Tap))
            _game.ChangeScene(new RunScene(_game, Environment.TickCount));
    }

    public void Draw(SpriteBatch sb)
    {
        sb.Draw(Assets.Pixel, new Rectangle(0, 0, Game1.VW, Game1.VH), new Color(30, 30, 45));

        Font.DrawCentered(sb, "HUB", Game1.VW / 2, 20, Color.White, 5);
        Font.Draw(sb, $"GOLD {Save.Data.Gold}", new Vector2(8, 8), Color.Gold, 2);
        Font.Draw(sb, $"BEST FLOOR {Save.Data.BestFloor}", new Vector2(8, 22), Color.LightGray, 2);
        Font.DrawCentered(sb, $"MAX HP {Save.Data.MaxHp}", Game1.VW / 2, 70, Color.IndianRed, 2);

        sb.Draw(Assets.Pixel, _left, Color.SlateGray);
        sb.Draw(Assets.Pixel, _right, Color.SlateGray);
        Font.DrawCentered(sb, "<", _left.Center.X, _left.Y + 15, Color.White, 4);
        Font.DrawCentered(sb, ">", _right.Center.X, _right.Y + 15, Color.White, 4);

        sb.Draw(Assets.Characters[Save.Data.Character], new Rectangle(190, 90, 100, 100), Color.White);

        sb.Draw(Assets.Pixel, _start, Color.ForestGreen);
        Font.DrawCentered(sb, "START RUN", _start.Center.X, _start.Y + 20, Color.White, 2);

        bool canBuy = Save.Data.Gold >= UpgradeCost;
        sb.Draw(Assets.Pixel, _upgrade, canBuy ? Color.Goldenrod : Color.DimGray);
        Font.DrawCentered(sb, "HP UP", _upgrade.Center.X, _upgrade.Y + 10, Color.White, 2);
        Font.DrawCentered(sb, $"COST {UpgradeCost}", _upgrade.Center.X, _upgrade.Y + 30, Color.White, 2);
    }
}