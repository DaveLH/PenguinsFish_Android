using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a player in the game
/// </summary>
/// 
public abstract class Player : ScriptableObject
{
    // Will this player start?
    //  (Determined by player designated "Youngest" on start screen)
    //
    public bool IsStartingPlayer { get; set; } 

    // This player's penguins
    //
    List<GamePenguin> m_myPenguins = new List<GamePenguin>();

    int m_numFish = 0;    // Current score (Number of fish acquired)

    GamePenguin.PenguinColor m_color;  // Player's color

    // Is this player computer-controlled?
    //
    public abstract bool IsAI { get; }

    // Is it this player currently an active player?
    //
    public bool IsActive { get; set; }

    // Is it this player's move?
    //
    public bool IsMyTurn { get; set; }

    // Is game waiting for player (human or AI) to move?
    //
    public bool IsThinking { get; set; }

    // Number of penguins player has
    //
    public int NumPenguins
    {
        get { return m_myPenguins.Count; }
    }


    public GamePenguin.PenguinColor Color
    {
        get
        {
            return m_color;
        }

        set
        {
            m_color = value;
        }
    }


    public string PlayerName
    {
        get { return Color.ToString(); }
    }


    public int Score
    {
        get
        {
            return m_numFish;
        }
    }


    // Retrieve last penguin that was added to player's list
    //  (Used by AI to more intellegently place penguins at game start, so they're not
    //    too close together where the other players can easily "corner" them, and hasten
    //    an early exit of the AI from the game.)
    //
    public GamePenguin lastAddedPenguin
    {
        get
        {
            int count = m_myPenguins.Count;

            if (count == 0) return null;  // No penguins to retrieve
            //
            else return m_myPenguins[count - 1];
        }
    }


    /// <summary>
    /// Display this message to say whose turn it is...
    /// </summary>
    /// 
    public abstract string TurnMessage { get; }


    protected abstract void Awake();

    public abstract void SetStatePlayer(GameManager gm);


    /// <summary>
    /// Utility to iterate through all of player's penguins
    /// </summary>
    /// <returns></returns>
    /// 
    public IEnumerable GetPenguins()
    {
        foreach(GamePenguin penguin in m_myPenguins)
        {
            yield return penguin;
        }
    }


    /// <summary>
    /// Handle background behaviour while a move is in progress -- For a human player, this is just waiting for the user to 
    /// click on a valid penguin and tile.  But for an AI subclass, it launches the algorithmn (in a separate thread)
    /// to choose a move automatically.
    /// </summary>
    /// 
    public virtual IEnumerator WaitTurn()
    {
        Debug.Log(this.ToString() + " can move.");

        // Wait until "My Turn" flag is set to FALSE before switching turns
        //
        yield return new WaitUntil(TurnJustFinished);
    }
    //
    bool TurnJustFinished()
    {
        return !IsMyTurn;
    }


    /// <summary>
    /// Give player a penguin
    /// </summary>
    /// 
    /// <remarks>
    /// CALLED BY: SpawnNewPenguin()
    /// </remarks>
    /// 
    public void AddPenguin(GameManager gm, GameTile placePenguinHere, bool bAI)
    {
        GamePenguin newPenguin = gm.CreatePenguin(placePenguinHere, Color, bAI);

        // Add new penguin to player's penguin list
        //
        m_myPenguins.Add(newPenguin);

        // Tell AIBoard to add the penguin
        //
        GameManager.GetGameManager().SendMessage("OnAddPenguinAIBoard", newPenguin);
    }


    public void RemovePenguin(GamePenguin penguin)
    {
        if(m_myPenguins.Contains(penguin))  // Verify we have this penguin
        {
            m_myPenguins.Remove(penguin);   // Remove from list
        }
    }


    /// <summary>
    /// Add to player's score
    /// </summary>
    /// <param name="num">Number of fish acquired this move</param>
    /// 
    public void AddFish(int num)
    {
        m_numFish += num;

        Debug.Log(PlayerName + "'s score: " + m_numFish.ToString());
    }


    /// <summary>
    /// When this player is the only one left, get all tiles accessible from all penguins and compute final score
    /// </summary>
    /// <param name="player">The last remaining player ay end of game</param>
    /// 
    public void AddFinal(TileManager refTileMgr)
    {
        refTileMgr.workTiles.Clear();   // Clear "workspace" (just once because we are checking multiple penguins and must avoid counting shared tiles twice)

        int finalScore = 0;

        foreach (GamePenguin penguin in m_myPenguins)
        {
            finalScore += TileManager.CheckForIsland(refTileMgr, penguin.CurrentTile, refTileMgr.workTiles, false);
        }

        AddFish(finalScore);   // Add this tally to the final total
    }

    /*
    /// <summary>
    /// Player takes all the cumulative fish on multiple tiles!
    /// </summary>
    /// <param name="allMyFish">Total score from previously evaluated tiles</param>
    /// 
    public void WinAll(int fishScoreValue)
    {
        AddFish(fishScoreValue);
    }
    
     * Old Version
     public void WinAll(List<GameTile> allMyFish)
     {
        foreach(GameTile tile in allMyFish)
        {
            AddFish(tile.FishScoreValue);
        }
     }
    */


    /// <summary>
    /// Do I own input penguin?
    /// </summary>
    /// <param name="penguin">Penguin to test</param>
    /// 
    public virtual bool IsMyPenguin(GamePenguin penguin)
    {
        foreach(GamePenguin p in m_myPenguins)
        {
            if (p == penguin) return true;   // Penguin found in list!
        }

        return false;
    }


    public override string ToString()
    {
        return m_color.ToString();
    }
}
