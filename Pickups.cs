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
}