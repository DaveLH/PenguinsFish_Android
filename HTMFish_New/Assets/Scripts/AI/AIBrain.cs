using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;

/// <summary>
/// Mechanism for AI to choose its moves
/// </summary>
/// <remarks>
/// DISCLAIMER: For Penguin use only.  Not suitable for finding the 
///     Ultimate Question of Life, the Univere and Everything
///     or which ceiling fans are going to fall.
/// </remarks>
/// 
public class AIBrain : MonoBehaviour
{
    public enum AIMode { RandomFishGrab, ImmediateHeuristic, PenguinStar, MiniMax };

    GameManager  m_refGM;
    AIBoardSim   m_refBoard;    // Reference to the current board state

    // AIProgressDlg m_dlg;   // Dialog box to display progress as AI runs on each move

    GamePenguin.PenguinColor m_me = GamePenguin.PenguinColor._None;   // Which color am I?

    [SerializeField] protected int  _miniMaxThreshhold = 25;     // Start invoking MiniMax when number of moves to evaluate becomes < this value
    [SerializeField] protected int  _difficultyLevel   = 1;      // How many moves to look ahead (for MiniMax)?
    [SerializeField] protected bool _debugMode         = false;  // Print a log of MiniMax execution and board states, and don't execute AI in a separate thread

    [SerializeField] AIMode _AIMethod = AIMode.MiniMax;

    int m_depthInPlys;   // How many total iterations we need to do

    Thread m_threadMM;   // Separate thread to use to run MiniMax algortithm (otherwise program would freeze while waiting for execution to finish)

    // Workspace to fetch legal tiles for next move
    //
    List<string> m_sourceTileList = new List<string>();


    // Get/set what color the AI is playing
    //
    public GamePenguin.PenguinColor AIColor
    {
        get { return m_me; }
        set { m_me = value; }
    }

    // Get type of AI to use
    //
    public AIMode AIMethodID
    {
        get { return _AIMethod; }
    }

    // Debug mode?
    //
    public bool IsDebugging
    {
        get { return _debugMode; }
    }

    // Is AI "thinking"?
    //
    public bool IsThinking
    {
        get; set; 
    }

    // Is MiniMax being executed?
    //
    public bool IsMiniMaxRunning
    {
        get { return m_threadMM.IsAlive; }
    }

    
    /// <summary>
    /// Clear move list and store chosen move to return to GameManager
    /// </summary>
    /// 
    void StoreMove(Command.GameMove foundMove, List<Command.GameMove> refMoveList)
    {
        Debug.Assert(foundMove != null);

        refMoveList.Clear();
        refMoveList.Add(foundMove);

        IsThinking = false;
    }


    /// <summary>
    /// Go through set of tiles and count the total number of 
    ///   available fish -- This is used to assign a heuristic "weight" to a tile
    ///   at the center of the tiles in the list
    /// </summary>
    /// <param name="tiles">List of related tiles</param>
    /// 
    public int TallyHeuristicScore(List<VTile> tiles)
    {
        int numFish = 0;

        foreach (VTile t in tiles)
        {
            numFish += t.numFish;
        }

        return numFish;
    }

    
    /// <summary>
    /// AI Method 1 (and simplest) -- Go "grabbing" three-fish tiles (if available)
    /// </summary>
    /// 
    public void OnAI_FishGrab(List<Command.GameMove> allPossibleMoves)
    {
        Debug.Log("In PlayerAI...");

        Command.GameMove move;

        List<Command.GameMove> list3FishMoves = new List<Command.GameMove>();
        List<Command.GameMove> list2FishMoves = new List<Command.GameMove>();

        Debug.Assert(AIColor != GamePenguin.PenguinColor._None);

        // Get all of current player's legal moves (Placed in possibleMoves)
        //
        // List<Command.GameMove> allPossibleMoves = BuildMoveList(m_refBoard.colorCurrentPlayer);

        // Sort moves
        //
        foreach (Command.GameMove gmove in allPossibleMoves)
        {
            int numFish = m_refBoard.tileTable[gmove.toTile].numFish;

                 if (numFish == 3) list3FishMoves.Add(gmove);
            else if (numFish == 2) list2FishMoves.Add(gmove);
        }

        // Get random move, maximizing fish number on destination tile
        //
        if (list3FishMoves.Count > 0)
        {
            move = list3FishMoves[Random.Range(0, list3FishMoves.Count)];
        }
        else if (list2FishMoves.Count > 0)
        {
            move = list2FishMoves[Random.Range(0, list2FishMoves.Count)];
        }
        else move = allPossibleMoves[Random.Range(0, allPossibleMoves.Count)];

        StoreMove(move, allPossibleMoves);
    }


    /// <summary>
    /// AI Method 2 -- Give a heuristc "weight" to every tile that can reached next
    ///   move and go there.
    /// </summary>
    /// <remarks>
    /// "Weight" of tile = Fish on tile + Sum of fish on all tiles accessible on next move
    ///    (Because we care about the penguins' mobility, a "sunk" tile = -1, so if a three-fish tile
    ///      is linked to only one other tile, the score with come out negative, and the tile will be avoided)
    /// </remarks>
    /// 
    public void OnAI_HeuristicNextMove(List<Command.GameMove> allPossibleMoves)
    {
        // Get a list of all the candidate moves sorted by heuristic "weight"
        //
        m_refBoard.SortMovesHeuristic(allPossibleMoves);

        // Make highest scored move the pending move
        //
        Command.GameMove move = allPossibleMoves[0];
        //
        StoreMove(move, allPossibleMoves);
    }


    /// <summary>
    /// AI Method 3 -- Use MiniMax with Alpha-Beta Pruning
    /// </summary>
    /// <remarks>
    /// "Weight" of tile = Fish on tile + Sum of fish on all tiles accessible on next move
    ///    (Because we care about the penguins' mobility, a "sunk" tile = -1, so if a three-fish tile
    ///      is linked to only one other tile, the score with come out negative, and the tile will be avoided)
    /// </remarks>
    /// 
    public void OnAI_MiniMax(List<Command.GameMove> allPossibleMoves)
    {
        // Use quick method when total possible moves is prohibitively large (More tiles = much slower MiniMax)
        //  or with three or more players (Since I haven't figured out how to make MiniMax work properly with
        //  more than two players,)
        //
        if (m_refGM.NumPlayers > 2 || m_refBoard.NumActiveTiles > _miniMaxThreshhold) OnAI_HeuristicNextMove(allPossibleMoves);
        //
        else 
        {
            m_threadMM = new Thread(new ParameterizedThreadStart(MiniMaxThread));

            m_threadMM.Start(allPossibleMoves);
        }
    }


    void MiniMaxThread(object ob)
    {
        List<Command.GameMove> allPossibleMoves = (List<Command.GameMove>)ob;

        try
        {
            m_depthInPlys = m_refGM.NumPlayers * _difficultyLevel;   // How many total iterations we need to do?

            // Clear previous move
            //
            m_refBoard.pendingMove = null;

            // "But the program will take me a little while to run..."
            //
            MinimaxAB(m_depthInPlys, m_me, allPossibleMoves, Mathf.NegativeInfinity, Mathf.Infinity);

            // If no move found, revert to immediate heuristics
            //
            if(m_refBoard.pendingMove == null) OnAI_HeuristicNextMove(allPossibleMoves);

            // Hand back move in original moveList
            //
            StoreMove(m_refBoard.pendingMove, allPossibleMoves);
        }
        catch (ThreadAbortException)
        {
            // If MiniMax is aborted due to it's running too long, choose a move by one-move heuristic method instead...
            //
            OnAI_HeuristicNextMove(allPossibleMoves);
        }
    }


    /// <summary>
    /// MiniMax with alpha-beta pruning
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="currPlayer"></param>
    /// <returns></returns>
    /// 
    public float MinimaxAB(int depth, GamePenguin.PenguinColor currPlayer, List<Command.GameMove> allPossibleMoves, float alpha, float beta)
    {
        float bestHeuristicScore, currHeuristicScore = 0.0f;

        Command.GameMove lastMove;

        if (depth == 0)  
        {
            return m_refBoard.GetHeuristicScore();   // This is a "leaf node" -- Return its heuristic value
        }

        // Find all valid possible moves (Initial call hands current allPossibleMoves for AI, so does
        //  not to be found again on first iteration)
        //
        if(allPossibleMoves == null) allPossibleMoves = BuildMoveList(currPlayer);

        // Evaluate list of moves
        //
        if (currPlayer == m_me)   // AI's Turn (Maximizing Player)
        {
            bestHeuristicScore = Mathf.NegativeInfinity;

            foreach (Command.GameMove testMove in allPossibleMoves)
            {
                m_refBoard.PushMove(testMove);  // "Try out" this move on AI Board

                // Get to next node
                //
                currHeuristicScore = MinimaxAB(depth - 1, m_refBoard.GetPlayerIndex(currPlayer, 1), null, alpha, beta);

                // Print info only at uppermost level...
                //
                if (depth == m_depthInPlys) MiniMaxDebugInfo(testMove, currHeuristicScore);

                // Undo move when test on this branch is done
                //
                lastMove = m_refBoard.PopMove();

                // Test heuristic result of this position
                //
                if (currHeuristicScore > alpha)
                {
                    // Print info only at uppermost level...
                    //
                    if (depth == m_depthInPlys)
                    {
                        if (_debugMode) Debug.Log("New best move...");

                        MiniMaxDebugInfo(lastMove, currHeuristicScore);

                        m_refBoard.pendingMove = lastMove;
                    }

                    alpha = bestHeuristicScore = currHeuristicScore;
                }

                if (alpha >= beta) break;   // Prune the tree
            }

            return bestHeuristicScore;
        }
        else   // Human player's Turn (Minimizing Players)
        {
            bestHeuristicScore = Mathf.Infinity;

            foreach (Command.GameMove testMove in allPossibleMoves)
            {
                m_refBoard.PushMove(testMove);

                currHeuristicScore = MinimaxAB(depth - 1, m_refBoard.GetPlayerIndex(currPlayer, 1), null, alpha, beta);

                // MiniMaxDebugInfo(testMove, currHeuristicScore);

                m_refBoard.PopMove();

                if (currHeuristicScore < beta)
                {
                    beta = bestHeuristicScore = currHeuristicScore;
                }

                if (alpha >= beta) break;   // Prune the tree
            }

            return bestHeuristicScore;
        }
    }


    /// <summary>
    /// Build a list of moves from list a valid tiles from AIBoard
    /// </summary>
    /// <param name="player"></param>
    /// 
    List<Command.GameMove> GetValidMoves(GamePenguin.PenguinColor player, VPenguin penguin)
    {
        // List of possible moves (Must be unique one for every node in MiniMax tree!)
        //
        List<Command.GameMove> possibleMoves = new List<Command.GameMove>();

        // Get temporary list of tiles current penguin can move to
        //
        m_sourceTileList.Clear();
        //
        m_refBoard.FetchLegalTiles(penguin.currTile, m_sourceTileList);

        possibleMoves.Clear();   // Ensure we have a clean slate

        // Build move object for every legal tile
        //
        foreach (string tileID in m_sourceTileList)
        {
            Command.MovePenguin move = new Command.MovePenguin(penguin, player, tileID, penguin.lastTile);

            possibleMoves.Add(move);
        }

        return possibleMoves;
    }


    /// <summary>
    /// Build a list of all possible moves for current player
    /// </summary>
    /// <param name="currPlayer"></param>
    /// <returns></returns>
    /// 
    public List<Command.GameMove> BuildMoveList(GamePenguin.PenguinColor currPlayer)
    {
        List<Command.GameMove> allPossibleMoves = new List<Command.GameMove>();

        List<VPenguin> penguins = m_refBoard.tablePlayerPenguins[currPlayer];

        // Accumulate all moves this player can make with any of their penguins
        //
        foreach (VPenguin pen in penguins)
        {
            // Get all of current player's legal moves (Placed in possibleMoves)
            //
            List<Command.GameMove> thisPensPossibleMoves = GetValidMoves(currPlayer, pen);

            allPossibleMoves.AddRange(thisPensPossibleMoves);
        }

        return allPossibleMoves;
    }



    void MiniMaxDebugInfo(Command.GameMove move, float score)
    {
        if (_debugMode)
        {
            Debug.Log("Checking move: " + move.ToString() + "; HScore = " + score);
        }
    }


    void Start ()
    {
        m_refGM    = GetComponent<GameManager>();
        m_refBoard = GetComponent<AIBoardSim>();

        // m_dlg = m_refGM.AIProgress;

        IsThinking = false;

        // m_me = GamePenguin.PenguinColor._None;
	}


    /// <summary>
    /// Give ourselves an "escape hatch" if the AI is taking too long...
    /// </summary>
    /// 
    void Update()
    {
        if (!Input.anyKey) return;   // Ignore if no input

        if (!IsThinking || m_threadMM == null) return;   // No effect unless MiniMax is active

        if(Input.GetKey(KeyCode.Escape))
        {
            if(m_threadMM.IsAlive)   // Break MiniMax execution and pick a move the "easy" way
            {
                m_threadMM.Abort();   // Throws an exception to halt MiniMax
            }
        }   
    }
}
