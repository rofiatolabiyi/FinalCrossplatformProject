namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    int currentLane = 1; // 0 = left, 1 = middle, 2 = right
    bool isGameRunning = false;
    int score = 0;

    IDispatcherTimer gameTimer;
    Random random = new();

    const double playerSize = 45;
    const double obstacleSpeed = 6;

    public MainPage()
    {
        InitializeComponent();
        ResetPositions();
    }

    // starting event handler
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

    // loop
    void StartGameLoop()
    {
        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16); // fps
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

    // moving player
    void MovePlayer()
    {
        double laneWidth = GameArea.Width / 3;
        double x = (laneWidth * currentLane) + (laneWidth / 2) - (playerSize / 2);

        AbsoluteLayout.SetLayoutBounds(Player,
            new Rect(x, GameArea.Height - 80, playerSize, playerSize));
    }

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

    // trucks
    void MoveObstacle()
    {
        double y = Obstacle.TranslationY + obstacleSpeed;
        Obstacle.TranslationY = y;

        if (y > GameArea.Height)
        {
            RespawnObstacle();
            score++;
            ScoreLabel.Text = $"Score: {score}";
        }
    }

    void RespawnObstacle()
    {
        int lane = random.Next(0, 3);
        double laneWidth = GameArea.Width / 3;
        double x = (laneWidth * lane) + (laneWidth / 2) - (playerSize / 2);

        Obstacle.TranslationY = -playerSize;

        AbsoluteLayout.SetLayoutBounds(Obstacle,
            new Rect(x, 0, playerSize, playerSize));
    }

    // collision
    void CheckCollision()
    {
        Rect playerRect = new Rect(
            Player.X + Player.TranslationX,
            Player.Y + Player.TranslationY,
            Player.Width,
            Player.Height);

        Rect obstacleRect = new Rect(
            Obstacle.X + Obstacle.TranslationX,
            Obstacle.Y + Obstacle.TranslationY,
            Obstacle.Width,
            Obstacle.Height);

        if (playerRect.IntersectsWith(obstacleRect))
        {
            EndGame();
        }
    }


    // reset
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
