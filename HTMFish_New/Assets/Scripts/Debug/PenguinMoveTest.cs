using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinMoveTest : MonoBehaviour
{
    Animator m_StateMach;

    Vector3 m_v3Dest;   // Move Penguin here

    public float moveSpeed = 1.0f;

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
        if (Vector3.Magnitude(transform.position - destinationTile.transform.position) > 0.1f)   // Set penguin's destination, if it's not there already
        {
            // Launch "Moving" State
            //
            IsMoving = true;
        }

        return true;
    }


    /// <summary>
    /// Called by Penguin's state machine every frame, as long as penguin is in a moving state
    /// </summary>
    /// 
    public void UpdatePenguinMove()
    {
        const float margin = 0.05f;   // How close to get to desired point before stopping

        Vector3 v3DesiredDestination = transform.position;  // For testing -- This will become a member var

        if (Vector3.Distance(transform.position, v3DesiredDestination) > margin)
        {
            float step = moveSpeed * Time.deltaTime;

            // Turn towards destination (so penguin appears to be walking forward
            //
            Vector3 v3Dir = Vector3.RotateTowards(transform.forward, v3DesiredDestination, step, 0.0f);
            //
            transform.rotation = Quaternion.LookRotation(v3Dir);

            // Move penguin towatds destination
            //
            transform.position = Vector3.Lerp(transform.position, v3DesiredDestination, step);
        }
        else
        {
            // End of move -- Go to "J'adoube" state
            //
            IsCentered = false;
            IsMoving = false;
        }
    }


    void Start ()
    {
        m_StateMach = GetComponent<Animator>();	
	}
	
	
	void Update ()
    {
		
	}
}
