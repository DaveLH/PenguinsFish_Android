using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a human player in the game
/// </summary>
/// 
public class PlayerHuman : Player
{
    /// <summary>
    /// Display this message to say whose turn it is...
    /// </summary>
    /// 
    public override string TurnMessage
    {
        get { return "It's " + PlayerName + "'s Turn"; }
    }

    // Is this player computer-controlled?
    //
    public override bool IsAI
    {
        get { return false; }   // Human player
    }


    protected override void Awake()
    {
        IsActive   = true;
        IsMyTurn   = IsStartingPlayer;
        IsThinking = false;
    }


    /// <summary>
    /// Tell state machine to go to human player
    /// </summary>
    /// <param name="gm"></param>
    /// 
    public override void SetStatePlayer(GameManager gm)
    {
        gm.StateParam_HumanMove = true;
        gm.StateParam_AIMove    = false;
    }


    public override string ToString()
    {
        return base.ToString() + " (Human)";
    }
}
