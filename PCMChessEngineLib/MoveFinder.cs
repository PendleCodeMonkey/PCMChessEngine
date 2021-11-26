using System.Collections.Generic;
using System.Linq;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="MoveFinder"/> class.
	/// </summary>
	/// <remarks>
	/// Performs move searching based on null-move pruning, iterative deepening, quiescent
	/// searching, static exchange evaluation, alpha-beta, PVS, and history heuristics.
	/// </remarks>
	internal class MoveFinder
	{
		#region data

		private static readonly int NullMoveReduction = 4;
		private static readonly int NullMoveThreshold = 319;

		private static IEvaluator _evaluator;
		private static int _maxSearchDepth = 5;
		private static int[,] _whiteHeuristics;
		private static int[,] _blackHeuristics;
		private static int[,] _triangularArray;
		private static int[] _triangularLength;
		private static bool _followPV;
		private static bool _allowNull;
		private static int[] _lastPV;

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MoveFinder"/> class.
		/// </summary>
		public MoveFinder()
		{
		}

		#endregion

		#region properties

		/// <summary>
		/// Gets a list of all legal moves found by the engine.
		/// </summary>
		public static List<int> LegalMoves { get; private set; }

		#endregion

		#region methods

		/// <summary>
		/// Set the maximum search depth
		/// </summary>
		/// <param name="depth">The maximum search depth</param>
		public static void SetDepth(int depth)
		{
			_maxSearchDepth = depth;
		}

		/// <summary>
		/// Sets the evaluator to be used
		/// </summary>
		/// <param name="eval">An instance of the class that will be used for evaluating moves.</param>
		public static void SetEvaluator(IEvaluator eval)
		{
			_evaluator = eval;
		}

		/// <summary>
		/// Returns the best move in a position (at least according to the engine).
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <returns>The best move the engine can find.</returns>
		public static int GetBestMove(Board b)
		{
			var moves = GetListOfMoves(b);
			return (moves != null && moves.Count() > 0) ? moves[0] : 0;
		}

		/// <summary>
		/// Returns a list of moves for the given board state.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <returns>The list of moves that the engine found.</returns>
		public static List<int> GetListOfMoves(Board b)
		{

			_whiteHeuristics = new int[64, 64];
			_blackHeuristics = new int[64, 64];
			_lastPV = new int[Board.MAX_MOVES];
			LegalMoves = new List<int>();

			for (int currDepth = 1; currDepth < _maxSearchDepth; currDepth++)
			{
				_triangularArray = new int[Board.MAX_MOVES, Board.MAX_MOVES];
				_triangularLength = new int[Board.MAX_MOVES];
				_followPV = true;
				_allowNull = true;
				AlphaBeta(b, int.MinValue + 1, int.MaxValue - 1, currDepth, 0);
			}

			return LegalMoves;
		}

		/// <summary>
		/// Perform Alpha-Beta pruning (using a recursive NegaMax technique).
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="alpha">The Alpha value (the minimum score that the maximizing player is assured of).</param>
		/// <param name="beta">The Beta value (the maximum score that the minimizing player is assured of).</param>
		/// <param name="depth">The current depth of the Alpha-Beta pruning.</param>
		/// <param name="ply">The current ply.</param>
		/// <returns>The score calculated for the current iteration of the Alpha-Beta pruning operation.</returns>
		private static int AlphaBeta(Board b, int alpha, int beta, int depth, int ply)
		{
			_triangularLength[ply] = ply;
			if (depth <= 0)
			{
				_followPV = false;
				// Perform a quiescence search.
				return QSearch(b, alpha, beta, ply);
			}

			if (b.IsEndOfGame())
			{
				_followPV = false;
				return _evaluator.Eval(b);
			}

			// Attempt null move (if permitted)
			if (_allowNull && !_followPV && b.MovingSideMaterial() > NullMoveThreshold && !b.IsCheck())
			{
				_allowNull = false;
				b.DoNullMove();
				int val2 = -AlphaBeta(b, -beta, -beta + 1, depth - NullMoveReduction, ply);
				b.UndoMove();
				_allowNull = true;
				if (val2 >= beta)
				{
					return val2;
				}
			}

			_allowNull = true;

			int movesFound = 0;
			int[] moves = new int[Board.MAX_MOVES];
			int numMoves = MoveGenerator.GetAllLegalMoves(b, moves);

			for (int i = 0; i < numMoves; i++)
			{
				PutBestMoveFirst(i, moves, numMoves, depth, ply, b.WhiteToMove);
				b.MakeMove(moves[i]);

				int val;
				if (movesFound != 0)
				{
					val = -AlphaBeta(b, -alpha - 1, -alpha, depth - 1, ply + 1);

					if (val > alpha && val < beta)
					{
						val = -AlphaBeta(b, -beta, -alpha, depth - 1, ply + 1);
					}
				}
				else
				{
					val = -AlphaBeta(b, -beta, -alpha, depth - 1, ply + 1);
				}

				b.UndoMove();

				if (val >= beta)
				{
					if (b.WhiteToMove)
					{
						_whiteHeuristics[Move.GetFrom(moves[i]), Move.GetTo(moves[i])] += depth * depth;
					}
					else
					{
						_blackHeuristics[Move.GetFrom(moves[i]), Move.GetTo(moves[i])] += depth * depth;
					}

					return beta;
				}
				if (val > alpha)
				{
					alpha = val;
					movesFound++;

					_triangularArray[ply, ply] = moves[i];

					for (int j = ply + 1; j < _triangularLength[ply + 1]; j++)
					{
						_triangularArray[ply, j] = _triangularArray[ply + 1, j];
					}

					_triangularLength[ply] = _triangularLength[ply + 1];

					if (ply == 0)
					{
						// Remember Principal Variation (PV)
						for (int n = 0; n < _triangularLength[0]; n++)
						{
							_lastPV[n] = _triangularArray[0, n];
						}
					}
				}
			}
			if (movesFound != 0)
			{
				if (b.WhiteToMove)
				{
					_whiteHeuristics[Move.GetFrom(_triangularArray[ply, ply]), Move.GetTo(_triangularArray[ply, ply])] += depth * depth;
				}
				else
				{
					_blackHeuristics[Move.GetFrom(_triangularArray[ply, ply]), Move.GetTo(_triangularArray[ply, ply])] += depth * depth;
				}
			}

			if (ply == 0 && depth == _maxSearchDepth - 1 && numMoves > 0)
			{
				// Take a copy of the list of legal moves (so we can subsequently return them to the client)
//				LegalMoves = moves.TakeWhile(x => x != 0).ToList();
				LegalMoves = moves.Take(numMoves).ToList();
			}

			return alpha;
		}

		/// <summary>
		/// Put the best move at the specified index in the moves list.
		/// </summary>
		/// <param name="nextIndex">The index (within the moves list) at which the best move should be placed.</param>
		/// <param name="moves">The list of moves.</param>
		/// <param name="numMoves">The number of moves in the moves list.</param>
		/// <param name="depth">The current Alpha-Beta pruning depth.</param>
		/// <param name="ply">The current ply.</param>
		/// <param name="whiteToMove"><c>true</c> if it is White's move, otherwise <c>false</c>.</param>
		private static void PutBestMoveFirst(int nextIndex, int[] moves, int numMoves, int depth, int ply, bool whiteToMove)
		{
			// If applicable, make next move the Principal Variation (PV)
			if (_followPV && depth > 1)
			{
				for (int i = nextIndex; i < numMoves; i++)
				{
					if (moves[i] == _lastPV[ply])
					{
						// Swap the moves so that the PV element is at nextIndex in the moves list.
						int temp = moves[i];
						moves[i] = moves[nextIndex];
						moves[nextIndex] = temp;
						return;
					}
				}
			}

			// Get best heuristic.
			int[,] heuristics = whiteToMove ? _whiteHeuristics : _blackHeuristics;
			int best = heuristics[Move.GetFrom(moves[nextIndex]), Move.GetTo(moves[nextIndex])];
			int bestLoc = nextIndex;

			for (int i = nextIndex + 1; i < numMoves; i++)
			{
				if (heuristics[Move.GetFrom(moves[i]), Move.GetTo(moves[i])] > best)
				{
					best = heuristics[Move.GetFrom(moves[i]), Move.GetTo(moves[i])];
					bestLoc = i;
				}
			}

			if (bestLoc > nextIndex)
			{
				// Swap array elements so that the move with best heuristics is at nextIndex in the moves list.
				int temp = moves[bestLoc];
				moves[bestLoc] = moves[nextIndex];
				moves[nextIndex] = temp;
			}
		}

		/// <summary>
		/// Perform a Quiescence Search (uses a recursive NegaMax technique).
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="alpha">Alpha value.</param>
		/// <param name="beta">Beta value.</param>
		/// <param name="ply">Current ply of the search.</param>
		/// <returns>The score calculated for the current iteration of the Quiescence Search operation.</returns>
		private static int QSearch(Board b, int alpha, int beta, int ply)
		{
			_triangularLength[ply] = ply;

			if (b.IsCheck())
			{
				return AlphaBeta(b, alpha, beta, 1, ply);
			}

			// In order to allow the quiescence search to stabilize, we need to be able to stop
			// searching without necessarily searching all available captures. In addition, we
			// need a score to return in case there are no captures available to be played.
			// This is done by a using the static evaluation as a "stand-pat" score.
			int standPat = _evaluator.Eval(b);

			if (standPat >= beta)
			{
				return standPat;
			}
			if (standPat > alpha)
			{
				alpha = standPat;
			}

			int[] captures = new int[Board.MAX_MOVES];
			int numCaptures = GenerateCaptures(b, captures);

			for (int i = 0; i < numCaptures; i++)
			{
				b.MakeMove(captures[i]);
				int val = -QSearch(b, -beta, -alpha, ply + 1);
				b.UndoMove();

				if (val >= beta)
				{
					return val;
				}
				if (val > alpha)
				{
					alpha = val;
					_triangularArray[ply, ply] = captures[i];
					for (int j = ply + 1; j < _triangularLength[ply + 1]; j++)
					{
						_triangularArray[ply, j] = _triangularArray[ply + 1, j];
					}
					_triangularLength[ply] = _triangularLength[ply + 1];
				}
			}

			return alpha;
		}

		/// <summary>
		/// Generate all capture moves for a specified board state.
		/// </summary>
		/// <param name="b">The state of the board to be considered.</param>
		/// <param name="captures">Array through which the capture moves are returned.</param>
		/// <returns>The number of returned capture moves.</returns>
		private static int GenerateCaptures(Board b, int[] captures)
		{
			int[] capturevals = new int[Board.MAX_MOVES];
			int numCaptures = MoveGenerator.GetAllCapturesAndPromotions(b, captures);

			for (int i = 0; i < numCaptures; i++)
			{
				int val = b.SEE(captures[i]);
				capturevals[i] = val;

				if (val < 0)
				{ // Isn't worth keeping so remove it.
					for (int elem = i; elem < numCaptures - 1; ++elem)
					{
						captures[i] = captures[i + 1];
						capturevals[i] = capturevals[i + 1];
					}
					numCaptures--;
					i--;
				}

				int insertloc = i;

				while (insertloc >= 0 && capturevals[i] > capturevals[insertloc])
				{
					int tempcap = captures[i];
					captures[i] = captures[insertloc];
					captures[insertloc] = tempcap;

					int tempval = capturevals[i];
					capturevals[i] = capturevals[insertloc];
					capturevals[insertloc] = tempval;

					insertloc--;
				}
			}

			return numCaptures;
		}

		#endregion
	}
}
