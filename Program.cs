using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.Authentication.ExtendedProtection;
using System.Threading;
using System.Timers;
using Microsoft.VisualBasic.FileIO;

namespace Program {
    class Game {
        static ConsoleKey key;

        static void Main(string[] args) {
            // Setup external terminal
            Console.Title = "Tetris";
            Console.SetWindowSize(22, 23);
            Console.SetBufferSize(22, 23);

            Dictionary<char, Shape> shapes = new Dictionary<char, Shape>();

            #region Shapes
                shapes.Add('I', new Shape([
                        [ new(1, 0), new(1, -1), new(1, -2), new(1, -3) ],
                        [ new(-1, -1), new(0, -1), new(1, -1), new(2, -1) ],
                        [ new(0, 0), new(0, -1), new(0, -2), new(0, -3) ],
                        [ new(-1, -2), new(0, -2), new(1, -2), new(2, -2) ]
                    ], ConsoleColor.Cyan
                ));
                shapes.Add('J', new Shape([
                        [ new(-1, 0), new(0, 0), new(0, -1), new(0, -2) ],
                        [ new(-1, -2), new(-1, -1), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(0, -1), new(0, -2), new(1, -2) ],
                        [ new(-1, -1), new(0, -1), new(1, -1), new(1, 0) ]
                    ], ConsoleColor.Blue
                ));
                shapes.Add('L', new Shape([
                        [ new(1, 0), new(0, 0), new(0, -1), new(0, -2) ],
                        [ new(-1, 0), new(-1, -1), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(0, -1), new(0, -2), new(-1, -2) ],
                        [ new(-1, -1), new(0, -1), new(1, -1), new(1, -2) ]
                    ], ConsoleColor.DarkYellow
                ));
                shapes.Add('O', new Shape([
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ]
                    ], ConsoleColor.Yellow
                ));
                shapes.Add('S', new Shape([
                        [ new(-1, 0), new(0, 0), new(0, -1), new(1, -1) ],
                        [ new(0, -1), new(0, 0), new(1, 0), new(1, 1) ],
                        [ new(-1, 1), new(0, 1), new(0, 0), new(1, 0) ],
                        [ new(0, 1), new(0, 0), new(-1, 0), new(-1, -1) ]
                    ], ConsoleColor.Green
                ));
                shapes.Add('Z', new Shape([
                        [ new(1, 0), new(0, 0), new(0, -1), new(-1, -1) ],
                        [ new(1, -1), new(0, 0), new(1, 0), new(0, 1) ],
                        [ new(-1, 0), new(0, 1), new(0, 0), new(1, 1) ],
                        [ new(-1, 1), new(0, 0), new(-1, 0), new(0, -1) ]
                    ], ConsoleColor.Red
                ));
            #endregion

            List<Block> placed = new List<Block>();
            Piece piece = new Piece(shapes['Z'], placed);
            Stopwatch watch = new Stopwatch();
            Random rand = new Random();
            bool drop = false;

            piece.UpdatePos(new Vector2(4, 0));
            watch.Start();

            while(key != ConsoleKey.Escape) {
                // Input
                Console.CursorVisible = false;
                Thread inputThread = new Thread(Input);
                inputThread.Start();

                
                if (!drop) switch(key) {
                    case ConsoleKey.Spacebar:
                        watch.Reset();
                        drop = true;
                        break;

                    case ConsoleKey.A:
                        piece.Rotate(-1);
                        break;

                    case ConsoleKey.D:
                        piece.Rotate(1);
                        break;

                    case ConsoleKey.DownArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.Y == 19)) break;
                        if (piece.UpdatePos(new Vector2(0, 1))) piece.UpdatePos(new Vector2(0, -1));
                        watch.Restart();
                        break;
                        
                    case ConsoleKey.LeftArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.X == 0)) break;
                        if (piece.UpdatePos(new Vector2(-1, 0))) piece.UpdatePos(new Vector2(1, 0));
                        break;
                        
                    case ConsoleKey.RightArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.X == 9)) break;
                        if (piece.UpdatePos(new Vector2(1, 0))) piece.UpdatePos(new Vector2(-1, 0));
                        break;
                }
                key = ConsoleKey.None;

                // Drop piece
                if (drop || watch.Elapsed.Seconds >= 1) {
                    do {
                        if (piece.UpdatePos(new Vector2(0, 1)) ||
                        piece.blocks[piece.rotation].Any(b => b.pos.Y == 20)) {
                            piece.UpdatePos(new Vector2(0, -1));
                            drop = false;
                            foreach(Block block in piece.blocks[piece.rotation])
                                placed.Add(block);
                            piece = new Piece(shapes.ElementAt(rand.Next(0, shapes.Count)).Value, placed);
                            piece.UpdatePos(new Vector2(4, 0));
                        }
                    } while(drop);

                    watch.Restart();
                }

                // Draw
                Console.Clear();
                Draw(piece, placed);
                Thread.Sleep(1);
            }
        }

        static void Draw(Piece piece, List<Block> placed) {
            // Draw frame
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, 0);
            Console.Write("╔════════════════════╗");
            for(int i = 1; i <= 20; i++) {
                Console.Write('║');
                Console.SetCursorPosition(Console.WindowWidth-1, i);
                Console.Write('║');
            }
            Console.Write("╚════════════════════╝");
            
            // Render game components
            piece.Draw();
            foreach(Block block in placed)
                block.Draw();
        }

        static void Input() {
            if (Console.KeyAvailable)
                key = Console.ReadKey(true).Key;
        }
    }

    struct Shape(Vector2[][] _placements, ConsoleColor _color) {
        public Vector2[][] placements = _placements;
        public ConsoleColor color = _color;
    }

    class Piece {
        // Create a copy of the jagged arr
        public Block[][] blocks = new Block[4][];
        public int rotation = 0;
        private List<Block> placed;

        public Piece(Shape shape, List<Block> _placed) {
            placed = _placed;

            for(int i = 0; i < 4; i++) {
                blocks[i] = new Block[shape.placements[0].Length];
                for(int j = 0; j < shape.placements[0].Length; j++)
                    blocks[i][j] = new Block(shape.placements[i][j], shape.color);
            }
        }

        public void Draw() {
            foreach(Block block in blocks[rotation])
                block.Draw();
        }

        public void Rotate(int rot) {
            rotation += rot;
            rotation = rotation < 0? 3: rotation > 3? 0: rotation;

            foreach(Block block in blocks[rotation])
                if (block.pos.Y <= 19 && block.pos.X >= 0 && block.pos.X <= 9 && !placed.Any(p => p.pos.Equals(block.pos))) return;

            Rotate(-rot);
        }

        public bool UpdatePos(Vector2 offset) {
            bool collided = false;
            for (int i = 0; i < 4; i++)
                foreach(Block block in blocks[i]) {
                    block.pos += offset;
                    if (i == rotation && !collided && placed.Any(p => p.pos.Equals(block.pos)))
                        collided = true;
                }
            return collided;
        }
    }

    class Block(Vector2 pos, ConsoleColor color)
    {
        public Vector2 pos = pos;
        readonly private ConsoleColor color = color;

        public void Draw() {
            if (pos.Y < 0) return;
            Console.SetCursorPosition((int)(1 + pos.X * 2), (int)(1 + pos.Y));
            Console.ForegroundColor = color;
            Console.Write("██");
        }
    }
}