using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script attached to root of tile hierarchy
/// </summary>
/// <remarks>
/// Methods for traversal and game state -- On-screen attributes and animation
///   components are fetched from child G.O. "Art_Root"
/// </remarks>
///
public class GameTile : MonoBehaviour, ITileTraverse
{
    [SerializeField] float _soundDelay;   // How much to delay sound effect when tile sinks

    Renderer m_rend, m_rendIce;   // Renderers for tile and ice flow

    Animator m_animState;   // Animation state machine

    AudioSource m_sinkSound;   // Sound of the tile sinking

    // Highlighting attributes
    //
    Color m_colorCurrent, m_colorIceCurrent, m_colorHighlight;

    // Child objects for fish on the tile
    //
    Fish[] m_arrMyFish;

    // Effect to play when fish is taken
    //
    public ParticleSystem fishTakePrefab;

    bool m_bCanSelect = true;    // Can this tile be selected?
    bool m_bSelected = false;   // Has it been selected?

    // Tile attributes
    //
    Tile.TileType m_type;
    //
    int m_row;
    char m_col;
    //
    bool m_bEmpty = true;    // Has tile been removed from play? (Initially true until tile is fully initialized)
    bool m_bPenguinHere;     // Is there a penguin currently on this tile?
    //
    GamePenguin m_currentPenguin = null;
    //
    int m_numFish;


    public Tile.TileType Type
    {
        get { return m_type; }
        set { m_type = value; }
    }

    public int row
    {
        get { return m_row; }
        set { m_row = value; }
    }

    public char col
    {
        get { return m_col; }
        set { m_col = value; }
    }

    public bool IsPenguinHere
    {
        get { return m_bPenguinHere; }
        set { m_bPenguinHere = value; }
    }

    public GamePenguin CurrentPenguin
    {
        get { return m_currentPenguin; }
        set { m_currentPenguin = value; }
    }

    public bool IsEmpty
    {
        get { return m_bEmpty; }
        set { m_bEmpty = value; }
    }

    public int numFish
    {
        get { return m_numFish; }
    }

    public string tileID
    {
        get { return ToString(); }
    }

    public override string ToString()
    {
        return col + row.ToString();
    }


    public void SetRowColumn(char newColumn, int newRow)
    {
        row = newRow;
        col = newColumn;
    }


    void SetHighlightColor(Color color)
    {
        m_colorHighlight = color;
    }


    public bool CanSelect
    {
        get { return m_bCanSelect; }
        set { m_bCanSelect = value; }
    }


    public bool IsSelected
    {
        get { return m_bSelected; }
    }


    public int FishScoreValue
    {
        get { return m_numFish; }
    }


    public void ClearSelected()
    {
        m_bSelected = false;
    }


    /// <summary>
    /// Place/Remove penguin from this tile
    /// </summary>
    /// <param name="penguin"></param>
    /// 
    public void PlacePenguin(GamePenguin penguin)
    {
        CurrentPenguin = penguin;   // Needed to apply an AI move to Game World

        IsPenguinHere = true;   // Needed for path traversal
    }
    /// 
    public void RemovePenguin()
    {
        CurrentPenguin = null;

        IsPenguinHere = false;
    }


    /// <summary>
    /// Remove fish from tile as player scores
    /// </summary>
    /// 
    void RemoveFish()
    {
        for (int i = 0; i < m_arrMyFish.Length; i++)
        {
            Fish fish = m_arrMyFish[i];

            ParticleSystem ps = Instantiate<ParticleSystem>(fishTakePrefab, fish.transform.position, 
                                                                            fish.transform.rotation, transform);

            fish.TakeFish(ps);
        }
    }


    /// <summary>
    /// Run animation to make the tile sink "into the billowy wave"
    /// </summary>
    /// 
    public void Sink()
    {
        RemoveFish();    // Remove fish from tile

        IsEmpty = true;    // This tile is now out of commission

        m_animState.SetBool("IsTileSinking", true);   // Trigger the animation on the "art"
    }


    /// <summary>
    /// Play "sinking" sound
    /// </summary>
    /// <remarks>
    /// CALLLED BY: GameManager::OnTriggerTileSink()
    /// </remarks>
    /// 
    public void PlaySinkSound()
    {
        if (m_sinkSound) m_sinkSound.PlayDelayed(_soundDelay);   
    }


    /// <summary>
    /// Highlight tile that can be validly selected on this turn
    /// </summary>
    /// 
    void Highlight(bool highlight)
    {
        if(highlight)  // Turn highlight on
        {
            m_colorCurrent    = m_rend.material.color;
            m_colorIceCurrent = m_rendIce.material.color;

            m_rend.material.color    = m_colorHighlight;
            m_rendIce.material.color = m_colorHighlight;

            // TODO: Make red if square cannot be legally reached
        }
        else  // Turn highlight off
        {
            m_rend.material.color = m_colorCurrent;
            m_rendIce.material.color = m_colorIceCurrent;
        }
    }


    /*** Deprecated
     * 
    // Mouse hovers over a selectable tile
    //
    void OnMouseEnter()
    {
        if (!IsEmpty)
        {
            if (CanSelect) Highlight(true);  
        }
    }


    // Mouse leaves tile
    //
    void OnMouseExit()
    {
        if (!IsEmpty)
        {
            Highlight(false);
        }
    }

    */


    /// <summary>
    /// Select this tile 
    /// </summary>
    /// <remarks>
    /// FLOW: InputProcesor::ProcessInput() => SendMessage(OnSelect) => (This method) => 
    ///   GameMgr::OnTileSelected() => GameMgr::State_Play::OnTileClicked() 
    /// </remarks>
    ///  
    public void OnSelect()
    {
        if (!IsEmpty && CanSelect)
        {
            GameManager.GetGameManager().OnTileSelected(this);
        }
    }


    public void InitTileParms(char col, int row, Tile.TileType type)
    {
        // Tile attributes
        //
        m_type = type;
        //
        m_row = row;
        m_col = col;
    }

    
    void Start ()
    {
        /* Set up tile so both the tile proper and its "ice" gets highlighted
               by mouse pointer */

        m_sinkSound = GetComponent<AudioSource>();

        Transform trArt = transform.Find("Art_Root");
        Transform trIce = transform.Find("Art_Root/Ice");

        m_rend    = trArt.GetComponent<Renderer>();
        m_rendIce = trIce.GetComponent<Renderer>();

        m_animState = trArt.GetComponent<Animator>();

        m_arrMyFish = trArt.GetComponentsInChildren<Fish>();  // Find fish that are on this tile

        m_numFish = m_arrMyFish.Length;   // Number of fish on the tile (will determine score value for this tile)

        m_colorCurrent    =    m_rend.material.color;
        m_colorIceCurrent = m_rendIce.material.color;

        SetHighlightColor(Color.green);

        fishTakePrefab = GameManager.GetGameManager().fishTakePrefab;

        m_bPenguinHere = false;   // Initially no penguin (Placed by users)
        m_bEmpty       = false;   // Becomes Empty after penguin leaves it with fish

        // Based on HTMFish rules, Tile is initially selectable only if it contains
        //   Exactly one fish
        //
        CanSelect = (m_numFish == 1);
    }


    /*** Path traversal methods -- No actual penguins move with these methods -- They just provide a "search tree" mechanism 
          for determining a penguin's current legal moves
     ***/
    /*
        /// <summary>
        /// Pseudo-container for path of tiles in a particular direction
        /// </summary>
        /// <param name="GetTileInDirection">Delegate function for one of the six directions a penguin can move in</param>
        /// 
        public IEnumerable GetPathInDirection(TileManager.TileTraverse GetTileInDirection)
        {
            GameTile t = this;

            for(;;)
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
    */

    /// <summary>
    /// Pseudo-container for all tiles that can be legally reached from this one
    /// </summary>
    /// 
    public IEnumerable AllPaths(TileManager tm)
    {
        GameTile t0 = this;

        foreach (TileManager.TileTraverse ThataWay in tm.funcsTraverse)
        {
            foreach (GameTile t in TileManager.GetPathInDirection(t0, ThataWay))
            {
                yield return t;
            }
        }
    }
}

