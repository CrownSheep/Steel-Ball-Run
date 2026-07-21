using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using SteelBallRun.Entities.Collider;

namespace SteelBallRun.Entities;

public class Mine : IGameEntity
{
    private const float MINE_SIZE = 28f;
    private const float SPEED_PPS = 40;
    
    public int DrawOrder { get; }
    
    public Vector2 Position { get; set; }
    
    private CircleCollider Collider { get; set; }
    
    private Sprite sprite;
    
    private Ball ball;

    public Mine(Sprite sprite, Ball ball, Vector2 position)
    {
        this.sprite = sprite;
        sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        this.ball = ball;
        Collider = new CircleCollider(MINE_SIZE / 2f);
        Position = position;
    }
    
    public void Update(GameTime gameTime)
    {
        float posX = Position.X - ball.Speed * (float) gameTime.ElapsedGameTime.TotalSeconds;
        Position = new Vector2(posX, Position.Y);
        
        if (!ball.IsAlive) return;
        
        Position = new Vector2(Position.X - SPEED_PPS * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);
        Collider.Center = Position;
        
        if (Collider.Intersects(ball.Collider))
        {
            ball.Die();
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        sprite.Draw(spriteBatch, Position);
    }
}