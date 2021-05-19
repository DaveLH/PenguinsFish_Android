using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Housekeeping on Start Scene 
/// </summary>
/// 
public class GameScene_Game : GameScene
{
    // GameManager m_refGameMgr;   // Ref to Game Manager

    protected override void StartScene()
    {
        // m_refGameMgr = GameManager.GetGameManager();
    }

    /// <summary>
    /// React to Player pressing "Quit" (Aborts current Game -- Same effect as "Escape" Key)
    /// </summary>
    /// 
    public void OnQuitButton()
    {
        m_refSC.ResetSC();
    }
}
