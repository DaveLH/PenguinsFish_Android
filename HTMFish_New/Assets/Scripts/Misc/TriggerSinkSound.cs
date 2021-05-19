using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trigger sound effect when tile sinks
/// </summary>
/// 
public class TriggerSinkSound : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision!");

        if (other.tag == "AnimTile")  // Animated sinking tile
        {
            Debug.Log("Collision!");

            // Look for sound clip (should be in root of tile object -- i.e. parent of "Art" object that h as the collider
            //
            AudioSource sound = other.GetComponentInParent<AudioSource>();

            // If found, play it
            //
            sound.Play();
        }
    }
}
