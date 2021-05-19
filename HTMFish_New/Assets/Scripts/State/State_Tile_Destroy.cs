using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State_Tile_Destroy : StateMachineBehaviour
{
	// Now in the last state -- Animation is over -- It's safe to delete tile "art" from scene
    //   (Though the empty root object remains so as not to screw up the tile navagation algorithm)
    //
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject tileArt = animator.gameObject;

        Destroy(tileArt);
	}
}
