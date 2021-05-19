using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Methods to allow traversing around game tiles
///   To be implemented by both GameTiles in scene and 
///   "virtual" tiles used by AI
/// </summary>
/// 
public interface ITileTraverse
{
    char col { get; }   // Column of tile location (a-h)
    int  row { get; }   // Row of tile location (1-8)

    bool IsPenguinHere { get; }   // Is a penguin blocking the path?

    bool IsEmpty { get; }    // Is tile an untraversable empty space? 

    int numFish { get; }   // If we need to tally fish totals

    // Identitfy tile by its row and column
    //
    string tileID { get; }

    // Trace path in all directions from this tile
    //
    IEnumerable AllPaths(TileManager tm);
}
