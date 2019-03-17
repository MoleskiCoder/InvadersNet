// <copyright file="Program.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var game = new Cabinet())
            {
                game.Run();
            }
        }
    }
}
