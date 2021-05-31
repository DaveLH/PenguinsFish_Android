using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SubStateMach_PlacePenguins : StateMachineBehaviour
{
    GameManager m_refGM;   // Access to the Game Manager

    // OnStateMachineEnter is called when entering a statemachine via its Entry Node
    //  Prepare for first phase of game -- Placing penguins on the board
    //
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        m_refGM = GameManager.GetGameManager();

        m_refGM.SetSelectMask(GameManager.Layers.Tiles);  // Initially only allow clicking on tiles while placing penguins

        m_refGM.InitPlayers();                         // Fetch player info from scene controller
        m_refGM.SendMessage("OnEnableOneFishTiles");   // TileManager enables tiles
        m_refGM.GoToStartingPlayer();                  // GameManager starts rotating players' turns  

        // Set up AI and Copy "snapshot" of initial condition of the Game board (tile layout + starting player) to internal AI board
        //  and initialize AI method to use
        //
        if (m_refGM.IsUsingAI)   // "IsUsingAI" is set above, in InitPlayers()
        {
            m_refGM.GetComponent<AIBoardSim>().FetchInitialBoard();
            m_refGM.SetAIMethodName();
        }
    }

    // When all penguins are placed, disable all tiles
    //
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        // Disable tile selection (must select penguin first)
        //
        m_refGM.SendMessage("OnDisableAllTiles");  // Method in TileManager
        //
        m_refGM.SetSelectMask(GameManager.Layers.None); 

        // TEMP: To Debug adding penguins to internal board
        //
        if (m_refGM.IsUsingAI) Debug.Log(m_refGM.GetComponent<AIBoardSim>().ToString());
    }

    // Update prompt to place penguins
    //
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_refGM.UpdateTurnText(m_refGM.CurrentPlayer.ToString() + ": Place a penguin.");
    }

    // OnStateExit -- Test if Penguin placement complete and if so, exit state machine (Go to Main Game Play)
    //
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // If all players have to right number of penguins, tell state machine to branch to Main Game Play
        //
        if (m_refGM.EveryoneGotTheirPenguins)
        {
            animator.SetBool("MainGame", true);
            animator.SetBool("PlacingPenguins", false);
        }
        // Else advance to next player to place penguin
    }
}
