using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RogueLite;

public class Game1 : Game
{
    public const int VW = 480, VH = 270;
    
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _sb;
    private IScene _scene;
    private Matrix _transform;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            IsFullScreen = true,
            SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight
        };
        Content.RootDirectory = "Content";
    }

    protected override void LoadContent()
    {
        _sb = new SpriteBatch(GraphicsDevice);
        Assets.Init(GraphicsDevice);

        var vp = GraphicsDevice.Viewport;
        float s = System.Math.Min(vp.Width / (float)VW, vp.Height / (float)VH);
        var offset = new Vector2((vp.Width - VW * s) / 2f, (vp.Height - VH * s) / 2f);
        Input.Scale = s;
        Input.Offset = offset;
        _transform = Matrix.CreateScale(s) * Matrix.CreateTranslation(offset.X, offset.Y, 0);

        Save.Load();
        ChangeScene(new HubScene(this));
    }

    public void ChangeScene(IScene scene) => _scene = scene;

    protected override void Update(GameTime gameTime)
    {
        Input.Update();
        _scene.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _sb.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transform);
        _scene.Draw(_sb);
        _sb.End();
        base.Draw(gameTime);
    }
}