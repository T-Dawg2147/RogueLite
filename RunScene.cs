using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueLite;

public class RunScene : IScene
{
    const int Tile = 16, W = 60, H = 40, Size = 10;

    class Enemy { public Vector2 Pos; public int Hp = 2; }
    class Bullet { public Vector2 Pos, Vel; public float Life = 1f; }

    readonly Game1 _game;
    readonly Random _rng;
    readonly bool[,] _wall = new bool[W, H];
    readonly List<Enemy> _enemies = new();
    readonly List<Bullet> _bullets = new();
    readonly Rectangle _back = new(4, 4, 24, 24);

    Vector2 _pos, _exit, _cam;
    int _floor = 1, _hp, _gold;
    float _shootTimer, _hurtTimer, _deadTime;
    bool _dead, _banked;

    Vector2 Center => _pos + new Vector2(Size / 2f);

    public RunScene(Game1 game, int seed)
    {
        _game = game;
        _rng = new Random(seed);
        _hp = Save.Data.MaxHp;
        BuildFloor();
    }

    // Drunkard's walk carves the cave, then we place the exit and enemies
    void BuildFloor()
    {
        _enemies.Clear();
        _bullets.Clear();
        for (int x = 0; x < W; x++)
            for (int y = 0; y < H; y++) _wall[x, y] = true;

        int cx = W / 2, cy = H / 2, carved = 0;
        var cells = new List<Point>();
        var dirs = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        _pos = new Vector2(cx * Tile + 3, cy * Tile + 3);

        while (carved < W * H * 0.35)
        {
            if (_wall[cx, cy]) { _wall[cx, cy] = false; carved++; cells.Add(new Point(cx, cy)); }
            var (dx, dy) = dirs[_rng.Next(4)];
            cx = Math.Clamp(cx + dx, 1, W - 2);
            cy = Math.Clamp(cy + dy, 1, H - 2);
        }

        // The exit goes in the carved cell farthest from the start
        var far = cells.MaxBy(c => Math.Abs(c.X - W / 2) + Math.Abs(c.Y - H / 2));
        _exit = new Vector2(far.X * Tile, far.Y * Tile);

        int target = 4 + _floor * 2;
        for (int i = 0; i < target * 5 && _enemies.Count < target; i++)
        {
            var c = cells[_rng.Next(cells.Count)];
            var p = new Vector2(c.X * Tile + 3, c.Y * Tile + 3);
            if (Vector2.Distance(p, _pos) < 120f) continue;
            _enemies.Add(new Enemy { Pos = p, Hp = 2 + _floor / 3 });
        }

        _hurtTimer = 1f;
    }

    bool SolidAt(Vector2 p)
    {
        int tx = (int)(p.X / Tile), ty = (int)(p.Y / Tile);
        return p.X < 0 || p.Y < 0 || tx >= W || ty >= H || _wall[tx, ty];
    }

    // Checks the four corners of a Size x Size box
    bool Blocked(Vector2 p) =>
        SolidAt(p) || SolidAt(p + new Vector2(Size - 1, 0)) ||
        SolidAt(p + new Vector2(0, Size - 1)) || SolidAt(p + new Vector2(Size - 1, Size - 1));

    // Axes are separate so things slide along walls
    void Move(ref Vector2 pos, Vector2 step)
    {
        var nx = pos + new Vector2(step.X, 0);
        if (!Blocked(nx)) pos = nx;
        var ny = pos + new Vector2(0, step.Y);
        if (!Blocked(ny)) pos = ny;
    }

    void Bank()
    {
        if (_banked) return;
        _banked = true;
        Save.Data.Gold += _gold;
        Save.Data.BestFloor = Math.Max(Save.Data.BestFloor, _floor);
        Save.Write();
    }

    public void Update(GameTime gt)
    {
        float dt = (float)gt.ElapsedGameTime.TotalSeconds;

        if (_dead)
        {
            _deadTime += dt;
            if (_deadTime > 1f && Input.Tap.HasValue) _game.ChangeScene(new HubScene(_game));
            return;
        }

        if (Ui.Hit(_back, Input.Tap))
        {
            Bank();
            _game.ChangeScene(new HubScene(_game));
            return;
        }

        Move(ref _pos, Input.Move * 90f * dt);
        UpdateShooting(dt);
        UpdateEnemies(dt);
        if (_dead) return;

        if (Vector2.Distance(Center, _exit + new Vector2(Tile / 2f)) < 10f)
        {
            _floor++;
            BuildFloor();
        }

        _cam = Vector2.Clamp(Center - new Vector2(Game1.VW, Game1.VH) / 2f,
            Vector2.Zero, new Vector2(W * Tile - Game1.VW, H * Tile - Game1.VH));
    }

    void UpdateShooting(float dt)
    {
        _shootTimer -= dt;
        if (_shootTimer <= 0 && _enemies.Count > 0)
        {
            var nearest = _enemies.MinBy(e => Vector2.DistanceSquared(e.Pos, _pos))!;
            var delta = nearest.Pos - _pos;
            float dist = delta.Length();
            if (dist > 0.01f && dist < 140f)
            {
                _bullets.Add(new Bullet { Pos = Center, Vel = delta / dist * 200f });
                _shootTimer = 0.5f;
            }
        }

        foreach (var b in _bullets)
        {
            b.Pos += b.Vel * dt;
            b.Life -= dt;
            if (SolidAt(b.Pos)) b.Life = 0;

            foreach (var e in _enemies)
            {
                if (e.Hp > 0 && Vector2.Distance(e.Pos + new Vector2(Size / 2f), b.Pos) < 7f)
                {
                    e.Hp--;
                    b.Life = 0;
                    break;
                }
            }
        }

        _bullets.RemoveAll(b => b.Life <= 0);
        _enemies.RemoveAll(e =>
        {
            if (e.Hp > 0) return false;
            _gold += 2;
            return true;
        });
    }

    void UpdateEnemies(float dt)
    {
        _hurtTimer -= dt;
        float speed = 35f + _floor * 3f;

        foreach (var e in _enemies)
        {
            var dir = _pos - e.Pos;
            if (dir.LengthSquared() > 1f)
            {
                dir.Normalize();
                Move(ref e.Pos, dir * speed * dt);
            }

            if (_hurtTimer <= 0 && Vector2.Distance(e.Pos, _pos) < Size)
            {
                _hp--;
                _hurtTimer = 1f;
                if (_hp <= 0)
                {
                    _dead = true;
                    Bank();
                    return;
                }
            }
        }
    }

    public void Draw(SpriteBatch sb)
    {
        var cam = new Point((int)_cam.X, (int)_cam.Y);

        // Only draw tiles that are on screen
        int x0 = Math.Max(0, cam.X / Tile), x1 = Math.Min(W - 1, (cam.X + Game1.VW) / Tile);
        int y0 = Math.Max(0, cam.Y / Tile), y1 = Math.Min(H - 1, (cam.Y + Game1.VH) / Tile);
        for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
                sb.Draw(_wall[x, y] ? Assets.Wall : Assets.Floor,
                    new Rectangle(x * Tile - cam.X, y * Tile - cam.Y, Tile, Tile), Color.White);

        var ex = new Rectangle((int)_exit.X - cam.X, (int)_exit.Y - cam.Y, Tile, Tile);
        sb.Draw(Assets.Pixel, ex, Color.Gold);
        sb.Draw(Assets.Pixel, new Rectangle(ex.X + 4, ex.Y + 4, 8, 8), new Color(40, 30, 10));

        foreach (var e in _enemies)
            sb.Draw(Assets.Enemy,
                new Rectangle((int)e.Pos.X - cam.X, (int)e.Pos.Y - cam.Y, Size, Size), Color.Tomato);

        foreach (var b in _bullets)
            sb.Draw(Assets.Pixel,
                new Rectangle((int)b.Pos.X - 2 - cam.X, (int)b.Pos.Y - 2 - cam.Y, 4, 4), Color.Yellow);

        sb.Draw(Assets.Characters[Save.Data.Character],
            new Rectangle((int)_pos.X - cam.X, (int)_pos.Y - cam.Y, Size, Size),
            _hurtTimer > 0.6f ? Color.Red : Color.White);

        // HUD
        sb.Draw(Assets.Pixel, _back, Color.Firebrick);
        for (int i = 0; i < _hp; i++)
            sb.Draw(Assets.Pixel, new Rectangle(36 + i * 10, 6, 8, 8), Color.Red);
        Font.Draw(sb, $"GOLD {_gold}", new Vector2(36, 18), Color.Gold, 2);
        string floor = $"FLOOR {_floor}";
        Font.Draw(sb, floor, new Vector2(Game1.VW - 8 - Font.Width(floor, 2), 8), Color.White, 2);

        if (Input.StickOrigin is Vector2 o && Input.StickPos is Vector2 p)
        {
            sb.Draw(Assets.Pixel, new Rectangle((int)o.X - 15, (int)o.Y - 15, 30, 30), Color.White * 0.25f);
            sb.Draw(Assets.Pixel, new Rectangle((int)p.X - 8, (int)p.Y - 8, 16, 16), Color.White * 0.6f);
        }

        if (_dead)
        {
            sb.Draw(Assets.Pixel, new Rectangle(0, 0, Game1.VW, Game1.VH), Color.Black * 0.6f);
            Font.DrawCentered(sb, "YOU DIED", Game1.VW / 2, 80, Color.Red, 6);
            Font.DrawCentered(sb, $"FLOOR {_floor}   GOLD {_gold}", Game1.VW / 2, 135, Color.Gold, 2);
            Font.DrawCentered(sb, "TAP TO CONTINUE", Game1.VW / 2, 165, Color.White, 2);
        }
    }
}