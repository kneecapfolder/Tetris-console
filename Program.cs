using System;
using System.Collections.Generic;
using System.Threading;

namespace Tetris {
    class Program {
        static ConsoleKey key;

        static void Main(string[] args) {
            // Setup external terminal
            Console.Title = "Tetris";
            Console.SetWindowSize(20, 20);
            Console.SetBufferSize(20, 20);

            while(key != ConsoleKey.Escape) {
                Thread inputThread = new Thread(input);
                Console.CursorVisible = false;
                inputThread.Start();
            }
        }

        static void input() {
            if (Console.KeyAvailable)
                key = Console.ReadKey().Key;
        }
    }
}