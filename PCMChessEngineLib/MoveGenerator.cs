namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="MoveGenerator"/> class.
	/// </summary>
	internal class MoveGenerator
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MoveGenerator"/> class.
		/// </summary>
		public MoveGenerator()
		{
		}

		#endregion

		#region methods

		/// <summary>
		/// Gets all moves available for the side to move.
		/// </summary>
		/// <remarks>
		/// If the generated moves need to be legal (i.e. we don't want to include moves that would put or keep the
		/// player in check, etc.), then MoveGenerator.GetAllLegalMoves should be called instead.
		/// </remarks>
		/// <param name="b">The state of the board.</param>
		/// <param name="moves">The integer array into which the moves are added.</param>
		/// <returns>The number of moves added to the supplied moves array.</returns>
		public static int GetAllMoves(Board b, int[] moves)
		{
			return GetAllMoves(b, moves, b.WhiteToMove ? Move.Player.White : Move.Player.Black);
		}

		/// <summary>
		/// Gets all legal moves available for the side to move.
		/// </summary>
		/// <remarks>
		/// If the legality of the moves generated is not important (i.e. the potential for moves
		/// staying or moving into check is unimportant), then MoveGenerator.GetAllMoves should be called instead.
		/// </remarks>
		/// <param name="b">The state of the board.</param>
		/// <param name="moves">The integer array into which the moves are added.</param>
		/// <returns>The number of legal moves added to the supplied moves array.</returns>
		public static int GetAllLegalMoves(Board b, int[] moves)
		{
			// Get all available moves.
			int numMoves = GetAllMoves(b, moves);

			// Extract only the moves that are legal.
			int legalMoves = 0;
			for (int i = 0; i < numMoves; i++)
			{
				// Attempt to make the move.
				if (b.MakeMove(moves[i]))
				{
					// Move succeeded so then keep it (incrementing the number of legal moves)
					moves[legalMoves++] = moves[i];

					// Undo the move we just made.
					b.UndoMove();
				}
			}

			// return the number of legal moves.
			return legalMoves;
		}

		/// <summary>
		/// Gets all legal moves that result in a capture or promotion.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="moves">The integer array into which the moves are added.</param>
		/// <returns>The number of legal capture or promotion moves added to the supplied moves array.</returns>
		public static int GetAllCapturesAndPromotions(Board b, int[] moves)
		{
			// Get all legal moves
			int numLegalMoves = GetAllLegalMoves(b, moves);

			// Extract only the moves that result in a capture or promotion.
			int captureOrPromotionMoves = 0;
			for (int i = 0; i < numLegalMoves; i++)
			{
				if (Move.IsCapture(moves[i]) || Move.IsPromotion(Move.GetFlag(moves[i])))
				{
					// This is a capture or promotion move so keep it (incrementing the number of capture/promotion moves)
					moves[captureOrPromotionMoves++] = moves[i];
				}
			}

			return captureOrPromotionMoves;
		}

		/// <summary>
		/// Gets a list of all moves that can be made by the specified player.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="moves">The integer array into which the moves are added.</param>
		/// <param name="player">Enumerated value indicating the player for which all moves should be generated.</param>
		/// <returns>The number of moves added to the supplied moves array.</returns>
		private static int GetAllMoves(Board b, int[] moves, Move.Player player)
		{
			int index = 0;

			index += MoveGetter.GetPawnMoves(b, moves, index, player);
			index += MoveGetter.GetKnightMoves(b, moves, index, player);
			index += MoveGetter.GetKingMoves(b, moves, index, player);
			index += MoveGetter.GetRookMoves(b, moves, index, player);
			index += MoveGetter.GetBishopMoves(b, moves, index, player);
			index += MoveGetter.GetQueenMoves(b, moves, index, player);

			return index;
		}

		#endregion
	}
}
