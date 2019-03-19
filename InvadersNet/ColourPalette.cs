// <copyright file="ColourPalette.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    using Microsoft.Xna.Framework;

    public class ColourPalette
    {
        private readonly Color[] colours = new Color[4];

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

        public void Load()
        {
            this.colours[0] = Color.Black;
            this.colours[1] = Color.White;
            this.colours[2] = Color.Red;
            this.colours[3] = Color.Green;
        }
    }
}
