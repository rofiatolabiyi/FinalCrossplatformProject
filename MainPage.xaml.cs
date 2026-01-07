using Microsoft.Maui.Storage;

namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    // player object
    private Player player;

    private bool isGameRunning;
    private int score;
    private double obstacleSpeed = 6;

    // timer
    private IDispatcherTimer gameTimer;
    private readonly Random random = new();

    public MainPage()
    {
        InitializeComponent();

        player = new Player(Player);

        GameArea.SizeChanged += OnGameAreaSizeChanged;
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

        if (Obstacle.Y + Obstacle.TranslationY > GameArea.Height)
        {
            RespawnObstacle();

            score++;
            ScoreLabel.Text = $"Score: {score}";

            // adds speed, capped at 20
            if (score % 5 == 0 && obstacleSpeed < 20)
                obstacleSpeed += 2;
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

    // settings handler (Shell navigation)
    private async void SettingsButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
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

        StartScreen.IsVisible = true;
        GameArea.IsVisible = false;
    }
}




