using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    // Current state variables
    // About chessman to be moved
    public (Chessman chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman;
    // Chessman to be captured
    public List<Chessman> capturedChessman;

    public int depth;

    public void SetState((Chessman chessman, (int x, int y) oldPosition, (int x, int y) newPosition, bool isMoved) movedChessman,
                          List<Chessman> capturedChessman, int depth)
    {
        this.movedChessman = movedChessman;
        this.capturedChessman = capturedChessman;
        this.depth = depth;
    }
}