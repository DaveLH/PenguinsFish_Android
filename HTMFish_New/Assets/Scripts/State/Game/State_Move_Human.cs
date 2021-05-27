using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Move_Human : State_Game
{
    /// <summary>
    /// Respond to human clicking on a tile (Penguin must have been selected first)
    /// </summary>
    /// <param name="gm">GameManager</param>
    /// <param name="tile">Target tile (where penguin will move to)</param>
    /// 
    public override void OnTileClicked(Animator animator, GameTile tile)
    {
        if (s_refGM.CurrentPenguin != null)   // First check that a penguin has been selected to move
        {
            // Tell the penguin to move -- Handle winning fish -- Drop departed tile
            //
            s_refGM.ExecuteHumanMove(tile);

            // Unselect penguin selected in previous call to OnPenguinClicked()
            //
            s_refGM.ClearPenguinCurrent();
        }
    }

    /// <summary>
    /// Respond to player clicking on a penguin they want to move
    /// </summary>
    /// <param name="penguin"></param>
    /// 
    public override void OnPenguinClicked(GamePenguin penguin)
    {
        // If a penguin is already selected, unselect it
        //
        if (s_refGM.CurrentPenguin) s_refGM.ClearPenguinCurrent();

        // Evaluate clicked penguin 
        //
        if (s_refGM.CurrentPlayer.IsMyPenguin(penguin))
        {
            // Compute valid moves
            //
            s_refGM.FindLegalMoves(penguin);

            // If there are no valid moves, remove penguin
            //
            if (penguin.ValidMoveList.Count == 0)
            {
                s_refGM.RemoveUnmovablePenguin(penguin);
            }
            else   // Make clicked penguin the current penguin
            {
                s_refGM.MakePenguinCurrent(penguin);

                // Check if penguin is alone on an "island" of tiles and remove it if it is
                //
                bool bPenguinRemoved = s_refGM.TryRemovePenguin();

                if(!bPenguinRemoved) penguin.OnSelect();   // If penguin not removed, visually highlight it and wait for player to move
            } 
        }
    }

    // OnStateEnter -- Prompt for turn
    /*
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    
    }
    */
    // Check for a keypress -- Currently only "R" key is a valid command, and only if a penguin is selected
    //
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (s_refGM.CurrentPenguin) s_refGM.ChkRKey();
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
    //
    //}
}
