using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;
using System.IO;

namespace CrossplatFinal;

public partial class MainPage : ContentPage
{
    // objects
    private readonly Player player;
    private readonly Pickups pickups;

    private bool isGameRunning;
    private int score;
    private double obstacleSpeed = 6;

    // audio
    private IAudioPlayer? coinPlayer;
    private IAudioPlayer? crashPlayer;
    private IAudioPlayer? musicPlayer;

    // timer
    private IDispatcherTimer? gameTimer;
    private readonly Random random = new();

    public MainPage()
    {
        InitializeComponent();

        player = new Player(Player);
        pickups = new Pickups(Pickup);

        LoadPlayerImage();
        UpdateHighScoreButton();
        UpdateCoinsButton();

        // keep HUD hidden until game starts (if you set IsVisible="False" in XAML, this is harmless)
        CoinsButton.IsVisible = false;
        ScoreButton.IsVisible = false;
        HighScoreButton.IsVisible = false;

        GameArea.SizeChanged += OnGameAreaSizeChanged;

        _ = InitAudio();
    }

    // ---------- PLAYER IMAGE (Cars + Custom Skin) ----------
    private void LoadPlayerImage()
    {
        // If user chose "custom skin mode", try to load it
        bool useCustom = Preferences.Get("use_custom_skin", false);

        if (useCustom)
        {
            string customPath = Preferences.Get("player_image", string.Empty);

            if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
            {
                Player.Source = ImageSource.FromFile(customPath);
                return;
            }

            // File missing -> disable custom mode so cars show
            Preferences.Set("use_custom_skin", false);
        }

        // Otherwise load selected car
        int carIndex = Preferences.Get("car_index", 0);

        Player.Source = carIndex switch
        {
            0 => "car1.png",
            1 => "car2.png",
            2 => "car3.png",
            _ => "car1.png"
        };
    }

    // ---------- AUDIO ----------
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

    private void ApplySoundVolume()
    {
        double vol = Preferences.Get("sound_volume", 0.8);
        if (coinPlayer != null) coinPlayer.Volume = vol;
        if (crashPlayer != null) crashPlayer.Volume = vol;
    }

    private void UpdateMusicPlayback()
    {
        if (musicPlayer == null) return;

        bool musicOn = Preferences.Get("music", true);
        double vol = Preferences.Get("music_volume", 0.5);

        musicPlayer.Volume = musicOn ? vol : 0;

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

    // ---------- HUD ----------
    private void UpdateCoinsButton()
    {
        CoinsButton.Text = $"🪙 Coins: {Preferences.Get("coins", 0)}";
    }

    private void UpdateHighScoreButton()
    {
        HighScoreButton.Text = $"High Score: {Preferences.Get("highscore", 0)}";
    }

    // ---------- START / LOOP ----------
    private void StartButton_Clicked(object sender, EventArgs e)
    {
        int difficulty = Preferences.Get("difficulty", 1);

        obstacleSpeed = difficulty switch
        {
            0 => 5,
            1 => 6,
            2 => 8,
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
        UpdateCoinsButton();

        ResetPositions();
        StartGameLoop();
    }

    private void StartGameLoop()
    {
        gameTimer?.Stop();
        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(16);
        gameTimer.Tick += GameLoop;
        gameTimer.Start();
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        if (!isGameRunning) return;

        MoveObstacle();

        pickups.Update();
        pickups.CheckOffScreen(GameArea.Height);

        if (pickups.TryCollect(Player, out int coinValue))
        {
            score += coinValue;
            ScoreButton.Text = $"Score: {score}";

            Preferences.Set("coins", Preferences.Get("coins", 0) + coinValue);
            UpdateCoinsButton();

            if (Preferences.Get("sound", true))
                coinPlayer?.Play();
        }

        CheckCollision();
    }

    // ---------- INPUT ----------
    private void OnTapLeft(object sender, EventArgs e)
    {
        if (!isGameRunning) return;
        player.MoveLeft();
        _ = player.MoveToLaneAnimated(GameArea.Width);
    }

    private void OnTapRight(object sender, EventArgs e)
    {
        if (!isGameRunning) return;
        player.MoveRight();
        _ = player.MoveToLaneAnimated(GameArea.Width);
    }

    private void OnSwipeLeft(object s, SwipedEventArgs e) => OnTapLeft(s, e);
    private void OnSwipeRight(object s, SwipedEventArgs e) => OnTapRight(s, e);

    // ---------- OBSTACLES ----------
    private void MoveObstacle()
    {
        Obstacle.TranslationY += obstacleSpeed;

        if (Obstacle.Y + Obstacle.TranslationY > GameArea.Height + Obstacle.Height)
        {
            RespawnObstacle();

            score++;
            ScoreButton.Text = $"Score: {score}";

            if (score % 5 == 0 && obstacleSpeed < 20)
                obstacleSpeed += 0.5;

            if (score % 3 == 0)
                pickups.Spawn(GameArea.Width);
        }
    }

    private void RespawnObstacle()
    {
        if (GameArea.Width <= 0) return;

        int lane = random.Next(0, 3);
        double laneWidth = GameArea.Width / 3;

        double centerX = (lane * laneWidth) + (laneWidth / 2);

        Obstacle.TranslationX = centerX - (Obstacle.Width / 2) - Obstacle.X;
        Obstacle.TranslationY = -Obstacle.Height;

        Obstacle.Opacity = 0;
        _ = Obstacle.FadeTo(1, 200);
    }

    // ---------- COLLISION ----------
    private void CheckCollision()
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
            EndGame();
    }

    // ---------- END GAME ----------
    private async void EndGame()
    {
        isGameRunning = false;
        gameTimer?.Stop();

        // crash animation
        await Player.RotateTo(20, 150);
        await Player.FadeTo(0, 200);
        await Player.FadeTo(1, 200);
        await Player.RotateTo(0, 150);

        if (Preferences.Get("sound", true))
            crashPlayer?.Play();

        int high = Preferences.Get("highscore", 0);
        if (score > high)
            Preferences.Set("highscore", score);

        UpdateHighScoreButton();

        StartScreen.IsVisible = true;
        GameArea.IsVisible = false;

        CoinsButton.IsVisible = false;
        ScoreButton.IsVisible = false;
        HighScoreButton.IsVisible = false;
    }

    // ---------- LIFECYCLE ----------
    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadPlayerImage();
        ApplySoundVolume();
        UpdateMusicPlayback();
        UpdateCoinsButton();
        UpdateHighScoreButton();
    }

    protected override void OnDisappearing()
    {
        coinPlayer?.Stop();
        crashPlayer?.Stop();
        base.OnDisappearing();
    }

    // ---------- RESET / SIZE ----------
    private void ResetPositions()
    {
        player.Reset();
        _ = player.MoveToLaneAnimated(GameArea.Width);

        pickups.CheckOffScreen(GameArea.Height); // harmless reset safety
        RespawnObstacle();
    }

    private void OnGameAreaSizeChanged(object? sender, EventArgs e)
    {
        GameArea.SizeChanged -= OnGameAreaSizeChanged;
        ResetPositions();
    }

    // ---------- BUTTONS ----------
    private async void SettingsButton_Clicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(SettingsPage));

    private async void CoinsButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("Coins", "Reset coins?", "Yes", "No"))
        {
            Preferences.Set("coins", 0);
            UpdateCoinsButton();
        }
    }

    private async void HighScoreButton_Clicked(object sender, EventArgs e)
    {
        if (await DisplayAlert("High Score", "Reset?", "Yes", "No"))
        {
            Preferences.Set("highscore", 0);
            UpdateHighScoreButton();
        }
    }

    private void ScoreButton_Clicked(object sender, EventArgs e) { }
}







