using Microsoft.Maui.Controls;

namespace CrossplatFinal;

public class Pickups
{
    private readonly Image pickup;
    private readonly Random random = new();

    private double speed = 5;

    private int currentValue = 0;

    public Pickups(Image pickupImage)
    {
        pickup = pickupImage;
    }

    // Spawn coin with weighted randomness
    public void Spawn(double gameWidth)
    {
        int roll = random.Next(100); // 0–99

        if (roll < 60)
        {
            pickup.Source = "bronzecoin.png";
            currentValue = 1;
        }
        else if (roll < 90)
        {
            pickup.Source = "silvercoin.png";
            currentValue = 5;
        }
        else
        {
            pickup.Source = "goldcoin.png";
            currentValue = 10;
        }

        int lane = random.Next(0, 3);
        double laneWidth = gameWidth / 3;
        double targetX = (laneWidth * lane) + (laneWidth / 2) - (pickup.Width / 2);

        pickup.TranslationX = targetX - pickup.X;
        pickup.TranslationY = -pickup.Height;
        pickup.Rotation = 0;
        pickup.IsVisible = true;
    }

    // Move + animate coin
    public void Update()
    {
        if (!pickup.IsVisible) return;

        pickup.TranslationY += speed;
        pickup.Rotation += 6; // spin
    }

    // Collision check
    public bool TryCollect(Image player, out int value)
    {
        value = 0;

        if (!pickup.IsVisible) return false;

        Rect playerRect = new(
            player.X + player.TranslationX,
            player.Y + player.TranslationY,
            player.Width,
            player.Height);

        Rect pickupRect = new(
            pickup.X + pickup.TranslationX,
            pickup.Y + pickup.TranslationY,
            pickup.Width,
            pickup.Height);

        if (playerRect.IntersectsWith(pickupRect))
        {
            pickup.IsVisible = false;
            value = currentValue;
            return true;
        }

        return false;
    }

    // Hide if off screen
    public void CheckOffScreen(double gameHeight)
    {
        if (pickup.IsVisible &&
            pickup.Y + pickup.TranslationY > gameHeight)
        {
            pickup.IsVisible = false;
        }
    }
}
