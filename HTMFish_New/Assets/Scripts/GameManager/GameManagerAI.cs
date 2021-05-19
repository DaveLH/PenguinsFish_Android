using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Section of GameManager related to AI
/// </summary>
/// 
public partial class GameManager : MonoBehaviour
{
    // public GameObject PenguinHsPrefab;
    // public GameObject PenguinAIPrefab;

    public ParticleSystem penguinGoPrefab;

    bool m_bUsingAI = false;

    string m_sAIMethodName;    // Method to invoke when AI needs to move

    /*
    bool m_bMainGamePlay   = false;   // Is setup complete and we've entered main game play?
    bool m_bMoveInProgress = false;   // Wait for this to become false before starting next move
    */
    protected AIBrain m_brain;   // The "brains" of the outfit

    // Available AI methods (Actual methods in AIBrain, Invoked via SendMessage("On"+<method name>)
    //
    [SerializeField] protected string FishGrabMethod;
    [SerializeField] protected string BestNextMoveMethod;
    [SerializeField] protected string PathFindMethod;
    [SerializeField] protected string MiniMaxMethod;

    // Is the computer playing in this game? (If not, we don't need to invoke usage of AI classes/methods)
    //
    public bool IsUsingAI
    {
        get { return m_bUsingAI; }
        set { m_bUsingAI = value; }
    }

    // Is AI currently "thinking" in a separate thread?
    //
    public bool IsAIThreadActive
    {
        get { return m_brain.IsMiniMaxRunning; }
    }

    // Get method for AI to "think" with (Set once in Start())
    //
    public string AIMethodName
    {
        get { return m_sAIMethodName; }
    }

    // Set method for AI to "think" with (Set in State machine at start of Game)
    //
    public void SetAIMethodName()
    {
        m_sAIMethodName = "OnAI_";   // Identifying prefix

        switch (m_brain.AIMethodID)
        {
            case AIBrain.AIMode.RandomFishGrab:
                m_sAIMethodName += FishGrabMethod;
                break;

            case AIBrain.AIMode.MiniMax:
                m_sAIMethodName += MiniMaxMethod;
                break;

            case AIBrain.AIMode.ImmediateHeuristic:
                m_sAIMethodName += BestNextMoveMethod;
                break;

            case AIBrain.AIMode.PenguinStar:
                m_sAIMethodName += PathFindMethod;   // Default -- Use for final release version
                break;
        } 
        
        
    }


    /// <summary>
    /// Does this player have a penguin on the designated tile?
    /// </summary>
    /// <param name="color">Player's color</param>
    /// <param name="tile">Tile we're checking for a penguin</param>
    /// <returns>Return the penguin if it's there</returns>
    /// 
    public VPenguin GetPenguinOn(GamePenguin.PenguinColor color, GameTile tile)
    {
        List<VPenguin> penguins = m_refAIBoard.tablePlayerPenguins[color];

        foreach (VPenguin pen in penguins)
        {
            if (pen.currTile == tile.tileID)
            {
                return pen;
            }
        }

        return null;   // No penguin of correct color found
    }



    /// <summary>
    /// Called when it's the AI's move
    /// </summary>
    /// <remarks>INVOKED BY: State_Move_AI::OnStateEnter()
    /// This method calls whichever AI computing method has been chosen in the inspector
    /// (See AIMethodName and AIBrain::_AIMethod)
    /// </remarks>
    /// 
    public void OnAIMove()
    {
        List <Command.GameMove> validMoves = m_brain.BuildMoveList(CurrentPlayer.Color);

        // If no legal moves, AI removes itself from game play
        //
        if (validMoves.Count == 0)
        {
            AIResign();

            return;
        }
        else if (RemoveAIPenguin()) return;   // Remove a lone penguin and claim its tiles
        // 
        else   // Find a move and play it
        {
            m_brain.IsThinking = true;   // This flag is turned off by AIBrain::StoreMove() at end of computation

            // Send valid moves to AIBrain for evaluation -- 
            //   Chosen move is returned as first element in validMoves (Other moves are discarded)
            //
            SendMessage(AIMethodName, validMoves);

            // Wait for AI to finish, then fetch move found (stored in validMoves[0]) and execute it
            //
            StartCoroutine(WaitExecuteAIMove(validMoves));
        }
    }


    /// <summary>
    /// Wait for AI to complete (Might be in a separate thread), then execute move
    /// </summary>
    /// <param name="moveRef"></param>
    /// <returns></returns>
    /// 
    IEnumerator WaitExecuteAIMove(List<Command.GameMove> moveRef)
    {
        yield return new WaitWhile(() => m_brain.IsThinking);

        // Verify a move was found
        //
        Debug.Assert(moveRef.Count == 1);

        // Update board and move penguin in Game World -- Handle winning fish -- Drop departed tile
        //
        ExecuteAIMove(moveRef[0]);

        Debug.Log("Remaining Tiles: " + m_refAIBoard.NumActiveTiles);   // This needs to be accurate
    }


    /// <summary>
    /// AI removes itself from game, and returns control to human players
    /// </summary>
    /// <remarks>
    /// Penguins currently not removed from board, but AI's final score will be computed as though they were
    /// </remarks>
    /// 
    void AIResign()
    {
        // Go to "Move Pending" (may later include animation), then Resign, then Next Player
        //
        StartAnimateMove();
        Resign();
        StateTrigger_NextPlayer();
    }

    /// <summary>
    /// Apply AI's move to the Game Board
    /// </summary>
    /// <param name="move">AI move just applied to internal board</param>
    /// 
    public void OnAIMove_Apply(Command.GameMove move)
    {

    }

    /// <summary>
    /// Check each of the AI's penguins, and see if any can be removed with a bunch of tiles
    /// </summary>
    /// 
    bool RemoveAIPenguin()
    {
        foreach(GamePenguin penguin in CurrentPlayer.GetPenguins())
        {
            int result = RemovePenguin(CurrentPlayer, penguin);

            if(result > 0) return true;   // To be fair, let AI only remove one penguin per turn
        }

        return false;
    }


    /*** Factory methods to create AIBoard commands ***/

    /// <summary>
    /// Pack current Penguin and current tile into a move penguin command to send to AI board,
    /// so it stays "in sync" with Game Board
    /// </summary>
    /// <returns></returns>
    /// 
    public Command.GameMove PortmanteauMovePenguin(GameTile origTile, GameTile destTile)
    {
        VPenguin pen = GetPenguinOn(CurrentPlayer.Color, origTile);  // Verify player has a penguin is on this tile that can move

        if (pen == null) return null;
        //
        else
        {
            // Create move object
            //
            Command.GameMove move = new Command.MovePenguin(pen, CurrentPlayer.Color, destTile.tileID, origTile.tileID);

            return move;
        }   
    }


    /// <summary>
    /// Pack current Penguin and current tile into a REmove penguin command to send to AI board,
    /// so it stays "in sync" with Game Board
    /// </summary>
    /// <returns></returns>
    /// 
    public Command.GameMove PortmanteauRemovePenguin(GameTile penguinTile)
    {
        VPenguin pen = GetPenguinOn(CurrentPlayer.Color, penguinTile);  // Verify player has a penguin is on this tile that can move

        if (pen == null) return null;
        //
        else
        {
            // Create remove penguin command
            //
            Command.GameMove move = new Command.RemovePenguin(pen, CurrentPlayer.Color);

            return move;
        }
    }


    /// <summary>
    /// Let AI place its penguin on an optimal tile
    /// </summary>
    /// 
    public void PlacePenguinAI(Player current)
    {
        const float MinDist = 3.0f;

        m_refAIBoard.UpdateHeuristicMap();

        List<VTile> allTiles = new List<VTile>(m_refAIBoard.tileTable.Values);

        AIBoardSim.SortVTiles(allTiles);

        int length = allTiles.Count;

        VTile vtile = null;

        GameTile tile = null;

        GamePenguin lastPen = current.lastAddedPenguin;

        // Scan highest scored tiles for first available one-fish tile
        //
        for (int index = 0; index < length; index++)
        {
            vtile = allTiles[index];

            tile = m_refTileMgr.tileTable[vtile.tileID];

            if (lastPen != null)
            {
                // Try not to place AI's penguins too close together, or it will be too easy for the 
                //  other players to "gang up" on them.
                //
                if (TileManager.Distance(tile, lastPen.CurrentTile) < MinDist) continue;
            }

            if (tile.numFish == 1 && !tile.IsPenguinHere) break;   // Appropriate tile found -- Loop ends
        }
        
        // Place penguin on that tile   
        //
        current.AddPenguin(this, tile, true);
    }


    /// <summary>
    /// Let AI place its penguin on a random (but still one-fish) tile
    /// </summary>
    /// 
    public void PlacePenguinAIRnd(Player current)
    {
        const float MinDist = 3.0f;

        Dictionary<string, GameTile>.ValueCollection allTiles = m_refTileMgr.tileTable.Values;

        int length = allTiles.Count;

        GameTile [] arrTiles = new GameTile[length];

        allTiles.CopyTo(arrTiles, 0);
        
        GameTile tile = null, tempTile;

        GamePenguin lastPen = current.lastAddedPenguin;

        do   // Find a random available one-fish tile with highest heuristic value
        {
            int index = Random.Range(0, length);

            tempTile = arrTiles[index];

            if(lastPen != null)
            {
                // Don't place AI's penguins too close together, or it will be too easy for the 
                //  other players to "gang up" on them.
                //
                if (TileManager.Distance(tempTile, lastPen.CurrentTile) < MinDist) continue;
            }

             if (tempTile.numFish == 1 && 
                !tempTile.IsPenguinHere)
                //
                tile = arrTiles[index];

        } while (tile == null);

        // Place penguin on that tile (TODO: Special prefab for AI)   
        //
        current.AddPenguin(this, tile, true);
    }

    /*
    /// <summary>
    /// Check if any moves are executing
    /// </summary>
    /// <returns>FALSE if prior move completed</returns>
    /// 
    public bool CheckMoveInProgress()
    {
        Player player = CurrentPlayer;

        if (player.IsThinking) return true;
        //
        else if (m_currentPenguin)
        {
            // Penguin moving to new tile in scene
            //
            if (m_currentPenguin.IsMoving) return true;
        }

        return false;
    }
    */

    /// <summary>
    /// Apply move found by AI algorithmns
    /// </summary>
    /// <remarks>INVOKED BY: State_Move_AI::OnStateUpdate() (When Player::IsThinking flag falls)</remarks>
    /// <param name="move"></param>
    /* 
    public void ApplyAIMove()
    {
        // Now move is found, apply it to internal board 
        //  (This will then invoke OnExeAIMove(), which will animate the move on screen)
        //
        m_refAIBoard.ApplyPendingMove();   
    }
    */

    
    /* ???
     * 
      bool WinFinalTiles(Player player, GamePenguin penguin)
      {
          List<GameTile> tileList = new List<GameTile>();

          int fishIsland = TileManager.CheckForIsland(m_refTileMgr, penguin.CurrentTile, tileList);

          if (fishIsland > 0)
          {
              player.AddFish(fishIsland);

              ExpungePenguin(penguin, penguinGoPrefab);

              m_refTileMgr.ExpungeTiles(tileList);  // Remove tiles from game

              return true;
          }

          return false;
      }


      /// <summary>
      /// End current move -- Start the next turn
      /// </summary>
      /// 
      public void EndMove()
      {
          NextPlayer();

          m_currentPenguin = m_penguins[m_currentPlayerIndex];
          Player player = m_players[m_currentPlayerIndex];

          m_refScoreGUI.SetTurnDisplay(player.TurnMessage);
          m_refScoreGUI.SetScoreDisplay(player.Color, player.Score);

          player.MakeMove();   // Wait for player's (human or AI) next move
      }


      /// <summary>
      /// Stop game play and display result
      /// </summary>
      /// 
      public void EndGame()
      {
          m_bMainGamePlay = false;

          Player winningPlayer = null, testPlayer;

          GamePenguin penguin;

          string sWinMsg;


          for (int n = 0; n < numPlayers; n++)
          {
              testPlayer = m_players[n];

              penguin = m_penguins[n];

              // "Fast forward" to the end by collecting any remaining available tiles that only one player can still collect
              //
              WinFinalTiles(testPlayer, penguin);

              m_refScoreGUI.SetScoreDisplay(testPlayer.Color, testPlayer.Score);

              // Determine winner
              //
              if (winningPlayer == null || testPlayer.Score > winningPlayer.Score)
              {
                  winningPlayer = testPlayer;
              }          
          }

          sWinMsg = winningPlayer.Color.ToString() + " Wins!";

          m_refScoreGUI.SetTurnDisplay(sWinMsg);

      }


      /// <summary>
      /// Delete penguin / player from game
      /// </summary>
      /// <param name="penguin">Penguin to delete</param>
      /// <param name="partPrefab">Particle system to mark penguin's disappearence</param>
      /// <remarks>
      /// CALLED BY: GameManager::OnExpungePenguin()
      /// </remarks>
      /// 
      public void ExpungePenguin(GamePenguin penguin, ParticleSystem partPrefab)
      {
          CurrentPlayer.RemovePenguin(penguin);     // Remove penguin from player's list

          // Start "disappear" effect
          //
          GameObject.Instantiate(partPrefab, penguin.transform.position, Quaternion.identity);

          // Eliminate penguin
          //
          Destroy(penguin.gameObject);
      }

   ***/

}
