using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Do housekeeping as a scene is loaded and unloaded -- Basically make scenes themselves act like States
/// </summary>
/// <remarks>
/// StartScene () handles scene startup; End () cleans up before scene is unloaded
/// OWNER: An empty object in scene
/// </remarks>
/// 
public abstract class GameScene : MonoBehaviour
{
    bool m_bSceneRunning = false;   // True if scene is running Updates ()

    protected SceneController m_refSC;    // Reference to the persistent Scene controller
	

	protected void Start ()
    {
        // Init Ref. to S.C.
        //
        m_refSC = SceneController.GetSceneController();

        StartScene();

        m_bSceneRunning = true;
    }


    protected virtual void StartScene()
    {
        // Custom startup
    }


    protected void Update ()
    {
		if(m_bSceneRunning)
        {
            UpdateScene();    // Do any updates
        }
        // Else ending...
	}


    protected virtual void UpdateScene()
    {
        // Abort a game with <ESC>
        //
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscape();
        }
    }


    /// <summary>
    /// What to do if Escape key is pressed
    /// </summary>
    /// <remarks>Default behaviour is to abort game, whereever it is, and go back to starting screen
    /// (If on the starting, exit game all together)</remarks>
    /// 
    protected virtual void OnEscape()
    {
        // Go back to Start Screen
        //
        m_refSC.ResetSC();
    }


    /// <summary>
    /// Call this to make scene do cleanup before being unloaded
    /// </summary>
    /// 
    public void OnExitScene()
    {
        m_bSceneRunning = false;
    } 
}
