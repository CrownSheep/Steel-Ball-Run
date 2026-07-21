using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Extended;

namespace SteelBallRun.Entities;

public class Wall : IGameEntity
{
    public int DrawOrder { get; }

    public RectangleF CollisionBox => new RectangleF(Position.X, Position.Y, wallSprite.Width, wallSprite.Height);
    
    public Vector2 Position { get; set; }
    
    private Sprite wallSprite;

    public Wall(Sprite wallSprite, Vector2 position)
    {
        this.wallSprite = wallSprite;
        Position = position;
    }
    
    public void Update(GameTime gameTime)
    {
        
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        wallSprite.Draw(spriteBatch, Position);
    }
}