using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// From Start Screen, display Playing instructions
/// </summary>
/// 
public class HowToPlay : MonoBehaviour
{
	public void OnOpenPanel()
    {
        gameObject.SetActive(true);
    }
	
	
	void Update ()
    {
        /* This was for the Windows version
         * 
        if(Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
        {
            gameObject.SetActive(false);
        }
		*/
	}


    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
