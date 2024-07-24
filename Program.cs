using System.Diagnostics;
using System.Numerics;

namespace Program {
    sealed class Game {
        static Dictionary<char, Shape> shapes = new Dictionary<char, Shape>();
        static ConsoleKey key;
        static List<Block> placed = new List<Block>();
        static Shape next;
        static Piece piece;
        static int score = 0;

        public static void Main(string[] args) {
            // Setup external terminal
            Console.Title = "Tetris";
            Console.SetWindowSize(22, 24);
            Console.SetBufferSize(22, 24);

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
                shapes.Add('T', new Shape([
                        [ new(-1, 0), new(0, 0), new(1, 0), new(0, -1) ],
                        [ new(0, -1), new(0, 0), new(0, 1), new(1, 0) ],
                        [ new(-1, 0), new(0, 0), new(1, 0), new(0, 1) ],
                        [ new(0, -1), new(0, 0), new(0, 1), new(-1, 0) ]
                    ], ConsoleColor.Magenta
                ));
            #endregion

            Random rand = new Random();
            next = shapes.ElementAt(rand.Next(0, shapes.Count)).Value;
            piece = new Piece(shapes.ElementAt(rand.Next(0, shapes.Count)).Value, placed);
            Stopwatch watch = new Stopwatch();
            bool drop = false;

            piece.UpdatePos(new Vector2(4, 0));
            watch.Start();

            Draw();
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
                        score++;
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
                        if (drop) score += 2;
                        if (piece.UpdatePos(new Vector2(0, 1)) ||
                        piece.blocks[piece.rotation].Any(b => b.pos.Y == 20)) {
                            piece.UpdatePos(new Vector2(0, -1));
                            drop = false;
                            foreach(Block b in piece.blocks[piece.rotation])
                                placed.Add(b);
                            piece = new Piece(next, placed);
                            next = shapes.ElementAt(rand.Next(0, shapes.Count)).Value;
                            piece.UpdatePos(new Vector2(4, 0));
                            Complete();
                        }
                    } while(drop);

                    watch.Restart();
                }

                Draw();
                Thread.Sleep(1);
            }
        }

        static void Draw(bool showPreview = true) {
            // Draw frame
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, 0);
            Console.Write("╔════════════════════╗");
            for(int i = 1; i <= 20; i++) {
                Console.Write('║');
                Console.SetCursorPosition(Console.WindowWidth-1, i);
                Console.Write('║');
            }
            Console.Write("╚════════════════════╝");

            // Display stats
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"score: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(score);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("next: ");
            Console.ForegroundColor = next.color;
            Console.Write(shapes.FirstOrDefault(s => s.Value.Equals(next)).Key);

            // Render piece preview
            if (showPreview) {
                List<Block> preview = new List<Block>();
                foreach(Block b in piece.blocks[piece.rotation])
                    preview.Add(new Block(b.pos, b.color));
                while(!preview.Any(b => b.pos.Y == 20 || placed.Any(p => p.pos.Equals(b.pos))))
                    foreach(Block b in preview)
                        b.pos.Y++;
                foreach(Block b in preview) {
                    b.pos.Y--;
                    b.Draw("░░");
                }
            }

            Block.DrawAll(piece.blocks[piece.rotation]);
            Block.DrawAll(placed);
        }

        static void Complete() {
            if (placed.Any(b => b.pos.Y == 0)) {
                key = ConsoleKey.Escape;
                return;
            }

            List<int> yValues = new List<int>();

            for(int i = 0; i < 20; i++)
                if (placed.Count(b => b.pos.Y == i) >= 10)
                    yValues.Add(i);
            if (yValues.Count == 0) return;

            score += new int[]{100, 300, 500, 800}[yValues.Count-1];
            
            List<Block> doomed = new List<Block>();
            foreach(Block b in placed)
                if (yValues.Contains((int)b.pos.Y)) {
                    doomed.Add(b);
                    b.color = ConsoleColor.White;
                }
            Draw(false);
            
            Thread.Sleep(300);
            doomed.ForEach(b => placed.Remove(b));
            Draw(false);

            Thread.Sleep(300);
            foreach(int y in yValues)
                foreach(Block b in placed)
                    if (b.pos.Y < y)
                        b.pos.Y++;
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

    sealed class Piece {
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

        public void Rotate(int rot) {
            rotation += rot;
            rotation = rotation < 0? 3: rotation > 3? 0: rotation;

            foreach(Block b in blocks[rotation])
                if (b.pos.Y > 19 || b.pos.X < 0 || b.pos.X > 9 || placed.Any(p => p.pos.Equals(b.pos))) {
                    Rotate(-rot);
                    return;
                }
        }

        public bool UpdatePos(Vector2 offset) {
            bool collided = false;
            for (int i = 0; i < 4; i++)
                foreach(Block b in blocks[i]) {
                    b.pos += offset;
                    if (i == rotation && !collided && placed.Any(p => p.pos.Equals(b.pos)))
                        collided = true;
                }
            return collided;
        }
    }

    sealed class Block(Vector2 pos, ConsoleColor color)
    {
        public Vector2 pos = pos;
        public ConsoleColor color = color;

        public void Draw(string str = "██") {
            if (pos.Y < 0) return;
            Console.SetCursorPosition((int)(1 + pos.X * 2), (int)(1 + pos.Y));
            Console.ForegroundColor = color;
            Console.Write(str);
        }

        public static void DrawAll(Block[] arr) {
            foreach(Block b in arr)
                b.Draw();
        }
        
        public static void DrawAll(List<Block> arr) {
            foreach(Block b in arr)
                b.Draw();
        }
    }
}