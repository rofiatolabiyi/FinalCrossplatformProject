using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;


namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    // player object
    private Player player;
    private Pickups pickups;
    private bool isGameRunning;
    private int score;
    private double obstacleSpeed = 6;

    // timer
    private IDispatcherTimer gameTimer;
    private readonly Random random = new();

    // constructor
    public MainPage()
    {
        InitializeComponent();
        LoadPlayerImage();
        UpdateHighScoreLabel();

        player = new Player(Player);
        pickups = new Pickups(Pickup);

        GameArea.SizeChanged += OnGameAreaSizeChanged;
    }


    // save and load players chosen image helper
    private void LoadPlayerImage()
    {
        string imagePath = Preferences.Get("player_image", string.Empty);

        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            Player.Source = ImageSource.FromFile(imagePath);
        }
        else
        {
            Player.Source = "player.png"; // default image
        }
    }

    private void UpdateHighScoreLabel()
    {
        int highScore = Preferences.Get("highscore", 0);
        HighScoreLabel.Text = $"High Score: {highScore}";
    }

    private void OnGameAreaSizeChanged(object sender, EventArgs e)
    {
        GameArea.SizeChanged -= OnGameAreaSizeChanged;
        ResetPositions();
    }

    // start game
    private void StartButton_Clicked(object sender, EventArgs e)
    {
        // Load settings
        int difficulty = Preferences.Get("difficulty", 1);

        obstacleSpeed = difficulty switch
        {
            0 => 5,  // easy
            1 => 6,  // normal
            2 => 8,  // hard
            _ => 6
        };

        StartScreen.IsVisible = false;
        GameArea.IsVisible = true;

        score = 0;
        ScoreLabel.Text = "Score: 0";
        isGameRunning = true;

        UpdateHighScoreLabel();

        ResetPositions();
        StartGameLoop();
    }

    private void StartGameLoop()
    {
        gameTimer?.Stop();

        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
    }

    private void GameLoop(object sender, EventArgs e)
    {
        if (!isGameRunning)
            return;

        MoveObstacle();

        pickups.Update();
        pickups.CheckOffScreen(GameArea.Height);

        if (pickups.TryCollect(Player, out int coinValue))
        {
            score += coinValue;
            ScoreLabel.Text = $"Score: {score}";
        }

        CheckCollision();
    }


    // player
    private void OnTapLeft(object sender, EventArgs e)
    {
        if (!isGameRunning) return;

        player.MoveLeft();
        player.MoveToLane(GameArea.Width);
    }

    private void OnTapRight(object sender, EventArgs e)
    {
        if (!isGameRunning) return;

        player.MoveRight();
        player.MoveToLane(GameArea.Width);
    }

    private void OnSwipeLeft(object sender, SwipedEventArgs e) => OnTapLeft(sender, e);
    private void OnSwipeRight(object sender, SwipedEventArgs e) => OnTapRight(sender, e);

    // obstacle
    private void MoveObstacle()
    {
        Obstacle.TranslationY += obstacleSpeed;

        // Slightly more reliable: respawn after it fully leaves the screen
        if (Obstacle.Y + Obstacle.TranslationY > GameArea.Height + Obstacle.Height)
        {
            RespawnObstacle();

            score++;
            ScoreLabel.Text = $"Score: {score}";

            // adds speed, capped at 20
            if (score % 5 == 0 && obstacleSpeed < 20)
                obstacleSpeed += 0.5;

            if (score % 3 == 0) // every 3 points
            {
                pickups.Spawn(GameArea.Width);
            }

        }
    }

    private void RespawnObstacle()
    {
        if (GameArea.Width <= 0) return;

        int lane = random.Next(0, 3);
        double laneWidth = GameArea.Width / 3;
        double targetX = (laneWidth * lane) + (laneWidth / 2) - (Obstacle.Width / 2);

        Obstacle.TranslationY = -Obstacle.Height;
        Obstacle.TranslationX = targetX - Obstacle.X;
    }

    // collision
    private void CheckCollision()
    {
        Rect playerRect = new(
            player.View.X + player.View.TranslationX,
            player.View.Y + player.View.TranslationY,
            player.View.Width,
            player.View.Height);

        Rect obstacleRect = new(
            Obstacle.X + Obstacle.TranslationX,
            Obstacle.Y + Obstacle.TranslationY,
            Obstacle.Width,
            Obstacle.Height);

        if (playerRect.IntersectsWith(obstacleRect))
            EndGame();
    }

    // settings handler
    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    // reset high score
    private void ResetHighScore_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("highscore", 0);
        UpdateHighScoreLabel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayerImage();
    }


    // end and reset
    private void ResetPositions()
    {
        player.Reset();
        player.MoveToLane(GameArea.Width);

        RespawnObstacle();
    }

    private void EndGame()
    {
        isGameRunning = false;
        gameTimer?.Stop();

        // save high score
        int highScore = Preferences.Get("highscore", 0);
        if (score > highScore)
            Preferences.Set("highscore", score);

        UpdateHighScoreLabel();

        StartScreen.IsVisible = true;
        GameArea.IsVisible = false;
    }
}





