using System;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Class that implements a complete evaluator. Takes into consideration material, pawn
	/// structure, king safety, piece placement, and rook-pawn cooperation (rooks
	/// on open files, rooks supporting passed pawns).
	/// </summary>
	internal class CompleteEvaluator : IEvaluator
	{
		#region data

		private static readonly int DoubledPawnPenalty = 10;
		private static readonly int IsolatedPawnPenalty = 10;
		private static readonly int BackwardPawnPenalty = 8;
		private static readonly int PassedPawnBonus = 20;
		private static readonly int BishopPairBonus = 50;
		private static readonly int RookDefendingPassedPawnBonus = 20;
		private static readonly int RookOnOpenFileBonus = 20;
		private static readonly int TwoRooksOnOpenFileBonus = 10;
		private static readonly int StrongShieldBonus = 9;
		private static readonly int WeakShieldBonus = 4;

		private static readonly int[] OwnPawnSafetyBonus = { 0, 8, 4, 2, 0, 0, 0, 0 };
		private static readonly int[] OppositionPawnSafetyBonus = { 0, 2, 1, 0, 0, 0, 0, 0 };
		private static readonly int[] KnightSafetyBonus = { 0, 4, 4, 0, 0, 0, 0, 0 };
		private static readonly int[] BishopSafetyBonus = { 0, 5, 4, 3, 2, 1, 0, 0 };
		private static readonly int[] RookSafetyBonus = { 0, 7, 5, 4, 3, 0, 0, 0 };
		private static readonly int[] QueenSafetyBonus = { 0, 10, 8, 5, 4, 0, 0, 0 };

		private static readonly ulong[] PassedPawnWhite;		// No pawns ahead that are in same
		private static readonly ulong[] PassedPawnBlack;        // or adjacent file.
		private static readonly ulong[] IsolatedPawnWhite;		// There are pawns in adjacent
		private static readonly ulong[] IsolatedPawnBlack;      // columns.
		private static readonly ulong[] BackwardPawnWhite;		// There are pawns that can
		private static readonly ulong[] BackwardPawnBlack;      // defend you.

		private static readonly ulong[] StrongKingSafetyWhite;   // Strong King safety is determined by three
		private static readonly ulong[] StrongKingSafetyBlack;   // nearest squares in file in front.
		private static readonly ulong[] WeakKingSafetyWhite;     // Weak King safety is determined by three
		private static readonly ulong[] WeakKingSafetyBlack;     // nearest squares in file two in front.


		// Array holding the Chebyshev distance from each square on the board to each square.
		// e.g. DISTANCES[0, x] give the distance from square 0 to square x, DISTANCES[10, x] give the distance from square 10 to square x, etc.
		private static readonly int[,] Distances;

		// Black piece-square tables.
		private static readonly int[] PawnPosBlack = {
			0,   0,   0,   0,   0,   0,   0,   0,
			5,  10,  15,  20,  20,  15,  10,   5,
			4,   8,  12,  16,  16,  12,   8,   4,
			3,   6,   9,  12,  12,   9,   6,   3,
			2,   4,   6,   8,   8,   6,   4,   2,
			1,   2,   3, -10, -10,   3,   2,   1,
			0,   0,   0, -40, -40,   0,   0,   0,
			0,   0,   0,   0,   0,   0,   0,   0
		};

		private static readonly int[] KnightPosBlack = {
			-10, -10, -10, -10, -10, -10, -10, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10, -30, -10, -10, -10, -10, -30, -10
		};

		private static readonly int[] BishopPosBlack = {
			-10, -10, -10, -10, -10, -10, -10, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10, -10, -20, -10, -10, -20, -10, -10
		};

		private static readonly int[] RookPosBlack = {
			  0,   0,   0,   0,   0,   0,   0,   0,
			 15,  15,  15,  15,  15,  15,  15,  15,
			  0,   0,   0,   0,   0,   0,   0,   0,
			  0,   0,   0,   0,   0,   0,   0,   0,
			  0,   0,   0,   0,   0,   0,   0,   0,
			  0,   0,   0,   0,   0,   0,   0,   0,
			  0,   0,   0,   0,   0,   0,   0,   0,
			-10,   0,   0,  10,  10,   0,   0, -10
		};

		private static readonly int[] QueenPosBlack = {
			-10, -10, -10, -10, -10, -10, -10, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,  10,  10,   5,   0, -10,
			-10,   0,   5,   5,   5,   5,   0, -10,
			-10,   0,   0,   0,   0,   0,   0, -10,
			-10, -10, -20, -10, -10, -20, -10, -10
		};

		private static readonly int[] KingPosOpeningAndMiddlegameBlack = {
			-40, -40, -40, -40, -40, -40, -40, -40,
			-40, -40, -40, -40, -40, -40, -40, -40,
			-40, -40, -40, -40, -40, -40, -40, -40,
			-40, -40, -40, -40, -40, -40, -40, -40,
			-40, -40, -40, -40, -40, -40, -40, -40,
			-40, -40, -40, -40, -40, -40, -40, -40,
			-20, -20, -20, -20, -20, -20, -20, -20,
			  0,  20,  40, -20,   0, -20,  40,  20
		};

		private static readonly int[] KingPosEndgameBlack = {
			  0,  10,  20,  30,  30,  20,  10,   0,
			 10,  20,  30,  40,  40,  30,  20,  10,
			 20,  30,  40,  50,  50,  40,  30,  20,
			 30,  40,  50,  60,  60,  50,  40,  30,
			 30,  40,  50,  60,  60,  50,  40,  30,
			 20,  30,  40,  50,  50,  40,  30,  20,
			 10,  20,  30,  40,  40,  30,  20,  10,
			  0,  10,  20,  30,  30,  20,  10,   0
		};

		// White piece-square tables (Note: values are initialized in the constructor using corresponding black piece-square table values)	
		private static readonly int[] PawnPosWhite;
		private static readonly int[] KnightPosWhite;
		private static readonly int[] BishopPosWhite;
		private static readonly int[] RookPosWhite;
		private static readonly int[] QueenPosWhite;
		private static readonly int[] KingPosOpeningAndMiddlegameWhite;
		private static readonly int[] KingPosEndgameWhite;

		// Board location mirror values (for converting the locations of player's pieces to the locations of the equivalent opponent piece)
		// So, for example, location 0 (square a1 - white Queen's Rook) maps to location 56 (square a8 - black Queen's Rook), etc.	
		private static readonly int[] Mirror = {
			56,  57,  58,  59,  60,  61,  62,  63,
			48,  49,  50,  51,  52,  53,  54,  55,
			40,  41,  42,  43,  44,  45,  46,  47,
			32,  33,  34,  35,  36,  37,  38,  39,
			24,  25,  26,  27,  28,  29,  30,  31,
			16,  17,  18,  19,  20,  21,  22,  23,
			8,   9,  10,  11,  12,  13,  14,  15,
			0,   1,   2,   3,   4,   5,   6,   7
		};

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="CompleteEvaluator"/> class.
		/// </summary>
		static CompleteEvaluator()
		{
			PassedPawnWhite = new ulong[64];
			IsolatedPawnWhite = new ulong[64];
			BackwardPawnWhite = new ulong[64];

			for (int i = 0; i < 64; i++)
			{
				int rank = BitboardUtils.GetRank(i);
				int file = BitboardUtils.GetFile(i);

				for (int j = rank; j < 8; j++)
				{
					if (file > 0)
					{
						PassedPawnWhite[i] |= BitboardUtils.GetSquare(j, file - 1);
					}
					PassedPawnWhite[i] |= BitboardUtils.GetSquare(j, file);
					if (file < 7)
					{
						PassedPawnWhite[i] |= BitboardUtils.GetSquare(j, file + 1);
					}
				}

				for (int j = 0; j < 8; j++)
				{
					if (file > 0)
					{
						IsolatedPawnWhite[i] |= BitboardUtils.GetSquare(j, file - 1);
					}
					if (file < 7)
					{
						IsolatedPawnWhite[i] |= BitboardUtils.GetSquare(j, file + 1);
					}
				}

				for (int j = 0; j < rank; j++)
				{
					if (file > 0)
					{
						BackwardPawnWhite[i] |= BitboardUtils.GetSquare(j, file - 1);
					}
					if (file < 7)
					{
						BackwardPawnWhite[i] |= BitboardUtils.GetSquare(j, file + 1);
					}
				}
			}

			StrongKingSafetyWhite = new ulong[64];
			WeakKingSafetyWhite = new ulong[64];

			// pawn shields for white only first three ranks
			for (int i = 0; i < 8 * 3; i++)
			{
				int file = BitboardUtils.GetFile(i);
				StrongKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 8);
				WeakKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 8);

				if (file > 0)
				{
					StrongKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 7);
				}
				else
				{
					StrongKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 10);
				}
				if (file < 7)
				{
					StrongKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 9);
				}
				else
				{
					StrongKingSafetyWhite[i] |= BitboardUtils.GetSquare(i + 6);
				}

				WeakKingSafetyWhite[i] = StrongKingSafetyWhite[i] << 8;
			}

			// Calculate Chebychev distances for each square on the board (a 64 x 64 array of ints)		
			Distances = new int[64, 64];
			for (int i = 0; i < 64; i++)
			{
				int fileFrom = BitboardUtils.GetFile(i);
				int rankFrom = BitboardUtils.GetRank(i);

				for (int j = 0; j < 64; j++)
				{
					int fileTo = BitboardUtils.GetFile(j);
					int rankTo = BitboardUtils.GetRank(j);

					if (Math.Abs(rankFrom - rankTo) > Math.Abs(fileFrom - fileTo))
					{
						Distances[i, j] = Math.Abs(rankFrom - rankTo);
					}
					else
					{
						Distances[i, j] = Math.Abs(fileFrom - fileTo);
					}
				}
			}

			PawnPosWhite = new int[64];
			KnightPosWhite = new int[64];
			BishopPosWhite = new int[64];
			RookPosWhite = new int[64];
			QueenPosWhite = new int[64];
			KingPosOpeningAndMiddlegameWhite = new int[64];
			KingPosEndgameWhite = new int[64];
			PassedPawnBlack = new ulong[64];
			IsolatedPawnBlack = new ulong[64];
			BackwardPawnBlack = new ulong[64];
			StrongKingSafetyBlack = new ulong[64];
			WeakKingSafetyBlack = new ulong[64];

			for (int i = 0; i < 64; i++)
			{
				// Initialize White player arrays using the values stored for the Black player, employing location mirroring.
				PawnPosWhite[i] = PawnPosBlack[Mirror[i]];
				KnightPosWhite[i] = KnightPosBlack[Mirror[i]];
				BishopPosWhite[i] = BishopPosBlack[Mirror[i]];
				RookPosWhite[i] = RookPosBlack[Mirror[i]];
				QueenPosWhite[i] = QueenPosBlack[Mirror[i]];
				KingPosOpeningAndMiddlegameWhite[i] = KingPosOpeningAndMiddlegameBlack[Mirror[i]];
				KingPosEndgameWhite[i] = KingPosEndgameBlack[Mirror[i]];

				for (int j = 0; j < 64; j++)
				{
					if ((PassedPawnWhite[i] & BitboardUtils.GetSquare(j)) != 0L)
					{
						PassedPawnBlack[Mirror[i]] |= BitboardUtils.GetSquare(Mirror[j]);
					}
					if ((IsolatedPawnWhite[i] & BitboardUtils.GetSquare(j)) != 0L)
					{
						IsolatedPawnBlack[Mirror[i]] |= BitboardUtils.GetSquare(Mirror[j]);
					}
					if ((BackwardPawnWhite[i] & BitboardUtils.GetSquare(j)) != 0L)
					{
						BackwardPawnBlack[Mirror[i]] |= BitboardUtils.GetSquare(Mirror[j]);
					}
					if ((StrongKingSafetyWhite[i] & BitboardUtils.GetSquare(j)) != 0L)
					{
						StrongKingSafetyBlack[Mirror[i]] |= BitboardUtils.GetSquare(Mirror[j]);
					}
					if ((WeakKingSafetyWhite[i] & BitboardUtils.GetSquare(j)) != 0L)
					{
						WeakKingSafetyBlack[Mirror[i]] |= BitboardUtils.GetSquare(Mirror[j]);
					}
				}
			}
		}

		#endregion

		#region methods

		/// <summary>
		/// Evaluate the state of the supplied board and return a score value for the player whose turn it is.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <returns>The score evaluated for the specified board.</returns>
		public int Eval(Board b)
		{
			if (b.IsMate())
			{
				return int.MinValue + b.MoveNumber;
			}
			if (b.IsDraw())
			{
				return 0;
			}

			// Calculate the score from White's perspective. We account for whose turn it is at the end of this method (returning a negated score for Black)
			int score = 0;

			int whiteKingLoc = BitboardUtils.GetLocationFromBitboard(b.WhiteKing);
			int blackKingLoc = BitboardUtils.GetLocationFromBitboard(b.BlackKing);

			bool endgame = b.IsEndGame();

			ulong whitePassedPawns = 0L;
			ulong blackPassedPawns = 0L;

			// Determine the number of white and black pieces (excluding the Kings)
			int numWhitePieces = BitOperations.BitCount(b.WhitePieces) - 1;
			int numBlackPiecess = BitOperations.BitCount(b.BlackPieces) - 1;

			// Evaluate the material imbalance.
			if (b.WhitePieceMaterial(includePawns: true) > b.BlackPieceMaterial(includePawns: true))
			{
				score += 45 + 3 * numWhitePieces - 6 * numBlackPiecess;
			}
			else
			{
				score -= 45 + 3 * numBlackPiecess - 6 * numWhitePieces;
			}

			// Adjust score for White Pawns
			ulong temp = b.WhitePawns;
			while (temp != 0L)
			{
				score += b.PieceMaterialValues[Move.PAWN];

				ulong pawn = BitOperations.LowestSetBit(temp);
				int pawnLocation = BitboardUtils.GetLocationFromBitboard(pawn);

				score += PawnPosWhite[pawnLocation];

				score += OppositionPawnSafetyBonus[Distances[pawnLocation, blackKingLoc]];

				if (endgame)
				{
					score += OwnPawnSafetyBonus[Distances[pawnLocation, whiteKingLoc]];
				}

				if ((IsolatedPawnWhite[pawnLocation] & b.WhitePawns) == 0)
				{
					score -= IsolatedPawnPenalty;
				}
				else
				{
					if ((BitboardMagicAttacks.WhitePawn[pawnLocation + 8] & b.BlackPawns) != 0
							&& (BackwardPawnWhite[pawnLocation] & b.WhitePawns) == 0)
					{
						score -= BackwardPawnPenalty;
					}
				}

				if ((PassedPawnWhite[pawnLocation] & b.BlackPawns) == 0)
				{
					score += PassedPawnBonus;
					whitePassedPawns |= pawn;
				}

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(pawnLocation)] & (b.WhitePawns ^ pawn)) != 0)
				{
					score -= DoubledPawnPenalty;
				}

				// We've finished with this Pawn, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~pawn;
			}

			// Adjust score for Black Pawns
			temp = b.BlackPawns;
			while (temp != 0L)
			{
				score -= b.PieceMaterialValues[Move.PAWN];

				ulong pawn = BitOperations.LowestSetBit(temp);
				int pawnLocation = BitboardUtils.GetLocationFromBitboard(pawn);

				score -= PawnPosBlack[pawnLocation];

				score -= OppositionPawnSafetyBonus[Distances[pawnLocation, whiteKingLoc]];

				if (endgame)
				{
					score -= OwnPawnSafetyBonus[Distances[pawnLocation, blackKingLoc]];
				}

				if ((IsolatedPawnBlack[pawnLocation] & b.BlackPawns) == 0)
				{
					score += IsolatedPawnPenalty;
				}
				else
				{
					if ((BitboardMagicAttacks.BlackPawn[pawnLocation - 8] & b.WhitePawns) != 0
							&& (BackwardPawnBlack[pawnLocation] & b.BlackPawns) == 0)
					{
						score += BackwardPawnPenalty;
					}
				}

				if ((PassedPawnBlack[pawnLocation] & b.WhitePawns) == 0)
				{
					score += PassedPawnBonus;
					blackPassedPawns |= pawn;
				}

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(pawnLocation)] & (b.BlackPawns ^ pawn)) != 0)
				{
					score += DoubledPawnPenalty;
				}

				// We've finished with this Pawn, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~pawn;
			}

			// Adjust score for White Knights
			temp = b.WhiteKnights;
			while (temp != 0L)
			{
				score += b.PieceMaterialValues[Move.KNIGHT];
				ulong knight = BitOperations.LowestSetBit(temp);
				int knightLocation = BitboardUtils.GetLocationFromBitboard(knight);

				score += KnightPosWhite[knightLocation];

				score += KnightSafetyBonus[Distances[knightLocation, blackKingLoc]];

				// We've finished with this Knight, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~knight;
			}

			// Adjust score for Black Knights
			temp = b.BlackKnights;
			while (temp != 0L)
			{
				score -= b.PieceMaterialValues[Move.KNIGHT];
				ulong knight = BitOperations.LowestSetBit(temp);
				int knightLocation = BitboardUtils.GetLocationFromBitboard(knight);

				score -= KnightPosBlack[knightLocation];

				score -= KnightSafetyBonus[Distances[knightLocation, whiteKingLoc]];

				// We've finished with this Knight, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~knight;
			}

			// Adjust score for White Bishops
			temp = b.WhiteBishops;
			while (temp != 0L)
			{
				score += b.PieceMaterialValues[Move.BISHOP];
				ulong bishop = BitOperations.LowestSetBit(temp);
				int bishopLocation = BitboardUtils.GetLocationFromBitboard(bishop);

				score += BishopPosWhite[bishopLocation];

				score += BishopSafetyBonus[Distances[bishopLocation, blackKingLoc]];

				// We've finished with this Bishop, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~bishop;
			}

			// Adjust score for Black Bishops
			temp = b.BlackBishops;
			while (temp != 0L)
			{
				score -= b.PieceMaterialValues[Move.BISHOP];
				ulong bishop = BitOperations.LowestSetBit(temp);
				int bishopLocation = BitboardUtils.GetLocationFromBitboard(bishop);

				score -= BishopPosBlack[bishopLocation];

				score -= BishopSafetyBonus[Distances[bishopLocation, whiteKingLoc]];

				// We've finished with this Bishop, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~bishop;
			}

			// Adjust score for Bishop pairs
			if (BitOperations.BitCount(b.WhiteBishops) > 1)
			{
				score += BishopPairBonus;
			}
			if (BitOperations.BitCount(b.BlackBishops) > 1)
			{
				score -= BishopPairBonus;
			}

			// Adjust score for White Rooks
			temp = b.WhiteRooks;
			while (temp != 0L)
			{
				score += b.PieceMaterialValues[Move.ROOK];
				ulong rook = BitOperations.LowestSetBit(temp);
				int rookLocation = BitboardUtils.GetLocationFromBitboard(rook);

				score += RookPosWhite[rookLocation];

				score += RookSafetyBonus[Distances[rookLocation, blackKingLoc]];

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & whitePassedPawns) != 0)
				{
					ulong pawnOnSameFile = BitOperations.HighestSetBit(BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & whitePassedPawns);
					if (BitboardUtils.GetLocationFromBitboard(rook) < BitboardUtils.GetLocationFromBitboard(pawnOnSameFile))
					{
						score += RookDefendingPassedPawnBonus;
					}
				}

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & b.BlackPawns) == 0L)
				{
					score += RookOnOpenFileBonus;

					if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & (b.WhiteRooks & ~rook)) != 0L)
					{
						score += TwoRooksOnOpenFileBonus;
					}
				}

				// We've finished with this Rook, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~rook;
			}

			// Adjust score for Black Rooks
			temp = b.BlackRooks;
			while (temp != 0L)
			{
				score -= b.PieceMaterialValues[Move.ROOK];
				ulong rook = BitOperations.LowestSetBit(temp);
				int rookLocation = BitboardUtils.GetLocationFromBitboard(rook);

				score -= RookPosBlack[rookLocation];

				score -= RookSafetyBonus[Distances[rookLocation, whiteKingLoc]];

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & blackPassedPawns) != 0)
				{
					ulong pawnOnSameFile = BitOperations.HighestSetBit(BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & blackPassedPawns);
					if (BitboardUtils.GetLocationFromBitboard(rook) < BitboardUtils.GetLocationFromBitboard(pawnOnSameFile))
					{
						score -= RookDefendingPassedPawnBonus;
					}
				}

				if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & b.WhitePawns) == 0L)
				{
					score -= RookOnOpenFileBonus;

					if ((BitboardUtils.MaskFile[BitboardUtils.GetFile(rookLocation)] & (b.BlackRooks & ~rook)) != 0L)
					{
						score -= TwoRooksOnOpenFileBonus;
					}
				}

				// We've finished with this Rook, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~rook;
			}

			// Adjust score for White Queens
			temp = b.WhiteQueens;
			while (temp != 0L)
			{
				score += b.PieceMaterialValues[Move.QUEEN];
				ulong queen = BitOperations.LowestSetBit(temp);
				int queenLocation = BitboardUtils.GetLocationFromBitboard(queen);

				score += QueenPosWhite[queenLocation];

				score += QueenSafetyBonus[Distances[queenLocation, blackKingLoc]];

				// We've finished with this Queen, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~queen;
			}

			// Adjust score for Black Queens
			temp = b.BlackQueens;
			while (temp != 0L)
			{
				score -= b.PieceMaterialValues[Move.QUEEN];
				ulong queen = BitOperations.LowestSetBit(temp);
				int queenLocation = BitboardUtils.GetLocationFromBitboard(queen);

				score -= QueenPosBlack[queenLocation];

				score -= QueenSafetyBonus[Distances[queenLocation, whiteKingLoc]];

				// We've finished with this Queen, so mask it out of the bitboard so we don't try to handle it again.
				temp &= ~queen;
			}

			if (endgame)
			{
				score += KingPosEndgameWhite[whiteKingLoc];
				score -= KingPosEndgameBlack[blackKingLoc];
			}
			else
			{
				// Adjust score for White King
				score += KingPosOpeningAndMiddlegameWhite[whiteKingLoc];

				score += StrongShieldBonus * BitOperations.BitCount(StrongKingSafetyWhite[whiteKingLoc] & b.WhitePawns);

				score += WeakShieldBonus * BitOperations.BitCount(WeakKingSafetyWhite[whiteKingLoc] & b.WhitePawns);

				// Adjust score for Black King
				score -= KingPosOpeningAndMiddlegameBlack[blackKingLoc];

				score -= StrongShieldBonus * BitOperations.BitCount(StrongKingSafetyBlack[blackKingLoc] & b.BlackPawns);

				score -= WeakShieldBonus * BitOperations.BitCount(WeakKingSafetyBlack[blackKingLoc] & b.BlackPawns);
			}

			return b.WhiteToMove ? score : -score;
		}

		#endregion
	}
}
