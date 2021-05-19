using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handle the dialog box that displays progress while the AI's MiniMax algortithm is running
/// </summary>
/// 
public class AIProgressDlg : MonoBehaviour
{
    Slider m_AIProgressSlider;

    GameManager m_refGM;

    AIBrain m_AI;   // Ref to MiniMax algorithms

    // Number of iterations (plys) MiniMax is using
    //  (Translates to fraction of progress bar)
    //
    protected int m_numIters = 0;

    public float ProgressMin
    {
        get { return m_AIProgressSlider.minValue; }
    }

    public float ProgressMax
    {
        get { return m_AIProgressSlider.maxValue; }
    }

    public float ProgressCurrent
    {
        get { return m_AIProgressSlider.value; }
        set { m_AIProgressSlider.value = value; }
    }

    public float ProgressRange
    {
        get { return ProgressMax - ProgressMin; }
    }

    // Progress bar advances for every ply it analizes
    //
    public float ProgressUnit
    {
        get { return ProgressRange / m_numIters; }
    }

    // Set progress bar to starting
    //
    public void SetStart(int numIters)
    {
        m_numIters = numIters;

        m_AIProgressSlider.value = 0.00f;
    }

    public void IncSlider()
    {
        m_AIProgressSlider.value += ProgressUnit;
    }


    void Awake ()
    {
        m_refGM = GameManager.GetGameManager();

        m_AI = m_refGM.GetComponent<AIBrain>();
    }

    void OnEnable()
    {
        m_AIProgressSlider = GetComponentInChildren<Slider>();
    }
}
