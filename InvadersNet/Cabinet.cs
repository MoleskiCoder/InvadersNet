﻿// <copyright file="Cabinet.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    public class Cabinet : Game
    {
        private const float ScreenRotation = -(float)System.Math.PI / 2f;

        private readonly GraphicsDeviceManager graphics;
        private readonly ColourPalette palette = new ColourPalette();
        private readonly Color[] pixels = new Color[DisplayWidth * DisplayHeight];
        private readonly Color[] gel = new Color[DisplayWidth * DisplayHeight];
        private readonly List<Keys> pressedKeys = new List<Keys>();
        private readonly Dictionary<PlayerIndex, GamePadButtons> pressedButtons = new Dictionary<PlayerIndex, GamePadButtons>();
        private readonly Vector2 texturePosition = new Vector2(0, DisplayWidth * PixelSize);
        private readonly Vector2 textureOrigin = new Vector2(0, 0);
        private readonly float textureScale = PixelSize;
        private readonly SpriteEffects flipped = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
        private readonly SpriteEffects unflipped = SpriteEffects.None;
        private readonly SoundEffects sounds = new SoundEffects();

        private SpriteBatch spriteBatch;
        private Texture2D bitmapTexture;

        private bool disposed = false;

        public Cabinet()
        {
            this.graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
            };
            this.Content.RootDirectory = Configuration.ContentRoot;
        }

        public Board Motherboard { get; } = new Board();

        private static int DisplayWidth => (int)Board.RasterSize.Width;

        private static int DisplayHeight => (int)Board.RasterSize.Height;

        private static int PixelSize => Board.PixelSize;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.bitmapTexture?.Dispose();
                    this.spriteBatch?.Dispose();
                    this.graphics?.Dispose();
                    this.sounds?.Dispose();
                }

                this.disposed = true;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Motherboard.Initialize();
            this.Motherboard.RaisePOWER();
            this.ConnectSoundEvents();
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.bitmapTexture = new Texture2D(this.GraphicsDevice, DisplayWidth, DisplayHeight);
            this.ChangeResolution(DisplayHeight, DisplayWidth); // Note the reversed layout: -90 degree rotation of display
            this.palette.Load();
            this.CreateGelPixels();

            this.pressedButtons[PlayerIndex.One] = new GamePadButtons();
            this.pressedButtons[PlayerIndex.Two] = new GamePadButtons();

            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / Configuration.FramesPerSecond);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            this.sounds.LoadContent(this.Content);
            this.Motherboard.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            this.CheckGamePads();
            this.CheckKeyboard();
            this.DrawFrame();
            this.bitmapTexture.SetData(this.pixels);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            this.DisplayTexture();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            this.Motherboard.LowerPOWER();
        }

        private void ConnectSoundEvents()
        {
            this.Motherboard.UfoSound += this.Motherboard_UfoSound;
            this.Motherboard.ShotSound += this.Motherboard_ShotSound;
            this.Motherboard.PlayerDieSound += this.Motherboard_PlayerDieSound;
            this.Motherboard.InvaderDieSound += this.Motherboard_InvaderDieSound;
            this.Motherboard.ExtendSound += this.Motherboard_ExtendSound;

            this.Motherboard.Walk1Sound += this.Motherboard_Walk1Sound;
            this.Motherboard.Walk2Sound += this.Motherboard_Walk2Sound;
            this.Motherboard.Walk3Sound += this.Motherboard_Walk3Sound;
            this.Motherboard.Walk4Sound += this.Motherboard_Walk4Sound;
            this.Motherboard.UfoDieSound += this.Motherboard_UfoDieSound;

            this.Motherboard.EnableAmplifier += this.Motherboard_EnableAmplifier;
            this.Motherboard.DisableAmplifier += this.Motherboard_DisableAmplifier;
        }

        private void Motherboard_DisableAmplifier(object sender, System.EventArgs e) => this.sounds.Disable();

        private void Motherboard_EnableAmplifier(object sender, System.EventArgs e) => this.sounds.Enable();

        private void Motherboard_UfoDieSound(object sender, System.EventArgs e) => this.sounds.PlayUfoDie();

        private void Motherboard_Walk4Sound(object sender, System.EventArgs e) => this.sounds.PlayWalk4();

        private void Motherboard_Walk3Sound(object sender, System.EventArgs e) => this.sounds.PlayWalk3();

        private void Motherboard_Walk2Sound(object sender, System.EventArgs e) => this.sounds.PlayWalk2();

        private void Motherboard_Walk1Sound(object sender, System.EventArgs e) => this.sounds.PlayWalk1();

        private void Motherboard_ExtendSound(object sender, System.EventArgs e) => this.sounds.PlayExtend();

        private void Motherboard_InvaderDieSound(object sender, System.EventArgs e) => this.sounds.PlayInvaderDie();

        private void Motherboard_PlayerDieSound(object sender, System.EventArgs e) => this.sounds.PlayPlayerDie();

        private void Motherboard_ShotSound(object sender, System.EventArgs e) => this.sounds.PlayShot();

        private void Motherboard_UfoSound(object sender, System.EventArgs e) => this.sounds.PlayUfo();

        private void CheckGamePads()
        {
            this.MaybeHandleGamePadOne();
            this.MaybeHandleGamePadTwo();
        }

        private void MaybeHandleGamePadOne()
        {
            var capabilities = GamePad.GetCapabilities(PlayerIndex.One);
            if (capabilities.IsConnected && (capabilities.GamePadType == GamePadType.GamePad))
            {
                this.HandleGamePadOne();
            }
        }

        private void HandleGamePadOne()
        {
            var state = GamePad.GetState(PlayerIndex.One);
            var current = state.Buttons;
            var previous = this.pressedButtons[PlayerIndex.One];

            // Fire button

            if (current.A == ButtonState.Pressed && (previous.A == ButtonState.Released))
            {
                this.Motherboard.PressShoot1P();
            }

            if (current.A == ButtonState.Released && (previous.A == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseShoot1P();
            }

            // Left button

            if (current.LeftShoulder == ButtonState.Pressed && (previous.LeftShoulder == ButtonState.Released))
            {
                this.Motherboard.PressLeft1P();
            }

            if (current.LeftShoulder == ButtonState.Released && (previous.LeftShoulder == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseLeft1P();
            }

            // Right button

            if (current.RightShoulder == ButtonState.Pressed && (previous.RightShoulder == ButtonState.Released))
            {
                this.Motherboard.PressRight1P();
            }

            if (current.RightShoulder == ButtonState.Released && (previous.RightShoulder == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseRight1P();
            }

            this.pressedButtons[PlayerIndex.One] = current;
        }

        private void MaybeHandleGamePadTwo()
        {
            var capabilities = GamePad.GetCapabilities(PlayerIndex.Two);
            if (capabilities.IsConnected && (capabilities.GamePadType == GamePadType.GamePad))
            {
                this.HandleGamePadTwo();
            }
        }

        private void HandleGamePadTwo()
        {
            var state = GamePad.GetState(PlayerIndex.Two);
            var current = state.Buttons;
            var previous = this.pressedButtons[PlayerIndex.Two];

            // Fire button

            if (current.A == ButtonState.Pressed && (previous.A == ButtonState.Released))
            {
                this.Motherboard.PressShoot2P();
            }

            if (current.A == ButtonState.Released && (previous.A == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseShoot2P();
            }

            // Left button

            if (current.LeftShoulder == ButtonState.Pressed && (previous.LeftShoulder == ButtonState.Released))
            {
                this.Motherboard.PressLeft2P();
            }

            if (current.LeftShoulder == ButtonState.Released && (previous.LeftShoulder == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseLeft2P();
            }

            // Right button

            if (current.RightShoulder == ButtonState.Pressed && (previous.RightShoulder == ButtonState.Released))
            {
                this.Motherboard.PressRight2P();
            }

            if (current.RightShoulder == ButtonState.Released && (previous.RightShoulder == ButtonState.Pressed))
            {
                this.Motherboard.ReleaseRight2P();
            }

            this.pressedButtons[PlayerIndex.Two] = current;
        }

        private void CheckKeyboard()
        {
            var state = Keyboard.GetState();
            var current = new HashSet<Keys>(state.GetPressedKeys());

            var newlyReleased = this.pressedKeys.Except(current);
            this.UpdateReleasedKeys(newlyReleased);

            var newlyPressed = current.Except(this.pressedKeys);
            this.UpdatePressedKeys(newlyPressed);

            this.pressedKeys.Clear();
            this.pressedKeys.AddRange(current);
        }

        private void UpdatePressedKeys(IEnumerable<Keys> keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.D1:
                        this.Motherboard.Press1P();
                        break;
                    case Keys.D2:
                        this.Motherboard.Press2P();
                        break;
                    case Keys.D3:
                        this.Motherboard.PressCredit();
                        break;
                    case Keys.Z:
                        this.Motherboard.PressLeft1P();
                        break;
                    case Keys.X:
                        this.Motherboard.PressRight1P();
                        break;
                    case Keys.OemPipe:
                        this.Motherboard.PressShoot1P();
                        break;
                    case Keys.OemComma:
                        this.Motherboard.PressLeft2P();
                        break;
                    case Keys.OemPeriod:
                        this.Motherboard.PressRight2P();
                        break;
                    case Keys.OemQuestion:
                        this.Motherboard.PressShoot2P();
                        break;
                }
            }
        }

        private void UpdateReleasedKeys(IEnumerable<Keys> keys)
        {
            foreach (var key in keys)
            {
                switch (key)
                {
                    case Keys.D1:
                        this.Motherboard.Release1P();
                        break;
                    case Keys.D2:
                        this.Motherboard.Release2P();
                        break;
                    case Keys.D3:
                        this.Motherboard.ReleaseCredit();
                        break;
                    case Keys.Z:
                        this.Motherboard.ReleaseLeft1P();
                        break;
                    case Keys.X:
                        this.Motherboard.ReleaseRight1P();
                        break;
                    case Keys.OemPipe:
                        this.Motherboard.ReleaseShoot1P();
                        break;
                    case Keys.OemComma:
                        this.Motherboard.ReleaseLeft2P();
                        break;
                    case Keys.OemPeriod:
                        this.Motherboard.ReleaseRight2P();
                        break;
                    case Keys.OemQuestion:
                        this.Motherboard.ReleaseShoot2P();
                        break;
                }
            }
        }

        private void DrawFrame()
        {
            this.Motherboard.RunVerticalBlank();

            for (var y = 0; y < DisplayHeight; ++y)
            {
                if (y == 96)
                {
                    this.Motherboard.TriggerInterruptScanLine96();
                }

                this.Motherboard.RunScanLine();
                this.DrawScanLine(y);
            }

            this.Motherboard.TriggerInterruptScanLine224();
        }

        private void DisplayTexture()
        {
            var flip = Configuration.CocktailTable && this.Motherboard.CocktailModeControl;
            var effect = flip ? this.flipped : this.unflipped;

            this.spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            this.spriteBatch.Draw(this.bitmapTexture, this.texturePosition, null, Color.White, ScreenRotation, this.textureOrigin, this.textureScale, effect, 1);
            this.spriteBatch.End();
        }

        private void DrawScanLine(int y)
        {
            var bytesPerScanLine = DisplayWidth >> 3;
            var address = (ushort)(y * bytesPerScanLine);
            for (var byteX = 0; byteX < bytesPerScanLine; ++byteX)
            {
                var video = this.Motherboard.VRAM.Peek(address++);
                for (var bit = 0; bit < 8; ++bit)
                {
                    var x = (byteX << 3) + bit;
                    var mask = 1 << bit;
                    var pixel = video & mask;
                    var index = x + (y * DisplayWidth);
                    this.pixels[index] = pixel == 0 ? Color.Black : this.gel[index];
                }
            }
        }

        private void CreateGelPixels()
        {
            for (var y = 0; y < DisplayHeight; ++y)
            {
                for (var x = 0; x < DisplayWidth; ++x)
                {
                    var colourIndex = (int)ColourPalette.CalculateColour(y, DisplayWidth - x - 1);
                    this.gel[x + (y * DisplayWidth)] = this.palette.Colour(colourIndex);
                }
            }
        }

        private void ChangeResolution(int width, int height)
        {
            this.graphics.PreferredBackBufferWidth = PixelSize * width;
            this.graphics.PreferredBackBufferHeight = PixelSize * height;
            this.graphics.ApplyChanges();
        }
    }
}
