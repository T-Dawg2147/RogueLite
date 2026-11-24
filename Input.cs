using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace RogueLite;

public static class Input
{
    public static Vector2? Tap;     // setfor one frame when a finger goes down
    public static Vector2 Move;     // joystick direction, length 0..1
    public static Vector2? StickOrigin, StickPos;

    public static float Scale = 1f;
    public static Vector2 Offset;

    static Vector2 ToVirtual(Vector2 p) => (p - Offset) / Scale;

    public static void Update()
    {
        Tap = null;
        var touches = TouchPanel.GetState();
        bool stickHeld = false;

        foreach (var t in touches)
        {
            var p = ToVirtual(t.Position);
            if (t.State == TouchLocationState.Pressed)
            {
                Tap = p;
                if (p.X < Game1.VW / 2f && StickOrigin == null) StickOrigin = p;
            }

            if (StickOrigin != null && p.X < Game1.VW / 2f &&
                (t.State == TouchLocationState.Moved || t.State == TouchLocationState.Pressed))
            {
                StickPos = p;
                stickHeld = true;
            }
        }

        if (!stickHeld)
        {
            StickOrigin = null;
            StickPos = null;
            Move = Vector2.Zero;
            return;
        }

        var d = (StickPos!.Value - StickOrigin!.Value) / 30f;
        if (d.Length() > 1f) d.Normalize();
        Move = d.Length() < 0.15f ? Vector2.Zero : d;
    }
}