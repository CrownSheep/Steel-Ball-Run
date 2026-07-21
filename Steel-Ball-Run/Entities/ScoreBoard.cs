using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SteelBallRun.Entities;

public class ScoreBoard : IGameEntity
{
    private const int TEXTURE_COORDS_NUMBER_WIDTH = 10;
    private const int TEXTURE_COORDS_NUMBER_HEIGHT = 11;

    private const float SCORE_INCREMENT_MULTIPLIER = 0.02f;
    
    private const int MAX_SCORE = 99_999;

    private Texture2D texture;
    private Ball ball;

    private double score;

    public double Score
    {
        get => score;
        set => score = MathHelper.Clamp((float) value, 0, MAX_SCORE);
    }

    public int DisplayScore => (int)Math.Floor(Score);
    public int HighScore { get; set; }
    
    public bool HasHighScore => HighScore > 0;
    public int DrawOrder => 0;
    public Vector2 Position { get; set; }

    public ScoreBoard(Texture2D texture, Vector2 position, Ball ball)
    {
        this.texture = texture;
        this.ball = ball;
        Position = position;
    }

    public void Update(GameTime gameTime)
    {
        if(ball.Initialized)
            Score += ball.Speed * 0.75f * SCORE_INCREMENT_MULTIPLIER * gameTime.ElapsedGameTime.TotalSeconds;
    }

    private Rectangle GetDigitTextureBounds(int digit)
    {
        if (digit is < 0 or > 9)
            throw new ArgumentOutOfRangeException(nameof(digit), "The value of digit must be between 0 and 9.");

        int posX = digit * TEXTURE_COORDS_NUMBER_WIDTH;

        return new Rectangle(posX, 0, TEXTURE_COORDS_NUMBER_WIDTH, TEXTURE_COORDS_NUMBER_HEIGHT);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        DrawScore(spriteBatch, DisplayScore, Position.X, Position.Y);
        
        if (HasHighScore)
        {
            DrawHighScore(spriteBatch, Position.X + 5, Position.Y + 15);
        }
    }

    private void DrawScore(SpriteBatch spriteBatch, int score, float startPosX, float y, Color? color = null, float scale = 1)
    {
        int[] scoreDigits = SplitDigits(score);

        float posX = startPosX;

        foreach (int digit in scoreDigits)
        {
            Rectangle textureCoords = GetDigitTextureBounds(digit);

            Vector2 screenPos = new Vector2(posX, y);

            spriteBatch.Draw(texture, screenPos, textureCoords, color ?? Color.White, 0, new Vector2(textureCoords.Width / 2f, textureCoords.Height / 2f),
                scale, SpriteEffects.None, 0f);

            posX += TEXTURE_COORDS_NUMBER_WIDTH * scale;
        }
    }
    
    private void DrawHighScore(SpriteBatch spriteBatch, float startPosX, float y)
    {
        Color color = new(Color.White, ball.IsAlive ? 0.95f : 0.97f);
        DrawScore(spriteBatch, HighScore, startPosX, y, color, 0.75f);
    }


    private int[] SplitDigits(int input)
    {
        string inputStr = input.ToString().PadLeft(5, '0');

        int[] result = new int[inputStr.Length];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = (int)char.GetNumericValue(inputStr[i]);
        }

        return result;
    }
}