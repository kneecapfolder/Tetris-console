using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;

namespace Program {
    class Game {
        static ConsoleKey key;

        static void Main(string[] args) {
            // Setup external terminal
            Console.Title = "Tetris";
            Console.SetWindowSize(20, 20);

            Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();

            #region Shapes
                shapes.Add("O", new Shape(
                    new bool[,,]{
                        {
                            {false, false, false, false},
                            {false, false, false, false},
                            {false, true, true, false},
                            {false, true, true, false}
                        },
                        {
                            {false, false, false, false},
                            {false, false, false, false},
                            {false, true, true, false},
                            {false, true, true, false}
                        },
                        {
                            {false, false, false, false},
                            {false, false, false, false},
                            {false, true, true, false},
                            {false, true, true, false}
                        },
                        {
                            {false, false, false, false},
                            {false, false, false, false},
                            {false, true, true, false},
                            {false, true, true, false}
                        }
                    }, ConsoleColor.Yellow
                ));
            #endregion

            Piece piece = new Piece(shapes["O"]);
            Block b = new(new(0, 0), ConsoleColor.Red);

            while(key != ConsoleKey.Escape) {
                // Input
                Console.CursorVisible = false;
                Thread inputThread = new Thread(Input);
                inputThread.Start();

                Console.Clear();

                piece.Draw();
                Thread.Sleep(1);
            }
        }

        static void Input() {
            if (Console.KeyAvailable)
                key = Console.ReadKey().Key;
        }
    }

    struct Shape(bool[,,] placements, ConsoleColor color)
    {
        public bool[,,] placements = placements;
        public ConsoleColor color = color;
    }

    class Piece {
        public List<List<Block>> blocks = new List<List<Block>>();
        public Vector2 pos = new Vector2(10, 0);

        public Piece(Shape shape) {
            for(int i = 0; i < 4; i++) {
                blocks.Add(new List<Block>());
                for(int y = 0; y < 4; y++)
                    for(int x = 0; x < 4; x++)
                        if (shape.placements[i, y, x])
                            blocks[i].Add(new Block(new Vector2(x, y), shape.color));
            }
        }

        public void Draw() {
            if (blocks.Count > 0)
                foreach(Block block in blocks[0])
                    block.draw();
        }
    }

    class Block {
        public Vector2 pos;
        private ConsoleColor color;

        public Block(Vector2 pos, ConsoleColor color) {
            this.pos = pos;
            this.color = color;
        }

        public void draw() {
            Console.SetCursorPosition((int)pos.X * 2, (int)pos.Y);
            Console.ForegroundColor = color;
            Console.Write("██");
        }
    }
}