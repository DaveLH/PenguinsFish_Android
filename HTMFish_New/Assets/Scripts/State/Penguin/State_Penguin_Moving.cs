using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Penguin_Moving : StateMachineBehaviour
{
    GamePenguin m_thisPenguin;   // The penguin affected by this controller

    float m_fTotalDistance;   // Total distance penguin will travel on this move

    bool m_bTileSink = true;   // Flag to trigger tile sink only once

    TurnKey m_key;   // Only used for "windup" AI penguin

    // Initialize penguin's movement
    //
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_thisPenguin = animator.GetComponent<GamePenguin>();

        m_bTileSink = true;

        m_thisPenguin.MoveProgress = 0.0f;   // Start progress from one tile to another

        m_fTotalDistance = m_thisPenguin.DesiredDistance;   // Store distance

        // Set motion speed for penguin
        //
        m_thisPenguin.CurrentMoveSpeed = m_thisPenguin.DistanceToMoveSpeed(m_fTotalDistance);

        // Set key script to animate if this is an AI penguin
        //
        if (m_thisPenguin.IsAI)
        {
            m_key = m_thisPenguin.GetComponent<TurnKey>();

            m_key.KeyAnimating = true;
        }
        
        Debug.Log("Distance on this move: " + m_fTotalDistance);
	}

    // Update the penguin as it moves
    //
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Move penguin on board
        //
        bool bSinkFlag = m_thisPenguin.UpdatePenguinMove();   

        if(bSinkFlag && m_bTileSink)   // Trigger sinking of tile
        {
            m_bTileSink = false;

            GameManager.GetGameManager().OnTriggerTileSink();
        }
    }

    // All we do here right now is stop the key for AI penguin
    //
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (m_key) m_key.KeyAnimating = false;
    }
}
