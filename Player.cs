using Microsoft.Maui.Controls;

namespace CrossplatFinal;

public class Player
{
    public Image View { get; }

    // lanes: 0 = left, 1 = middle, 2 = right
    public int Lane { get; private set; } = 1;

    // size of car
    public double Size { get; } = 160;

    // constructor
    public Player(Image view)
    {
        View = view;
    }

    // Move player to the current lane
    public void MoveToLane(double gameAreaWidth)
    {
        if (gameAreaWidth <= 0) return;

        double laneWidth = gameAreaWidth / 3;
        double targetX = (laneWidth * Lane) + (laneWidth / 2) - (Size / 2);

        // Move using translation
        View.TranslationX = targetX - View.X;
    }

    // Move left
    public void MoveLeft()
    {
        if (Lane > 0)
            Lane--;
    }

    // Move right
    public void MoveRight()
    {
        if (Lane < 2)
            Lane++;
    }

    // Reset player to middle lane
    public void Reset()
    {
        Lane = 1;
        View.TranslationX = 0;
    }
}

