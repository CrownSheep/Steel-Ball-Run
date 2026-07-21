using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace SteelBallRun.Entities.Managers;

public class ObstacleManager : IGameEntity
{
    private const float MIN_SPAWN_DISTANCE = 10;

    private const int MIN_OBSTACLE_DISTANCE = 1;
    private const int MAX_OBSTACLE_DISTANCE = 3;

    private const int OBSTACLE_DISTANCE_SPEED_TOLERANCE = 4;

    private const int OBSTACLE_DESPAWN_POS_X = -200;

    private double lastSpawnScore = -1;
    private double currentTargetDistance;

    private readonly EntityManager entityManager;
    private readonly Ball ball;
    private readonly ScoreBoard scoreBoard;

    private readonly Random random;

    public bool IsEnabled { get; set; }
    public bool CanSpawnObstacles => IsEnabled && scoreBoard.Score >= MIN_SPAWN_DISTANCE + new Random().Next(2, 10);
    public int DrawOrder => 0;

    private Sprite mineSprite;

    private int lastLane;

    public ObstacleManager(Sprite mineSprite, EntityManager entityManager, Ball ball, ScoreBoard scoreBoard)
    {
        this.entityManager = entityManager;
        this.ball = ball;
        this.mineSprite = mineSprite;
        this.scoreBoard = scoreBoard;
        random = new Random();
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled)
            return;

        if (CanSpawnObstacles && lastSpawnScore <= 0 || scoreBoard.Score - lastSpawnScore >= currentTargetDistance)
        {
            currentTargetDistance = random.NextDouble() * (MAX_OBSTACLE_DISTANCE - MIN_OBSTACLE_DISTANCE) +
                                    MIN_OBSTACLE_DISTANCE;

            currentTargetDistance += (ball.Speed - Ball.START_MOVING_SPEED) /
                (Ball.MAX_MOVING_SPEED - Ball.START_MOVING_SPEED) * OBSTACLE_DISTANCE_SPEED_TOLERANCE;

            lastSpawnScore = scoreBoard.Score;

            SpawnRandomObstacle();
        }

        foreach (Mine obstacle in entityManager.GetEntitiesOfType<Mine>()
                     .Where(obstacle => obstacle.Position.X < OBSTACLE_DESPAWN_POS_X))
        {
            entityManager.RemoveEntity(obstacle);
        }
    }

    private void SpawnRandomObstacle()
    {
        int lane = random.Next(5);

        if (lane == lastLane)
        {
            SpawnRandomObstacle();
            return;
        }

        if (lastLane is not (0 or 4))
        {
            lane = ball.Position.Y switch
            {
                >= 195 => 4,
                <= 75 => 0,
                _ => lane
            };
        }

        lastLane = lane;
        float posY = 64 + mineSprite.Height / 2f + lane * 29;
        // posY = Math.Clamp(posY, 60 + mineSprite.Height, (int) (1080 / 4f) - 60) - mineSprite.Height / 2f;

        Mine mine = new Mine(mineSprite, ball, new Vector2(1920 / 4f, posY));

        entityManager.AddEntity(mine);
    }

    public void Reset()
    {
        foreach (Mine obstacle in entityManager.GetEntitiesOfType<Mine>())
        {
            entityManager.RemoveEntity(obstacle);
        }

        currentTargetDistance = 0;
        lastSpawnScore = -1;
    }
}