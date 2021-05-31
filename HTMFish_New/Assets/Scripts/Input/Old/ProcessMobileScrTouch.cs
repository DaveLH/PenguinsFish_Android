using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Process a screen touch on a mobile device
/// </summary>
/// 
/*
public class ProcessMobileScrTouch : InputProcessor
{
    public override bool ListenClick(out Vector3 v3ClickPoint)
    {
        v3ClickPoint = Vector3.zero;

        foreach (Touch touch in Input.touches)  // Scan for a "Begin touch"
        {
            if (touch.phase == TouchPhase.Began)
            {
                v3ClickPoint = touch.position;

                return true;
            }
        }

        return false;   // No new touches detected
    }
}
*/