using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manage the GUI showing turns and scores
/// </summary>
/// 
public class Scorekeeper : MonoBehaviour
{
    // Text in GUI
    //
    public Text textWhoseTurn;
    public Text textBlueScore;
    public Text textRedScore;
    public Text textGreenScore;
    public Text textGoldScore;

    Dictionary<GamePenguin.PenguinColor, Text> m_scoreGUIMap = new Dictionary<GamePenguin.PenguinColor, Text>();


    public void SetTurnDisplay(string msg)
    {
        textWhoseTurn.text = msg;
    }


    public void SetScoreDisplay(GamePenguin.PenguinColor color, int score)
    {
        Text guiText = null;

        if (m_scoreGUIMap.ContainsKey(color)) guiText = m_scoreGUIMap[color];
        
        if(guiText) guiText.text = score.ToString();
    }


    void Start()
    {
        // Build Color <=> GUI Text Map
        //
        m_scoreGUIMap.Add(GamePenguin.PenguinColor.Blue,  textBlueScore);
        m_scoreGUIMap.Add(GamePenguin.PenguinColor.Gold,  textGoldScore);
        m_scoreGUIMap.Add(GamePenguin.PenguinColor.Red,   textRedScore);
        m_scoreGUIMap.Add(GamePenguin.PenguinColor.Green, textGreenScore);
    }
}
