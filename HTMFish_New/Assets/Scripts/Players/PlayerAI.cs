using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents player the computer is controller through AI
/// </summary>
/// 
public class PlayerAI : Player
{
    /// <summary>
    /// Display this message to say whose turn it is...
    /// </summary>
    /// 
    public override string TurnMessage
    {
        get { return PlayerName + " (Computer) is Thinking..."; }
    }

    // Is this player computer-controlled?
    //
    public override bool IsAI
    {
        get { return true; }   // Computer player
    }


    protected override void Awake()
    {
        IsActive   = true;
        IsMyTurn   = IsStartingPlayer = false;  // Temporary defaults
        IsThinking = false;
    }


    /// <summary>
    /// Tell state machine to go to AI execution
    /// </summary>
    /// <param name="gm"></param>
    /// 
    public override void SetStatePlayer(GameManager gm)
    {
        gm.StateParam_HumanMove = false;
        gm.StateParam_AIMove    = true;
    }


    /// <summary>
    /// Start AI finding a move
    /// </summary>
    /// <remarks>
    /// FLOW: Game_State_Play::OnAI() -> AIBrain::OnLaunchAIThread()
    /// Or: Game_State_Init::OnAI() -> GameMgr::PlacePenguinAIRnd()
    /// </remarks>
    /* 
    public override IEnumerator WaitTurn()
    {
        m_refGM.CurrentState.OnAI(m_refGM, this);  // Launch AI making a move

        yield return base.WaitTurn();
    }
    */

    // Disallow user clicking on computer AI's penguins
    //
    public override bool IsMyPenguin(GamePenguin penguin)
    {
        return false;
    }


    public override string ToString()
    {
        return base.ToString() + " (AI)";
    }
}
