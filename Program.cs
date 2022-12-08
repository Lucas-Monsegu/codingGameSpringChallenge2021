using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

class Masks
{
    public static ulong map = 0x3C1F_0FC7_F1F8_7C1E;
    public static ulong[][] neighbors = new ulong[64][];

    public static int[] posToIndex = {
        -1, 34, 33, 32, 31, -1, -1, -1, -1,
        -1, 35, 17, 16, 15, 30, -1, -1, -1,
        -1, 36, 18, 6, 5, 14, 29, -1, -1,
        -1, 19, 7, 1, 0, 4, 13, 28, -1,
        -1, -1, 20, 8, 2, 3, 12, 27, -1,
        -1, -1, -1, 21, 9, 10, 11, 26, -1,
        -1, -1, -1, -1, 22, 23, 24, 25, -1
    };
    public static int[] indexToPos = {
        31, 30, 40, 41, 32, 22, 21, 29, 39, 49, 50, 51,
        42, 33, 23, 13, 12, 11, 20, 28, 38, 48, 58, 59,
        60, 61, 52, 43, 34, 24, 14, 4, 3, 2, 1, 10, 19
    };
    private static ulong shiftRight(ulong bitBoard)
    {
        return bitBoard >> 1 & map;
    }
    private static ulong shiftUpRight(ulong bitBoard)
    {
        return bitBoard << 9 & map;
    }
    private static ulong shiftUpLeft(ulong bitBoard)
    {
        return bitBoard << 10 & map;
    }

    private static ulong shiftLeft(ulong bitBoard)
    {
        return bitBoard << 1 & map;
    }
    private static ulong shiftDownLeft(ulong bitBoard)
    {
        return bitBoard >> 9 & map;
    }
    private static ulong shiftDownRight(ulong bitBoard)
    {
        return bitBoard >> 10 & map;
    }
    private static ulong MoveAllDirections(ulong bitBoard, int index, int range)
    {
        bitBoard = bitBoard | (1ul << index);
        for (int i = 0; i < range; ++i)
        {
            bitBoard = shiftRight(bitBoard) | shiftUpRight(bitBoard) | shiftUpLeft(bitBoard) | shiftLeft(bitBoard) | shiftDownLeft(bitBoard) | shiftDownRight(bitBoard);
        }
        bitBoard = bitBoard & ~(1ul << index);
        return bitBoard;
    }

    public static void GenerateNeighbors()
    {
        for (int i = 0; i < 64; ++i)
        {
            if (Forest.graph[i, 0] == 0 && Forest.graph[i, 1] == 0)
            {
                continue;
            }
            ulong one = 0;
            for (int x = 0; x < 6; ++x)
            {

                if (Forest.graph[i, x] != -1)
                {
                    one = one | (1ul << Forest.graph[i, x]);

                }
            }
            ulong two = MoveAllDirections(0, i, 2);
            ulong three = MoveAllDirections(0, i, 3);
            neighbors[i] = new ulong[] { one, two, three };
        }
    }


    public static void print(ulong bitBoard)
    {
        if (((1ul << 63) & bitBoard) != 0)
        {
            Console.Error.Write("E");
        }
        for (int y = 6; y >= 0; --y)
        {
            string s = new String(' ', Math.Max(0, y - 3));
            for (int x = 8; x >= 0; --x)
            {
                int val = y * 9 + x;
                int pos = posToIndex[val];
                string g = "";
                ulong res = (1ul << val) & bitBoard;
                if (pos != -1)
                {
                    g = res != 0 ? "1" : "0";
                }
                else
                {
                    g = res == 0 ? "" : "E";
                }
                s += ' ' + g;
            }
            Console.Error.WriteLine(s);
        }
    }
    public static void PrintBinary(ulong bitboard)
    {
        Console.Error.WriteLine(Convert.ToString((long)bitboard, 2).PadLeft(64, '0'));
    }
}
enum ACTION
{
    SEED,
    GROW,
    COMPLETE,
    WAIT
}

class Move
{
    public ACTION action;
    public byte from;
    public byte to;
    public byte size;

    public Move(ACTION action, byte from = 100, byte to = 100, byte size = 0)
    {
        this.action = action;
        this.from = from;
        this.to = to;
        this.size = size;
    }
    public override string ToString()
    {
        if (this.from == 100)
        {
            return this.action.ToString();
        }
        if (this.to == 100)
        {
            return this.action.ToString() + " " + Masks.posToIndex[this.from];
        }
        else
        {
            return this.action.ToString() + " " + Masks.posToIndex[this.from] + " " + Masks.posToIndex[this.to] + " " + size;

        }
    }
    public void Say()
    {
        List<string> test = new List<string>() { this.action.ToString() };
        if (this.from != 100)
        {
            test.Add(Masks.posToIndex[this.from].ToString());
        }
        if (this.to != 100)
        {
            test.Add(Masks.posToIndex[this.to].ToString());
        }
        Console.WriteLine(String.Join(" ", test));
    }
}

class Board
{
    public ulong treesPos;
    public ulong oTreesPos;
    public ulong treesDormant;
    public ulong treesSize0;
    public ulong treesSize1;
    public ulong treesSize2;
    public ulong treesSize3;

    public int score;
    public int suns;
    public int oSuns;

    public float eval = -1;

    public void AddTree(int index, bool myTree, bool isDormant = true, int size = 1, int fromTreeIndex = -1)
    {
        int pos = index;
        if (myTree)
        {
            treesPos = treesPos | (1ul << pos);
        }
        else
        {
            oTreesPos = oTreesPos | (1ul << pos);
        }
        switch (size)
        {
            case 0:
                treesSize0 = treesSize0 | (1ul << pos);
                break;
            case 1:
                treesSize1 = treesSize1 | (1ul << pos);
                break;
            case 2:
                treesSize2 = treesSize2 | (1ul << pos);
                break;
            case 3:
                treesSize3 = treesSize3 | (1ul << pos);
                break;
        }
        if (isDormant)
        {
            treesDormant = treesDormant | (1ul << pos);
        }
        if (fromTreeIndex != -1)
        {
            treesDormant = treesDormant | (1ul << fromTreeIndex);
        }
    }
    public void GrowTree(int index, int size)
    {
        switch (size)
        {
            case 0:

                treesSize0 = treesSize0 & ~(1ul << index);
                treesSize1 = treesSize1 | (1ul << index);

                break;
            case 1:
                treesSize1 = treesSize1 & ~(1ul << index);
                treesSize2 = treesSize2 | (1ul << index);
                break;
            case 2:
                treesSize2 = treesSize2 & ~(1ul << index);
                treesSize3 = treesSize3 | (1ul << index);
                break;
        }
        treesDormant = treesDormant | (1ul << index);
    }

    private void CutTree(int index, bool myTree = true)
    {
        if (myTree)
        {
            treesPos = treesPos & ~(1ul << index);
        }
        else
        {
            oTreesPos = oTreesPos & ~(1ul << index);
        }
        treesSize3 = treesSize3 & ~(1ul << index);
    }
    private void PayInSun(int suns)
    {
        this.suns -= suns;
        if (this.suns < 0)
        {
            Console.Error.WriteLine("ERROOOOOOOOOOOR NEGATIVE NUMBER OF SUNS: " + this.suns);
        }
    }

    public void Apply(Move move)
    {
        switch (move.action)
        {
            case ACTION.SEED:
                ApplySeed(move);
                break;
            case ACTION.GROW:
                ApplyGrow(move);
                break;
            case ACTION.COMPLETE:
                ApplyComplete(move);
                break;
            case ACTION.WAIT:
                ApplyWait(move);
                break;
        }
    }

    private void ApplySeed(Move move)
    {
        PayInSun(PriceToSeed());
        AddTree(move.to, true, true, 0);

    }
    private void ApplyGrow(Move move)
    {
        PayInSun(PriceToGrow(move.size));
        GrowTree(move.from, move.size);
    }
    private void ApplyWait(Move move)
    {
        return;
    }
    private void ApplyComplete(Move move)
    {
        PayInSun(PriceToComplete());
        score += Forest.CompleteTreeAndGetBonusScore(move.from);
        this.CutTree(move.from, true);
    }

    public float GetRichnessForTrees(ulong bitboard)
    {
        float c = 0f;
        Optimizer.ForEach((index) =>
        {
            c += 1 + Forest.richness[index];
        }, bitboard);
        return c;
    }

    public float GetNeighboorsValue(ulong bitBoard)
    {
        float c = 0;
        Optimizer.ForEach((index) =>
        {
            ulong friends = Optimizer.CountOne(GetAllFriendlyNeighbors(index, 3));
            ulong opponents = Optimizer.CountOne(GetAllOpponentNeighbors(index, 3));
            c += (float)opponents - (float)friends;
        }, bitBoard);
        return c;
    }

    public float Evaluate()
    {
        if (eval != -1)
        {
            return eval;
        }
        float scoreValuation = 0.01f;
        if ((int)Optimizer.CountOne(treesSize3) >= Forest.dayEntilEnd)
        {
            scoreValuation = 100f;
        }
        float richness = GetRichnessForTrees(treesSize0) + GetRichnessForTrees(treesSize1) * 2f + GetRichnessForTrees(treesSize2) * 4f + GetRichnessForTrees(treesSize3) * 8f;
        float neighborsValue = GetNeighboorsValue(treesSize0) * 0.5f + GetNeighboorsValue(treesSize1) * 1f + GetNeighboorsValue(treesSize2) * 1.5f + GetNeighboorsValue(treesSize3) * 2;
        // Console.Error.WriteLine("NEIGHBORS VALUE" + neighborsValue.ToString("#.000") + " richesness: " + richness.ToString("#.000"));
        float evaluation = suns / 3f + richness + neighborsValue;
        evaluation += score * scoreValuation;

        this.eval = evaluation;
        return evaluation;
    }

    public int PriceToGrow(int size)
    {

        return (size) * size + 1 + (int)Optimizer.CountOne(this.GetMyTreeBySize(size + 1));
    }
    public int PriceToComplete()
    {
        return 4;
    }
    public int PriceToSeed()
    {
        return (int)Optimizer.CountOne(this.GetMyTreeBySize(0));
    }
    public ulong GetMyTreeBySize(int size)
    {
        switch (size)
        {
            case 0:
                return (this.treesPos & this.treesSize0);
            case 1:
                return (this.treesPos & this.treesSize1);
            case 2:
                return (this.treesPos & this.treesSize2);
            case 3:
                return (this.treesPos & this.treesSize3);
            default:
                throw new Exception("INVALID SIZE IN GETMYTREEBYSIZE");

        }
    }
    public ulong GetMySeedsActive()
    {
        ulong myTrees = (this.treesPos & this.treesSize0);
        return (this.treesPos & this.treesSize0) ^ (this.treesDormant & myTrees);
    }
    public ulong GetMyTree1Active()
    {

        ulong myTrees = (this.treesPos & this.treesSize1);
        return (this.treesPos & this.treesSize1) ^ (this.treesDormant & myTrees);
    }
    public ulong GetMyTree2Active()
    {
        ulong myTrees = (this.treesPos & this.treesSize2);
        return (this.treesPos & this.treesSize2) ^ (this.treesDormant & myTrees);

    }
    public ulong GetMyTree3Active()
    {
        ulong myTrees = (this.treesPos & this.treesSize3);
        return (this.treesPos & this.treesSize3) ^ (this.treesDormant & myTrees);

    }

    public ulong GetAllFreeNeighboors(ulong index, int range)
    {
        return Masks.neighbors[index][range - 1] & ~(treesPos | oTreesPos);
    }
    public ulong GetAllOpponentNeighbors(ulong index, int range)
    {
        return Masks.neighbors[index][range - 1] | (oTreesPos);
    }
    public ulong GetAllFriendlyNeighbors(ulong index, int range)
    {
        return Masks.neighbors[index][range - 1] | (treesPos);
    }

    public Board Clone()
    {
        Board b = (Board)this.MemberwiseClone();
        b.eval = -1;
        return b;
    }

    public void print()
    {
        for (int y = 6; y >= 0; --y)
        {
            string s = new String(' ', Math.Max(0, y - 3));
            for (int x = 8; x >= 0; --x)
            {
                int val = y * 9 + x;
                int pos = Masks.posToIndex[val];
                string g = "";
                ulong res = (1ul << val) & treesPos;
                ulong res2 = (1ul << val) & oTreesPos;
                if (pos != -1)
                {
                    if (res2 != 0)
                    {
                        g = "O";
                    }
                    else
                    {
                        if (res != 0)
                        {
                            ulong isDormant = (1ul << val) & treesDormant;
                            g = isDormant != 0 ? "S" : "X";
                        }
                        else
                        {
                            g = "□";
                        }

                    }
                }
                else
                {
                    g = "";
                }
                s += ' ' + g;
            }
            Console.Error.WriteLine(s);
        }
    }
}

class Optimizer
{
    public static void ForEach(Action<ulong> action, ulong bitBoard)
    {
        {
            while (bitBoard != 0)
            {
                ulong index = Bmi1.X64.TrailingZeroCount(bitBoard);
                action(index);
                bitBoard = Bmi1.X64.ResetLowestSetBit(bitBoard);
            }
        }
    }
    public static ulong CountOne(ulong bitboard)
    {
        return Popcnt.X64.PopCount(bitboard);
    }
}

class BeamElement
{
    public Board board;
    public int evaluation;

    public Move firstMove;

    public BeamElement(Board board, Move? firstMove = null)
    {
        this.board = board;
        this.evaluation = 0;
        if (firstMove != null)
        {
            this.firstMove = firstMove;
        }
    }
    public BeamElement CopyAndApply(Move move)
    {
        BeamElement n = new BeamElement(this.board.Clone());
        n.board.Apply(move);
        if (n.firstMove == null)
        {
            n.firstMove = move;
        }
        return n;
    }
}



class BeamSearch
{
    static BeamElement[] elements = new BeamElement[500];


    public static void BullShitDecision()
    {
        var element = new BeamElement(Forest.currentBoard, null);
        List<Move> moves = GenerateMoves(element);
        Console.Error.WriteLine("NB MOVES:" + moves.Count);
        foreach (var move in moves)
        {
            //Console.Error.WriteLine("POSSIBLE MOVE " + move);
        }
        List<BeamElement> elements = MoveToElement(element, moves);
        elements.Sort((BeamElement element1, BeamElement element2) =>
        {
            return -element1.board.Evaluate().CompareTo(element2.board.Evaluate());
        });
        foreach (var el in elements)
        {
            Console.Error.WriteLine(el.firstMove.ToString() + " EVALUATED " + el.board.eval.ToString("#.000") + "SCORE: " + el.board.score);
        }
        elements[0].firstMove.Say();
    }
    public static List<BeamElement> MoveToElement(BeamElement element, List<Move> moves)
    {
        return moves.Select((Move move) => { return element.CopyAndApply(move); }).ToList();
    }

    public static List<Move> GenerateMoves(BeamElement element)
    {
        Board board = element.board;
        Console.Error.WriteLine("IN GENERATE MOVE" + board.suns);
        List<Move> moves = new List<Move>();
        moves.Add(new Move(ACTION.WAIT, 0));
        ulong mySeeds = board.GetMySeedsActive();
        ulong myTrees1 = board.GetMyTree1Active();
        ulong myTrees2 = board.GetMyTree2Active();
        ulong myTrees3 = board.GetMyTree3Active();
        ulong[] growables = new ulong[] { mySeeds, myTrees1, myTrees2 };
        for (int i = 0; i < growables.Length; ++i)
        {
            if (board.PriceToGrow(i) <= board.suns)
            {
                Optimizer.ForEach((index) =>
               {
                   moves.Add(new Move(ACTION.GROW, (byte)index, 100, (byte)i));
               }, growables[i]);
            }

        }
        if (board.PriceToComplete() <= board.suns)
        {
            Optimizer.ForEach((index) =>
                    {
                        moves.Add(new Move(ACTION.COMPLETE, (byte)index));
                    }, myTrees3);
        }
        ulong[] canSeed = new ulong[] { myTrees1, myTrees2, myTrees3 };
        Console.Error.WriteLine("PRIC TO SEED: " + board.PriceToSeed() + " MY SUNS: " + board.suns);
        if (board.PriceToSeed() <= board.suns)
        {
            for (int i = 0; i < canSeed.Length; ++i)
            {
                int range = i + 1;
                Console.Error.WriteLine(Optimizer.CountOne(canSeed[i]));
                Optimizer.ForEach((index) =>
                                    {
                                        Console.Error.WriteLine("SEEDS FOR" + Masks.posToIndex[index]); // ICI 63
                                        Console.Error.WriteLine("COUNT ONE" + Optimizer.CountOne(board.GetAllFreeNeighboors(index, range)));
                                        Optimizer.ForEach((indexMask) =>
                                        {

                                            moves.Add(new Move(ACTION.SEED, (byte)index, (byte)indexMask, (byte)(range)));

                                        }, board.GetAllFreeNeighboors(index, range));
                                    }, canSeed[i]);
            }

        }



        return moves;
    }
}

class Forest
{
    public static int[,] graph = new int[64, 6];


    public static int[] richness = new int[64];
    public static int suns;
    public static int score;
    public static int opponentSuns;
    public static int opponentScore;
    public static Board currentBoard;
    public static int day;

    public static int dayEntilEnd;
    public static int nutrients = 20;

    public static int CompleteTreeAndGetBonusScore(int index)
    {
        int score = nutrients + richness[index];
        nutrients -= 1;
        return score;
    }
}

class Player
{
    static void Main(string[] args)
    {
        Console.Error.WriteLine("TEST");
        string[] inputs;
        int numberOfCells = int.Parse(Console.ReadLine()); // 37
        for (int i = 0; i < numberOfCells; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int index = int.Parse(inputs[0]); // 0 is the center cell, the next cells spiral outwards
            int richness = int.Parse(inputs[1]); // 0 if the cell is unusable, 1-3 for usable cells
            Forest.richness[Masks.indexToPos[i]] = 2 * richness - 2;
            for (int n = 2; n < 8; ++n)
            {
                int neigh = int.Parse(inputs[n]);
                if (neigh >= 0)
                {
                    Forest.graph[Masks.indexToPos[i], n - 2] = Masks.indexToPos[neigh];
                }
                else
                {
                    Forest.graph[Masks.indexToPos[i], n - 2] = -1;
                }


            }

        }

        Masks.GenerateNeighbors();
        // game loop
        while (true)
        {
            Forest.day = int.Parse(Console.ReadLine());// the game lasts 24 days: 0-23
            Forest.dayEntilEnd = 23 - Forest.day;
            Forest.nutrients = int.Parse(Console.ReadLine()); // the base score you gain from the next COMPLETE action
            inputs = Console.ReadLine().Split(' ');
            Forest.suns = int.Parse(inputs[0]); // your sun points
            Forest.score = int.Parse(inputs[1]); // your current score
            inputs = Console.ReadLine().Split(' ');
            Forest.opponentSuns = int.Parse(inputs[0]); // opponent's sun points
            Forest.opponentScore = int.Parse(inputs[1]); // opponent's score
            bool oppIsWaiting = inputs[2] != "0"; // whether your opponent is asleep until the next day
            int numberOfTrees = int.Parse(Console.ReadLine()); // the current amount of trees
            Forest.currentBoard = new Board();
            Forest.currentBoard.suns = Forest.suns;
            Forest.currentBoard.score = Forest.score;
            Forest.currentBoard.oSuns = Forest.opponentSuns;
            for (int i = 0; i < numberOfTrees; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int cellIndex = int.Parse(inputs[0]); // location of this tree
                int size = int.Parse(inputs[1]); // size of this tree: 0-3
                bool isMine = inputs[2] != "0"; // 1 if this is your tree
                bool isDormant = inputs[3] != "0"; // 1 if this tree is dormant
                Forest.currentBoard.AddTree(Masks.indexToPos[cellIndex], isMine, isDormant, size);
            }
            //Forest.currentBoard.print();

            int numberOfPossibleActions = int.Parse(Console.ReadLine()); // all legal actions
            for (int i = 0; i < numberOfPossibleActions; i++)
            {
                string possibleAction = Console.ReadLine(); // try printing something from here to start with
                //Console.Error.WriteLine("POSSIBLE MOVE " + possibleAction);
            }
            BeamSearch.BullShitDecision();

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // GROW cellIdx | SEED sourceIdx targetIdx | COMPLETE cellIdx | WAIT <message>
            //Console.WriteLine("WAIT");
        }
    }
}