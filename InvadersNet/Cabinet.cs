// <copyright file="Cabinet.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Cabinet : Game
    {
        private readonly Configuration configuration;
        private readonly GraphicsDeviceManager graphics;
        private readonly ColourPalette palette = new ColourPalette();
        private readonly Color[] pixels = new Color[DisplayWidth * DisplayHeight];
        private SpriteBatch spriteBatch;
        private Texture2D bitmapTexture;

        private int cycles = 0;
        private int fps;
        private uint startTicks = 0;
        private uint frames = 0;
        private bool vsync = false;

        private bool disposed = false;

        public Cabinet(Configuration configuration)
        {
            this.configuration = configuration;
            this.Motherboard = new Board(configuration);
            this.graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false,
            };
        }

        public Board Motherboard { get; }

        private static int DisplayWidth => (int)Board.RasterSize.Height;

        private static int DisplayHeight => (int)Board.RasterSize.Width;

        private int PixelSize => this.Motherboard.PixelSize;

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
            this.palette.Load(this.GraphicsDevice);
            this.ChangeResolution(DisplayWidth, DisplayHeight);
            this.bitmapTexture = new Texture2D(this.GraphicsDevice, DisplayWidth, DisplayHeight);
            this.Motherboard.Initialize();
            this.Motherboard.RaisePOWER();
        }

        protected override void Update(GameTime gameTime)
        {
            this.RunFrame();
            base.Update(gameTime);
        }

        protected virtual void RunFrame()
        {
            this.cycles = this.DrawFrame(this.cycles);
        }

        private int DrawFrame(int prior)
        {
            var flip = this.configuration.CocktailTable && this.Motherboard.CocktailModeControl;
            var interlaced = this.configuration.Interlaced;

            var renderOdd = !interlaced || (interlaced && (this.frames % 2 == 1));
            var renderEven = !interlaced || (interlaced && (this.frames % 2 == 0));

            var blackColour = this.palette.Colour(ColourPalette.ColourIndex.Black);
            if (interlaced)
            {
                this.graphics.GraphicsDevice.Clear(this.palette.Colour(ColourPalette.ColourIndex.Black));
            }

            this.spriteBatch.Begin();
            try
            {
                // This code handles the display rotation
                var bytesPerScanLine = (int)Board.RasterSize.Width >> 3;
                for (var inputY = 0; inputY < (int)Board.RasterSize.Height; ++inputY)
                {
                    if (inputY == 96)
                    {
                        this.Motherboard.TriggerInterruptScanLine96();
                    }

                    prior = this.Motherboard.RunScanLine(prior);
                    var evenScanLine = inputY % 2 == 0;
                    var oddScanLine = !evenScanLine;
                    if (oddScanLine && !renderOdd)
                    {
                        continue;
                    }

                    if (evenScanLine && !renderEven)
                    {
                        continue;
                    }

                    var address = (ushort)(bytesPerScanLine * inputY);
                    var outputX = flip ? (int)Board.RasterSize.Height - inputY - 1 : inputY;
                    for (var byteX = 0; byteX < bytesPerScanLine; ++byteX)
                    {
                        var video = this.Motherboard.VRAM.Peek(address++);
                        for (var bit = 0; bit < 8; ++bit)
                        {
                            var inputX = (byteX << 3) + bit;
                            var outputY = flip ? inputX : (int)(Board.RasterSize.Width - inputX - 1);
                            var outputPixel = outputX + (outputY * DisplayWidth);
                            var mask = 1 << bit;
                            var inputPixel = video & mask;
                            if (interlaced)
                            {
                                if (inputPixel != 0)
                                {
                                    var colourIndex = (int)ColourPalette.CalculateColour(outputX, outputY);
                                    var colour = this.palette.Colour(colourIndex);
                                    this.pixels[outputPixel] = colour;
                                }
                            }
                            else
                            {
                                var colourIndex = (int)ColourPalette.CalculateColour(outputX, outputY);
                                var colour = inputPixel == 0 ? blackColour : this.palette.Colour(colourIndex);
                                this.pixels[outputPixel] = colour;
                            }
                        }
                    }
                }

                this.Motherboard.TriggerInterruptScanLine224();

                this.bitmapTexture.SetData(this.pixels);
                this.spriteBatch.Draw(this.bitmapTexture, new Rectangle(0, 0, DisplayWidth * this.PixelSize, DisplayHeight * this.PixelSize), this.palette.Colour(ColourPalette.ColourIndex.White));
            }
            finally
            {
                this.spriteBatch.End();
            }

            return this.Motherboard.RunVerticalBlank(prior);
        }

        private void ChangeResolution(int width, int height)
        {
            this.graphics.PreferredBackBufferWidth = this.PixelSize * width;
            this.graphics.PreferredBackBufferHeight = this.PixelSize * height;
            this.graphics.ApplyChanges();
        }
    }
}
