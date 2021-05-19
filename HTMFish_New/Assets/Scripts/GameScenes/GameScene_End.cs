using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Display final scores
/// </summary>
/// 
public class GameScene_End : GameScene
{
    public Text textWon;      // Big banner saying who won
    public Text textColors;   // List of colors
    public Text textScores;   // List of their scores

    /// <summary>
    /// Populate screen with final results
    /// </summary>
    /// 
    public void DisplayScores()
    {
        int highestScore = 0;

        bool bTie = false;

        GamePenguin.PenguinColor winningColor = GamePenguin.PenguinColor._None;

        textWon.text = "";
        textColors.text = "";
        textScores.text = "";

        for (GamePenguin.PenguinColor color = GamePenguin.PenguinColor.Red; color < GamePenguin.PenguinColor._None; color++)
        {
            if(m_refSC.AllPlayerScores.ContainsKey(color))   // Ignore colors not playing this session
            {
                int score = m_refSC.AllPlayerScores[color];

                textColors.text += color.ToString() + ":\n";
                textScores.text += score.ToString() +  "\n";

                // Determine who got the most fish
                //
                if (score > highestScore)   
                {
                    highestScore = score;
                    winningColor = color;

                    bTie = false;
                }
                else if(score == highestScore)
                {
                    bTie = true; 
                }
            }
        }

        // If this game ended in a tie for first place
        //   (TODO: To be entirely "According to Hoyle", if there's a tie in fish score, 
        //     the win should go to who won the most tiles.)
        //
        if (bTie)   
        {
            textWon.text = "This Game is a Draw!";
        }
        else textWon.text = winningColor.ToString() + " Won!";  
    }


    protected override void StartScene()
    { 
        DisplayScores();
	}
	

    /// <summary>
    /// "Quit" Button
    /// </summary>
    /// 
    public void OnBtn_Quit()
    {
        Debug.Log("Quit Game...");

        Application.Quit();
    }


    /// <summary>
    /// "Restart" Button
    /// </summary>
    /// 
    public void OnBtn_Restart()
    {
        // Go back to Start Screen
        //
        m_refSC.ResetSC();
    }
}
