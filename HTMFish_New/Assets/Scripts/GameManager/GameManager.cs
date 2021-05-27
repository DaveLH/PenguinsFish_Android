using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Core data and methods for running and managing the Game
/// </summary>
/// 
public partial class GameManager : MonoBehaviour
{
    // Inactive = Not in game, Human = Human player, Youngest = Youngest Player (goes first), AI = computer
    //   (Set in dropdowns for each player on starting screen)
    //
    public enum PlayerStatus { Inactive = 0, Human = 1, Youngest = 2, AI = 3 };
        
    static GameManager s_singleton;

    // Other Core objects we need to talk to
    //
    TileManager   m_refTileMgr;
    AIBoardSim    m_refAIBoard;

    GamePenguin m_currentPenguin = null;

    GameTile m_currentTile = null;

    public int dimension;   // Dimensions of board will be dimension X dimension

    // Where to start the array of tiles
    //
    public Vector3 StartingPoint;

    // Panels to display score
    //
    public GameObject[] scorePanels;

    // Prefabs for all color penguins
    //
    public GameObject[] PenguinPrefabs;
    public GameObject[] PenguinAIPrefabs;

    // Particle prefab for when penguin vanishes from game
    //
    public ParticleSystem penguinDepartPrefab;

    // Effect to play when fish is taken
    //
    public ParticleSystem fishTakePrefab;

    // Flag to set when tiles are all set up and first player is set to move
    //   (Used for initialization by AIBoardSim::FetchInitialBoard()
    //
    // bool m_bGameSetupComplete = false;
    //
    // public bool IsGameSetupComplete() { return m_bGameSetupComplete; }


#region State

    /*** State objects ***/
    /*
    public GameState.State_Initialize  state_Init = new GameState.State_Initialize();
    public GameState.State_Play        state_Play = new GameState.State_Play();
    public GameState.State_EndGame     state_GEnd = new GameState.State_EndGame();
    */

    /*** State Data ***/

    // GameState.BState m_currentState0;   // TODO: Deprecate

    
    /*
    public void TransitionTo(GameState.BState newState)
    {
        if (m_currentState != null)
        {
            Debug.Log("Exiting Game State: " + m_currentState.ToString());

            // Transition out of old state
            //
            m_currentState.OnStateExit(this);
        }

        m_currentState = newState;

        // Go into new state
        //
        newState.OnStateEnter(this);

        Debug.Log("New Game State:" + m_currentState.ToString());
    }


    /// <summary>
    /// 
    /// </summary>
    /// 
    public void UpdateState()
    {
        m_currentState.OnStateUpdate(this);
    }
    */
#endregion

#region Properties

    public Transform CurrentTileTrnsfm
    {
        get
        {
            return CurrentTile.transform;
        }
    }


    public GameTile CurrentTile
    {
        get
        {
            return m_currentTile;
        }

        set
        {
            m_currentTile = value;
        }
    }


    public GamePenguin CurrentPenguin
    {
        get
        {
            return m_currentPenguin;
        }

        set
        {
            m_currentPenguin = value;
        }
    }


    /// <summary>
    /// Are all the penguins set up?
    /// </summary>
    /// 
    public bool PenguinSetupDone
    {
        get
        {
            return EveryoneGotTheirPenguins;
        }
    }


    /// <summary>
    /// Check for end of game play (Only one player left that can move)
    /// </summary>
    /// <remarks>
    /// TODO: With new state system, This probably can be deprecated
    /// </remarks>
    /// 
    public bool IsGameEnd
    {
        get { return (NumActivePlayers <= 1); }
    }

#endregion


    /// <summary>
    /// Get interface to unique GameManager
    /// </summary>
    /// 
    public static GameManager GetGameManager()
    {
        return s_singleton;
    }


    // TODO: Enable tiles of penguin's legal moves
    //
    public void EnableLegalMoves() { }


    /// <summary>
    /// Check for player removing selected penguin with "R"-keypress
    /// </summary>
    /// <remarks>
    /// CALLED BY: State_Move_Human::OnStateUpdate()
    /// </remarks>
    /// 
    public void ChkRKey()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // On "R" key, attempt to remove selected penguin (and tiles if penguin is alone on ice floe)
            //
            bool bSuccess = TryRemovePenguin();  

            // If island can't be claimed, unselect this penguin, and wait for another move
            //
            if (!bSuccess) UnselectPenguin();
        } 
    }


    /// <summary>
    /// Attempt to remove a penguin by checking if it's alone on an "island" and then letting it claim the remaining fish therein
    /// </summary>
    /// <remarks>
    /// CALLED BY: ChkRKey() or State_Move_Human::OnPenguinClicked()  (Depending on whether target platform has a keyboard)
    /// </remarks>
    /// <returns>
    /// TRUE if penguin (and its "island") could be removed, FALSE if penguin is not alone on "island"
    /// </returns>
    /// 
    public bool TryRemovePenguin()
    {
        // Attempt to remove selected penguin (and tiles if penguin is alone on ice floe)
        //
        int result = RemovePenguin(CurrentPlayer, CurrentPenguin);

        // If island can't be claimed, return false
        //
        return (result >= 0);
    }


    /// <summary>
    /// Respond to a tile click
    /// </summary>
    /// <remarks>
    /// CALLED BY: Tile::OnMouseDown()
    /// </remarks>
    /// 
    public void OnTileSelected (GameTile tile)
    {
        m_currentState.OnTileClicked(m_stateMach, tile);
	}


    /// <summary>
    /// Respond to a penguin click
    /// </summary>
    /// <remarks>
    /// CALLED BY: GamePenguin::OnMouseDown()
    /// </remarks>
    /// 
    public void OnPenguinSelected(GamePenguin penguin)
    {
        // Unselect any currently selected penguin
        //
        UnselectPenguin();

        m_currentState.OnPenguinClicked(penguin);
    }


    /// <summary>
    /// Invoked when a move ends, i.e. when the penguin has reach its destination tile and 
    ///   its animation ends 
    /// </summary>
    /// <remarks>
    /// FLOW: GamePenguin::J_adoube() loops until GamePenguin::IsCentered is true, 
    ///   then -> [Penguin_]State_J_adoube::OnStateExit() -> OnMoveEnd()
    /// </remarks>
    /// 
    public void OnMoveEnd()
    {
        // Trigger "NextPlayer" state
        //
        StateParam_MovePending = false;
        //
        StateTrigger_NextPlayer();
    }



    /// <summary>
    /// Find penguin's current legal moves and put them in a list
    /// </summary>
    /// <param name="penguin">Current penguin</param>
    /// 
    public void FindLegalMoves(GamePenguin penguin)
    {
        m_refTileMgr.FindValidTiles(penguin.CurrentTile, penguin.ValidMoveList);
    }


    /// <summary>
    /// Preliminary initialization of stuff
    /// </summary>
    /// 
    void Awake()
    {
        s_singleton = this;

        // GameObject guiCanvas = GameObject.Find("Canvas");

        m_refTileMgr = GetComponent<TileManager>();
        m_refAIBoard = GetComponent<AIBoardSim>();
        m_brain      = GetComponent<AIBrain>();
        m_stateMach  = GetComponent<Animator>();

        InitScoreMap();

        m_allPlayerScores = SceneController.GetSceneController().AllPlayerScores;
    }


    /// <summary>
    /// De facto "Entry Point" for main game -- Set up the board, and initialize the state machine
    /// </summary>
    /// 
    void Start()
    {
        m_refTileMgr.SetupBoard(StartingPoint, dimension);  // Setup tiles on game board

        // Initialize players and start game (Initialization Game State)
        //
        StateParam_PlacingPenguins = true;  // Flag sent to state machine
    }


    void Update()
    {
        // UpdateState();
    }

}
