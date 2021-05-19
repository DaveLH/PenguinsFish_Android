using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game moves made by Human and AI Players
/// </summary>
/// 
public static class Command
{
    public abstract class GameMove    // Base class
    {
        public GamePenguin.PenguinColor playerColor;   // Player that's making this move

        public VPenguin penguin;  // Penguin to move

        public string fromTile;    // ID of Tile to move penguin from
        public string toTile;      // ID of Tile to place/move to
        public string prevTile;    // Where the penguin was before previous move

        // AI algorithmns give this move a score
        //
        public float HeuristicScore = 0;

        public abstract void PlayMove(AIBoardSim brd);   // Play a move, either from user input, or AI trying out a move 
        public abstract void RollBack(AIBoardSim brd);   // Roll back move (Done "mentally" by AI)

        // Method to use for sorting by heuristic score
        //
        public int CompareTo(GameMove move2)
        {
            // A null value means that this move is "Higher" in score
            //
            if (move2 == null) return 1;
            //
            else
                return HeuristicScore.CompareTo(move2.HeuristicScore);
        }
    }


    // Basic Penguin Move
    //
    public class MovePenguin : GameMove
    {
        public override void PlayMove(AIBoardSim brd)
        {
            brd.MovePenguin(penguin, toTile);
        }

        public override void RollBack(AIBoardSim brd)
        {
            brd.UnmovePenguin(penguin, prevTile);
        }

        public MovePenguin(VPenguin pen, GamePenguin.PenguinColor player, string destTile, string priorTile)
        {
            playerColor = player;

            penguin  = pen;
            toTile   = destTile;
            prevTile = priorTile;
            fromTile = pen.currTile;
        }

        public override string ToString()
        {
            return "'Move from " + fromTile + " to " + toTile + "'";
        }
    }

    // Remove Penguin (and tiles if necessary) from board
    //
    public class RemovePenguin : GameMove
    {
        public bool RemoveMultiTiles { get; set; }  // Check for "island" removal, or just tile penguin is standing on?

        public RemovePenguin(VPenguin pen, GamePenguin.PenguinColor player)
        {
            playerColor = player;
            penguin     = pen;
        }

        public override void PlayMove(AIBoardSim brd)
        {
            brd.RemovePenguin(penguin);
        }

        public override void RollBack(AIBoardSim brd)
        {
            brd.RestorePenguin(penguin);
        }
    }

}
