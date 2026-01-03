namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    int currentLane = 1; // 0 = left, 1 = middle, 2 = right
    int obstacleLane;
    int score = 0;
    bool isJumping = false;

    IDispatcherTimer gameTimer;
    Random random = new();

    public MainPage()
    {
        InitializeComponent();
        SetupGestures();
        StartGame();
    }

    void SetupGestures()
    {
        var left = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
        left.Swiped += (_, _) => MoveLeft();
        

        var right = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
        right.Swiped += (_, _) => MoveRight();

        var up = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
        up.Swiped += (_, _) => Jump();

        GameArea.GestureRecognizers.Add(left);
        GameArea.GestureRecognizers.Add(right);
        GameArea.GestureRecognizers.Add(up);
    }

    void StartGame()
    {
        obstacleLane = random.Next(0, 3);
        Grid.SetColumn(Obstacle, obstacleLane);
        Obstacle.TranslationY = -60;

        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
    }

    void GameLoop(object sender, EventArgs e)
    {
        Obstacle.TranslationY += 8;

        if (Obstacle.TranslationY > Height)
        {
            Obstacle.TranslationY = -60;
            obstacleLane = random.Next(0, 3);
            Grid.SetColumn(Obstacle, obstacleLane);

            score++;
            ScoreLabel.Text = $"Score: {score}";
        }

        CheckCollision();
    }

    void MoveLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            Grid.SetColumn(Player, currentLane);
        }
    }

    void MoveRight()
    {
        if (currentLane < 2)
        {
            currentLane++;
            Grid.SetColumn(Player, currentLane);
        }
    }

    async void Jump()
    {
        if (isJumping) return;

        isJumping = true;
        await Player.TranslateTo(0, -120, 250);
        await Player.TranslateTo(0, 0, 250);
        isJumping = false;
    }

    void CheckCollision()
    {
        if (currentLane != obstacleLane)
            return;

        if (Obstacle.TranslationY > Height - 150 && !isJumping)
        {
            gameTimer.Stop();
            DisplayAlert("Game Over", $"Score: {score}", "OK");
        }
    }
}
