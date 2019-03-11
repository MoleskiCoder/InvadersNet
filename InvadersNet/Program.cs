// <copyright file="Program.cs" company="Adrian Conlon">
// Copyright (c) Adrian Conlon. All rights reserved.
// </copyright>

namespace Invaders
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new Configuration();
            using (var game = new Cabinet(configuration))
            {
                game.Run();
            }
        }
    }
}
