// <copyright file="ColourPalette.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class ColourPalette : IDisposable
    {
        private readonly Color[] colours = new Color[4];
        private readonly Texture2D[] pixels = new Texture2D[4];

        private bool disposed = false;

        public enum ColourIndex
        {
            Black,
            White,
            Red,
            Green,
        }

        public static ColourIndex CalculateColour(int x, int y)
        {
            if (y < 32)
            {
                return ColourIndex.White;
            }

            if (y < (32 + 32))
            {
                return ColourIndex.Red;
            }

            if (y < (32 + 32 + 120))
            {
                return ColourIndex.White;
            }

            if (y < (32 + 32 + 120 + 56))
            {
                return ColourIndex.Green;
            }

            if (x < 16)
            {
                return ColourIndex.White;
            }

            if (x < (16 + 118))
            {
                return ColourIndex.Green;
            }

            return ColourIndex.White;
        }

        public Color Colour(ColourIndex index) => this.Colour((int)index);

        public Color Colour(int index) => this.colours[index];

        public Texture2D Pixel(ColourIndex index) => this.Pixel((int)index);

        public Texture2D Pixel(int index) => this.pixels[index];

        public void Load(GraphicsDevice hardware)
        {
            this.colours[0] = Color.Black;
            this.colours[1] = Color.White;
            this.colours[2] = Color.Red;
            this.colours[3] = Color.Green;

            for (var i = 0; i < 4; ++i)
            {
                this.pixels[i] = new Texture2D(hardware, 1, 1);
                this.pixels[i].SetData<Color>(new Color[] { this.colours[i] });
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.pixels != null)
                    {
                        foreach (var pixel in this.pixels)
                        {
                            pixel.Dispose();
                        }
                    }
                }

                this.disposed = true;
            }
        }
    }
}
