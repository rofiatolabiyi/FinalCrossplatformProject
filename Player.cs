using System;
using System.Collections.Generic;
using System.Text;

namespace CrossplatFinal.Models;

public class Player
{
    public Image View { get; }

    // Current lane: 0 = left, 1 = middle, 2 = right
    public int Lane { get; private set; } = 1;

    // size of car
    public double Size { get; } = 160;

    // constructor
    public Player(Image view)
    {
        View = view;
    }

    // Move to a specific lane
    public void MoveToLane(double gameAreaWidth)
    {
        double laneWidth = gameAreaWidth / 3;
        double targetX = (laneWidth * Lane) + (laneWidth / 2) - (Size / 2);

        // Move using translation
        View.TranslationX = targetX - View.X;
    }

    // moving in lanes

    public void MoveLeft()
    {
        if (Lane > 0)
            Lane--;
    }

    public void MoveRight()
    {
        if (Lane < 2)
            Lane++;
    }

    public void Reset()
    {
        Lane = 1;
    }
}

