namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="MoveGetter"/> class.
	/// This class implements methods to get moves for each type of piece.
	/// </summary>
	internal class MoveGetter
	{
		#region data

		// Bitboards representing the moves that can be made by a king in each of the 64 squares on the board.
		private static readonly ulong[] kingMoves = { 0x303L, 0x707L, 0xe0eL, 0x1c1cL, 0x3838L, 0x7070L, 0xe0e0L, 0xc0c0L,
			0x30303L, 0x70707L, 0xe0e0eL, 0x1c1c1cL, 0x383838L, 0x707070L, 0xe0e0e0L, 0xc0c0c0L,
			0x3030300L, 0x7070700L, 0xe0e0e00L, 0x1c1c1c00L, 0x38383800L, 0x70707000L, 0xe0e0e000L, 0xc0c0c000L,
			0x303030000L, 0x707070000L, 0xe0e0e0000L, 0x1c1c1c0000L, 0x3838380000L, 0x7070700000L, 0xe0e0e00000L, 0xc0c0c00000L,
			0x30303000000L, 0x70707000000L, 0xe0e0e000000L, 0x1c1c1c000000L, 0x383838000000L, 0x707070000000L, 0xe0e0e0000000L, 0xc0c0c0000000L,
			0x3030300000000L, 0x7070700000000L, 0xe0e0e00000000L, 0x1c1c1c00000000L, 0x38383800000000L, 0x70707000000000L, 0xe0e0e000000000L, 0xc0c0c000000000L,
			0x303030000000000L, 0x707070000000000L, 0xe0e0e0000000000L, 0x1c1c1c0000000000L, 0x3838380000000000L, 0x7070700000000000L, 0xe0e0e00000000000L, 0xc0c0c00000000000L,
			0x303000000000000L, 0x707000000000000L, 0xe0e000000000000L, 0x1c1c000000000000L, 0x3838000000000000L, 0x7070000000000000L, 0xe0e0000000000000L, 0xc0c0000000000000L };

		// Bitboards representing the moves that can be made by a knight in each of the 64 squares on the board.
		private static readonly ulong[] knightMoves = { 0x20400L, 0x50800L, 0xa1100L, 0x142200L, 0x284400L, 0x508800L, 0xa01000L, 0x402000L,
			0x2040004L, 0x5080008L, 0xa110011L, 0x14220022L, 0x28440044L, 0x50880088L, 0xa0100010L, 0x40200020L,
			0x204000402L, 0x508000805L, 0xa1100110aL, 0x1422002214L, 0x2844004428L, 0x5088008850L, 0xa0100010a0L, 0x4020002040L,
			0x20400040200L, 0x50800080500L, 0xa1100110a00L, 0x142200221400L, 0x284400442800L, 0x508800885000L, 0xa0100010a000L, 0x402000204000L,
			0x2040004020000L, 0x5080008050000L, 0xa1100110a0000L, 0x14220022140000L, 0x28440044280000L, 0x50880088500000L, 0xa0100010a00000L, 0x40200020400000L,
			0x204000402000000L, 0x508000805000000L, 0xa1100110a000000L, 0x1422002214000000L, 0x2844004428000000L, 0x5088008850000000L, 0xa0100010a0000000L, 0x4020002040000000L,
			0x400040200000000L, 0x800080500000000L, 0x1100110a00000000L, 0x2200221400000000L, 0x4400442800000000L, 0x8800885000000000L, 0x100010a000000000L, 0x2000204000000000L,
			0x4020000000000L, 0x8050000000000L, 0x110a0000000000L, 0x22140000000000L, 0x44280000000000L, 0x88500000000000L, 0x10a00000000000L, 0x20400000000000L };

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MoveGetter"/> class.
		/// </summary>
		public MoveGetter()
		{
		}

		#endregion

		#region methods

		/// <summary>
		/// Get the moves that could be made by the specified player's King. 
		/// </summary>
		/// <remarks>
		/// Takes possible King-side and Queen-side castling into consideration.
		/// </remarks>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the King's moves.</param>
		/// <param name="player">Enumerated value indicating which player's King is moving.</param>
		/// <returns>The number of King moves added to the moves array.</returns>
		public static int GetKingMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong king = player == Move.Player.White ? b.WhiteKing : b.BlackKing;
			ulong rooks = player == Move.Player.White ? b.WhiteRooks : b.BlackRooks;
			ulong playerPieces = player == Move.Player.White ? b.WhitePieces : b.BlackPieces;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			bool castleKingside = player == Move.Player.White ? b.WhiteCastleKingside : b.BlackCastleKingside;
			bool castleQueenside = player == Move.Player.White ? b.WhiteCastleQueenside : b.BlackCastleQueenside;
			int numMovesGenerated = 0;

			// Get the location of the king on the board.
			int fromLocation = BitboardUtils.GetLocationFromBitboard(king);

			// Get a bitboard containing the squares that a king at that location could move to.
			ulong moveLocations = kingMoves[fromLocation] & ~playerPieces;

			while (moveLocations != 0L)
			{
				ulong to = BitOperations.LowestSetBit(moveLocations);
				int toLocation = BitboardUtils.GetLocationFromBitboard(to);
				bool capture = (to & opponentPieces) != 0L;         // It's a capture if the 'to' location contains an opponent's piece.
				moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.KING, capture, Move.NO_FLAG);
				numMovesGenerated++;
				moveLocations &= ~to;       // Remove the current 'to' square from the bitboard of squares that can be moved to.
			}

			// If King-side castling is available then check if it is actually possible (i.e. there must be a Rook three squares to the right of the King and
			// the 2 squares between the King at that Rook must be unoccupied).
			if (castleKingside)
			{
				// Check that there is a Rook 3 squares to the right of the King and that the squares between the King and that Rook are unoccupied.
				if ((king << 3 & rooks) != 0L &&
					(king << 1 & b.AllPieces) == 0L &&
					(king << 2 & b.AllPieces) == 0L)
				{
					// Check that none of the squares that the King will be passing through are being attacked by the opposing side (as this
					// would make castling an invalid move). Castling is also illegal if the King is under attack prior to the castling (i.e. is already in check).
					if (!BitboardMagicAttacks.IsSquareAttacked(b, king, player == Move.Player.White) &&
							!BitboardMagicAttacks.IsSquareAttacked(b, king << 1, player == Move.Player.White) &&
							!BitboardMagicAttacks.IsSquareAttacked(b, king << 2, player == Move.Player.White))
					{
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, fromLocation + 2, Move.KING, false, Move.FLAG_CASTLE_KINGSIDE);
						numMovesGenerated++;
					}
				}
			}

			// If Queen-side castling is available then check if it is actually possible (i.e. there must be a Rook four squares to the left of the King and
			// the 3 squares between the King at that Rook must be unoccupied).
			if (castleQueenside)
			{
				// Check that there is a Rook 4 squares to the left of the King and that the squares between the King and that Rook are unoccupied.
				if ((king >> 4 & rooks) != 0L &&
					(king >> 1 & b.AllPieces) == 0L &&
					(king >> 2 & b.AllPieces) == 0L &&
					(king >> 3 & b.AllPieces) == 0L)
				{
					// Check that none of the squares that the King will be passing through are being attacked by the opposing side (as this
					// would make castling an invalid move). Castling is also illegal if the King is under attack prior to the castling (i.e. is already in check).
					if (!BitboardMagicAttacks.IsSquareAttacked(b, king, player == Move.Player.White) &&
							!BitboardMagicAttacks.IsSquareAttacked(b, king >> 1, player == Move.Player.White) &&
							!BitboardMagicAttacks.IsSquareAttacked(b, king >> 2, player == Move.Player.White))
					{
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, fromLocation - 2, Move.KING, false, Move.FLAG_CASTLE_QUEENSIDE);
						numMovesGenerated++;
					}
				}
			}

			// Finally, return the number of King moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}

		/// <summary>
		/// Get the moves that could be made by the specified player's Knight(s). 
		/// </summary>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the Knight(s) moves.</param>
		/// <param name="player">Enumerated value indicating which player's Knight is moving.</param>
		/// <returns>The number of Knight moves added to the moves array.</returns>
		public static int GetKnightMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong knights = player == Move.Player.White ? b.WhiteKnights : b.BlackKnights;
			ulong playerPieces = player == Move.Player.White ? b.WhitePieces : b.BlackPieces;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			int numMovesGenerated = 0;

			while (knights != 0L)
			{
				ulong from = BitOperations.LowestSetBit(knights);
				int fromLocation = BitboardUtils.GetLocationFromBitboard(from);
				ulong moveLocations = knightMoves[fromLocation] & ~playerPieces;

				while (moveLocations != 0L)
				{
					ulong to = BitOperations.LowestSetBit(moveLocations);
					int toLocation = BitboardUtils.GetLocationFromBitboard(to);
					bool capture = (to & opponentPieces) != 0L;         // It's a capture if the 'to' location contains an opponent's piece.
					int move = Move.GenerateMove(fromLocation, toLocation, Move.KNIGHT, capture, Move.NO_FLAG);
					moves[index + numMovesGenerated] = move;
					numMovesGenerated++;
					moveLocations &= ~to;       // Remove the current 'to' square from the bitboard of squares that can be moved to.
				}

				knights &= ~from;               // Remove the current Knight from the bitboard containing the player's Knights (as we've finished with this one)
			}

			// Finally, return the number of Knight moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}

		/// <summary>
		/// Get the moves that could be made by the specified player's Pawn(s). 
		/// </summary>
		/// <remarks>
		/// Takes into consideration double-square jumps, promotions, and en-passant. 
		/// </remarks>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the Pawn(s) moves.</param>
		/// <param name="player">Enumerated value indicating which player's Pawn is moving.</param>
		/// <returns>The number of Pawn moves added to the moves array.</returns>
		public static int GetPawnMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong pawns = player == Move.Player.White ? b.WhitePawns : b.BlackPawns;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			int numMovesGenerated = 0;

			while (pawns != 0L)
			{
				ulong from = BitOperations.LowestSetBit(pawns);
				int fromLocation = BitboardUtils.GetLocationFromBitboard(from);
				int rank = player == Move.Player.White ? BitboardUtils.RANK_7 : BitboardUtils.RANK_2;       // Check rank 7 for white, rank 2 for black
				ulong border = player == Move.Player.White ? BitboardUtils.SingleBorderRight : BitboardUtils.SingleBorderLeft;
				ulong shiftedFrom;
				if ((from & BitboardUtils.MaskRank[rank]) != 0L)
				{
					// Check for possible promotions. No need to check for en-passant (as that's not possible for this pawn in its current location)
					shiftedFrom = player == Move.Player.White ? from << 7 : from >> 7;
					if ((from & border) == 0L && (shiftedFrom & opponentPieces) != 0L)
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_QUEEN);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_KNIGHT);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_ROOK);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_BISHOP);
						numMovesGenerated++;
					}

					shiftedFrom = player == Move.Player.White ? from << 9 : from >> 9;
					border = player == Move.Player.White ? BitboardUtils.SingleBorderLeft : BitboardUtils.SingleBorderRight;
					if ((from & border) == 0L && (shiftedFrom & opponentPieces) != 0L)
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_QUEEN);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_KNIGHT);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_ROOK);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_PROMOTE_BISHOP);
						numMovesGenerated++;
					}

					shiftedFrom = player == Move.Player.White ? from << 8 : from >> 8;
					if ((shiftedFrom & b.AllPieces) == 0L)
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.FLAG_PROMOTE_QUEEN);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.FLAG_PROMOTE_KNIGHT);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.FLAG_PROMOTE_ROOK);
						numMovesGenerated++;
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.FLAG_PROMOTE_BISHOP);
						numMovesGenerated++;
					}
				}
				else
				{
					// Check for a capture or en-passant (no need to check for promotions here as this pawn is not on the correct rank for promotion)
					shiftedFrom = player == Move.Player.White ? from << 7 : from >> 7;
					if (((from & border) == 0L) &&
							(((shiftedFrom & opponentPieces) != 0L) || BitboardUtils.GetLocationFromBitboard(shiftedFrom) == b.EnPassantLoc))
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						if (BitboardUtils.GetLocationFromBitboard(shiftedFrom) == b.EnPassantLoc)
						{
							moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_EN_PASSANT);
						}
						else
						{
							moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.NO_FLAG);
						}
						numMovesGenerated++;
					}

					shiftedFrom = player == Move.Player.White ? from << 9 : from >> 9;
					border = player == Move.Player.White ? BitboardUtils.SingleBorderLeft : BitboardUtils.SingleBorderRight;
					if (((from & border) == 0L) &&
							(((shiftedFrom & opponentPieces) != 0L) || BitboardUtils.GetLocationFromBitboard(shiftedFrom) == b.EnPassantLoc))
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						if (BitboardUtils.GetLocationFromBitboard(shiftedFrom) == b.EnPassantLoc)
						{
							moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.FLAG_EN_PASSANT);
						}
						else
						{
							moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, true, Move.NO_FLAG);
						}
						numMovesGenerated++;
					}

					bool oneSquareAheadIsClear = false;         // Is the location one square ahead clear (used when determining if a double-square jump is possible)

					// Check for a single square forward jump
					shiftedFrom = player == Move.Player.White ? from << 8 : from >> 8;
					if ((shiftedFrom & b.AllPieces) == 0L)
					{
						oneSquareAheadIsClear = true;
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.NO_FLAG);
						numMovesGenerated++;
					}

					// Check for a possible double-square jump (which can only occur for a white pawn on rank 2 or a black pawn on rank 7, and then only when the location
					// one square ahead is clear)
					rank = player == Move.Player.White ? BitboardUtils.RANK_2 : BitboardUtils.RANK_7;       // Rank 2 for white, rank 7 for black
					shiftedFrom = player == Move.Player.White ? from << 16 : from >> 16;
					if ((from & BitboardUtils.MaskRank[rank]) != 0L &&
							oneSquareAheadIsClear && (shiftedFrom & b.AllPieces) == 0L)
					{
						int toLocation = BitboardUtils.GetLocationFromBitboard(shiftedFrom);
						moves[index + numMovesGenerated] = Move.GenerateMove(fromLocation, toLocation, Move.PAWN, false, Move.NO_FLAG);
						numMovesGenerated++;
					}
				}

				pawns &= ~from;             // Remove the current Pawn from the bitboard containing the player's Pawns (as we've finished with this one)
			}

			// Finally, return the number of Pawn moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}

		/// <summary>
		/// Get the moves that could be made by the specified player's Bishop(s). 
		/// </summary>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the Bishop(s) moves.</param>
		/// <param name="player">Enumerated value indicating which player's Bishop is moving.</param>
		/// <returns>The number of Bishop moves added to the moves array.</returns>
		public static int GetBishopMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong bishops = player == Move.Player.White ? b.WhiteBishops : b.BlackBishops;
			ulong playerPieces = player == Move.Player.White ? b.WhitePieces : b.BlackPieces;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			int numMovesGenerated = 0;

			while (bishops != 0L)
			{
				ulong from = BitOperations.LowestSetBit(bishops);
				int fromLocation = BitboardUtils.GetLocationFromBitboard(from);
				ulong moveLocations = BitboardMagicAttacks.GetBishopAttacks(fromLocation, b.AllPieces & ~from);
				moveLocations &= ~playerPieces;

				while (moveLocations != 0L)
				{
					ulong to = BitOperations.LowestSetBit(moveLocations);
					int toLocation = BitboardUtils.GetLocationFromBitboard(to);
					bool capture = (to & opponentPieces) != 0L;         // It's a capture if the 'to' location contains an opponent's piece.
					int move = Move.GenerateMove(fromLocation, toLocation, Move.BISHOP, capture, Move.NO_FLAG);
					moves[index + numMovesGenerated] = move;
					numMovesGenerated++;
					moveLocations &= ~to;       // Remove the current 'to' square from the bitboard of squares that can be moved to.
				}

				bishops &= ~from;               // Remove the current Bishop from the bitboard containing the player's Bishops (as we've finished with this one)
			}

			// Finally, return the number of Bishop moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}

		/// <summary>
		/// Get the moves that could be made by the specified player's Rook(s). 
		/// </summary>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the Rook(s) moves.</param>
		/// <param name="player">Enumerated value indicating which player's Rook is moving.</param>
		/// <returns>The number of Rook moves added to the moves array.</returns>
		public static int GetRookMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong rooks = player == Move.Player.White ? b.WhiteRooks : b.BlackRooks;
			ulong playerPieces = player == Move.Player.White ? b.WhitePieces : b.BlackPieces;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			int numMovesGenerated = 0;

			while (rooks != 0L)
			{
				ulong from = BitOperations.LowestSetBit(rooks);
				int fromLocation = BitboardUtils.GetLocationFromBitboard(from);
				ulong moveLocations = BitboardMagicAttacks.GetRookAttacks(fromLocation, b.AllPieces & ~from);
				moveLocations &= ~playerPieces;

				while (moveLocations != 0L)
				{
					ulong to = BitOperations.LowestSetBit(moveLocations);
					int toLocation = BitboardUtils.GetLocationFromBitboard(to);
					bool capture = (to & opponentPieces) != 0L;         // It's a capture if the 'to' location contains an opponent's piece.
					int move = Move.GenerateMove(fromLocation, toLocation, Move.ROOK, capture, Move.NO_FLAG);
					moves[index + numMovesGenerated] = move;
					numMovesGenerated++;
					moveLocations &= ~to;       // Remove the current 'to' square from the bitboard of squares that can be moved to.
				}

				rooks &= ~from;                 // Remove the current Rook from the bitboard containing the player's Rooks (as we've finished with this one)
			}

			// Finally, return the number of Rook moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}

		/// <summary>
		/// Get the moves that could be made by the specified player's Queen. 
		/// </summary>
		/// <remarks>
		/// Queen pieces are basically handled as a combination of a Bishop (for diagonal movements) and a Rook (for up, down, left, right movement)
		/// </remarks>
		/// <param name="b">The current state of the board.</param>
		/// <param name="moves">Array into which the new moves will be added.</param>
		/// <param name="index">The index into the moves array at which we should start adding the Queen moves.</param>
		/// <param name="player">Enumerated value indicating which player's Queen is moving.</param>
		/// <returns>The number of Queen moves added to the moves array.</returns>
		public static int GetQueenMoves(Board b, int[] moves, int index, Move.Player player)
		{
			ulong queens = player == Move.Player.White ? b.WhiteQueens : b.BlackQueens;
			ulong playerPieces = player == Move.Player.White ? b.WhitePieces : b.BlackPieces;
			ulong opponentPieces = player == Move.Player.White ? b.BlackPieces : b.WhitePieces;
			int numMovesGenerated = 0;

			while (queens != 0L)
			{
				ulong from = BitOperations.LowestSetBit(queens);
				int fromLocation = BitboardUtils.GetLocationFromBitboard(from);
				ulong moveLocations = BitboardMagicAttacks.GetQueenAttacks(fromLocation, b.AllPieces & ~from);
				moveLocations &= ~playerPieces;

				while (moveLocations != 0L)
				{
					ulong to = BitOperations.LowestSetBit(moveLocations);
					int toLocation = BitboardUtils.GetLocationFromBitboard(to);
					bool capture = (to & opponentPieces) != 0L;         // It's a capture if the 'to' location contains an opponent's piece.
					int move = Move.GenerateMove(fromLocation, toLocation, Move.QUEEN, capture, Move.NO_FLAG);
					moves[index + numMovesGenerated] = move;
					numMovesGenerated++;
					moveLocations &= ~to;       // Remove the current 'to' square from the bitboard of squares that can be moved to.
				}

				queens &= ~from;                // Remove the current Queen from the bitboard containing the player's Queens (as we've finished with this one)
			}

			// Finally, return the number of Queen moves that have been generated (and added to the moves array)
			return numMovesGenerated;
		}
		#endregion
	}
}
