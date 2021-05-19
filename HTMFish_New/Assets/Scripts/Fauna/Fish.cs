using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script to handle acquisitoon of fish (Destruction with particle effect) when penguin wins it
/// </summary>
/// 
public class Fish : MonoBehaviour
{	
	// Run "gathering" of fish (disappear from game)
    //
	public void TakeFish(ParticleSystem partSys)
    {
        // Particles passed as an arg, but currently effect is set to auto-"Play on Awake"

        Destroy(gameObject);
	}
}
