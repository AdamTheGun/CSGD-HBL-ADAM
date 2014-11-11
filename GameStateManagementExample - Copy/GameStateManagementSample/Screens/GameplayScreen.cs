#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace GameStateManagement
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont spriteFont;
        Texture2D TGrassBlock;
        Texture2D TBall;

        Vector2[] GroundBlockPos =  new Vector2[10];
        Vector2 BlockSpawnLocation;
        Vector2 PlayerBallPos;
        int GameSpeed;
        float BallMovementSpeed;
        int jumpCount = 0;
        int jumpCounter = 0;
        float soundCounter = 0;

        int WidthRatio,HeightRatio;
        float rotationValue = 0.0f;

        bool gameReady = false;
        float deltaTime;

        int ScreenWidth;
        int ScreenHeight;


        AudioEngine audioEngine;
        SoundBank soundBank;
        WaveBank waveBank;

        WaveBank musicWB;


        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            

        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "Content");


            spriteFont = content.Load<SpriteFont>("gamefont");
            TGrassBlock = content.Load<Texture2D>("Grass-Block");
            TBall = content.Load<Texture2D>("Ball");

            audioEngine = new AudioEngine("Content\\LifeOfBalls.xgs");
            waveBank = new WaveBank(audioEngine, "Content\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content\\Sound Bank.xsb");


            ScreenWidth = ScreenManager.WindowRect.Right;
            ScreenHeight = ScreenManager.WindowRect.Bottom;

            BlockSpawnLocation = new Vector2(ScreenWidth, ScreenHeight);
            for (int i = 0; i < GroundBlockPos.Length; i++) 
            {
                GroundBlockPos[i].X = BlockSpawnLocation.X;
                GroundBlockPos[i].Y = BlockSpawnLocation.Y - 100;
            }
            WidthRatio = (int)(Math.Truncate((float)(ScreenWidth / 8.53)));
            HeightRatio = (int)(Math.Truncate((float)(ScreenHeight/4.8)));
            PlayerBallPos = new Vector2(WidthRatio * 2, HeightRatio * 2);
            GameSpeed = 5;
            BallMovementSpeed = 5;
            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, false);
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (IsActive)
            {
                MoveGround();
                if (GroundBlockPos[0].X < 0) 
                {
                    gameReady = true;
                }
                if (gameReady) 
                {
                    rotationValue += 2.0f * deltaTime;
                    PlayerBallPos.Y += BallMovementSpeed;
                    if ((PlayerBallPos.Y+TBall.Height/4) >= GroundBlockPos[0].Y)
                    {
                        BallMovementSpeed = 0;
                        jumpCount = 0;
                        jumpCounter = 0;
                    }
                    else 
                    {
                        BallMovementSpeed += 0.2f;
                    }
                    if (!soundBank.IsInUse)
                    {
                        soundBank.GetCue("Music").Play();
                    }
                    if (soundCounter >= 5)
                    {
                        soundBank.GetCue("Ambience").Play();
                        soundCounter = 0;
                    }
                    soundCounter += 1 * deltaTime;


                    /*if (BallMovementSpeed != 5) 
                    {
                        BallMovementSpeed += 1 *(int)deltaTime;
                    }*/
                    audioEngine.Update();
                }

            }

        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                       input.GamePadWasConnected[playerIndex];

            if (input.IsPauseGame(ControllingPlayer) || gamePadDisconnected)
            {
                ScreenManager.AddScreen(new PauseMenuScreen(), ControllingPlayer);
                return;
            }
            else
            {
                
            }
            if ((PlayerBallPos.Y + TBall.Height / 2) >= GroundBlockPos[0].Y)
            {
                jumpCounter++;
                if (jumpCount == 0 && keyboardState.IsKeyDown(Keys.W))
                {   
                    BallMovementSpeed -= 5;
                    jumpCount++;
                }
            }
            if (jumpCount == 1 && keyboardState.IsKeyDown(Keys.W))
            {
                if (jumpCounter >= 8)
                {
                    BallMovementSpeed -= 5;
                    jumpCount++;
                }
            }
        }

        public void MoveGround() 
        {
            for (int i = 0; i < GroundBlockPos.Length; i++)
            {
                if (i == 0)
                {
                    GroundBlockPos[i].X -= GameSpeed;
                }
                else if (GroundBlockPos[i].X != BlockSpawnLocation.X)
                {
                    GroundBlockPos[i].X -= GameSpeed;
                }
                else if (GroundBlockPos[i - 1].X < ScreenWidth - 100)
                {
                    GroundBlockPos[i].X -= GameSpeed;
                }
                if (GroundBlockPos[i].X < -100)
                {
                    GroundBlockPos[i].X = BlockSpawnLocation.X;
                }
            }
            
        }
        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (gameReady)
                ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                                   Color.CornflowerBlue, 0, 0);
            else 
            {
                ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                                   Color.Black, 0, 0);
            }            
            ScreenManager.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            ScreenManager.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            ScreenManager.GraphicsDevice.BlendState = BlendState.Opaque;

        

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();
            if (gameReady)
            {
                spriteBatch.Draw(TBall,PlayerBallPos,null,
                                Color.White,rotationValue,new Vector2(TBall.Width/2,TBall.Height/2),0.5f,SpriteEffects.None,0);
                for (int i = 0; i < GroundBlockPos.Length; i++)
                {
                    //Added Math.Truncate with its screen ratios to make the textures scale appropriately to the game screen
                    spriteBatch.Draw(TGrassBlock, new Rectangle((int)GroundBlockPos[i].X, (int)GroundBlockPos[i].Y,WidthRatio,HeightRatio) , Color.White);
                }
            }
            //spriteBatch.DrawString(ScreenManager.Font, "Insert Gameplay Here", new Vector2(100, 100), Color.DarkRed);

            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0 )
            {
                ScreenManager.FadeBackBufferToBlack(1 - TransitionAlpha);
            }
        }


        #endregion
    }
}
