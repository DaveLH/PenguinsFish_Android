using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_PlacePenguin_Human : State_Game
{
    public override void OnTileClicked(Animator animator, GameTile tile)
    {
        // In game setup, create a new penguin for the current player at the tile clicked
        //
        s_refGM.CurrentPlayer.AddPenguin(s_refGM, tile, false);

        // Trigger transition to next player
        //
        animator.SetTrigger("NextPlayer");
    }

    // OnStateEnter -- Does not override base

    // OnStateExit -- See SubStateMach_PlacePenguins (Same for both Human and AI states)
}
