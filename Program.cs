﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

            Dictionary<string, Shape> shapes = new Dictionary<string, Shape>();

            #region Shapes
                shapes.Add("O", new Shape([
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(1, 0), new(0, -1), new(1, -1) ]
                    ], ConsoleColor.Yellow
                ));
                shapes.Add("I", new Shape([
                        [ new(1, 0), new(1, -1), new(1, -2), new(1, -3) ],
                        [ new(-2, -1), new(-1, -1), new(0, -1), new(1, -1) ],
                        [ new(0, 0), new(0, -1), new(0, -2), new(0, -1) ],
                        [ new(-1, -2), new(0, -2), new(1, -2), new(2, -2) ]
                    ], ConsoleColor.Cyan
                ));
            #endregion

            Piece piece = new Piece(shapes["I"]);
            piece.UpdatePos(new Vector2(4, 0));

            List<Block> placed = new List<Block>();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while(key != ConsoleKey.Escape) {
                // Input
                Console.CursorVisible = false;
                Thread inputThread = new Thread(Input);
                inputThread.Start();

                switch(key) {
                    case ConsoleKey.DownArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.Y == 19)) break;
                        piece.UpdatePos(new Vector2(0, 1));
                        watch.Restart();
                        break;
                        
                    case ConsoleKey.LeftArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.X == 0)) break;
                        piece.UpdatePos(new Vector2(-1, 0));
                        break;
                        
                    case ConsoleKey.RightArrow:
                        if (piece.blocks[piece.rotation].Any(block => block.pos.X == 9)) break;
                        piece.UpdatePos(new Vector2(1, 0));
                        break;
                }
                key = ConsoleKey.None;

                // Drop piece
                if (watch.Elapsed.Seconds >= 1) {
                    watch.Restart();
                    if (piece.blocks[piece.rotation].Any(block => block.pos.Y == 19)) {
                        foreach(Block block in piece.blocks[piece.rotation])
                            placed.Add(block);
                        piece = new Piece(shapes["I"]);
                        piece.UpdatePos(new Vector2(4, 0));
                    }
                    else piece.UpdatePos(new Vector2(0, 1));
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
            Console.Write('╔');
            for(int i = 0; i < Console.WindowWidth-2; i++)
                Console.Write('═');
            Console.Write('╗');

            for(int i = 1; i < Console.WindowWidth-1; i++) {
                Console.Write('║');
                Console.SetCursorPosition(Console.WindowWidth-1, i);
                Console.Write('║');
            }
            
            Console.Write('╚');
            for(int i = 0; i < Console.WindowWidth-2; i++)
                Console.Write('═');
            Console.Write('╝');
            
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

        public Piece(Shape shape) {
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

        public void UpdatePos(Vector2 offset) {
            foreach(Block block in blocks[rotation])
                block.pos += offset;
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