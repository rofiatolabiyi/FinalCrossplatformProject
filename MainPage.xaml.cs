using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;
using System.IO;

namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    // objects
    private Player player;
    private Pickups pickups;
    private bool isGameRunning;
    private int score;
    private double obstacleSpeed = 6;

    // audio
    private IAudioPlayer coinPlayer;
    private IAudioPlayer crashPlayer;
    private IAudioPlayer musicPlayer;


    // timer
    private IDispatcherTimer gameTimer;
    private readonly Random random = new();

    // constructor
    public MainPage()
    {
        InitializeComponent();
        LoadPlayerImage();
        UpdateHighScoreButton();
        UpdateCoinsButton();

        player = new Player(Player);
        pickups = new Pickups(Pickup);

        GameArea.SizeChanged += OnGameAreaSizeChanged;

        _ = InitAudio(); // fire-and-forget init
    }

    // save and load players chosen image helper
    private void LoadPlayerImage()
    {
        // 1️⃣ Use uploaded skin if it exists
        string customImagePath = Preferences.Get("player_image", string.Empty);

        if (!string.IsNullOrEmpty(customImagePath) && File.Exists(customImagePath))
        {
            Player.Source = ImageSource.FromFile(customImagePath);
            return;
        }

        // 2️⃣ Otherwise use selected car
        int carIndex = Preferences.Get("car_index", 0);

        Player.Source = carIndex switch
        {
            0 => "car1.png",
            1 => "car2.png",
            2 => "car3.png",
            _ => "car1.png"
        };
    }


    // COINS

    //coins helper collected button
    private void UpdateCoinsButton()
    {
        int totalCoins = Preferences.Get("coins", 0);
        CoinsButton.Text = $"🪙 Coins: {totalCoins}";
    }
    //button handler
    private async void CoinsButton_Clicked(object sender, EventArgs e)
    {
        bool reset = await DisplayAlert(
            "Coins",
            "Reset total coins?",
            "Yes",
            "No");

        if (reset)
        {
            Preferences.Set("coins", 0);
            UpdateCoinsButton();
        }
    }


    // async sound method called from constructor
    private async Task InitAudio()
    {
        try
        {
            coinPlayer = AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("coin.mp3"));

            crashPlayer = AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("crash.mp3"));

            musicPlayer = AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("music.mp3"));

            musicPlayer.Loop = true;
            musicPlayer.Volume = 0.5;

            ApplySoundVolume();    
            UpdateMusicPlayback();  
        }
        catch
        {
            coinPlayer = null;
            crashPlayer = null;
            musicPlayer = null;
        }
    }


    //music helper method
    private void UpdateMusicPlayback()
    {
        bool musicOn = Preferences.Get("music", true);
        double musicVol = Preferences.Get("music_volume", 0.5);

        if (musicPlayer == null)
            return;

        musicPlayer.Volume = musicOn ? musicVol : 0;

        if (musicOn)
        {
            if (!musicPlayer.IsPlaying)
                musicPlayer.Play();
        }
        else
        {
            musicPlayer.Stop();
        }
    }


    // sound slider helper method
    private void ApplySoundVolume()
    {
        double vol = Preferences.Get("sound_volume", 0.8);

        if (coinPlayer != null) coinPlayer.Volume = vol;
        if (crashPlayer != null) crashPlayer.Volume = vol;
    }


    //highscore
    private void UpdateHighScoreButton()
    {
        int highScore = Preferences.Get("highscore", 0);
        HighScoreButton.Text = $"High Score: {highScore}";

    }

    private void OnGameAreaSizeChanged(object sender, EventArgs e)
    {
        GameArea.SizeChanged -= OnGameAreaSizeChanged;
        ResetPositions();
    }

    // start game
    private void StartButton_Clicked(object sender, EventArgs e)
    {
        // load settings
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
        CoinsButton.IsVisible = true;
        ScoreButton.IsVisible = true;
        HighScoreButton.IsVisible = true;

        score = 0;
        ScoreButton.Text = "Score: 0";

        isGameRunning = true;

        UpdateHighScoreButton();

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
            ScoreButton.Text = $"Score: {score}";

            int totalCoins = Preferences.Get("coins", 0);
            totalCoins += coinValue;
            Preferences.Set("coins", totalCoins);

            UpdateCoinsButton();

            // play coin if sounds on
            bool soundOn = Preferences.Get("sound", true);
            if (soundOn)
                coinPlayer?.Play();
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

        // respawn after it fully leaves the screen
        if (Obstacle.Y + Obstacle.TranslationY > GameArea.Height + Obstacle.Height)
        {
            RespawnObstacle();

            score++;
            ScoreButton.Text = $"Score: {score}";

            // adds speed, capped at 20
            if (score % 5 == 0 && obstacleSpeed < 20)
                obstacleSpeed += 0.5;

            // spawn pickup every 3 points
            if (score % 3 == 0)
                pickups.Spawn(GameArea.Width);
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
        UpdateHighScoreButton();
    }

    private void ScoreButton_Clicked(object sender, EventArgs e)
    {
       
    }

    private async void HighScoreButton_Clicked(object sender, EventArgs e)
    {
        bool reset = await DisplayAlert(
            "High Score",
            "Reset high score?",
            "Yes",
            "No");

        if (reset)
        {
            Preferences.Set("highscore", 0);
            UpdateHighScoreButton();
        }
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlayerImage();
        ApplySoundVolume();
        UpdateMusicPlayback();
        UpdateCoinsButton();
    }

    protected override void OnDisappearing()
    {
        coinPlayer?.Stop();
        crashPlayer?.Stop();
        musicPlayer?.Stop();
        base.OnDisappearing();
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

        // play crash 
        bool soundOn = Preferences.Get("sound", true);
        if (soundOn)
            crashPlayer?.Play();

        // save high score
        int highScore = Preferences.Get("highscore", 0);
        if (score > highScore)
            Preferences.Set("highscore", score);

        UpdateHighScoreButton();

        StartScreen.IsVisible = true;
        GameArea.IsVisible = false;
        CoinsButton.IsVisible = false;
        ScoreButton.IsVisible = false;
        HighScoreButton.IsVisible = false;

    }
}






