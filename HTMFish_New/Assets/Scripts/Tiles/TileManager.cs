using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to internally handle the tiles, i.e.  
///  Lookup table for tiles, utils for navigating them, checking if a GameTile
///   has been selected, and handling their deletion
/// </summary>
/// <remarks>
/// Columns run in positive-X and rows in positive-Z.  Also columns are designated by letters a-h
///   and rows by numerals 1-8.  This is all parallel to the square naming convention in chess.
/// </remarks>
/// 
public class TileManager : MonoBehaviour
{
    // For distribution of GameTile types at startup
    //
    public static int num1FishTiles = 0;
    public static int num2FishTiles = 0;
    public static int num3FishTiles = 0;
    //
    const int total1FishTiles = 28;
    const int total2FishTiles = 20;
    const int total3FishTiles = 12;

    // All the possible GameTile types
    //
    public enum TileType { OneFish = 0, TwoFish = 1, BlueFish = 2 };


    // GameTile table
    //
    public Dictionary<string, GameTile> tileTable = new Dictionary<string, GameTile>();

    // Keep track of a list of tiles being evaluated
    //
    [HideInInspector]
    //
    public List<GameTile> workTiles = new List<GameTile>();

    // Current GameTile selected by user
    //
    GameTile selectedTile = null;

    bool m_bTilesSetup       = false;    // Returns TRUE when setup of tiles is complete
    bool m_bSelectTiles      = true;     // Are we in a game state that allows GameTile selection?
    bool m_bSelectedTileFlag = false;    // Has a GameTile been selected?

    // This property is set by the Game state machine
    //
    public bool ListenForSelectedTiles
    {
        get { return m_bSelectTiles;  }
        set { m_bSelectTiles = value; }
    }


    public bool BoardSetupComplete()
    {
        return m_bTilesSetup;
    }


    /// <summary>
    /// Pick a random type for each new tile, maintaining set proportions for num of each GameTile
    /// </summary>
    /// <returns>Type to make this GameTile</returns>
    /// 
    public static TileType GetRndType()
    {
        const int rndThreshhold_OneFish = total1FishTiles;
        const int rndThreshhold_TwoFish = total1FishTiles + total2FishTiles;
        const int rndThreshhold_ThrFish = total1FishTiles + total2FishTiles + total3FishTiles;

        // Get random tile types, but in correct proportions
        //
        int r = Random.Range(0, rndThreshhold_ThrFish);
        //
        if (r <= rndThreshhold_OneFish)
        {
            return TileType.OneFish;
        }
        else if (r <= rndThreshhold_TwoFish)
        {
            return TileType.TwoFish;
        }
        else  
        {
            return TileType.BlueFish;
        }
    }


    /// <summary>
    /// Check if a GameTile has been selected/clicked
    /// </summary>
    /// <remarks>
    /// CALLED BY: Update()
    /// </remarks>
    /// 
    public bool CheckSelectedTile()
    {
        selectedTile        = null;
        m_bSelectedTileFlag = false;

        foreach (KeyValuePair<string, GameTile> tile_kvp in tileTable)
        {
            if (tile_kvp.Value.IsSelected)
            {
                selectedTile = tile_kvp.Value;

                m_bSelectedTileFlag = true;  
            }
        }

        return m_bSelectedTileFlag;
    }


    /// <summary>
    /// Clear selection on selected GameTile (Should never be more than one selected at a time)
    /// </summary>
    /// <remarks>
    /// CALLED BY: Update()
    /// </remarks>
    /// 
    public void ClearSelection()
    {
        selectedTile.ClearSelected();

        selectedTile = null;
    }


    /// <summary>
    /// Turn off selectability for all tiles
    /// </summary>
    /// <remarks>
    /// Call after setup and each move -- Clicking on a penguin will enable those tiles it can move to
    /// </remarks>
    /// 
    public void ClearAllSelectable()
    {
        foreach (KeyValuePair<string, GameTile> tile_kvp in tileTable)
        {
            tile_kvp.Value.CanSelect = false;
        }
    }


    /// <summary>
    /// Disable all tiles to wait for a penguin click
    /// </summary>
    /// <remarks>
    /// INVOKED BY: GameState.State_Play::O.S.Enter => GameManager::SendMessage()
    /// </remarks>
    /// 
    public void OnDisableAllTiles()
    {
        ClearAllSelectable();
    }


    /// <summary>
    /// At penguin set-up, enable all tiles that have one fish
    ///   (All penguins must start on a one-fish tile -- See rules of HTMFish)
    /// </summary>
    /// <remarks>
    /// INVOKED BY: GameState::State_Initialize::OnStateEnter()
    /// </remarks>
    /// 
    public void OnEnableOneFishTiles()
    {
        Dictionary<string, GameTile>.ValueCollection allTiles = tileTable.Values;  // Get collection of all tiles

        foreach (GameTile tile in allTiles)
        {
            if (tile.FishScoreValue == 1)
            {
                tile.CanSelect = true;   // Enable tile if it has only one fish
            }
            else tile.CanSelect = false;
        }
    }


    /// <summary>
    /// Enable selection of all tiles the selected penguin can move to
    /// </summary>
    /// <param name="lstValidTiles">List of IDs of valid tiles, output by GameManager::FindLegalMoves(GamePenguin penguin)</param>
    /// 
    public void EnableValidTiles(List<GameTile> lstValidTiles)
    {
        foreach (GameTile tile in lstValidTiles)
        {
            string tileID = tile.ToString();

            if (tileTable.ContainsKey(tileID))   // Verify tile exists
            {
                tileTable[tileID].CanSelect = true;   // Enable it
            }
        }
    }


    /// <summary>
    /// "Sink" a bunch of tiles at once
    /// </summary>
    /// 
    public void ExpungeTiles(List<GameTile> tiles)
    {
        foreach(GameTile tile in tiles)
        {
            if(!tile.IsEmpty) tile.Sink();
        }
    }


    /// <summary>
    /// Get rough distance between two tiles (assumes a square grid for brevity)
    /// </summary>
    /// <remarks>
    /// This method is used to prevent AI from placing penguins too close to each other.
    /// </remarks>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>Distance in integral units</returns>
    /// 
    public static float Distance(GameTile t1, GameTile t2)
    {
        int rowDist = Mathf.Abs(t1.row - t2.row);
        int colDist = Mathf.Abs(t1.col - t2.col);

        return Mathf.Sqrt(rowDist * rowDist + colDist * colDist);
    }



    /*** Tile Traversal ***/

    /// <summary>
    /// Build list of legal moves from current tile
    /// </summary>
    /// <param name="startTile">Starting tile</param>
    /// <param name="lstValidTiles">List to hold the valid tiles we can move to (Belongs to a penguin)</param>
    /// 
    public void FindValidTiles(GameTile startTile, List<GameTile> lstValidTiles)
    {
        lstValidTiles.Clear();   // Ensure list is empty

        foreach (GameTile tile in startTile.AllPaths(this))   // AllPaths() traverses a "search tree" for valid moves
        {
            if (tile != null) lstValidTiles.Add(tile);   // Add to list -- Exclude nulls returned at end of a path
        }

        EnableValidTiles(lstValidTiles);   // Highlight only the tiles we can move to
    }


    
    /*** Utility funcs to get adjacent tiles to GameTile at input column, row ***/

    // GameTile to the "east"
    //
    public static string GetNameTile_E(char column, int row)
    {
        column++;

        return column + row.ToString();
    }

    // GameTile to the "southeast"
    //
    public static string GetNameTile_SE(char column, int row)
    {
        row--;

        if (row % 2 == 0) column++;

        return column + row.ToString();
    }

    // GameTile to the "southwest"
    //
    public static string GetNameTile_SW(char column, int row)
    {
        row--;

        if (row % 2 == 1) column--;

        return column + row.ToString();
    }

    // GameTile to the "west"
    //
    public static string GetNameTile_W(char column, int row)
    {
        column--;

        return column + row.ToString();
    }

    // GameTile to the "northwest"
    //
    public static string GetNameTile_NW(char column, int row)
    {
        row++;

        if (row % 2 == 1) column--;

        return column + row.ToString();
    }

    // GameTile to the "northeast"
    //
    public static string GetNameTile_NE(char column, int row)
    {
        row++;

        if (row % 2 == 0) column++;

        return column + row.ToString();
    }


    /// <summary>
    /// Verify Tile of given ID exists and return it if it does
    /// </summary>
    /// <param name="sTileID"></param>
    /// <returns>GameTile if found, NULL if not</returns>
    /// 
    GameTile ValidateTile(string sTileID)
    {
        if (tileTable.ContainsKey(sTileID))
        {
            return tileTable[sTileID];
        }
        else return null;   // GameTile doesn't exist
    }


    /*** Get adjacent tile to input tile in a curent direction ***/

    public delegate ITileTraverse TileTraverse(ITileTraverse t);

    // This can be a fixed array because the hexagonal tiles will always have six possible directions
    //
    public TileTraverse[] funcsTraverse = new TileTraverse[6];


    // ITileTraverse to the "east"
    //
    public ITileTraverse GetTile_E(ITileTraverse t)
    {
        string sTileID = GetNameTile_E(t.col, t.row);

        return ValidateTile(sTileID);
    }

    // ITileTraverse to the "southeast"
    //
    public ITileTraverse GetTile_SE(ITileTraverse t)
    {
        string sTileID = GetNameTile_SE(t.col, t.row);

        return ValidateTile(sTileID);
    }

    // ITileTraverse to the "southwest"
    //
    public ITileTraverse GetTile_SW(ITileTraverse t)
    {
        string sTileID = GetNameTile_SW(t.col, t.row);

        return ValidateTile(sTileID);
    }

    // ITileTraverse to the "west"
    //
    public ITileTraverse GetTile_W(ITileTraverse t)
    {
        string sTileID = GetNameTile_W(t.col, t.row);

        return ValidateTile(sTileID);
    }

    // ITileTraverse to the "northwest"
    //
    public ITileTraverse GetTile_NW(ITileTraverse t)
    {
        string sTileID = GetNameTile_NW(t.col, t.row);

        return ValidateTile(sTileID);
    }

    // ITileTraverse to the "northeast"
    //
    public ITileTraverse GetTile_NE(ITileTraverse t)
    {
        string sTileID = GetNameTile_NE(t.col, t.row);

        return ValidateTile(sTileID);
    }


    // Trace path of tiles (GameTile or VTile) in one of six possible directions (Because hexagons)
    //
    public static IEnumerable GetPathInDirection(ITileTraverse startTile, TileTraverse GetTileInDirection)
    {
        ITileTraverse t = startTile;

        for (;;)
        {
            t = GetTileInDirection(t);   // Traverse to next tile in designated direction

            if (t == null) yield break;  // End of path reached -- Terminate traversal
            //
            else
            {
                if (t.IsPenguinHere || t.IsEmpty) yield break;   // Stop if there's a penguin in the way, or no physical tile present
                //
                else yield return t;    // Else return the tile just traversed to
            }
        }
    }


    /// <summary>
    /// Recursive func that checks neighboring tiles for penguins in order to determine if a penguin is
    ///   standing alone on a contiguous "island" of tiles
    /// </summary>
    /// <param name="centerTile">Tile from which to check adjacent tiles</param>
    /// <param name="checkedTiles">List of tiles already checked</param>
    /// <param name="bCheckPenguins">Check if there's another penguin on this "floe" (mid-game) or ignore (end of game)</param>
    /// <returns>If we are on an "island", return the total number of fish there
    /// (player wins all on next move), else -1 if another penguin found.</returns>
    /// <remarks>Note that "checkedTiles" is not cleared in this method, because we may want to call it successively to
    /// check multiple penguins on the same "ice floe"</remarks>
    /// 
    public static int CheckForIsland<TileType>(TileManager tm, TileType centerTile, List<TileType> checkedTiles, bool bCheckPenguins)
        //
        where TileType : ITileTraverse 
    {
        int childNodeFish  = 0;
        int localTotalFish = 0;

        if (!checkedTiles.Contains(centerTile))
        {
            checkedTiles.Add(centerTile);   // Store starting tile right away

            localTotalFish = centerTile.numFish;   // Add fish from starting tile
        }

        foreach (TileTraverse ThataWay in tm.funcsTraverse)
        {
            TileType tile = (TileType)ThataWay(centerTile);   // Get adjacent tile in current direction

            if (tile == null) continue;   // We've reached the edge -- No tile in this direction

            // Check possible conditions, if this tile hasn't already been checked
            //   (If this is an empty tile or one we already checked, don't go further in this direction)
            //
            if (!checkedTiles.Contains(tile) && !tile.IsEmpty)  
            {
                checkedTiles.Add(tile);   // Add to checked tiles

                if (bCheckPenguins && tile.IsPenguinHere) return -1;  // We're done -- Target Penguin not alone here -- Return
                //
                else   // Go to next tile
                {
                    localTotalFish += tile.numFish;   // Add fish from this tile

                    childNodeFish = CheckForIsland(tm, tile, checkedTiles, bCheckPenguins);

                    if (childNodeFish == -1) return -1;  // Once penguin found we can just break out with -1
                    //
                    localTotalFish += childNodeFish;   // Add fish found on this iteration and continue
                }
            }
        }

        return localTotalFish;  // Return the fish total 
    }


    void InitTraversalTable()
    {
        funcsTraverse[0] = GetTile_E;
        funcsTraverse[1] = GetTile_SE;
        funcsTraverse[2] = GetTile_SW;
        funcsTraverse[3] = GetTile_W;
        funcsTraverse[4] = GetTile_NW;
        funcsTraverse[5] = GetTile_NE;
    }

    
    void Awake ()
    {
        InitTraversalTable();
    }

    /*
    void Update ()
    {
        if (ListenForSelectedTiles)
        {
            if (CheckSelectedTile())   // A GameTile was selected
            {
                // Send message to GameManager
                //
                gameObject.SendMessage("OnTileClicked", selectedTile);

                // So previous line is not called repeaatedly!
                //
                m_bSelectedTileFlag = false;

                ClearSelection();
            } 
        }
    }
    */

    /// <summary>
    /// Set up all the tiles on the game board
    /// </summary>
    /// <remarks>
    /// CALLED BY: GameManager::SetupBoard()
    /// </remarks>
    /// 
    public void SetupBoard(Vector3 v3Start, int dim)
    {
        SetupTiles setup = GetComponent<SetupTiles>();

        // Setup the tiles
        //
        m_bTilesSetup = setup.SetupAll(v3Start, dim);
    }


    /// <summary>
    /// Display all tiles in table and their neighbors -- For debugging
    /// </summary>
    /// 
    public void DumpTiles()
    {
        StartCoroutine(_dumpTiles());
    }
    //
    public IEnumerator _dumpTiles()
    {
        foreach (KeyValuePair<string, GameTile> tileInfo in tileTable)
        {
            Debug.Log("GameTile (" + tileInfo.Key + "): " + tileInfo.Value);

            yield return null;
        }
    }
}
