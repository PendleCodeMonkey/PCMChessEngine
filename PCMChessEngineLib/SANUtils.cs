namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Helper class that converts moves to/from SAN (Standard Algebraic Notation) format.
	/// </summary>
	internal class SANUtils
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="SANUtils"/> class.
		/// </summary>
		public SANUtils()
		{
		}

		#endregion

		#region methods

		/// <summary>
		/// Gets the integer representing the move that the passed SAN string describes.
		/// </summary>
		/// <param name="b">The position the move is being made from (i.e. the board state before the move was made).</param>
		/// <param name="san">The SAN string describing the move.</param>
		/// <returns>The integer representing the move that the passed SAN string describes.</returns>
		internal static int GetMove(Board b, string san)
		{
			int[] moves = new int[Board.MAX_MOVES];
			int num_moves = MoveGenerator.GetAllLegalMoves(b, moves);

			for (int i = 0; i < num_moves; i++)
			{
				if (san.Equals(GetSAN(b, moves[i])))
				{
					return moves[i];
				}
			}
			return -1;
		}

		/// <summary>
		/// Gets the SAN representation of a move.
		/// </summary>
		/// <param name="b">The position the move is being made from (i.e. the board state before the move was made).</param>
		/// <param name="move">The integer representing the move being made.</param>
		/// <returns>The SAN representation of the move.</returns>
		internal static string GetSAN(Board b, int move)
		{
			string san;

			// First handle kingside or queenside castling.
			if (Move.GetFlag(move) == Move.FLAG_CASTLE_KINGSIDE ||
				Move.GetFlag(move) == Move.FLAG_CASTLE_QUEENSIDE)
			{
				san = Move.GetFlag(move) == Move.FLAG_CASTLE_KINGSIDE ? "O-O" : "O-O-O";
				b.MakeMove(move);
				if (b.IsMate())
				{
					san += "#";
				}
				else if (b.IsCheck())
				{
					san += "+";
				}
				b.UndoMove();
				return san;
			}

			int flag = Move.GetFlag(move);
			int piece = Move.GetPieceType(move);
			int from = Move.GetFrom(move);
			int to = Move.GetTo(move);
			int from_col = BitboardUtils.GetFile(from);
			int from_row = BitboardUtils.GetRank(from);

			bool amb_file = false, amb_rank = false, amb_move = false;

			int[] moves = new int[Board.MAX_MOVES];
			int num_moves = MoveGenerator.GetAllLegalMoves(b, moves);

			for (int i = 0; i < num_moves; i++)
			{
				if (moves[i] == move || Move.GetTo(moves[i]) != to)
				{
					continue;
				}

				if (Move.IsPromotion(Move.GetFlag(move)))
				{
					if (Move.GetFlag(moves[i]) != flag)
					{
						continue;
					}
				}

				int pieceX = Move.GetPieceType(moves[i]);
				if (pieceX != piece)
				{
					continue;
				}

				int sq = Move.GetFrom(moves[i]);

				int sq_col = BitboardUtils.GetFile(sq);
				int sq_row = BitboardUtils.GetRank(sq);

				if (sq_col == from_col)
				{
					amb_file = true;
				}
				if (sq_row == from_row)
				{
					amb_rank = true;
				}

				amb_move = true;
			}

			san = BitboardUtils.GetMovingPiece(move);

			if (amb_move)
			{
				if (!amb_file)
				{
					san += BitboardUtils.GetFileString(from);
				}
				else if (!amb_rank)
				{
					san += BitboardUtils.GetRankString(from);
				}
				else
				{
					san += BitboardUtils.IntLocationToAlgebraic(from);
				}
			}
			if (Move.IsCapture(move))
			{
				if (Move.GetPieceType(move) == Move.PAWN && !amb_rank)
				{
					san += BitboardUtils.GetFileString(from);
				}
				san += "x";
			}
			san += BitboardUtils.IntLocationToAlgebraic(to);
			if (Move.IsPromotion(Move.GetFlag(move)))
			{
				san += "=";
				switch (Move.GetFlag(move))
				{
					case Move.FLAG_PROMOTE_BISHOP:
						san += "B";
						break;
					case Move.FLAG_PROMOTE_KNIGHT:
						san += "N";
						break;
					case Move.FLAG_PROMOTE_ROOK:
						san += "R";
						break;
					case Move.FLAG_PROMOTE_QUEEN:
						san += "Q";
						break;
				}
			}

			b.MakeMove(move);
			if (b.IsMate())
			{
				san += "#";
			}
			else if (b.IsCheck())
			{
				san += "+";
			}
			b.UndoMove();

			return san;
		}

		#endregion
	}
}
