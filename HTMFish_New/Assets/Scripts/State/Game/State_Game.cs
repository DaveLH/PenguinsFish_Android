using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for Game-related states -- Includes polymorphic methods to call from GameManager
/// </summary>
/// 
public abstract class State_Game : StateMachineBehaviour
{
    protected static GameManager s_refGM;   // Reference to GameManager, for anyone who needs it

    /*** State-dependant Handling of user input ***/

    // When a tile is clicked
    //
    public virtual void OnTileClicked(Animator animator, GameTile tile)
    {
        // Disable click
    }

    // When user clicks on a penguin
    //
    public virtual void OnPenguinClicked(GamePenguin penguin)
    {
        // Disable click
    }

    // When the "R" (Remove penguin) key is pressed
    //
    public virtual void OnPenguinRemove(GamePenguin penguin)
    {
        // Disable key
    }

    // On every state change, set reference to state in GameManager, and vice versa
    //  (Inherited classes overriding this should always call the base first.)
    //
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(s_refGM == null) s_refGM = GameManager.GetGameManager();

        s_refGM.CurrentState = this;
    }
}

