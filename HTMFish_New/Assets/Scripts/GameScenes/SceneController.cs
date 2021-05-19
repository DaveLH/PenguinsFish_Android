using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Manage transitions from Start Screen => Game => End Screen
/// </summary>
/// 
public class SceneController : MonoBehaviour
{
    public enum SceneID { StartScreen = 1, Game = 2, EndScreen = 3 };

    public CanvasGroup faderScreen;   // Handle fading in and out

    static SceneController s_singleton;   // Unique instance

    GameScene m_currentGameScene = null;   // Interface to managment for each scene

    bool IsFading { get; set; }

    public float fadeLength = 1.0f;   // Duration of a fade in seconds

    public Camera defaultCamera;   // Camera to use in the split second between scenes, when we might otherwise get the "No Cameras Rendering" error

    const int MaxPlayerNum = 4;   // Maximum players in game

    [HideInInspector]   // Reflects what colors were checked on start screen
    //
    public Dictionary<GamePenguin.PenguinColor, 
                      GameManager.PlayerStatus> PlayersActiveStatus = new Dictionary<GamePenguin.PenguinColor, GameManager.PlayerStatus>();

    [HideInInspector]   // Directory of final scores (includes players who have resigned from game)
    //
    public Dictionary<GamePenguin.PenguinColor, int> AllPlayerScores = new Dictionary<GamePenguin.PenguinColor, int>();

    // State objects
    //
    // SceneState.State_StartScreen stateStart;
    // SceneState.State_Game        stateGame;
    // SceneState.State_EndScreen   stateEnd;


    /// <summary>
    /// Get interface to persistent Scene Controller
    /// </summary>
    /// 
    public static SceneController GetSceneController()
    {
        return s_singleton;
    }


    /// <summary>
    /// Get interface to the GameScene manager in current scene
    /// </summary>
    /// 
    public static GameScene GetGameScene()
    {
        return GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameScene>();
    }


    void Awake ()
    {
        s_singleton = this;

        // Initialize Player-active table
        //
        PlayersActiveStatus.Add(GamePenguin.PenguinColor.Blue,  GameManager.PlayerStatus.Inactive);
        PlayersActiveStatus.Add(GamePenguin.PenguinColor.Red,   GameManager.PlayerStatus.Inactive);
        PlayersActiveStatus.Add(GamePenguin.PenguinColor.Green, GameManager.PlayerStatus.Inactive);
        PlayersActiveStatus.Add(GamePenguin.PenguinColor.Gold,  GameManager.PlayerStatus.Inactive);
    }


    void Start ()
    {
        if(!faderScreen) faderScreen = GetComponentInChildren<CanvasGroup>();

        faderScreen.alpha = 1.0f;

        TransitionTo(SceneID.StartScreen);   // Load starting screen
	}


    /// <summary>
    /// Reset the scene controller when starting over
    /// </summary>
    /// 
    public void ResetSC()
    {
        AllPlayerScores.Clear();   // Erase old scores

        faderScreen.alpha = 1.0f;

        TransitionTo(SceneID.StartScreen);   // Return to starting screen
    }


    /// <summary>
    /// Transition to new Scene (as if it was a state)
    /// </summary>
    /// 
    public void TransitionTo(SceneID sceneID)
    {
        if (!IsFading)
        {
            StartCoroutine(DoSceneChange(sceneID));   // Start the scene transition
        }
    }


    IEnumerator DoSceneChange(SceneID scene)
    {
        // Run scene exit, except if this is the first scene in session
        //
        if (m_currentGameScene)
        {
            m_currentGameScene.OnExitScene();

            // Fade to black
            //
            yield return StartCoroutine(Fade(1.0f));
        }

        // Switch scenes
        //
        yield return StartCoroutine(SwapScenes(scene));

        // Fade in to scene
        //
        yield return StartCoroutine(Fade(0.0f));

        // New scene "state"
        //
        m_currentGameScene = GetGameScene();

        // m_currentGameScene.Start() handles startup stuff, so no need for an explicit call
    }


    IEnumerator SwapScenes(SceneID newScene)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (currentSceneIndex != 0)   // Don't unload scene controller scene, if it's currently the only one loaded (at startup) 
        {
            defaultCamera.enabled = true;   // Switch to default camera during scene transition (Otherwise, "No cameras rendering" error pops up)

            // Out with the old...
            //
            yield return SceneManager.UnloadSceneAsync(currentSceneIndex);   // Otherwise, swap out this scene for new one
        }
        
        // ... In with the new
        //
        yield return SceneManager.LoadSceneAsync((int)newScene, LoadSceneMode.Additive);

        Scene currentScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        SceneManager.SetActiveScene(currentScene);

        // Finally, set the camera
        //
        SetCamera();
    }


    IEnumerator Fade(float destAlpha)
    {
        // Start fade
        //
        IsFading = true;
        //
        faderScreen.blocksRaycasts = true;

        float fFadeSpeed = Mathf.Abs(faderScreen.alpha - destAlpha) / fadeLength;

        while(!Mathf.Approximately(faderScreen.alpha, destAlpha))
        {
            faderScreen.alpha = Mathf.MoveTowards(faderScreen.alpha, destAlpha, fFadeSpeed * Time.deltaTime);

            yield return null;
        }

        // End fade
        //
        IsFading = false;
        //
        faderScreen.blocksRaycasts = false;
    }


    /// <summary>
    /// Set camera to use in scene
    /// </summary>
    /// <remarks>
    /// CALLED BY: SwapScenes()
    /// </remarks>
    /// 
    public void SetCamera()
    {
        // Look for a "MainCamera" in scene just loaded
        //
        Camera mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        // If found, set it as active -- Otherwise use default
        //
        if (mainCamera != null)
        {
               mainCamera.enabled = true;
            defaultCamera.enabled = false;
        }
        else defaultCamera.enabled = true;
    }
}
