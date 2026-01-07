using Microsoft.Maui.Controls;

namespace CrossplatFinal;

public class Pickups
{
    //declaring variabeles
    private readonly Image pickup;
    private readonly Random random;

    private double speed = 5;

    public Pickups(Image pickupImage)
    {
        pickup = pickupImage;
        random = new Random();
    }

    // spawn coin at top in random lane
    public void Spawn(double gameWidth)
    {
        int lane = random.Next(0, 3);
        double laneWidth = gameWidth / 3;
        double targetX = (laneWidth * lane) + (laneWidth / 2) - (pickup.Width / 2);

        pickup.TranslationX = targetX - pickup.X;
        pickup.TranslationY = -pickup.Height;
        pickup.IsVisible = true;
    }

    // move coin down
    public void Update()
    {
        if (!pickup.IsVisible) return;

        pickup.TranslationY += speed;
    }

    // check if player collected coin
    public bool CheckCollected(Image player)
    {
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
            return true;
        }

        return false;
    }

    // hide coin if it goes off screen
    public void CheckOffScreen(double gameHeight)
    {
        if (pickup.IsVisible &&
            pickup.Y + pickup.TranslationY > gameHeight)
        {
            pickup.IsVisible = false;
        }
    }

}