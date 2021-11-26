using PendleCodeMonkey.ChessEngineLib;
using System;
using System.Diagnostics;
using System.IO;

namespace PCMChessEngineConsoleApp
{
	class Program
	{
		static void Main(string[] args)
		{
			TestChessEngine();
		}

		static void TestChessEngine()
		{
			const int MAX_MOVES = 512;

			Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); // the start positions in Forsyth–Edwards Notation ("FEN")

			ChessEngine engine = new ChessEngine(board);

			string path2 = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
			path2 = Path.Combine(path2, "GameDump.txt");

			using StreamWriter writer = new StreamWriter(path2);

			int move = 0;
			while (!board.IsEndOfGame() && move < MAX_MOVES)
			{
				Console.WriteLine("Move: " + (move + 1).ToString());

				var computerMove = engine.GetRandomEngineMove();            // Make a 'weighted' random selection of the available moves (to mix it up a little).
//				var computerMove = engine.GetBestEngineMove();				// Use this instead of the above to always use the 'best' available move.
				engine.MakeMove(computerMove);

				var boardStr = board.ToHumanReadableString(true);			// Output only the board (not the additional info).
				writer.Write(boardStr);
				writer.WriteLine();
				move++;

				// Flush the StreamWriter every now and then.
				if (move % 10 == 0)
				{
					writer.Flush();
				}
			}
		}
	}
}
