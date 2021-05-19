using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_J_adoube : StateMachineBehaviour
{
    GamePenguin m_thisPenguin;   // The penguin affected by this controller

    // Create a link to the penguin
    //
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_thisPenguin = animator.GetComponent<GamePenguin>();
    }

    // Readjust the penguin
    //
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_thisPenguin.J_adoube();
    }

    // Alert Game that penguin move is ended (Triggers "NextPlayer" in Main Game State Machine)
    //
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameManager.GetGameManager().SendMessage("OnMoveEnd");
    }

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
