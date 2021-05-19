using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All the Game State classes
/// </summary>
/// 
public static class GameState 
{
    /*** Game States ***/

    public abstract class BState
    {
        public abstract void OnStateEnter(GameManager gm);
        public abstract void OnStateExit(GameManager gm);
        public abstract void OnStateUpdate(GameManager gm);

        // Any preliminary stuff to start player's turn? (For AI, start calculating next move)
        //
        // public abstract void OnTurnStart(Player player);

        // When a move is ready to be executed
        //
        public abstract void OnTileClicked(GameManager gm, GameTile tile);

        // When user clicks on a penguin
        //
        public abstract void OnPenguinClicked(GameManager gm, GamePenguin penguin);

        // Update prompt for player to move
        //
        public abstract void OnUpdateTurnText(GameManager gm);

        /// <summary>
        /// Handle AI computing its next move
        /// </summary>
        /// <param name="gm">Game Manager</param>
        /// <param name="AIPlayer">Current computer-controlled player</param>
        /// 
        public abstract void OnAI(GameManager gm, Player AIPlayer);
    }


    // Setup Tiles
    /*
    public class State_SetupTiles : BState
    {
        public override void OnStateEnter(GameManager gm)
        {
            gm.SetupBoard();   // Set up the game board

            gm.TransitionTo(gm.state_Init);   // Initialize Players
        }

        public override void OnStateExit(GameManager gm)
        { }

        public override void OnStateUpdate(GameManager gm)
        { }

        public override void OnPenguinClicked(GameManager gm, GamePenguin penguin)
        { }

        public override void OnTileClicked(GameManager gm, GameTile tile)
        { }
    }
    */
    // Initialize players and place penguins 
    //
    public class State_Initialize : BState
    {
        public override void OnStateEnter(GameManager gm)
        {
            gm.InitPlayers();                         // Fetch player info from scene controller
            gm.SendMessage("OnEnableOneFishTiles");   // TileManager enables tiles
            gm.GoToStartingPlayer();                  // GameManager starts rotating players' turns  

            OnUpdateTurnText(gm);   // Print first turn display
        }

        public override void OnStateExit(GameManager gm)
        {
            // Tell other Managers that we're entering main game play
            //
            // gm.SendMessage("SetMainGameFlag", true);
        }

        public override void OnStateUpdate(GameManager gm)
        {
            if(gm.PenguinSetupDone)
            {
                // gm.TransitionTo(gm.state_Play);   // Start main game play
            }
            // Else continue placing penguins 
        }
    
        public override void OnPenguinClicked(GameManager gm, GamePenguin penguin)
        { }

        /// <summary>
        /// Execute a pending move -- This can be invoked by either a human clicking on a tile, or the AI computing its next move
        /// </summary>
        /// <param name="gm">GameManager</param>
        /// <param name="tile">Target tile (where penguin will be placed)</param>
        /// 
        public override void OnTileClicked(GameManager gm, GameTile tile)
        {
            // In game setup, create a new penguin for the current player at the tile clicked
            //
            gm.CurrentPlayer.AddPenguin(gm, tile, false);

            // Go to next player
            //
            gm.NextPlayer();    // Branches to a method in PlayerManager
        }

        /// <summary>
        /// Give AI a new Penguin and place it on a random (but legal) tile
        ///   (TODO: More intelligent penguin placement)
        /// </summary>
        /// <param name="gm"></param>
        /// <param name="AIPlayer"></param>
        /// 
        public override void OnAI(GameManager gm, Player AIPlayer)
        {
            gm.PlacePenguinAIRnd(AIPlayer);

            gm.NextPlayer();
        }

        /// <summary>
        /// Prompt to place penguins
        /// </summary>
        /// <param name="gm"></param>
        /// 
        public override void OnUpdateTurnText(GameManager gm)
        {
            gm.UpdateTurnText(gm.CurrentPlayer.ToString() + ": Place a penguin.");
        }
    }

   
    // Main Game Play: Set first player to move, then wait for them to start moving
    //
    public class State_Play : BState
    {
        public override void OnStateEnter(GameManager gm)
        {
            // Unselect any Tile or Penguin from previous state
            //
            gm.CurrentTile = null;
            gm.UnselectPenguin();

            // Disable tile selection (must select penguin first)
            //
            gm.SendMessage("OnDisableAllTiles");  // Method in TileManager

            // Set first player
            //
            gm.GoToStartingPlayer();    // Branches to a method in PlayerManager

            OnUpdateTurnText(gm);   // Print first turn display
        }

        public override void OnStateExit(GameManager gm)
        { }

        public override void OnStateUpdate(GameManager gm)
        {
            if (!gm.IsGameEnd)
            {
                // Check for player "resigning" a penguin
                //   (Everything else is handled elsewhere)
                //
                if (gm.CurrentPenguin) gm.ChkRKey();
            }
            else   // "End Game" flag 
            {
                // Game Over -- Goto "End Game" state and display the final scores
                //
                SceneController.GetSceneController().TransitionTo(SceneController.SceneID.EndScreen);
            }
        }


        /// <summary>
        /// Prompt to place penguins
        /// </summary>
        /// <param name="gm"></param>
        /// 
        public override void OnUpdateTurnText(GameManager gm)
        {
            gm.UpdateTurnText("It's " + gm.CurrentPlayer.ToString() + "'s Turn.");
        }


        /// <summary>
        /// Launch AI calculation of next move
        /// </summary>
        /// <param name="gm"></param>
        /// <param name="AIPlayer"></param>
        /// 
        public override void OnAI(GameManager gm, Player AIPlayer)
        {
            gm.SendMessage("OnLaunchAIThread");   // Method in AIBrain
        }


        public override void OnPenguinClicked(GameManager gm, GamePenguin penguin)
        {
            // If a penguin is already selected, unselect it
            //
            if (gm.CurrentPenguin) gm.UnselectPenguin();

            // Evaluate clicked penguin 
            //
            if (gm.CurrentPlayer.IsMyPenguin(penguin))
            {
                // Compute valid moves
                //
                gm.FindLegalMoves(penguin);

                // If there are no valid moves, remove penguin
                //
                if (penguin.ValidMoveList.Count == 0)
                {
                    gm.CurrentPlayer.AddFish(penguin.CurrentTile.FishScoreValue);   // Take fish on this tile

                    penguin.CurrentTile.Sink();   // Remove tile

                    // gm.OnExpungePenguin()   // Remove Penguin

                    // gm.NextPlayer();   // Go to next player
                }
                else   // Make clicked penguin the current penguin
                {
                    gm.CurrentPenguin = penguin;
                    gm.CurrentTile    = penguin.CurrentTile;

                    penguin.OnSelect();
                }
            }
        }

        /// <summary>
        /// Execute a pending move -- This can be invoked by either a human clicking on a tile, or the AI computing its next move
        /// </summary>
        /// <param name="gm">GameManager</param>
        /// <param name="tile">Target tile (where penguin will move to)</param>
        /// 
        public override void OnTileClicked(GameManager gm, GameTile tile)
        {
            if (gm.CurrentPenguin != null)
            {
                // Tell the penguin to move
                //
                if (gm.CurrentPenguin.MoveTo(tile))
                {
                    // Player acquires this tile's fish
                    //
                    gm.CurrentPlayer.AddFish(gm.CurrentTile.FishScoreValue);

                    // Start tile sinking effect
                    //
                    gm.CurrentTile.Sink();
                    
                    // Clear selections
                    //
                    gm.CurrentPenguin = null;
                    gm.CurrentTile = null;

                    // Clear selectability of tiles
                    //
                    gm.SendMessage("OnDisableAllTiles");

                    // Go to next player
                    //
                    gm.NextPlayer();    // Branches to a method in PlayerManager 
                }
            }
        }
    }


    // Ending game -- Collect final fish, transition to end screen where final scores are displayed.
    //
    public class State_EndGame : BState
    {
        public override void OnStateEnter(GameManager gm)
        {
            // Do the final scoring from tiles the remaining player can acquire
            //
            gm.RemoveFinal();

            // Transition to ending screen (display final scores)
            //
            SceneController.GetSceneController().TransitionTo(SceneController.SceneID.EndScreen);
        }

        public override void OnStateExit(GameManager gm)
        { }

        public override void OnStateUpdate(GameManager gm)
        { }

        public override void OnPenguinClicked(GameManager gm, GamePenguin penguin)
        { }

        public override void OnTileClicked(GameManager gm, GameTile tile)
        { }

        public override void OnAI(GameManager gm, Player AIPlayer)
        { }

        public override void OnUpdateTurnText(GameManager gm)
        { } 
    }
}
