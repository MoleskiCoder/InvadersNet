// <copyright file="Configuration.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    public static class Configuration
    {
        public static bool DebugMode { get; set; } = false;

        public static bool ProfileMode { get; set; } = false;

        public static bool DrawGraphics { get; set; } = true;

        public static int PixelSize { get; set; } = 2;

        public static bool ShowWatchdogOutput { get; set; } = false;

        public static int FramesPerSecond { get; set; } = 60;

        public static int CyclesPerFrame => CyclesPerSecond / FramesPerSecond;

        public static int CyclesPerVerticalBlank => CyclesPerFrame / 6;

        public static int CyclesPerRasterScan => CyclesPerFrame - CyclesPerVerticalBlank;

        public static int CyclesPerSecond { get; set; } = 2000000;

        public static bool CocktailTable { get; set; } = false;

        public static string ContentRoot { get; } = "../../";

        public static string RomDirectory { get; } = "roms/";

        public static string SoundDirectory { get; } = "sounds/";
    }
}
