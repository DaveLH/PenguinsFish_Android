using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulate the game board for AI use
/// </summary>
/// 
public class AIBoardSim : MonoBehaviour
{
    // Ref to GameManager
    //
    GameManager m_refGM;

    // Ref to TileManager
    //
    TileManager m_refTM;

    // Current Player Index
    //
    // GamePenguin.PenguinColor m_currentPlayerIndex;

    // "Map" of board
    //
    public Dictionary<string, VTile> tileTable = new Dictionary<string, VTile>();

    // Local Table of players and penguins
    //
    public Dictionary<GamePenguin.PenguinColor, List<VPenguin>> tablePlayerPenguins = new Dictionary<GamePenguin.PenguinColor, List<VPenguin>>();
    
    // List of tiles (as string ID's) the players can currently move to
    //
    public Dictionary<GamePenguin.PenguinColor, List<string>> tableValidTileMoves = new Dictionary<GamePenguin.PenguinColor, List<string>>();

    // List of tiles won by players on last move
    //
    public Dictionary<GamePenguin.PenguinColor, List<VTile>> tableWonTiles = new Dictionary<GamePenguin.PenguinColor, List<VTile>>();

    // Use stack for "tree" of moves AI is testing
    //
    public Stack<Command.GameMove> testMoveStack = new Stack<Command.GameMove>();

    // Best move found to play
    //
    public Command.GameMove pendingMove = null;

    // Number of tiles currently in play 
    //
    int m_numTotalTiles = 0;
    //
    public int NumActiveTiles
    {
        get { return m_numTotalTiles; }
    }

    // Current player being evaluated by AI "thinking ahead"
    //
    GamePenguin.PenguinColor m_colorCurrEvalPlayer;
    //
    public GamePenguin.PenguinColor colorCurrEvalPlayer
    {
        get { return m_colorCurrEvalPlayer; }
        set { m_colorCurrEvalPlayer = value; }
    }
    
    // Current player whose turn it actually is (Read-only fetched from GameManager)
    //
    public GamePenguin.PenguinColor colorCurrentPlayer
    {
        get { return m_refGM.CurrentPlayer.Color;  }
    }


    // Current best move found by MiniMax
    //
    public string CurrentMiniMaxMove
    {
        get
        {
            return pendingMove.ToString();
        }
    }


    // Get list of active penguins a player has
    //
    List<VPenguin> GetPlayerPenguins(GamePenguin.PenguinColor player)
    {
        return tablePlayerPenguins[player];
    }


    void Start ()
    {
        m_refGM = GetComponent<GameManager>();
        m_refTM = GetComponent<TileManager>();

        // Initialize "valid tile moves" table
        //
        tableValidTileMoves.Add(GamePenguin.PenguinColor.Blue,  new List<string>());
        tableValidTileMoves.Add(GamePenguin.PenguinColor.Gold,  new List<string>());
        tableValidTileMoves.Add(GamePenguin.PenguinColor.Red ,  new List<string>());
        tableValidTileMoves.Add(GamePenguin.PenguinColor.Green, new List<string>());

        // Initialize "won tiles" table
        //
        tableWonTiles.Add(GamePenguin.PenguinColor.Blue,  new List<VTile>());
        tableWonTiles.Add(GamePenguin.PenguinColor.Gold,  new List<VTile>());
        tableWonTiles.Add(GamePenguin.PenguinColor.Red,   new List<VTile>());
        tableWonTiles.Add(GamePenguin.PenguinColor.Green, new List<VTile>());
    }


    /// <summary>
    /// Get sequence of players taking turns for AI move evaluation (Also works backwards)
    /// </summary>
    /// <param name="prevPlayer">Player that just finsihed moving or being evaluated</param>
    /// <param name="direction">Move forward (1) or backward (-1) one player?</param>
    /// <remarks>
    /// CALLED BY: NextEvalPlayer(), PrevEvalPlayer(), AIBrain::Minimax**()
    /// </remarks>
    /// <returns></returns>
    /// 
    public GamePenguin.PenguinColor GetPlayerIndex(GamePenguin.PenguinColor prevPlayer, int direction)
    {
        int newPlayerIndex = (int)prevPlayer;

        do
        {
            // Avoid underflow if going backwards
            //
            if (direction < 0 && newPlayerIndex == (int)GamePenguin.PenguinColor.Red) newPlayerIndex = (int)GamePenguin.PenguinColor._None;

            newPlayerIndex += direction;  // Increment/Decrement to next/previous player

            // Wrap around from end of list to start
            //
            if (direction > 0 && newPlayerIndex == (int)GamePenguin.PenguinColor._None) newPlayerIndex = (int)GamePenguin.PenguinColor.Red;

        } while (!tablePlayerPenguins.ContainsKey((GamePenguin.PenguinColor)newPlayerIndex));  // Skip index values for inactive players (Not in list)

        return ((GamePenguin.PenguinColor)newPlayerIndex);
    }


    // External Player rotation methods that use GetPlayerIndex() -- Used for AI evaluation
    //
    public GamePenguin.PenguinColor NextEvalPlayer()  // (Not to be confused with "NextPlayer" methods for the Game Board)
    {
        colorCurrEvalPlayer = GetPlayerIndex(colorCurrEvalPlayer, 1);

        return colorCurrEvalPlayer;
    }
    //
    public GamePenguin.PenguinColor PrevEvalPlayer()  
    {
        colorCurrEvalPlayer = GetPlayerIndex(colorCurrEvalPlayer, -1);

        return colorCurrEvalPlayer;
    }


    /// <summary>
    /// Accept move just made by human player and apply it to AI board
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    /// 
    public void UpdateAIBoard(GamePenguin.PenguinColor playerColor, Command.GameMove move)
    {
        Debug.Assert(playerColor == colorCurrentPlayer);   // Board should be on current player

        ApplyMove(move);   // Apply move to AI board ("move" object is polymorphic)
    }


    /// <summary>
    /// Get a heuristic score for the current position for a player, for Minimax evaluation
    /// </summary>
    /// <param name="currPlayer">Current Player being evaluated</param>
    /// <returns></returns>
    /// <remarks>
    /// This is the "Utility" method utilized by the MiniMax algorithm in AIBrain.  Relies on
    ///   formula for each tile returned by HeuristicFormula() --
    ///   This method may be a bit arbitrary, but it seems to work. -- Perhaps someone more astute in HTMFish strategy
    ///    can suggest improvements... Somehow, there just aren't that many books out there on HTMFish as there
    ///    are for chess... One TODO might be to figure out a way for the AI not only to go for the "most fishy"
    ///    tiles, but also to work to block opponents' penguins, since the best way to thwart the AI right now seems to be to 
    ///    block and isolate its penguins as much as possible.
    /// </remarks>
    /// 
    public float GetHeuristicScore()
    {
        float fHScore = 0.0f;

        UpdateHeuristicMap();   // Make sure the "Map" is updated

        List<VPenguin> penguinList = tablePlayerPenguins[colorCurrentPlayer];

        foreach (VPenguin pen in penguinList)   // Evaluate each of this player's penguins
        {
            string currTileID  = pen.currTile;

            fHScore += HeuristicFormula(currTileID);   // Add up heuritics for each penguin
        }

        return fHScore;
    }

    /// <summary>
    /// Function to use to get a heuristic score for each tile
    /// </summary>
    /// <remarks>
    /// Heuristic Score = Sum total of fish on all "neighboring" (can be reached in one move) tiles
    ///   * Fish on current tile / 2  (This causes the penguin to target 3-fish tiles first, but still be
    ///     "mindful" of areas of the board that are more fish-rich overall)
    /// </remarks>
    /// 
    public float HeuristicFormula(string tileID)
    {
        return TallyLegalTiles(tileID) * tileTable[tileID].numFish / 2.0f;
    }


    /// <summary>
    /// Get snapshot of board after the tiles are set up, before entering "Place Penguins" mode
    /// </summary>
    /// 
    public void FetchInitialBoard()
    {
        foreach (string tileID in m_refTM.tileTable.Keys)
        {
            GameTile gtile = m_refTM.tileTable[tileID];

            VTile tile = ScriptableObject.CreateInstance<VTile>();

            tile.InitFromGameTile(gtile);  // Copy properties from GameTile

            this.tileTable.Add(tileID, tile);
        }

        m_numTotalTiles = tileTable.Count;

        Debug.Log(this.ToString());
    }


    /// <summary>
    /// Get a list of all tiles that can be legally moved to from input tile (Using GameTile Methods in TileManager)
    ///   and tally the total number of fish on all tiles in the list
    /// </summary>
    /// <returns>Total number of fish on accessible tiles</returns>
    /// <param name="sIDTileOrigin">ID of starting tile, e.g. "e4"</param>
    /// <param name="workTable">List to use to store valid tile IDs</param>
    /// <remarks>
    /// CALLED BY:
    ///   GetHeuristicScore() -- To count up the total number of fish on all tiles accessible in one move (to compute Heuristic score for a penguin's position)
    ///   AIBrain::GetValidMoves() -- To get all legal moves for a penguin -- Each move is then evaluated by the AI via TallyLegalTiles()
    ///   
    /// In other words, this is called by AIBrain::GetValidMoves() for the tile a penguin is currently standing on, then
    ///   is called by GetHeuristicScore() for each tile the penguin can move to, to compute
    ///   which tile would be the most advantagous for the penguin to move to, and cumulatively, what is the best move for
    ///   ALL of a player's penguins.
    /// </remarks>
    /// 
    public int FetchLegalTiles(string sIDTileOrigin, List<string> workTable)
    {
        int totalNumFish = 0;

        workTable.Clear();

        GameTile startTile = m_refTM.tileTable[sIDTileOrigin];   // Get starting tile from ID

        // Traverse all legal paths and place tiles in list
        //
        foreach (ITileTraverse tile in startTile.AllPaths(m_refTM))
        {
            workTable.Add(tile.tileID);

            totalNumFish += tile.numFish;
        }

        return totalNumFish;  // Return total number of fish on tiles -- List of tiles themselves stored in workTable
    }
    /// 
    public int FetchLegalTiles(string sIDTileOrigin, List<VTile> workTable)
    {
        int totalNumFish = 0;

        workTable.Clear();

        GameTile startTile = m_refTM.tileTable[sIDTileOrigin];   // Get starting tile from ID

        // Traverse all legal paths and place tiles in list
        //
        foreach (ITileTraverse tile in startTile.AllPaths(m_refTM))
        {
            workTable.Add(tileTable[tile.tileID]);   // Get VTile that corresponds to GameTile

            totalNumFish += tile.numFish;
        }

        return totalNumFish;  // Return total number of fish on tiles -- List of tiles themselves stored in workTable
    }
    ///
    /// (Just get the total number of fish -- Don't need to store the tiles themselves)
    ///
    public int TallyLegalTiles(string sIDTileOrigin)
    {
        int totalNumFish = 0;

        GameTile startTile = m_refTM.tileTable[sIDTileOrigin];   // Get starting tile from ID

        // Traverse all legal paths and place tiles in list
        //
        foreach (ITileTraverse tile in startTile.AllPaths(m_refTM))
        {
            totalNumFish += tile.numFish;
        }

        return totalNumFish;  // Return total number of fish on tiles -- List of tiles themselves stored in workTable
    }


    /// <summary>
    /// Go through all the tiles on the board and assign their current heuristic
    ///   value based on total number of fish on it and on all adjacent tiles
    /// </summary>
    /// <param name="tiles">List of board's tiles</param>
    /// 
    public void UpdateHeuristicMap()
    {
        foreach (string tid in tileTable.Keys)
        {
            if (tileTable[tid].IsEmpty) tileTable[tid].HeuristicScore = -1;  // AI routines will deduct score from tiles near a sunk tile
            //
            // Heuristic Score = Sum total of fish on all "neighboring" (can be reached in one move) tiles
            //   * Fish on current tile / 2  (This causes the penguin to target 3-fish tiles first, but still be
            //     "mindful" of areas of the board that are more fish-rich overall)
            //
            else tileTable[tid].HeuristicScore = HeuristicFormula(tid);
        }
    }


    /// <summary>
    /// Sort a list of VTiles by descending (highest first) Heuristic score
    /// </summary>
    /// <param name="sortedTiles"></param>
    /// 
    public static void SortVTiles(List<VTile> sortedTiles)
    {
        sortedTiles.Sort(delegate (VTile tile1, VTile tile2) { return -tile1.CompareTo(tile2); });
    }


    /// <summary>
    /// Sort a list of Move objects by descending (highest first) Heuristic score
    /// </summary>
    /// 
    public static void SortMoves(List<Command.GameMove> candidateMoves)
    {
        candidateMoves.Sort(delegate (Command.GameMove m1, Command.GameMove m2) { return -m1.CompareTo(m2); });
    }


    /// <summary>
    /// Sort list of candidate moves by heuristic values, extracted from Heuristic map of board
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// "Sunk" tiles are ignored.
    /// </remarks>
    /// 
    public void SortMovesHeuristic(List<Command.GameMove> candidateMoves)
    {
        UpdateHeuristicMap();   // Ensure heuristic board map is up-to-date

        // Get heuristic tiles values from board for each candidate move
        //
        foreach (Command.GameMove move in candidateMoves)
        {
            move.HeuristicScore = tileTable[move.toTile].HeuristicScore;
        }

        // Sort by descending score
        //
        SortMoves(candidateMoves);
    }

   
    /// <summary>
    /// Print current board layout (for debugging)
    /// </summary>
    /// 
    public override string ToString()
    {
        int size = GameManager.GetGameManager().dimension;

        string id, sRow, str;
        string idr = "87654321";
        string idc = "abcdefgh";

        str = "";
        
        for(int row = 0; row < size; row++)
        {
            sRow = idr[row].ToString() + ": |";

            for(int col = 0; col < size - row % 2; col++)
            {
                id = string.Format("{0}{1}", idc[col], idr[row]);

                VTile tile = tileTable[id];

                sRow += tile.tileDesc + "|";
            }

            str += sRow + '\n';
        }

        str += "-----------------------\n";
        str += "_:_|a|b|c|d|e|f|g|h|\n";
        str += colorCurrentPlayer.ToString() + " to move.\n";

        return str;
    }


    #region AI Board Operations

    /*** Basic operations on internal board ***/

    /// <summary>
    /// Update board from effects of moving penguin
    /// </summary>
    /// <param name="pen"></param>
    /// <param name="toTile"></param>
    /// 
    public void MovePenguin(VPenguin pen, string toTileID)
    {
        VTile fromTile = tileTable[pen.currTile];
        VTile destTile = tileTable[toTileID];

        pen.lastTile  = pen.currTile;
        pen.currTile  = toTileID;
        pen.numFish  += fromTile.numFish;

        fromTile.IsPenguinHere = false;

        SetTile(fromTile, false);

        destTile.IsPenguinHere = true;
    }

    /// <summary>
    /// Undo moving penguin
    /// </summary>
    /// <param name="pen"></param>
    /// <param name="toTile"></param>
    /// 
    public void UnmovePenguin(VPenguin pen, string priorTileID)
    {
        // Prepare to backtrack to tile before this move (from "to-tile" back to "from-tile")
        //
        VTile fromTile = tileTable[pen.lastTile];   
        VTile   toTile = tileTable[pen.currTile];   

        toTile.IsPenguinHere = false;

        fromTile.IsPenguinHere = true;

        SetTile(fromTile, true);

        pen.currTile  = pen.lastTile;
        pen.lastTile  = priorTileID;
        pen.numFish  -= fromTile.numFish;
    }

    public void AddPenguin(string tileID, GamePenguin.PenguinColor color)
    {
        List<VPenguin> penguinList;

        VTile tile = tileTable[tileID];

        tile.IsPenguinHere = true;

        VPenguin newPenguin = ScriptableObject.CreateInstance<VPenguin>();

        newPenguin.currTile = tileID;
        newPenguin.lastTile = tileID;
        newPenguin.numFish  = 0;
        newPenguin.isActive = true;

        // Check that color already exists in table, if not, initialize it
        //
        if (!tablePlayerPenguins.ContainsKey(color))
        {
            penguinList = new List<VPenguin>();

            tablePlayerPenguins.Add(color, penguinList);
        }
        else penguinList = tablePlayerPenguins[color];

        // Add new penguin to list for player
        //
        penguinList.Add(newPenguin);
    }

    /// <summary>
    /// Remove penguin from board (and tile it was standing on)
    /// </summary>
    /// <param name="tileID">Tile penguin is on</param>
    /// <param name="color">Penguin color</param>
    /// 
    public void RemovePenguin(VPenguin penguin)
    {
        SetPenguin(penguin, false);
    }

    /// <summary>
    /// Undo a penguin removal
    /// </summary>
    /// <param name="tileID">Tile penguin is on</param>
    /// <param name="color">Penguin color</param>
    /// 
    public void RestorePenguin(VPenguin penguin)
    {
        SetPenguin(penguin, true);
    }

    /// <summary>
    /// Set a penguin's status
    /// </summary>
    /// <param name="penguin"></param>
    /// 
    void SetPenguin(VPenguin penguin, bool bActive)
    {
        Debug.Assert(penguin);

        penguin.isActive = bActive;

        SetTile(penguin.currTile, bActive);
    }

    /// <summary>
    /// Set status for a single tile (Invoked by RemovePenguin() and SetTileStatus())
    /// </summary>
    /// <param name="bMakeActive">Active or Inactive ("Sunk")?</param>
    /// <param name="tileID">TileID</param>
    /// 
    void SetTile(string tileID, bool bMakeActive)
    {
        SetTile(tileTable[tileID], bMakeActive);
    }
    //
    void SetTile(VTile tile, bool bMakeActive)
    {
        tile.IsActive = bMakeActive;

        // Make sure total tile tally is up to date
        //
        if (bMakeActive) m_numTotalTiles++;   // Unsunk tile
        //
        else m_numTotalTiles--;   // Sunk tile
    }

    /// <summary>
    /// Take status of a list of GameTiles and copy it to their AIBoard counterparts
    /// (Used primarily when tiles "sink")
    /// </summary>
    /// <param name="listGTiles">List of tiles set status for</param>
    /// <param name="bMakeActive">Make tiles active or inactive (Inactive when tiles "sink")</param>
    /// <returns>Total score (fish) value for tile collection</returns>
    /// 
    public int SetTileStatus(List<GameTile> listGTiles, bool bMakeActive)
    {
        int fishValue = 0;

        foreach(GameTile tile in listGTiles)
        {
            string id = tile.tileID;

            SetTile(id, bMakeActive);  // Set status for VTile

            fishValue += tile.FishScoreValue;   // Add up fish
        }

        return fishValue;
    }


    /*** Operations on move stack ***/

    public void PushMove(Command.GameMove testMove)
    {
        testMove.PlayMove(this);    // Apply move to internal board

        testMoveStack.Push(testMove);   // Save move to the stack

        NextEvalPlayer();   // Advance to next player / ply
    }


    public Command.GameMove PopMove()
    {
        Command.GameMove lastMove = testMoveStack.Pop();   // Retrieve last move

        lastMove.HeuristicScore = GetHeuristicScore();

        lastMove.RollBack(this);    // Undo move on internal board

        PrevEvalPlayer();   // Backup one ply

        return lastMove;
    }


    // After a move the user has just made
    //
    public void ApplyMove(Command.GameMove move)
    {
        move.PlayMove(this);    // Apply move to internal board (Already done in Game World)
    }


    // Play a move the AI chose "for real" -- First apply to AIBoard, then send to "real" board
    //
    public void ApplyPendingMove()
    {
        if(pendingMove == null)   // TODO: If no legal moves, remove penguin
        {
            Debug.Log("*** No move found for " + colorCurrentPlayer.ToString());
        }
        else
        {
            pendingMove.PlayMove(this);    // Apply move to internal board 

            SendMessage("OnAIMove_Apply", pendingMove);   // Apply move to Game World (Receiver: GameManager::OnAIMove()) 
        }
    }

    #endregion

    #region For Possible Future Use

    /// <summary>
    /// Find common tiles between two groups, i.e. intersection points in tiles paths
    /// </summary>
    /// <param name="group1"></param>
    /// <param name="group2"></param>
    /// <returns></returns>
    /// <remarks>Not currently used in AI calculations but may be in future</remarks>
    /// 
    public List<VTile> GetIntersectons(List<VTile> group1, List<VTile> group2)
    {
        List<VTile> intersection = new List<VTile>();

        foreach (VTile tile in group1)
        {
            if (group2.Contains(tile)) intersection.Add(tile);
        }

        return intersection;
    }


    /// <summary>
    /// Sort list of all tiles by heuristic values, using Heuristic map of board, and return it
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// "Sunk" tiles are ignored.
    /// </remarks>
    /// 
    public List<VTile> SortAllTilesHeuristic()
    {
        // Ensure heuristic board map is up-to-date
        //
        UpdateHeuristicMap();

        // Copy board map to working list
        //
        List<VTile> sortedTiles = new List<VTile>(tileTable.Values);

        // Sort by descending score
        //
        SortVTiles(sortedTiles);

        // Return sorted list
        //
        return sortedTiles;
    }

    #endregion
}
