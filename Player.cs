using Microsoft.Maui.Controls;

namespace CrossplatFinal;

public enum PickupType
{
    Coin,
    Shield
}

public class Pickups
{
    private readonly Image pickupView;
    private readonly Random random = new();

    private double speed = 6;

    public bool IsActive { get; private set; }
    public PickupType CurrentType { get; private set; } = PickupType.Coin;

    // coin images (3 options)
    private readonly string[] coinImages = { "coin1.png", "coin2.png", "coin3.png" };

    public Pickups(Image pickupView)
    {
        this.pickupView = pickupView;
        pickupView.IsVisible = false;
    }

    public void SetSpeed(double obstacleSpeed) => speed = obstacleSpeed;

    public void Spawn(double gameAreaWidth)
    {
        if (gameAreaWidth <= 0) return;

        // 25% chance shield, 75% coin
        CurrentType = random.NextDouble() < 0.25 ? PickupType.Shield : PickupType.Coin;

        // choose image
        if (CurrentType == PickupType.Shield)
            pickupView.Source = "shield.png";
        else
            pickupView.Source = coinImages[random.Next(coinImages.Length)];

        // lane positioning
        int lane = random.Next(0, 3);
        double laneWidth = gameAreaWidth / 3;
        double targetX = (laneWidth * lane) + (laneWidth / 2) - (pickupView.WidthRequest / 2);

        pickupView.TranslationY = -pickupView.HeightRequest;
        pickupView.TranslationX = targetX - pickupView.X;

        pickupView.IsVisible = true;
        IsActive = true;
    }

    public void Update()
    {
        if (!IsActive) return;
        pickupView.TranslationY += speed;
    }

    public void CheckOffScreen(double gameAreaHeight)
    {
        if (!IsActive) return;

        if (pickupView.Y + pickupView.TranslationY > gameAreaHeight + pickupView.HeightRequest)
        {
            pickupView.IsVisible = false;
            IsActive = false;
        }
    }

    public bool TryCollect(Image playerView, out PickupType type)
    {
        type = PickupType.Coin;
        if (!IsActive || !pickupView.IsVisible) return false;

        Rect playerRect = new(
            playerView.X + playerView.TranslationX,
            playerView.Y + playerView.TranslationY,
            playerView.Width,
            playerView.Height);

        Rect pickupRect = new(
            pickupView.X + pickupView.TranslationX,
            pickupView.Y + pickupView.TranslationY,
            pickupView.Width,
            pickupView.Height);

        if (playerRect.IntersectsWith(pickupRect))
        {
            type = CurrentType;
            pickupView.IsVisible = false;
            IsActive = false;
            return true;
        }

        return false;
    }
}


