// <copyright file="Configuration.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    public class Configuration
    {
        public Configuration()
        {
        }

        public bool DebugMode { get; set; } = false;

        public bool ProfileMode { get; set; } = false;

        public bool DrawGraphics { get; set; } = true;

        public int PixelSize { get; set; } = 2;

        public bool ShowWatchdogOutput { get; set; } = false;

        public bool Interlaced { get; set; } = false;

        public bool VsyncLocked { get; set; } = true;

        public int FramesPerSecond { get; set; } = 60;

        public int CyclesPerFrame => this.CyclesPerSecond / this.FramesPerSecond;

        public int CyclesPerVerticalBlank => this.CyclesPerFrame / 6;

        public int CyclesPerRasterScan => this.CyclesPerFrame - this.CyclesPerVerticalBlank;

        public int CyclesPerSecond { get; set; } = 2000000;

        public bool CocktailTable { get; set; } = false;

        public string RomDirectory { get; } = @"..\..\..\roms";

        public string SoundDirectory { get; } = "sounds";
    }
}
