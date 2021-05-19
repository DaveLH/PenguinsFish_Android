using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface to State Machine
/// </summary>
/// 
public partial class GameManager : MonoBehaviour
{
    Animator m_stateMach;   // The state machine

    State_Game m_currentState;   // Interface to current Game state

    public State_Game CurrentState
    {
        get { return m_currentState; }
        set
        {
            m_currentState = value;
        }
    }

    // Placing penguins on board at start of game
    //
    public bool StateParam_PlacingPenguins
    {
        get { return m_stateMach.GetBool("PlacingPenguins"); }
        set { m_stateMach.SetBool("PlacingPenguins", value); }
    }

    // The Main Game is in progress
    //
    public bool StateParam_MainGame
    {
        get { return m_stateMach.GetBool("MainGame"); }
        set { m_stateMach.SetBool("MainGame", value); }
    }

    // It's a human player's turn to move
    //
    public bool StateParam_HumanMove
    {
        get { return m_stateMach.GetBool("HumanMove"); }
        set { m_stateMach.SetBool("HumanMove", value); }
    }

    // It's the computer's turn to move
    //
    public bool StateParam_AIMove
    {
        get { return m_stateMach.GetBool("AIMove"); }
        set { m_stateMach.SetBool("AIMove", value); }
    }

    // A penguin is animated -- Executing a move
    //
    public bool StateParam_MovePending
    {
        get { return m_stateMach.GetBool("MovePending"); }
        set { m_stateMach.SetBool("MovePending", value); }
    }

    // A penguin was removed from game -- Go directly to next player
    //
    public bool StateParam_PenguinRemoved
    {
        get { return m_stateMach.GetBool("PenguinRemoved"); }
        set { m_stateMach.SetBool("PenguinRemoved", value); }
    }

    // Trigger transition to next player
    //
    public void StateTrigger_NextPlayer()
    {
        m_stateMach.SetTrigger("NextPlayer");
    }

    // Trigger final tallying of scores and transition to end screen
    //
    public void StateTrigger_EndTheGame()
    {
        m_stateMach.SetTrigger("GameEnd");
    }
}
