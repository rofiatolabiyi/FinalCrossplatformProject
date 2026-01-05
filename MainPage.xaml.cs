namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    //declarations
    int currentLane = 1; // 0 = left, 1 = middle, 2 = right
    bool isGameRunning;
    int score;

    IDispatcherTimer gameTimer;
    Random random = new();

    const double playerSize = 45;
    double obstacleSpeed = 6;

    public MainPage()
    {
        InitializeComponent();

        // waiting until layout is ready
        GameArea.SizeChanged += OnGameAreaSizeChanged;
    }

    void OnGameAreaSizeChanged(object sender, EventArgs e)
    {
        GameArea.SizeChanged -= OnGameAreaSizeChanged;
        ResetPositions();
    }

    // start
    private void StartButton_Clicked(object sender, EventArgs e)
    {
        StartScreen.IsVisible = false;
        GameArea.IsVisible = true;

        score = 0;
        ScoreLabel.Text = "Score: 0";
        isGameRunning = true;

        ResetPositions();
        StartGameLoop();
    }

    // game loop
    void StartGameLoop()
    {
        gameTimer?.Stop();

        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
    }

    void GameLoop(object sender, EventArgs e)
    {
        if (!isGameRunning)
            return;

        MoveObstacle();
        CheckCollision();
    }

    // player
    void MovePlayer()
    {
        if (GameArea.Width <= 0) return;

        double laneWidth = GameArea.Width / 3;
        double targetX = (laneWidth * currentLane) + (laneWidth / 2) - (playerSize / 2);

        Player.TranslationX = targetX - Player.X;
    }

    // input
    private void OnTapLeft(object sender, EventArgs e)
    {
        if (!isGameRunning) return;

        if (currentLane > 0)
            currentLane--;

        MovePlayer();
    }

    private void OnTapRight(object sender, EventArgs e)
    {
        if (!isGameRunning) return;

        if (currentLane < 2)
            currentLane++;

        MovePlayer();
    }

    private void OnSwipeLeft(object sender, SwipedEventArgs e)
    {
        OnTapLeft(sender, e);
    }

    private void OnSwipeRight(object sender, SwipedEventArgs e)
    {
        OnTapRight(sender, e);
    }

    // obstacle
    void MoveObstacle()
    {
        Obstacle.TranslationY += obstacleSpeed;

        if (Obstacle.Y + Obstacle.TranslationY > GameArea.Height)
        {
            RespawnObstacle();
            score++;
            ScoreLabel.Text = $"Score: {score}";
        }
    }

    void RespawnObstacle()
    {
        if (GameArea.Width <= 0) return;

        int lane = random.Next(0, 3);
        double laneWidth = GameArea.Width / 3;
        double targetX = (laneWidth * lane) + (laneWidth / 2) - (playerSize / 2);

        Obstacle.TranslationY = -playerSize;
        Obstacle.TranslationX = targetX - Obstacle.X;
    }

    // collision
    void CheckCollision()
    {
        Rect playerRect = new(
            Player.X + Player.TranslationX,
            Player.Y + Player.TranslationY,
            Player.Width,
            Player.Height);

        Rect obstacleRect = new(
            Obstacle.X + Obstacle.TranslationX,
            Obstacle.Y + Obstacle.TranslationY,
            Obstacle.Width,
            Obstacle.Height);

        if (playerRect.IntersectsWith(obstacleRect))
        {
            EndGame();
        }
    }

    // end
    void ResetPositions()
    {
        currentLane = 1;
        MovePlayer();
        RespawnObstacle();
    }

    void EndGame()
    {
        isGameRunning = false;
        gameTimer?.Stop();

        StartScreen.IsVisible = true;
        GameArea.IsVisible = false;
    }
}

