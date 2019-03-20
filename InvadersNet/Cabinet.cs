// <copyright file="Cabinet.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
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
        private readonly List<Keys> pressed = new List<Keys>();
        private readonly Vector2 texturePosition = new Vector2(0, DisplayWidth * PixelSize);
        private readonly Vector2 textureOrigin = new Vector2(0, 0);
        private readonly float textureScale = PixelSize;
        private readonly SpriteEffects flipped = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
        private readonly SpriteEffects unflipped = SpriteEffects.None;
        private SpriteBatch spriteBatch;
        private Texture2D bitmapTexture;

        private int cycles = 0;

        private bool disposed = false;

        public Cabinet() => this.graphics = new GraphicsDeviceManager(this)
        {
            IsFullScreen = false,
        };

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
                }

                this.disposed = true;
            }
        }

        protected override void LoadContent()
        {
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.palette.Load();
            this.ChangeResolution(DisplayHeight, DisplayWidth); // Note the reversed layout: -90 degree rotation of display
            this.bitmapTexture = new Texture2D(this.GraphicsDevice, DisplayWidth, DisplayHeight);
            this.CreateGelPixels();
            this.Motherboard.Initialize();
            this.Motherboard.RaisePOWER();
        }

        protected override void Update(GameTime gameTime)
        {
            this.RunFrame();
            this.CheckKeyboard();
            base.Update(gameTime);
        }

        protected virtual void RunFrame() => this.cycles = this.DrawFrame(this.cycles);

        private void CheckKeyboard()
        {
            var state = Keyboard.GetState();
            var current = new HashSet<Keys>(state.GetPressedKeys());

            var newlyReleased = this.pressed.Except(current);
            this.UpdateReleasedKeys(newlyReleased);

            var newlyPressed = current.Except(this.pressed);
            this.UpdatePressedKeys(newlyPressed);

            this.pressed.Clear();
            this.pressed.AddRange(current);
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

        private int DrawFrame(int prior)
        {
            for (var y = 0; y < DisplayHeight; ++y)
            {
                if (y == 96)
                {
                    this.Motherboard.TriggerInterruptScanLine96();
                }

                prior = this.Motherboard.RunScanLine(prior);
                this.PaintScanLine(y);
            }

            this.bitmapTexture.SetData(this.pixels);
            this.DrawTexture();

            this.Motherboard.TriggerInterruptScanLine224();
            return this.Motherboard.RunVerticalBlank(prior);
        }

        private void DrawTexture()
        {
            var flip = Configuration.CocktailTable && this.Motherboard.CocktailModeControl;
            var effect = flip ? this.flipped : this.unflipped;

            this.spriteBatch.Begin();
            this.spriteBatch.Draw(this.bitmapTexture, this.texturePosition, null, Color.White * 0.5f, ScreenRotation, this.textureOrigin, this.textureScale, effect, 1);
            this.spriteBatch.End();
        }

        private void PaintScanLine(int y)
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
