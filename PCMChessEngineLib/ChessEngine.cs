using System;
using System.Collections.Generic;
using System.Linq;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// The class through which the chess engine functionality can be accessed by clients.
	/// </summary>
	public class ChessEngine
	{
		#region data

		// The start positions for the pieces on the board (in Forsyth–Edwards Notation ["FEN"] format).
		public static string START_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

		private Board _board;

		private readonly Random _rnd = new Random();

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ChessEngine"/> class.
		/// </summary>
		/// <remarks>
		/// Initializes the board using the correct starting positions for all pieces.
		/// </remarks>
		public ChessEngine()
		{
			_board = new Board(START_FEN);      // Initialize the board with the correct starting positions.
			InitMoveFinderSettings();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ChessEngine"/> class.
		/// </summary>
		/// <param name="b">The initial state of the board.</param>
		public ChessEngine(Board b)
		{
			_board = b;
			InitMoveFinderSettings();
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets a value indicating if White has won.
		/// </summary>
		public bool WhiteWins => _board.IsMate() && !_board.WhiteToMove;

		/// <summary>
		/// Gets a value indicating if Black has won.
		/// </summary>
		public bool BlackWins => _board.IsMate() && _board.WhiteToMove;

		/// <summary>
		/// Gets a value indicating if the game is a draw.
		/// </summary>
		public bool IsDraw => _board.IsDraw();

		#endregion

		#region methods

		/// <summary>
		/// Initialize to a board with all pieces in their correct starting positions.
		/// </summary>
		/// <returns>The initialized board.</returns>
		public Board InitBoard()
		{
			_board = new Board(START_FEN);      // Initialize the board with the correct starting positions.
			return _board;
		}

		/// <summary>
		/// Initialize the settings for the move finder.
		/// </summary>
		private void InitMoveFinderSettings()
		{
			MoveFinder.SetDepth(5);
			MoveFinder.SetEvaluator(new CompleteEvaluator());
		}

		/// <summary>
		/// Attempt to make the specified move.
		/// </summary>
		/// <param name="move">An integer representation of the move (see the <see cref="Move"/> class for details).</param>
		/// <returns><c>true</c> if the move was successfully made, otherwise <c>false</c>.</returns>
		public bool MakeMove(int move) => _board.MakeMove(move);

		/// <summary>
		/// Undo the previous move.
		/// </summary>
		/// <remarks>
		/// Note that we undo two moves (i.e. the last move for both players).
		/// </remarks>
		/// <returns><c>true</c> if the last move was undone, otherwise <c>false</c>.</returns>
		public bool UndoMove()
		{
			if (_board.MoveNumber > 0)
			{
				_board.UndoMove();
				_board.UndoMove();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the piece that is at a specified location.
		/// </summary>
		/// <param name="loc">An integer (in  the range 0 to 63) representing a position.</param>
		/// <returns>A character representing the piece at the specified location (or a space character if the square is empty).</returns>
		public char Get(int loc) => _board.GetPieceAt(loc);

		/// <summary>
		/// Attempt to make the specified move.
		/// </summary>
		/// <param name="fromX">Zero-based X position of the origin of the move.</param>
		/// <param name="fromY">Zero-based Y position of the origin of the move.</param>
		/// <param name="toX">Zero-based X position of the destination of the move.</param>
		/// <param name="toY">Zero-based Y position of the destination of the move.</param>
		/// <returns><c>true</c> if the move was successfully made, otherwise <c>false</c>.</returns>
		public bool MakeMove(int fromX, int fromY, int toX, int toY)
		{
			int from = ((7 - fromY) * 8) + fromX;
			int to = ((7 - toY) * 8) + toX;

			int[] moves = new int[Board.MAX_MOVES];
			int num_moves = MoveGenerator.GetAllLegalMoves(_board, moves);
			for (int i = 0; i < num_moves; i++)
			{
				if (from == Move.GetFrom(moves[i]) && to == Move.GetTo(moves[i]))
				{
					return MakeMove(moves[i]);
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the best move that the engine can find.
		/// </summary>
		/// <returns>Integer representation of the best move.</returns>
		public int GetBestEngineMove()
		{
			int move = MoveFinder.GetBestMove(_board);
			return move;
		}

		/// <summary>
		/// Randomly selects one move from the list of legal moves found by the engine.
		/// </summary>
		/// <remarks>
		/// This method applies weighting to the move selection (so that the better moves have a
		/// higher weighting and are therefore more likely to be randomly selected).
		/// </remarks>
		/// <returns>Integer representation of the random move.</returns>
		public int GetRandomEngineMove()
		{
			var moves = MoveFinder.GetListOfMoves(_board);
			int numMoves = moves.Count();
			int nSum = numMoves * (numMoves + 1) / 2;
			int rnd = _rnd.Next(nSum);
			int n = numMoves;
			int moveIndex = 0;
			while (rnd >= n)
			{
				rnd -= n;
				n--;
				moveIndex++;
			}
			return moves[moveIndex];
		}

		/// <summary>
		/// Retrieve a collection of all legal moves found by the engine.
		/// </summary>
		/// <remarks>
		/// Returns a collections of integers that are a representation of each move (see the <see cref="Move"/> class for details).
		/// The first move in the returned list is the one that has been determined to be the 'best'.
		/// </remarks>
		/// <returns>A collection of the moves found by the engine.</returns>
		public List<int> GetListOfEngineMoves()
		{
			return MoveFinder.GetListOfMoves(_board);
		}

		/// <summary>
		/// Retrieve a list of suggested moves.
		/// </summary>
		/// <returns>
		/// A list of tuples, each consisting of the following:
		///		move - An integer representation of the move.
		///		san - A string representation of the move in SAN (Standard Algebraic Notation) format.
		/// </returns>
		public List<(int move, string san)> GetSuggestedMoves()
		{
			List<(int move, string san)> list = new List<(int, string)>();

			var moves = GetListOfEngineMoves();
			foreach (var move in moves)
			{
				if (move == 0)
				{
					break;
				}
				list.Add((move, SANUtils.GetSAN(_board, move)));
			}
			return list;
		}

		#endregion
	}
}
