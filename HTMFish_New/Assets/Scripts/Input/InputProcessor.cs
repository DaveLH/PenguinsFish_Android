using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapter class to get input from various platforms and generate platform-independent events
/// </summary>
/// 
public class InputProcessor : MonoBehaviour
{
    private PenguinPointAndClick m_inputActions;    // User input actions to process

    private GameManager m_refGM;

    private void Awake()
    {
        m_inputActions = new PenguinPointAndClick();

        m_refGM = GameManager.GetGameManager();
    }

    private void Start()
    {
        // Register events
        //
        m_inputActions.MainGamePlay.SelectGameObject.performed += _ => ProcessInput();
    }

    private void OnEnable()
    {
        m_inputActions.Enable();
    }

    private void OnDisable()
    {
        m_inputActions.Disable();
    }

    /// <summary>
    ///  Listen for "clicking" on an object in the scene (not necessarily a mouse click)
    ///  and return the screen position the "click" happened at
    /// </summary>
    // public abstract bool ListenClick(out Vector3 v3ClickPoint);

    /// <summary>
    /// Get a Raycast ray from the point on screen that was touched or clicked
    /// </summary>
    /// <param name="v3Pos"></param>
    /// <returns></returns>
    Ray GetRayFromInputPoint(Vector3 v3Pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(v3Pos);

        return ray;
    }


    /// <summary>
    /// Get object "hit" by user input
    /// </summary>
    /// <returns></returns>
    /// 
    public GameObject GetObjectHit(Vector3 v3ScreenPos, int layermask)
    {
        RaycastHit hit;

        Ray ray = GetRayFromInputPoint(v3ScreenPos);

        if (Physics.Raycast(ray, out hit, layermask))
        {
            return hit.collider.gameObject;
        }
        else return null;
    }


    /// <summary>
    /// Determine which GameObject (Penguin or Tile) was selected, based on user input
    /// </summary>
    public void ProcessInput()
    {
        Vector2 v2ScreenPos = m_inputActions.MainGamePlay.ScreenPosition.ReadValue<Vector2>();
        Vector3 v3ScreenPos = v2ScreenPos;

        // Did we "click" on a penguin or tile?
        //
        GameObject targetObject = GetObjectHit(v3ScreenPos, m_refGM.CurrentSelectMask);

        // If yes, notify it that it was selected
        //
        if (targetObject) targetObject.SendMessage("OnSelect");
    }
}
