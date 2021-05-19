using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePenguin : MonoBehaviour
{
    public enum PenguinColor { Red, Blue, Gold, Green, _None };    // One penguin color for each player 

    [HideInInspector] public bool IsMyTurn   { get; set; }
    [HideInInspector] public bool IsSelected { get; set; }

    [SerializeField] protected float moveSpeed = 1f;   // Default Time in sec. it takes to make a move from one tile to another
    [SerializeField] protected float moveSpMax = 3f;   // Max. Time in sec. it takes to make a move from one tile to another
    [SerializeField] protected float turnSpeed = 1f;   // Time in sec. it takes to make a turn at start of move

    [SerializeField] protected float margin      = 0.5f;   // How close to get to desired point before stopping
    [SerializeField] protected float angleMargin = 5.0f;    // How close to facing direction of movement before starting to move

    [SerializeField] protected Color highlightColor = Color.green;

    Renderer m_rend;

    Color m_normalColor;

    Animator m_StateMach;   // Penguin's animation state (Moving or Not Moving)

    GameManager m_refGameMgr;

    // Unique ID for each penguin
    //
    PenguinColor m_color;  // Whose "team" am I on?
    //
    // int m_num;   // What's my "jersey number"?

    GameTile m_currentTile;   // Tile penguin is currently standing on
    GameTile m_destinTile;    // Destination tile

    Vector3 m_v3DesiredDestination;   // Where we want to end up on this move

    List<GameTile> m_validTiles = new List<GameTile>();   // List of all tiles I can currently legally move to


    public bool IsAI  // Is this penguin controlled by the AI?
    {
        get; set;
    }


    public float MoveProgress   // Used in Lerp() when the penguin walks from one tile to another
    {
        get; set;
    }


    public GameTile CurrentTile
    {
        get { return m_currentTile; }
        set { m_currentTile = value; }
    }


    public PenguinColor color
    {
        get
        {
            return m_color;
        }

        set
        {
            m_color = value;
        }
    }


    public Color currentColor
    {
        get { return m_rend.material.color; }
        set { m_rend.material.color = value; }
    }



    // Set location to move penguin to
    //
    public Vector3 DesiredDestination
    {
        get
        {
            return m_v3DesiredDestination;
        }

        set
        {
            m_v3DesiredDestination = value;
        }
    }


    // Distance between origin and destination tiles
    //
    public float DesiredDistance
    {
        get
        {
            return Vector3.Distance(m_currentTile.transform.position, DesiredDestination);
        }
    }


    // State Machine attribs
    //
    public bool IsMoving    // Is penguin moving to another tile?
    {
        get
        {
            return m_StateMach.GetBool("IsMoving");
        }

        set
        {
            m_StateMach.SetBool("IsMoving", value);
        }
    }
    //
    public bool IsCentered    // After move, is penguin centered on its new tile?
    {
        get
        {
            return m_StateMach.GetBool("IsCentered");
        }

        set
        {
            m_StateMach.SetBool("IsCentered", value);
        }
    }
    //
    public float WalkSpeed   // Speed to run walk cycle anim (to match linear movement)
    {
        get
        {
            return m_StateMach.GetFloat("WalkSpeed");
        }

        set
        {
            m_StateMach.SetFloat("WalkSpeed", value);
        }
    }


    public float CurrentMoveSpeed   // Current move speed for penguin
    {
        get; set;
    }


    public List<GameTile> ValidMoveList
    {
        get
        {
            return m_validTiles;
        }
    }


    /// <summary>
    /// Formula to set penguin's move speed based on distance from target tile (in tiles)
    /// </summary>
    /// <param name="fDistance"></param>
    /// <returns></returns>
    /// 
    public float DistanceToMoveSpeed(float distance)
    {
        const float MaxDist = 10f;

        float distRatio = distance / MaxDist;

        return Mathf.Lerp(moveSpeed, moveSpMax, distRatio);
    }


    /// <summary>
    /// Formula to set speed to run walk cycle based on motion speed
    /// </summary>
    /// <param name="fMoveSpeed"></param>
    /// <returns></returns>
    /// 
    public static float MoveSpeedToWalkCylSpeed(float fMoveSpeed)
    {
        return fMoveSpeed * 2.0f;
    }


    public void InitPenguinData(PenguinColor color, GameTile startingTile, bool bAI)
    {
        m_color       = color;
        m_currentTile = startingTile;

        // Set AI status
        //
        IsAI = bAI;
    }


    /// <summary>
    /// Highlight penguin when clicked
    /// </summary>
    /// 
    public void Highlight(bool highlight)
    {
        if (highlight)  // Turn highlight on
        {
            currentColor = highlightColor;
        }
        else  // Turn highlight off
        {
            currentColor = m_normalColor;
        }
    }


    void OnMouseDown()
    {
        m_refGameMgr.OnPenguinSelected(this);
    }


    /// <summary>
    /// Select this penguin after mouse click
    /// </summary>
    /// <remarks>
    /// FLOW: OnMouseDown() => GameMgr::State_Play::OnClickPenguin() => OnSelect()
    /// </remarks>
    /// 
    public void OnSelect()
    {
        IsSelected = true;

        Highlight(true);
    }

 
    /// <summary>
    /// Unselect this penguin when a tile or another penguin has been selected
    /// </summary>
    /// <remarks>
    /// FLOW: <Tile>.OnMouseDown() => State_Selected::OnStateExit() => this.OnSelectOff()
    /// </remarks>
    /// 
    public void OnSelectOff()
    {
        IsSelected = false;

        Highlight(false);
    }


    /// <summary>
    /// Set penguin's destination
    /// </summary>
    /// 
    public bool SetDestination(GameTile destination)
    {
        if (m_currentTile == destination) return false;   // Don't actually move penguin
        //
        else
        {
            m_destinTile = destination;

            DesiredDestination = m_destinTile.transform.position;

            return true;
        }
    }


    /// <summary>
    /// Make the onscreen penguin move to the destination tile
    /// </summary>
    /// <param name="destinationTile">Selected tile to move to</param>
    /// <remarks>
    /// CALLED BY: GameManager::ExecuteMove_GameWorld()
    /// </remarks>
    /// <returns>TRUE if a legal move was successfully executed</returns>
    /// 
    public bool MoveTo(GameTile destinationTile)
    {
        // Make move only if this tile is in the list of legal moves compiled on Penguin click, 
        //  or if it was "pre-approved" by AI on its turn
        //
        if (SetDestination(destinationTile))   // Set penguin's destination
        {
            // Change "penguin" status for tiles both departed from and arrived at
            //
            CurrentTile.RemovePenguin();
            destinationTile.PlacePenguin(this);

            OnSelectOff();   // Turn off selection as we move

            // Launch "Moving" State
            //
            IsMoving = true;
        }

        return true;
    }


    /// <summary>
    /// Check that indicated destination tile is a valid tile the pengun can move to
    /// </summary>
    /// <param name="destinationTile">The deired destination Tile</param>
    /// <returns>TRUE if penguin can move there</returns>
    /// 
    public bool ValidateDestination(GameTile destinationTile)
    {
        return ValidMoveList.Contains(destinationTile);
    }


    void Start ()
    {
        m_refGameMgr = GameManager.GetGameManager();

        m_rend = GetComponentInChildren<Renderer>();  // Mesh is child of GamePenguin object

        m_StateMach = GetComponent<Animator>();   // State machine

        m_normalColor = currentColor;
    }

    

    void Update ()
    {
        // Penguin movement handled by the state machine
	}


    /// <summary>
    /// Called by Penguin's state machine every frame, as long as penguin is in a moving state
    /// </summary>
    /// <returns>TRUE to trigger tile sinking</returns>
    /// 
    public bool UpdatePenguinMove()
    {
        if (Vector3.Distance(transform.position, DesiredDestination) > margin)
        {
            float fStepMove = moveSpeed * Time.deltaTime;
            float fStepTurn = turnSpeed * Time.deltaTime;

            // Turn towards destination (so penguin will appear to be walking forward
            //
            Vector3 v3DesiredLook = DesiredDestination - transform.position;
            //
            float fAngleDiff = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(v3DesiredLook));
            //
            Quaternion qRot = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v3DesiredLook), fStepTurn);
            //
            transform.rotation = qRot;

            if (fAngleDiff < angleMargin)   // Start moving once we're facing the right direction
            {
                // Set walking speed
                //
                WalkSpeed = MoveSpeedToWalkCylSpeed(CurrentMoveSpeed);

                // Update progress
                //
                MoveProgress += fStepMove;

                // Move penguin towatds destination
                //
                transform.position = Vector3.Lerp(m_currentTile.transform.position, DesiredDestination, MoveProgress);

                return true;
            }
            else
            {
                // Move feet only slowly as we turn...
                //
                WalkSpeed = 0.5f;

                return false;
            }
        }
        else
        {
            // Set new current tile
            //
            m_currentTile = m_destinTile;

            // End of move -- Go to "J'adoube" state
            //
            IsCentered = false;
            IsMoving   = false;

            return false;
        }
    }


    /// <summary>
    /// While idle, subly re-adjust the penguin position so it is centralized on the tile
    /// </summary>
    /// <remarks>
    /// Called by state machine behaviour
    /// </remarks>
    /// 
    public void J_adoube()
    {
        const float marginAdj  = 0.05f;  // When this close, don't bother further adjusting
        const float marginWalk = 0.25f;  // When this close, stop running walk anim

        float fDist = Vector3.Distance(transform.position, m_currentTile.transform.position);

        CurrentMoveSpeed = DistanceToMoveSpeed(fDist) * 1.5f;

        if (fDist > marginAdj)
        {
            transform.position = Vector3.Lerp(transform.position, m_currentTile.transform.position, CurrentMoveSpeed * Time.deltaTime);

            if (fDist > marginWalk) WalkSpeed = MoveSpeedToWalkCylSpeed(CurrentMoveSpeed);
            //
            else WalkSpeed = 0.0f;
        }
        else   // End state
        {
            IsCentered = true;
        }
    }
}
