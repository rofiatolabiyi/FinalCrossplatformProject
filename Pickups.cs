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
}