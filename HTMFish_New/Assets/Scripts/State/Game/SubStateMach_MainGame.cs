using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sub-State Machine running for the duration of the main Game
/// </summary>
/// 
public class SubStateMach_MainGame : StateMachineBehaviour
{
    GameManager m_refGM;

    // Initialize main game play
    //
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        m_refGM = GameManager.GetGameManager();

        // Unselect any Tile or Penguin from previous state
        //
        m_refGM.ClearPenguinCurrent();

        // Disable tile selection (must select penguin first)
        //
        m_refGM.SendMessage("OnDisableAllTiles");  // Method in TileManager

        // Set first player
        //
        m_refGM.GoToStartingPlayer();    // Branches to a method in PlayerManager
    }
    
    // Update prompt to display turn prompt
    //
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(!m_refGM) m_refGM = GameManager.GetGameManager();

        m_refGM.UpdateCurrentTurnText();

        // TEMP: To Debug results of last move on internal board
        //
        if (m_refGM.IsUsingAI) Debug.Log(m_refGM.GetComponent<AIBoardSim>().ToString());
    }
}
