using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class holding tile utility funcs, e.g. how to navigate to adjacent tiles
/// </summary>
/// 
public static class Tile 
{
    // For distribution of tile types at startup
    //
    public static int num1FishTiles = 0;
    public static int num2FishTiles = 0;
    public static int num3FishTiles = 0;
    //
    const int total1FishTiles = 28;
    const int total2FishTiles = 20;
    const int total3FishTiles = 12;

    // All the possible tile types
    //
    public enum TileType { Empty = 0, OneFish = 1, TwoFish = 2, BlueFish = 3  };

    /// <summary>
    /// Pick a random type for new type, maintaining set proportions for num of each tile
    /// </summary>
    /// <returns>Type to make this tile</returns>
    /// 
    static TileType GetRndType()
    {
        TileType type = (TileType)Random.Range(1, 4);

        if(type == TileType.BlueFish && num3FishTiles < total3FishTiles)
        {
            num3FishTiles++;

            return type;
        }
        else if (type == TileType.TwoFish && num2FishTiles < total2FishTiles)
        {
            num2FishTiles++;

            return type;
        }
        else if (type == TileType.OneFish && num1FishTiles < total1FishTiles)  // 1-fish tile is the most numerous
        {
            num1FishTiles++;

            return type;
        }
        else  // We shouldn't ever reach this, but for foolproofness...
        {
            return TileType.Empty;
        }
    }
}

