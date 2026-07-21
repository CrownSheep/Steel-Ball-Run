using System;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Extended.Input;
using SteelBallRun.Entities;
using SteelBallRun.Entities.Managers;

namespace SteelBallRun;

public class Game1 : Game
{
    private const string GAME_TITLE = "Steel Ball Run";
    private const float FADE_IN_ANIMATION_SPEED = 820f;
    
    private const int WINDOW_WIDTH = 1920;
    private const int WINDOW_HEIGHT = 1080;
    
    public const float SCREEN_SCALING = 4f;

    private Color BackgroundColor => new Color(51, 55, 56);
    public GameState State { get; private set; }
    public static Keys PlayKey { get; private set; }
    
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;

    private Texture2D fadeInTexture;
    
    private Sprite backgroundSprite;
    private Sprite startTextSprite;
    
    private Sprite ballSprite;
    private Sprite southSprite;
    private Sprite northSprite;
    
    private Sprite gravityArrowSprite;

    private EntityManager entityManager;
    private ObstacleManager obstacleManager;

    private Ball ball;
    
    private ScoreBoard scoreBoard;
    
    private Wall southWall;
    private Wall northWall;
    
    private float fadeInTexturePosX = 60;
    
    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        graphics.IsFullScreen = true;

        Window.Title = GAME_TITLE;
        
        entityManager = new EntityManager();
    }

    protected override void Initialize()
    {
        graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
        graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
        graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
        fadeInTexture.SetData([BackgroundColor]);
        
        AsepriteFile ballFile = Content.Load<AsepriteFile>("ball");
        AsepriteFile southFile = Content.Load<AsepriteFile>("steel_wall");
        AsepriteFile northFile = Content.Load<AsepriteFile>("steel_wall");
        AsepriteFile mineFile = Content.Load<AsepriteFile>("mine");
        AsepriteFile arrowFile = Content.Load<AsepriteFile>("gravity_arrow");
        AsepriteFile backgroundFile = Content.Load<AsepriteFile>("background");
        AsepriteFile startTextFile = Content.Load<AsepriteFile>("start_text");
        
        Texture2D numbersTexture = Content.Load<Texture2D>("numbers");
        
        ballSprite = ballFile.CreateSprite(GraphicsDevice, 0, true);
        southSprite = southFile.CreateSprite(GraphicsDevice, 0, true);
        northSprite = northFile.CreateSprite(GraphicsDevice, 0, true);
        gravityArrowSprite = arrowFile.CreateSprite(GraphicsDevice, 0, true);
        backgroundSprite = backgroundFile.CreateSprite(GraphicsDevice, 0, true);
        startTextSprite = startTextFile.CreateSprite(GraphicsDevice, 0, true);
        Sprite mineSprite = mineFile.CreateSprite(GraphicsDevice, 0, true);

        northWall = new Wall(northSprite, new Vector2(WINDOW_WIDTH / SCREEN_SCALING - northSprite.Width, 0));
        southWall = new Wall(southSprite, new Vector2(WINDOW_WIDTH / SCREEN_SCALING - southSprite.Width, WINDOW_HEIGHT / SCREEN_SCALING - southSprite.Height));
        
        ball = new Ball(new Vector2(ballSprite.Width, WINDOW_HEIGHT / 2f / SCREEN_SCALING), ballSprite, southWall, northWall);
        ball.Died += BallDied;
        scoreBoard = new ScoreBoard(numbersTexture, new Vector2(WINDOW_WIDTH / 2f / SCREEN_SCALING - 5 * 10 / 2f, WINDOW_HEIGHT / 2f / SCREEN_SCALING), ball);
        obstacleManager = new ObstacleManager(mineSprite, entityManager, ball, scoreBoard);
        entityManager.AddEntity(obstacleManager);
        entityManager.AddEntity(southWall);
        entityManager.AddEntity(northWall);
        entityManager.AddEntity(scoreBoard);
        entityManager.AddEntity(ball);
    }

    protected override void Update(GameTime gameTime)
    {
        // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
        //     Keyboard.GetState().IsKeyDown(Keys.Escape))
        //     Exit();
        
        KeyboardExtended.Update();
        KeyboardStateExtended keyboardState = KeyboardExtended.GetState();
        // if (keyboardState.WasKeyPressed(Keys.S))
        // {
        //     TakeScreenCap();
        // }
        
        entityManager.UpdateEntities(gameTime);
        if (State == GameState.Playing)
        {
            
        } else if (State == GameState.GameOver)
        {
            if (keyboardState.WasKeyPressed(PlayKey))
            {
                Replay();
            }
        }
        else if (State == GameState.Transition)
        {
            fadeInTexturePosX += (float)gameTime.ElapsedGameTime.TotalSeconds * FADE_IN_ANIMATION_SPEED;
            if (fadeInTexturePosX >= WINDOW_WIDTH / SCREEN_SCALING)
            {
                State = GameState.Playing;
                ball.Initialize();
                obstacleManager.IsEnabled = true;
            }
        }
        else if (State == GameState.Initial)
        {
            if (keyboardState.WasAnyKeyJustDown())
            {
                PlayKey = keyboardState.GetPressedKeys()[0];
                StartGame();
            }
        }
        
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(BackgroundColor);

        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(SCREEN_SCALING));
        backgroundSprite.Draw(spriteBatch, new Vector2(0, 60));
        gravityArrowSprite.FlipVertically = ball.GravityDirection == GravityDirection.Up;
        gravityArrowSprite.Draw(spriteBatch, new Vector2(0, 60));
        entityManager.DrawEntities(spriteBatch, gameTime);
        
        spriteBatch.Draw(fadeInTexture, new Rectangle((int)Math.Round(fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
        if(State == GameState.Initial)
            startTextSprite.Draw(spriteBatch, new Vector2(60, 0));
        
        spriteBatch.End();

        base.Draw(gameTime);
    }
    
    public void TakeScreenCap()
    {
        int w = GraphicsDevice.PresentationParameters.BackBufferWidth;
        int h = GraphicsDevice.PresentationParameters.BackBufferHeight;

        Draw(new GameTime());

        int[] backBuffer = new int[w * h];
        GraphicsDevice.GetBackBufferData(backBuffer);

        Texture2D texture = new Texture2D(GraphicsDevice, w, h, false, GraphicsDevice.PresentationParameters.BackBufferFormat);
        texture.SetData(backBuffer);

        Stream stream = File.OpenWrite($"Save_{scoreBoard.Score}.png");

        texture.SaveAsPng(stream, w, h);
        stream.Dispose();
        
        texture.Dispose();
    }
    
    private void BallDied(object sender, EventArgs e)
    {
        State = GameState.GameOver;
        obstacleManager.IsEnabled = false;
        if (scoreBoard.DisplayScore > scoreBoard.HighScore)
        {
            scoreBoard.HighScore = scoreBoard.DisplayScore;
        }
    }
    
    private void StartGame()
    {
        if (State != GameState.Initial) return;

        scoreBoard.Score = 0;
        State = GameState.Transition;
        // ball.BeginJump();
    }
    
    public void Replay()
    {
        if (State != GameState.GameOver) return;

        State = GameState.Playing;
        ball.Initialize();
            
        obstacleManager.Reset();
        obstacleManager.IsEnabled = true;
            
        scoreBoard.Score = 0;
    }
}