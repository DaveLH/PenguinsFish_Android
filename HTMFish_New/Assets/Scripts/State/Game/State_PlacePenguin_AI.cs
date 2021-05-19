using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PlacePenguin_AI : State_Game
{
    // Make the AI Move automatically
    //
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        // Instantiate new penguin on random one-fish tile
        //
        s_refGM.PlacePenguinAI(s_refGM.CurrentPlayer);

        // Trigger transition to next player (Well, that was a fun millisecond-or-so...)
        //
        animator.SetTrigger("NextPlayer");
    }

    // OnStateExit -- See SubStateMach_PlacePenguins (Same for both Human and AI states)
}
