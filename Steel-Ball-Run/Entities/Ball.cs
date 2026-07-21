using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Extended.Input;
using SteelBallRun.Entities.Collider;

namespace SteelBallRun.Entities;

public class Ball : IGameEntity
{
    public const float GRAVITY = 2200f;
    private const float MAX_GRAVITY_SPEED = 850f;
    
    public const float START_MOVING_SPEED = 280f;
    public const float MAX_MOVING_SPEED = 700f;

    private const float BALL_SIZE = 29f;
    private const float COLLIDER_SIZE = 28f;
    
    private const float ACCELERATION_PPS_PER_SECOND = 2.6f;

    public int DrawOrder { get; }
    
    public CircleCollider Collider { get; private set; }
    
    public float Speed { get; private set; }
    public bool IsAlive { get; private set; }
    public bool Initialized { get; private set; }

    public Vector2 Position { get; set; }

    public GravityDirection GravityDirection { get; set; }
    
    private Sprite sprite;
    
    private float rotation;
    
    private Wall southWall;
    private Wall northWall;

    private float verticalVelocity;
    
    private float startPosY;
    
    public event EventHandler Died;
    
    public Ball(Vector2 startPos, Sprite sprite, Wall southWall, Wall northWall)
    {
        this.sprite = sprite;
        sprite.Origin = new Vector2(BALL_SIZE / 2f);
        this.southWall = southWall;
        this.northWall = northWall;
        this.Position = startPos;
        Collider = new CircleCollider(COLLIDER_SIZE / 2f);
        Speed = START_MOVING_SPEED;
        startPosY = startPos.Y;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if(!Initialized || !IsAlive) return;
        
        if (KeyboardExtended.GetState().WasKeyPressed(Game1.PlayKey))
        {
            if (GravityDirection == GravityDirection.Down)
            {
                GravityDirection = GravityDirection.Up;
            }
            else
            {
                GravityDirection = GravityDirection.Down;
            }
        }
        
        Speed += ACCELERATION_PPS_PER_SECOND * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (Speed > MAX_MOVING_SPEED)
            Speed = MAX_MOVING_SPEED;

        float gravity = GravityDirection == GravityDirection.Down ? GRAVITY : -GRAVITY;

        verticalVelocity += gravity * deltaTime;

        verticalVelocity = Math.Clamp(verticalVelocity, -MAX_GRAVITY_SPEED, MAX_GRAVITY_SPEED);

        Vector2 nextPosition = Position + new Vector2(0, verticalVelocity * deltaTime);
        
        Collider.Center = nextPosition;
        
        bool colliding = Collider.Intersects(northWall.CollisionBox) || Collider.Intersects(southWall.CollisionBox);

        if (!colliding)
        {
            Position = nextPosition;
            rotation += (float) 0.03 * Speed;
        }
        else
        {
            rotation += (float) 0.02 * Speed;
            verticalVelocity = 0;
        }
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        sprite.Rotation = float.DegreesToRadians(rotation);
        sprite.Draw(spriteBatch, Position);
    }
    
    public void Initialize()
    {
        Speed = START_MOVING_SPEED;
        IsAlive = true;
        Initialized = true;
        Position = new Vector2(Position.X, startPosY);
        GravityDirection = GravityDirection.Down;
    }
    
    public bool Die()
    {
        if (!IsAlive)
            return false;

        Speed = 0;
        IsAlive = false;
        Initialized = false;
        verticalVelocity = 0;
        OnDied();
        return true;
    }
    
    protected virtual void OnDied()
    {
        EventHandler handler = Died;
        handler?.Invoke(this, EventArgs.Empty);
    }
}