using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A "virtual tile" used by AI to evaluate candidate moves
/// </summary>
/// 
public class VTile : ScriptableObject, ITileTraverse
{
    char m_col;
    int m_row;

    int  m_numFish;    // Number of fish on tile
    bool m_isActive;   // Is tile active or sunk?
    bool m_isPenguin;  // Is there a penguin on this tile?

    float m_fHScore = 0;    // Heuristic score for this tile (computed by an external method)


    public char col
    {
        get { return m_col; }
        set { m_col = value; }
    }

    public int row
    {
        get { return m_row; }
        set { m_row = value; }
    }

    public int numFish
    {
        get { return m_numFish; }
        set { m_numFish = value; }
    }

    public bool IsPenguinHere
    {
        get { return m_isPenguin; }
        set { m_isPenguin = value; }
    }

    public bool IsEmpty
    {
        get { return !m_isActive; }
    }

    public bool IsActive
    {
        get { return m_isActive; }
        set { m_isActive = value; }
    }

    public float HeuristicScore
    {
        get { return m_fHScore; }
        set { m_fHScore = value; }
    }

    public string tileID
    {
        get { return col + row.ToString(); }
    }

    public string tileDesc  // Text description of contents of tile
    {
        get
        {
            if (IsActive)
            {
                if (IsPenguinHere) return "P";
                //
                else return m_numFish.ToString();
            }
            else return "0";  // Tile removed from game
        }
    }


    public override string ToString()
    {
        return tileID;
    }


    public int CompareTo(VTile tile2)
    {
        // A null value means that this move is "Higher" in score
        //
        if (tile2 == null) return 1;
        //
        else
            return HeuristicScore.CompareTo(tile2.HeuristicScore);
    }


    // Initialize Virtual Tile from GameTile
    //
    public void InitFromGameTile(GameTile gt)
    {
        col = gt.col;
        row = gt.row;

        IsPenguinHere = gt.IsPenguinHere;

        numFish  =  gt.FishScoreValue;
        IsActive = !gt.IsEmpty;
    }


    // Trace path in all directions
    //
    public IEnumerable AllPaths(TileManager tm)
    {
        ITileTraverse t0 = this;

        foreach (TileManager.TileTraverse ThataWay in tm.funcsTraverse)
        {
            foreach (ITileTraverse t in TileManager.GetPathInDirection(t0, ThataWay))
            {
                yield return t;
            }
        }
    }
}
