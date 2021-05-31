using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines an interface for objects in scene (Penguins and tiles) to work in conjunction with the 
/// Input adapter to generically respond to input
/// </summary>
/// 
public interface IInput 
{
    void OnSelect();   // Handle object getting selected (e.g. by a mouse click or finger touch)
}
