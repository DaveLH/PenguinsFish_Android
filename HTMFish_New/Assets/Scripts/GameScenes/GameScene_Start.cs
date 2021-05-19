using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Housekeeping on Start Scene 
/// </summary>
/// 
public class GameScene_Start : GameScene
{
    /*
    // Links to checkboxes
    //
    public Toggle checkboxBlue;
    public Toggle checkboxRed;
    public Toggle checkboxGreen;
    public Toggle checkboxGold;
    */

    // Links to player dropdowns on starting screen
    //
    [SerializeField] protected Dropdown dropdnBlue;
    [SerializeField] protected Dropdown dropdnRed;
    [SerializeField] protected Dropdown dropdnGreen;
    [SerializeField] protected Dropdown dropdnGold;

    // Link to PLAY button
    //
    [SerializeField] protected Button playButton;

    // Keep track of number of selected players
    //
    protected int m_numPlayers = 0;


    protected override void StartScene()
    {
        // Un restart, Display previous settings
        //
        UpdateDropdowns();
    }


    protected override void OnEscape()
    {
        Debug.Log("Quit Game...");

        Application.Quit();   // Exit app
    }


    /// <summary>
    /// Turn PLAY button on and off (enabled only if selected players is 2 or more, and there's at least one human player)
    /// </summary>
    /// 
    public void SetPlayBtn()
    {
        m_numPlayers = 0;

        int numHumanPlayers = 0;

        foreach (GamePenguin.PenguinColor c in m_refSC.PlayersActiveStatus.Keys)
        {
            if (m_refSC.PlayersActiveStatus[c] != GameManager.PlayerStatus.Inactive)
            {
                m_numPlayers++;

                if (m_refSC.PlayersActiveStatus[c] != GameManager.PlayerStatus.AI) numHumanPlayers++;
            }

            if (m_numPlayers > 1 && numHumanPlayers > 0)   // Stop when valid number of selected players found
            {
                playButton.interactable = true;

                return;
            }
        }

        playButton.interactable = false;
    }


    /// <summary>
    /// When user makes a dropdown selection
    /// </summary>
    /// 
    public void OnChgDropdownBlue()
    {
        OnChgDropdown(dropdnBlue, GamePenguin.PenguinColor.Blue);
    }
    //
    public void OnChgDropdownRed()
    {
        OnChgDropdown(dropdnRed, GamePenguin.PenguinColor.Red);
    }
    //
    public void OnChgDropdownGreen()
    {
        OnChgDropdown(dropdnGreen, GamePenguin.PenguinColor.Green);
    }
    //
    public void OnChgDropdownGold()
    {
        OnChgDropdown(dropdnGold, GamePenguin.PenguinColor.Gold);
    }
    //
    public void OnChgDropdown(Dropdown ddobj, GamePenguin.PenguinColor col)
    {
        GamePenguin.PenguinColor col2;

        // Update which colors are selected, and player status
        //
        m_refSC.PlayersActiveStatus[col] = (GameManager.PlayerStatus)ddobj.value;

        // Repeatedly scan current selections until all inconsistencies are removed
        //
        while(true)
        {
            col2 = ValidateDropdown(col);

            if (col2 == GamePenguin.PenguinColor._None) break;

            m_refSC.PlayersActiveStatus[col2] = GameManager.PlayerStatus.Human;
        }

        // Update display after validation
        //
        UpdateDropdowns();

        // Set status of play button, based on if two or more players are enabled
        //
        SetPlayBtn();
    }


    /// <summary>
    /// Verify values selected in dropdowns and remove inconsistencies (i.e. More than 1 "Youngest" or "AI")
    /// </summary>
    /// <param name="lastChanged">
    /// The player color the user just changed.  This is assumed to be the choice the user wants, and any
    ///  previous selection of "AI" or "Youngest" is what gets changed.
    /// </param>
    /// <returns>
    /// Returns color value that is redundant ("AI" or "Youngest") and should be changed to "Human" (because we can have any number of human players)
    /// </returns>
    /// <remarks>
    /// (This requires its own method/loop because you cannot modify a container
    ///   while it's being looped through -- So we have to break and restart whenever
    ///   a problem is found.)
    /// </remarks>
    /// 
    GamePenguin.PenguinColor ValidateDropdown(GamePenguin.PenguinColor lastChanged)
    {
        GamePenguin.PenguinColor lastAIFound = GamePenguin.PenguinColor._None;
        GamePenguin.PenguinColor lastYgFound = GamePenguin.PenguinColor._None;

        foreach (GamePenguin.PenguinColor c in m_refSC.PlayersActiveStatus.Keys)
        {
            if (m_refSC.PlayersActiveStatus[c] == GameManager.PlayerStatus.Youngest)
            {
                if (lastYgFound != GamePenguin.PenguinColor._None)  // One was previously found
                {
                    return (lastYgFound == lastChanged) ? c : lastYgFound;
                }
                else lastYgFound = c;
            }
            else if (m_refSC.PlayersActiveStatus[c] == GameManager.PlayerStatus.AI)
            {
                if (lastAIFound != GamePenguin.PenguinColor._None)  // One was previously found
                {
                    return (lastAIFound == lastChanged) ? c : lastAIFound;
                }
                else lastAIFound = c;
            }
        }

        return GamePenguin.PenguinColor._None;   // No issues -- Validation can end
    }


    /// <summary>
    /// Update display after validation has changed a value
    /// </summary>
    /// 
    void UpdateDropdowns()
    {
        dropdnBlue.value  = (int)m_refSC.PlayersActiveStatus[GamePenguin.PenguinColor.Blue];
        dropdnRed.value   = (int)m_refSC.PlayersActiveStatus[GamePenguin.PenguinColor.Red];
        dropdnGreen.value = (int)m_refSC.PlayersActiveStatus[GamePenguin.PenguinColor.Green];
        dropdnGold.value  = (int)m_refSC.PlayersActiveStatus[GamePenguin.PenguinColor.Gold];
    }


    /// <summary>
    /// When user clicks "Play" button
    /// </summary>
    /// <remarks>
    /// User should not be able to click on PLAY if fewer than two player colors have been selected
    /// </remarks>
    /// 
    public void OnClickPlay()
    {
        // Start transition to game play
        //
        m_refSC.TransitionTo(SceneController.SceneID.Game);
    }


    /// <summary>
    /// When user clicks "X" button
    /// 
    public void OnClickQuit()
    {
        OnEscape();
    }
}
