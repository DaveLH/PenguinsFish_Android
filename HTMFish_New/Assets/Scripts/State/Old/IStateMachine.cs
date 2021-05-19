using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*** Interface for State Machine ***/

public interface IStateMachine
{
    // Called by MonoBehaviour->Update()
    //
    void UpdateState();   

    // Transition to a new state
    //
    void TransitionTo(IState newState);
}
