using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Part of GameManager that manages "refereeing" the players (Whose turn it is, &c.)
/// </summary>
/// 
public partial class GameManager : MonoBehaviour
{
    // List of all participating players
    //
    List<Player> m_players = new List<Player>();

    // Map of colors to GUI Score Texts
    //
    Dictionary<GamePenguin.PenguinColor, Text> m_mapScoreToGUI = new Dictionary<GamePenguin.PenguinColor, Text>();

    // Reference (read and write) to score tallies maintained in Persistant SceneController
    //
    Dictionary<GamePenguin.PenguinColor, int> m_allPlayerScores;

    Player m_currentPlayer;   // Which player's turn is it?

    int m_currentPlayerIndex;   // Which player's turn is it?

    int m_numActivePlayers = 0;   // How many players are actively in the game (May be less than NumPlayers if someone drops out)

    const int MaxPlayers = 4;   // Maximum possible players 

    GameTile m_condemnedTile = null;  // Tile that's about to sink


    // Wire players' scores to GUI display
    //
    [SerializeField] protected Text scoreBlue;
    [SerializeField] protected Text scoreRed;
    [SerializeField] protected Text scoreGreen;
    [SerializeField] protected Text scoreGold;

    // Display whose turn it is
    //
    [SerializeField] protected Text turnText;

    // Display progress when AI is computing a move
    //
    [SerializeField] protected GameObject _AIProgressDialog;
    //
    protected AIProgressDlg m_AIDlg = null;
    //
    public AIProgressDlg AIProgress
    {
        get
        {
            if(m_AIDlg == null) m_AIDlg = _AIProgressDialog.GetComponentInChildren<AIProgressDlg>();

            return m_AIDlg;
        }
    }


    public void UpdateTurnText(string sText)
    {
        turnText.text = sText;
    }

    public void UpdateCurrentTurnText()
    {
        UpdateTurnText(CurrentPlayer.TurnMessage);
    }


#region Properties

    /// <summary>
    /// Get current color whose move it is 
    /// </summary>
    /// 
    public GamePenguin.PenguinColor CurrentTurn
    {
        get
        {
            return CurrentPlayer.Color;
        }
    }


    /// <summary>
    /// Get object for current player 
    /// </summary>
    /// 
    public Player CurrentPlayer
    {
        get
        {
            return m_currentPlayer;
        }
        set
        {
            m_currentPlayer = value;
        }
    }


    /// <summary>
    /// Are all the penguins set up?
    /// </summary>
    /// 
    public bool EveryoneGotTheirPenguins
    {
        get
        {
            foreach(Player pl in m_players)
            {
                if(pl.NumPenguins < PenguinsPerPlayer(NumPlayers))
                {
                    return false;
                }
            }

            return true;
        }
    }


    public int NumPlayers
    {
        get
        {
            return m_players.Count;
        }
    }

    public int NumActivePlayers
    {
        get
        {
            return m_numActivePlayers;
        }
    }
#endregion


#region Handle Score Display

    /// <summary>
    /// Map scores to GUI elements to display them on screen
    /// </summary>
    /// 
    void InitScoreMap()
    {
        m_mapScoreToGUI.Add(GamePenguin.PenguinColor.Blue, scoreBlue);
        m_mapScoreToGUI.Add(GamePenguin.PenguinColor.Red, scoreRed);
        m_mapScoreToGUI.Add(GamePenguin.PenguinColor.Gold, scoreGold);
        m_mapScoreToGUI.Add(GamePenguin.PenguinColor.Green, scoreGreen);
    }


    /// <summary>
    /// Fetch the GUI panel for input player color's score
    /// </summary>
    /// <param name="color">Player Color</param>
    /// <returns>UI Panel</returns>
    /// 
    GameObject GetScorePanel(GamePenguin.PenguinColor color)
    {
        return scorePanels[(int)color];
    }


    /// <summary>
    /// After player's last penguin is removed from game and casn no longer move.
    ///   indicate this by "ghosting out" the score display.
    /// </summary>
    /// <param name="color">Player no longer playing</param>
    /// 
    void GhostScorePanel(GamePenguin.PenguinColor color)
    {
        GameObject panel = GetScorePanel(color);

        Text[] texts = panel.GetComponentsInChildren<Text>();

        foreach(Text txt in texts)
        {
            txt.color     = Color.gray;        // Grayed-out
            txt.fontStyle = FontStyle.Italic;  // Just to emphasize the point
        }

        UpdateScore();   // Update the display
    }


    /// <summary>
    /// Update scores displayed on screen
    /// </summary>
    /// 
    void UpdateScore()
    {
        foreach (Player player in m_players)
        {
            // Debug.Log("GUI Score Map [" + player.ToString() + "] = " + m_mapScoreToGUI[player.Color].ToString());  

            m_allPlayerScores[player.Color] = player.Score;  // (Extra table really needed?)

            m_mapScoreToGUI[player.Color].text = m_allPlayerScores[player.Color].ToString();   // Onscreen scores
        }
    }

#endregion


#region Player Initailization

    /// <summary>
    /// Initialze players from input from start screen
    /// </summary>
    /// <remarks>The player to go first is the player designated the youngest (per the official rules of HTMFish), or if none
    /// is designated, Red goes first.  To avoid any weirdness, Red is thus disallowed from being an AI</remarks>
    /// 
    public void InitPlayers()
    {
        bool bFirstPlayer = false, bFirstPlayerFound = false;
        bool bAI;

        GamePenguin.PenguinColor color;

        SceneController sc = SceneController.GetSceneController();

        for (color = GamePenguin.PenguinColor.Red; color < GamePenguin.PenguinColor._None; color++)
        {
            if (sc.PlayersActiveStatus.ContainsKey(color))   // It *should* -- Else we have a problem...
            {
                if (sc.PlayersActiveStatus[color] == PlayerStatus.Inactive) GetScorePanel(color).SetActive(false);   // Turn off scorer unused color  
                //
                else // This player color was selected on start screen
                {
                    // Is this a computer or human player?
                    //
                    bAI = (sc.PlayersActiveStatus[color] == PlayerStatus.AI);

                    // Is this the designated first (youngest) player?  
                    //  (Should be mutually exclusive with AI, i.e. AI can't go first)
                    //
                    bFirstPlayer = (sc.PlayersActiveStatus[color] == PlayerStatus.Youngest);

                    // Instiantiate player object
                    //
                    CreatePlayer(color, (bFirstPlayer && !bFirstPlayerFound), bAI);

                    if (bFirstPlayer) bFirstPlayerFound = true;   // Can only be one first player!

                    // If there's an AI player, tell AI what color player it's playing for, and turn on
                    //  access to AI methods
                    //
                    if (bAI)
                    {
                        m_brain.AIColor = color;

                        IsUsingAI = true;
                    }

                    GetScorePanel(color).SetActive(true);   // Turn on scorer for this player
                }
            }
        }
    }


    /// <summary>
    /// Add a player to game
    /// </summary>
    /// <param name="color">Player's penguin color</param>
    /// <param name="bFirstPlayer">Will this be the first player to move?</param>
    /// <param name="bAIPlayer">Should this player be controlled by the computer? (otherwise, human player)</param>
    /// 
    public void CreatePlayer(GamePenguin.PenguinColor color, bool bFirstPlayer, bool bAIPlayer)
    {
        Player newPlayer;

        if(bAIPlayer) newPlayer = ScriptableObject.CreateInstance<PlayerAI>();
        //
        else newPlayer = ScriptableObject.CreateInstance<PlayerHuman>();
        
        newPlayer.Color = color;

        newPlayer.IsStartingPlayer = bFirstPlayer;

        m_players.Add(newPlayer);   // Player is now in game

        m_allPlayerScores.Add(color, 0);   // Start keeping score

        m_numActivePlayers++;
    }

#endregion

   
#region Turn Managerment

    /// <summary>
    /// Launch next player's turn 
    /// </summary>
    /// 
    public void NextPlayer()
    {
        // Check if there's only one player left -- If so, end game
        //
        if (m_numActivePlayers == 1)
        {
            // Prevent state machine from going back to waiting for players to move
            //
            StateParam_HumanMove = false;
            StateParam_AIMove    = false;

            RotateTurns();   // Do one final turn update, so the last remaining player can win fish on its "island"

            StateTrigger_EndTheGame();   // End of game -- Add final fishes to scores, then transition to ending screen
        }
        else   // Else advance to next player
        {
            CurrentPlayer.IsMyTurn = false;   // Move ended -- Change player state to "Not my turn" 

            StateParam_PenguinRemoved = false;   // Clear this flag if it was set

            UpdateScore();   // Update displayed scores
            RotateTurns();   // Move current player to next player

            CurrentPlayer.IsMyTurn = true;   // Changes new player's state to "My turn" 

            // Set params in state machine based on type of player
            //  (So state machine knows what state to branch to after "NextPlayer")
            //
            CurrentPlayer.SetStatePlayer(this);  // SetStatePlayer() is polymorphic 
        }
    }

    
    /// <summary>
    /// Rotate whose turn it is
    /// </summary>
    /// <returns>Currently it's this player's turn</returns>
    /// 
    void RotateTurns()
    {
        if (NumActivePlayers >= 1)   // Verify valid number of players 
        {
            // Increment to new player's turn
            //
            IncTurn(); 
        }
    }


    /// <summary>
    /// Set whose turn it is by incrementing index, skipping inactive players
    /// </summary>
    /// 
    void IncTurn()
    {
        do
        {
            m_currentPlayerIndex++;

            m_currentPlayerIndex = m_currentPlayerIndex % NumPlayers;

            CurrentPlayer = m_players[m_currentPlayerIndex]; 

        } while (!CurrentPlayer.IsActive);
    }


    /// <summary>
    /// Reset Current Player to start of list
    /// </summary>
    /// 
    void ResetTurns()
    {
        m_currentPlayerIndex = 0;   // Reset counter

        CurrentPlayer = m_players[m_currentPlayerIndex];  // Initialize Current Player
    }


    /// <summary>
    /// At start of play move current player index to starting player
    /// </summary>
    /// <remarks>
    /// Starting player is determined by first player encountered in start screen input
    ///   designated "Youngest", in accordence with the official rules of HTMFish.
    ///   If no "Youngest" is found, the first human player in list goes first.
    ///   (The computer/AI is disallowed from ever going first.)
    /// </remarks>
    /// 
    public void GoToStartingPlayer()
    {
        ResetTurns();

        do
        {
            if (CurrentPlayer.IsStartingPlayer) return;   // We're at Starting player -- End

            IncTurn();  
        }
        while (m_currentPlayerIndex != 0);   // Stop when we wrap back round to zero

        // If no starting player found, it's the first discovered human player by default.
        //  TODO -- Check to insure it's not possible for no players to be designated human
        //             (PLAY button should be disabled on Start Screen)
        //
        while (CurrentPlayer.IsAI) IncTurn();   // If first player in list is AI, skip it (This should iterate at most once)
        //
        CurrentPlayer.IsStartingPlayer = true;
    }



    #endregion


#region Penguin Management

    /// <summary>
    /// Return how many penguins each player gets (Based on total number of players -- See Rules for HTMFish)
    /// </summary>
    /// <param name="numPlayers"></param>
    /// <returns></returns>
    /// 
    public static int PenguinsPerPlayer(int numPlayers)
    {
        const int Max = 4;   // Maximum players allowed
        const int Caroline = 6;   // Determining factor

        if (numPlayers >= 1 && numPlayers <= Max) return Caroline - numPlayers;
        //
        else return 0;    // Invalid player number
    }


    /// <summary>
    /// Create physical Penguin game object and place him on input tile
    /// </summary>
    /// <param name="tile">Tile to place penguin</param>
    /// <param name="color">Color of penguin</param>
    /// <param name="bAI">Instantiate AI penguin?</param>
    /// <remarks>
    /// FLOW: State_Initialize::OnTileClicked() => Player::AddPenguin() => GameManager::CreatePenguin()
    /// </remarks>
    /// 
    public GamePenguin CreatePenguin(GameTile tile, GamePenguin.PenguinColor color, bool bAI)
    {
        GameObject prefab = null;

        if (bAI)
        {
             prefab = PenguinAIPrefabs[(int)color];   // Choose the prefab for AI
        }
        else prefab = PenguinPrefabs[(int)color];   // Choose the prefab for current player 

        Vector3 v3Pos = tile.transform.position;                                // Point to place the penguin at
        Vector3 v3Rot = new Vector3(0.0f, Random.Range(0.0f, 359.99f), 0.0f);   // Random initial rotation

        // Instantiate a new penguin
        //
        GameObject gobNewPenguin = Instantiate(prefab, v3Pos, Quaternion.Euler(v3Rot));

        GamePenguin newPenguin = gobNewPenguin.GetComponent<GamePenguin>();

        // Initialize non-GameObject-related data
        //
        newPenguin.InitPenguinData(color, tile, bAI);

        // Link penguin to its tile
        //
        tile.PlacePenguin(newPenguin);

        return newPenguin;
    }


    /// <summary>
    /// Mark penguin clicked on as "selected", if current players owns this penguin
    /// </summary>
    /// <param name="penguin"></param>
    /// 
    public void SelectPenguin(GamePenguin penguin)
    {
        if (CurrentPlayer.IsMyPenguin(penguin))   // If current player picked one of their own penguins
        {
            ChangePenguin(penguin);
        }
    }


    /// <summary>
    /// Clear penguin selection
    /// </summary>
    /// 
    public void UnselectPenguin()
    {
        if (CurrentPenguin) CurrentPenguin.OnSelectOff();

        CurrentPenguin = null;
    }


    /// <summary>
    /// If player selects a penguin, highlight it, if another one was previously clicked, clear its selection
    /// </summary>
    /// <param name="newPenguin"></param>
    /// 
    void ChangePenguin(GamePenguin newPenguin)
    {
        if (m_currentPenguin) m_currentPenguin.OnSelectOff();  // Unselect currently selected

        m_currentPenguin = newPenguin;   // New selection

        // Set Selected flag on new penguin
        //
        m_currentPenguin.OnSelect();
    }


    /// <summary>
    /// Set the current penguin -- Necessary before calling GamePenguin::MoveTo()
    /// </summary>
    /// <param name="penguin"></param>
    /// <remarks>
    /// This is called either when the player clicks on a penguin (State_Move_Human::OnPenguinClicked()), 
    ///   or when the AI has found a move and is ready to move the GamePenguin on the board.
    ///   </remarks>
    ///   
    public void MakePenguinCurrent(GamePenguin penguin)
    {
        CurrentPenguin = penguin;
        CurrentTile    = penguin.CurrentTile;
    }


    /// <summary>
    /// Clear the current penguin references (Compliment to MakePenguinCurrent())
    /// </summary>
    /// 
    public void ClearPenguinCurrent()
    {
        CurrentPenguin = null;
        CurrentTile    = null;
    }


    /// <summary>
    /// Apply a move to the penguins and board in Game World
    /// </summary>
    /// <param name="penguin">Penguin to move</param>
    /// <param name="tile">Destination tile</param>
    /// <remarks>
    /// CALLED BY: ExecuteHumanMove(), ExecuteAIMove()
    /// </remarks>
    /// 
    void ExecuteMove_GameWorld(GamePenguin penguin, GameTile tile)
    {
        GameTile startTile = penguin.CurrentTile;

        if (penguin.MoveTo(tile))  // If a legal move was selected, start animating the penguin's move
        {
            // Player acquires this tile's fish
            //
            CurrentPlayer.AddFish(startTile.FishScoreValue);

            // Set flag for tile just departed to sink (not until penguin has left it, else it will look like
            //  it's "walking on water"!
            //
            m_condemnedTile = startTile;

            // Go to "MovePending" state until all animations complete
            //
            StartAnimateMove();
        }
    }


    /// <summary>
    /// Tell tile to start sinking
    /// </summary>
    /// <param name="startTile"></param>
    /// <remarks>
    /// CALLED BY: Event: Penguin no longer standing on tile
    /// </remarks>
    /// 
    public void OnTriggerTileSink()
    {
        m_condemnedTile.Sink();
        // m_condemnedTile.PlaySinkSound();

        m_condemnedTile = null;
    }


    /// <summary>
    /// Launch "Move pending" state (Animation of move)
    /// </summary>
    /// 
    public void StartAnimateMove()
    {
        StateParam_HumanMove   = false;
        StateParam_AIMove      = false;
        StateParam_MovePending = true;
    }


    /// <summary>
    /// Apply a move made by the human user to the AI's internal board
    /// </summary>
    /// <param name="move">"Command.Move" object instantiated from GamePenguin that just moved</param>
    /// <remarks>
    /// CALLED BY: ExecuteHumanMove(), ExecuteAIMove()
    /// </remarks>
    /// 
    void ExecuteMove_Internal(Command.GameMove move)
    {
        // If a human just moved, update the internal board the AI uses
        //   (If the AI moved, the board is updated by the AI algorithms, so we need not do it here.)
        //
        m_refAIBoard.UpdateAIBoard(CurrentPlayer.Color, move);
    }

    /*
    /// <summary>
    /// Internal move execution routine
    /// </summary>
    /// <param name="tile">Destination tile</param>
    /// <param name="bHumanMove">Did a human (rather than the AI make this move?)</param>
    /// 
    void ExecuteMove(GameTile tile, bool bHumanMove)
    {
        if (CurrentPenguin.MoveTo(tile))  // If a legal move was selected, start animating the penguin's move
        {
            // Player acquires this tile's fish
            //
            CurrentPlayer.AddFish(CurrentTile.FishScoreValue);

            // Start tile sinking animation for the tile just departed
            // TODO: *** This may be a bug -- Shouldn't CurrentTile be the *selected* (destination) tile??
            //
            CurrentTile.Sink();

            // If a human just moved, update the internal board the AI uses
            //   (If the AI moved, the board is updated by the AI algorithms, so we need not do it here.)
            //
            if (IsUsingAI && bHumanMove)
            {
                m_refAIBoard.UpdateAIBoard(CurrentPlayer.Color, PortmanteauMovePenguin(CurrentTile, tile)); 
            }

            // Go to "MovePending" state until all animations complete
            //
            StateParam_AIMove = false;
            StateParam_MovePending = true;

            // Clear selections
            //
            CurrentPenguin = null;
            CurrentTile = null;
        }
        // Else continue to wait for a valid tile to be clicked
    }
    */

    /// <summary>
    /// If user selected a valid normal penguin move, execute that move, both onscreen and internally
    /// </summary>
    /// <param name="tile">Destination tile</param>
    /// <remarks>
    /// CALLED BY: SubStateMach_MainGame->State_Move_Human::OnTileClicked()
    /// </remarks>
    /// 
    public void ExecuteHumanMove(GameTile tile)
    {
        if (CurrentPenguin.ValidateDestination(tile))   // First check that selected tile is a legal move
        {
            ExecuteMove_GameWorld(CurrentPenguin, tile);

            if (IsUsingAI)   // Only need to do this bit if AI is in this Game
            {
                // Instantiate a command to send to the internal board
                //
                Command.GameMove move = PortmanteauMovePenguin(CurrentPenguin.CurrentTile, tile);

                // Tell the internal board to update to reflect the new move 
                //
                ExecuteMove_Internal(move); 
            }
        }
    }


    /// <summary>
    /// After AI selects move and updates the internal board, then animate the move on the Game Board
    /// </summary>
    /// <param name="tile">Destination tile</param>
    /// <remarks>FLOW: AIBrain::MakeMove_***() -> OnExeAIMove() -> ExecuteAIMove()
    /// </remarks>
    /// 
    public void ExecuteAIMove(Command.GameMove move)
    {
        ExecuteMove_Internal(move);
        
        // Simulate a penguin click from AI move
        //
        GamePenguin penguin = m_refTileMgr.tileTable[move.fromTile].CurrentPenguin;

        // Simulate a destination tile click from AI move
        //
        GameTile tile = m_refTileMgr.tileTable[move.toTile];

        // Execute move onscreen
        //
        ExecuteMove_GameWorld(penguin, tile);
    }


    /*
    /// <summary>
    /// Apply a move made by AI onto the Game Board
    /// </summary>
    /// <remarks>INVOKED BY: AIBoardSim::ApplyPendingMove()</remarks>
    /// <param name="move">The command for the move to execute, dispatched by the internal AI board</param>
    /// 
    public void OnExeAIMove(Command.GameMove move)
    {
        // No need to update internal board -- Already done in AIBoardSim::ApplyPendingMove()

        // Tell the penguin to move in Game World -- Handle winning fish -- Drop departed tile
        //
        ExecuteAIMove(move);
    }
    */

    /// <summary>
    /// Add a penguin to the AI board, if AI is in use
    /// </summary>
    /// <param name="penguin">New penguin to add</param>
    /// <remarks>
    /// INVOKED BY: Player::AddPenguin()
    /// </remarks>
    /// 
    public void OnAddPenguinAIBoard(GamePenguin penguin)
    {
        if (IsUsingAI)
        {
            m_refAIBoard.AddPenguin(penguin.CurrentTile.name, penguin.color);
        }
    }


    /// <summary>
    /// If a player's last penguin is removed from board ("R" key), then remove player from Turn list
    /// (This is not strictly analogous to "resigning" in chess -- Player will still get a final score, 
    ///  and may even be the winner!)
    /// </summary>
    /// <param name="pl">Player resigning from game play</param>
    /// 
    public void Resign()
    {
        Player pl = CurrentPlayer;

        // Remove current player from active play
        //
        pl.IsActive = false;

        m_numActivePlayers--;

        // Add info to panel
        //
        GhostScorePanel(pl.Color);
    }


    /// <summary>
    /// Remove one of al players' penguins when it can claim a
    ///   whole "island" of tiles
    /// </summary>
    /// <returns>
    /// The total number of fish of the tiles to be added to the player's score
    /// </returns>
    /// 
    public int RemovePenguin(Player player, GamePenguin penguin)
    {
        m_refTileMgr.workTiles.Clear();   // Clear "workspace"

        int islandFishTally = TileManager.CheckForIsland(m_refTileMgr, penguin.CurrentTile, m_refTileMgr.workTiles, true);

        if (islandFishTally >= 0)   // -1 = Penguin not alone on contiguous tiles
        {
            player.AddFish(islandFishTally);       // Add all fish on local tiles to score

            ExpungePenguin(penguin, penguinDepartPrefab);  // Destroy penguin game object with particle effects
           
            m_refTileMgr.ExpungeTiles(m_refTileMgr.workTiles);  // Sink/Destroy all tiles on penguin's "island"

            // Update the internal board the AI uses -- First apply penguin removal, then turn off the virtual tiles
            //
            if (IsUsingAI)
            {
                Command.GameMove move = PortmanteauRemovePenguin(penguin.CurrentTile);
                //
                 ExecuteMove_Internal(move);
                ExpungeTiles_Internal(m_refTileMgr.workTiles);
            }

            // No point in waiting for all tiles to vanish, so go directly to "NextPlayer" state
            //
            StateParam_PenguinRemoved = true;
            //
            StateTrigger_NextPlayer();
        }

        return islandFishTally;
    }


    /// <summary>
    /// When a penguin has no legal moves (i.e. is blocked by other penguins, or is alone on a single isloated tile),
    ///   clicking on it triggers removal of it and the single tile it's standing on from the board.
    /// </summary>
    /// <param name="penguin">Selected penguin to remove</param>
    /// <remarks>
    /// CALLED BY: SubStateMach_MainGame->State_Move_xxx::OnPenguinClicked()
    /// </remarks>
    /// 
    public void RemoveUnmovablePenguin(GamePenguin penguin)
    {
        CurrentPlayer.AddFish(penguin.CurrentTile.FishScoreValue);   // Player wins fish on this tile

        penguin.CurrentTile.Sink();   // Animate sinking tile, and then remove it

        ExpungePenguin(penguin, penguinDepartPrefab);  // Remove Penguin

        // Update the internal board the AI uses
        //
        if (IsUsingAI)
        {
            Command.GameMove move = PortmanteauRemovePenguin(penguin.CurrentTile);

            ExecuteMove_Internal(move);
        }

        // TODO: Delete? -- StateParam_MovePending = true;   // Go to animation state while sinking tile anim plays

        // No point in waiting for the tile to vanish, so go directly to "NextPlayer" state
        //
        StateParam_PenguinRemoved = true;
        //
        StateTrigger_NextPlayer();
    }


    /// <summary>
    /// "Sink" a bunch of tiles on the internal board
    /// (i.e. Set "Active" to false)
    /// </summary>
    /// 
    public void ExpungeTiles_Internal(List<GameTile> tiles)
    {
        m_refAIBoard.SetTileStatus(tiles, false);
    }


    /// <summary>
    /// At end of game, remove the last remaining player's penguins, and tally final score
    /// </summary>
    /// 
    public void RemoveFinal()
    {
        Debug.Assert(m_numActivePlayers == 1);  // This method should only execute when there's one player left.
        Debug.Assert(CurrentPlayer.IsActive);   // And the CurrentPlayer pointer should be on that last player

        CurrentPlayer.AddFinal(m_refTileMgr);

        UpdateScore();   // Final update of scores for final screen

        m_refTileMgr.ExpungeTiles(m_refTileMgr.workTiles);   // Destroy all the remaining tiles we just counted
    }


    /// <summary>
    /// Delete the penguin, from player list and from scene
    /// </summary>
    /// <param name="penguin">Penguin to delete</param>
    /// <param name="partprefan">Particle system to mark penguin's disappearence</param>
    /// <remarks>
    /// CALLED BY: RemovePenguin(), RemoveUnmovablePenguin()
    /// </remarks>
    /// 
    public void ExpungePenguin(GamePenguin penguin, ParticleSystem partPrefab)
    {
        CurrentPlayer.RemovePenguin(penguin);     // Remove penguin from player's list

        // Start "disappear" effect
        //
        Instantiate(partPrefab, penguin.transform.position, Quaternion.identity);   

        // Eliminate penguin
        //
        Destroy(penguin.gameObject);
        
        if (CurrentPlayer.NumPenguins == 0) Resign();   // If player has no penguins left, remove from game play
    }


    /// <summary>
    /// Determine if a penguin is alone on an "island" with no other penguins
    /// </summary>
    /// <param name="tm">Ref to TileManager</param>
    /// <param name="penguin">Penguin to evaluate</param>
    /// <returns>TRUE if no other penguins</returns>
    /// 
    public static bool IsAlone(GamePenguin penguin, TileManager tm)
    {
        return (TileManager.CheckForIsland(tm, penguin.CurrentTile, tm.workTiles, true) >= 0);
    }

#endregion


#region Deprecated Methods

    /*** Deprecated methods -- Should probably be deleted
     * 
    public void OnExpungePenguin(GamePenguin penguin)
    {
        ExpungePenguin(penguin, penguinDepartPrefab);
    }
    

    /// <summary>
    /// Determine if a penguin is alone on an "island" with no other penguins (old version, pre-AI)
    /// </summary>
    /// <param name="tm">Ref to TileManager</param>
    /// <param name="penguin">Penguin to evaluate</param>
    /// <returns>TRUE if no other penguins</returns>
    /// 
    public static bool IsAlone0(GamePenguin penguin, TileManager tm)
    {
        return (!CheckForPenguins(tm, penguin.CurrentTile, tm.workTiles));
    }
    

    /// <summary>
    /// Recursive func that checks neighboring tiles for penguins in order to determine if a penguin is
    ///   standing alone on a contiguous "island" of tiles
    /// </summary>
    /// <param name="centerTile">Tile from which to check adjacent tiles</param>
    /// <param name="checkedTiles">List of tiles already checked</param>
    /// <returns>TRUE if a penguin found</returns>
    /// 
    public static bool CheckForPenguins(TileManager tm, GameTile centerTile, List<GameTile> checkedTiles)
    {
        if (!checkedTiles.Contains(centerTile)) checkedTiles.Add(centerTile);   // Store starting tile right away

        foreach (TileManager.TileTraverse ThataWay in tm.funcsTraverse)
        {
            GameTile tile = (GameTile)ThataWay(centerTile);   // Get adjacent tile in current direction

            if (tile == null) continue;   // We've reached the edge -- No tile in this direction

            // Check possible conditions (if this tile hasn't already been checked)
            //
            if (!checkedTiles.Contains(tile) && !tile.IsEmpty)  // If this is an empty tile or one we already checked, don't go further in this direction
            {
                checkedTiles.Add(tile);   // Add to checked tiles

                if (tile.IsPenguinHere) return true;  // We're done -- Target Penguin not alone here -- Return
                //
                else   // Go to next tile
                {
                    if (CheckForPenguins(tm, tile, checkedTiles)) return true;  // Once penguin found we can just break out

                    // Else continue
                }
            }
        }

        return false;  // No penguins found -- Return
    }
*/
#endregion

}
