using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VPenguin : ScriptableObject
{
    public int    numFish;    // Number of fish won
    public bool   isActive;   // Is penguin active?
    public string currTile;   // Current tile penguin is standing on
    public string lastTile;   // Tile penguin was last standing on
}
