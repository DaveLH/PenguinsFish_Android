using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_NextPlayer : State_Game
{
    // OnStateEnter -- Advances to next player -- That's all this state does.
    //  It's basically only a state at all to ensure everything executes in the proper sequence.
    //
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        // Advance to next player
        //  (Also sets the state machine's params, according to if a human or the AI moves next,
        //    of if there's only one player left, transition to ending state)
        //
        s_refGM.NextPlayer();
    }
}
