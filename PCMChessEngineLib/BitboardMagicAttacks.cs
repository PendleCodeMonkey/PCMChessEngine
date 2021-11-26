using System;
using System.Text;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="BitboardMagicAttacks"/> class that generates moves using bitboards.
	/// </summary>
	/// <remarks>
	/// Heavily based on functionality in Alberto Ruibal's Carballo program (see https://github.com/albertoruibal/carballo/).
	/// </remarks>
	internal class BitboardMagicAttacks
	{
		#region data

		private static bool _initialized = false;
	
		private static readonly byte[] _rookShifts =
				{ 12, 11, 11, 11, 11, 11, 11, 12, 11, 10, 10, 10, 10, 10, 10, 11, 11, 10, 10, 10, 10, 10, 10, 11, 11, 10, 10, 10, 10, 10, 10, 11, 11, 10, 10, 10, 10, 10, 10, 11, 11, 10, 10, 10, 10, 10, 10, 11, 11, 10, 10, 10, 10, 10, 10, 11, 12, 11, 11, 11, 11, 11, 11, 12 };
	
		private static readonly byte[] _bishopShifts =
				{ 6, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 7, 7, 7, 7, 5, 5, 5, 5, 7, 9, 9, 7, 5, 5, 5, 5, 7, 9, 9, 7, 5, 5, 5, 5, 7, 7, 7, 7, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 6, 5, 5, 5, 5, 5, 5, 6 };
	
		private static readonly ulong[] _magicRookNumbers =
				{ 0x1080108000400020L, 0x40200010004000L, 0x100082000441100L, 0x480041000080080L, 0x100080005000210L, 0x100020801000400L, 0x280010000800200L, 0x100008020420100L, 0x400800080400020L, 0x401000402000L, 0x100801000200080L, 0x801000800800L, 0x800400080080L, 0x800200800400L, 0x1000200040100L, 0x4840800041000080L, 0x20008080004000L, 0x404010002000L, 0x808010002000L, 0x828010000800L, 0x808004000800L, 0x14008002000480L, 0x40002100801L, 0x20001004084L, 0x802080004000L, 0x200080400080L, 0x810001080200080L, 0x10008080080010L, 0x4000080080040080L, 0x40080020080L, 0x1000100040200L, 0x80008200004124L, 0x804000800020L, 0x804000802000L, 0x801000802000L, 0x2000801000800804L, 0x80080800400L, 0x80040080800200L, 0x800100800200L, 0x8042000104L, 0x208040008008L, 0x10500020004000L, 0x100020008080L, 0x2000100008008080L, 0x200040008008080L, 0x8020004008080L, 0x1000200010004L, 0x100040080420001L, 0x80004000200040L, 0x200040100140L, 0x20004800100040L, 0x100080080280L, 0x8100800400080080L, 0x8004020080040080L, 0x9001000402000100L, 0x40080410200L, 0x208040110202L, 0x800810022004012L, 0x1000820004011L, 0x1002004100009L, 0x41001002480005L, 0x81000208040001L, 0x4000008201100804L, 0x2841008402L };
		private static readonly ulong[] _magicBishopNumbers =
				{ 0x1020041000484080L, 0x20204010a0000L, 0x8020420240000L, 0x404040085006400L, 0x804242000000108L, 0x8901008800000L, 0x1010110400080L, 0x402401084004L, 0x1000200810208082L, 0x20802208200L, 0x4200100102082000L, 0x1024081040020L, 0x20210000000L, 0x8210400100L, 0x10110022000L, 0x80090088010820L, 0x8001002480800L, 0x8102082008200L, 0x41001000408100L, 0x88000082004000L, 0x204000200940000L, 0x410201100100L, 0x2000101012000L, 0x40201008200c200L, 0x10100004204200L, 0x2080020010440L, 0x480004002400L, 0x2008008008202L, 0x1010080104000L, 0x1020001004106L, 0x1040200520800L, 0x8410000840101L, 0x1201000200400L, 0x2029000021000L, 0x4002400080840L, 0x5000020080080080L, 0x1080200002200L, 0x4008202028800L, 0x2080210010080L, 0x800809200008200L, 0x1082004001000L, 0x1080202411080L, 0x840048010101L, 0x40004010400200L, 0x500811020800400L, 0x20200040800040L, 0x1008012800830a00L, 0x1041102001040L, 0x11010120200000L, 0x2020222020c00L, 0x400002402080800L, 0x20880000L, 0x1122020400L, 0x11100248084000L, 0x210111000908000L, 0x2048102020080L, 0x1000108208024000L, 0x1004100882000L, 0x41044100L, 0x840400L, 0x4208204L, 0x80000200282020cL, 0x8a001240100L, 0x2040104040080L };

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BitboardMagicAttacks"/> class.
		/// </summary>
		static BitboardMagicAttacks()
		{
			if (!_initialized)
			{
				WhitePawn = new ulong[64];
				BlackPawn = new ulong[64];
				Knight = new ulong[64];
				King = new ulong[64];
				Rook = new ulong[64];
				RookMask = new ulong[64];
				RookMagic = new ulong[64][];
				Bishop = new ulong[64];
				BishopMask = new ulong[64];
				BishopMagic = new ulong[64][];

				GenerateAttacks();
			}
		}

		#endregion

		#region properties

		public static ulong[] WhitePawn { get; }
		public static ulong[] BlackPawn { get; }
		public static ulong[] Knight { get; }
		public static ulong[] King { get; }

		public static ulong[] Rook { get; }
		public static ulong[] RookMask { get; }
		public static ulong[][] RookMagic { get; }
		public static ulong[] Bishop { get; }
		public static ulong[] BishopMask { get; }
		public static ulong[][] BishopMagic { get; }
		#endregion

		#region methods

		/// <summary>
		/// Determines if a specified square is being attacked for a given side.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="square">Bitboard containing the target square.</param>
		/// <param name="white"><c>true</c> when checking for white being attacked, otherwise <c>false</c>.</param>
		/// <returns><c>true</c> if the square is being attacked, otherwise <c>false</c>.</returns>
		public static bool IsSquareAttacked(Board b, ulong square, bool white)
		{
			return IsIndexAttacked(b, BitboardUtils.SquareToIndex(square), white);
		}

		/// <summary>
		/// Returns a bitboard containing all attackers of a specified square.
		/// </summary>
		/// <param name="b">The board to consider.</param>
		/// <param name="i">An integer (in the range 0 to 63) representing the targeted position.</param>
		/// <returns>A bitboard containing the locations of all attackers of the square.</returns>
		public static ulong GetIndexAttacks(Board b, int i)
		{
			if (i < 0 || i > 63)
			{
				// Invalid board position.
				return 0UL;
			}

			ulong all = b.AllPieces;

			return ((b.BlackPieces & BitboardMagicAttacks.WhitePawn[i] | b.WhitePieces &
					BitboardMagicAttacks.BlackPawn[i]) & (b.WhitePawns | b.BlackPawns)) |
					(BitboardMagicAttacks.King[i] & (b.WhiteKing | b.BlackKing)) |
					(BitboardMagicAttacks.Knight[i] & (b.WhiteKnights | b.BlackKnights)) |
					(GetRookAttacks(i, all) & ((b.WhiteRooks | b.BlackRooks) | (b.WhiteQueens | b.BlackQueens))) |
					(GetBishopAttacks(i, all) & ((b.WhiteBishops | b.BlackBishops) | (b.WhiteQueens | b.BlackQueens)));
		}

		/// <summary>
		/// Finds all attackers that attack a square through another square.
		/// </summary>
		/// <remarks>
		/// Calling this method is exactly the same as calling GetXrayAttacks(b, i, b.all_pieces)
		/// </remarks>
		/// <param name="b">The board representing the position to consider.</param>
		/// <param name="i">The target location (in the range 0 to 63).</param>
		/// <returns></returns>
		public static ulong GetXrayAttacks(Board b, int i)
		{
			return GetXrayAttacks(b, i, b.AllPieces);
		}

		/// <summary>
		/// Finds all attackers that attack a square through another square.
		/// </summary>
		/// <param name="b">The board to consider.</param>
		/// <param name="i">The target location (in the range 0 to 63).</param>
		/// <param name="all">A bitboard representing the places through which we can move.</param>
		/// <returns></returns>
		public static ulong GetXrayAttacks(Board b, int i, ulong all)
		{
			if (i < 0 || i > 63)
			{
				return 0;
			}

			return ((GetRookAttacks(i, all) & ((b.WhiteRooks | b.BlackRooks) | (b.WhiteQueens | b.BlackQueens))) |
				(GetBishopAttacks(i, all) & ((b.WhiteBishops | b.BlackBishops) | (b.WhiteQueens | b.BlackQueens)))) & all;
		}

		/// <summary>
		/// Gets a bitboard containing all squares a rook at a specified location can move to.
		/// </summary>
		/// <param name="index">The location of the rook.</param>
		/// <param name="all">The locations of all pieces on the board.</param>
		/// <returns>The locations the rook can move to.</returns>
		public static ulong GetRookAttacks(int index, ulong all)
		{
			int i = Transform(all & RookMask[index], _magicRookNumbers[index], _rookShifts[index]);
			return RookMagic[index][i];
		}

		/// <summary>
		/// Gets a bitboard containing all squares a bishop at a specified location can move to.
		/// </summary>
		/// <param name="index">The location of the bishop.</param>
		/// <param name="all">The locations of all pieces on the board.</param>
		/// <returns>The locations the bishop can move to.</returns>
		public static ulong GetBishopAttacks(int index, ulong all)
		{
			int i = Transform(all & BishopMask[index], _magicBishopNumbers[index], _bishopShifts[index]);
			return BishopMagic[index][i];
		}

		/// <summary>
		/// Gets a bitboard containing all squares a queen at a specified location can move to.
		/// </summary>
		/// <param name="index">The location of the queen.</param>
		/// <param name="all">The locations of all pieces on the board.</param>
		/// <returns>The locations the queen can move to.</returns>
		public static ulong GetQueenAttacks(int index, ulong all)
		{
			// A queen's moves are basically just a combination of the moves that a rook and a bishop can make.
			return GetRookAttacks(index, all) | GetBishopAttacks(index, all);
		}


		// private methods


		/// <summary>
		/// Generate lists of bitboards representing the squares that could be attacked by
		/// each type of piece when it is located at each of the 64 square on the board. 
		/// </summary>
		/// <remarks>
		/// Generates arrays of bitboards (one per square on the board, starting at the bottom left corner at index 0
		/// and working through to the top right corner at index 63) with one bitboard array for each type of piece.
		/// Each bitboard holds a bit representation of the squares that the piece ccould attack from that square.
		/// For example, WhitePawn[0] contains a bitboard representing the squares that a white pawn could attack from square 0 (i.e. square a1),
		/// Knight[36] contains a bitboard representing the squares that any knight [white or black] could attack from square 36 (i.e. square e5)
		/// </remarks>
		private static void GenerateAttacks()
		{
			ulong square = 1ul;
			byte i = 0;
			while (square != 0)
			{
				WhitePawn[i] = SquareAttacked(square, 7, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderRight) |
							SquareAttacked(square, 9, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderLeft);

				BlackPawn[i] = SquareAttacked(square, -7, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, -9, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderRight);

				Knight[i] = SquareAttacked(square, +17, BitboardUtils.DoubleBorderUp | BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, +15, BitboardUtils.DoubleBorderUp | BitboardUtils.SingleBorderRight) |
							SquareAttacked(square, -15, BitboardUtils.DoubleBorderDown | BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, -17, BitboardUtils.DoubleBorderDown | BitboardUtils.SingleBorderRight) |
							SquareAttacked(square, +10, BitboardUtils.SingleBorderUp | BitboardUtils.DoubleBorderLeft) |
							SquareAttacked(square, +6, BitboardUtils.SingleBorderUp | BitboardUtils.DoubleBorderRight) |
							SquareAttacked(square, -6, BitboardUtils.SingleBorderDown | BitboardUtils.DoubleBorderLeft) |
							SquareAttacked(square, -10, BitboardUtils.SingleBorderDown | BitboardUtils.DoubleBorderRight);

				King[i] = SquareAttacked(square, +8, BitboardUtils.SingleBorderUp) |
							SquareAttacked(square, -8, BitboardUtils.SingleBorderDown) |
							SquareAttacked(square, -1, BitboardUtils.SingleBorderRight) |
							SquareAttacked(square, +1, BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, +9, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, +7, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderRight) |
							SquareAttacked(square, -7, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderLeft) |
							SquareAttacked(square, -9, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderRight);

				Rook[i] = SquareAttackedSlider(square, +8, BitboardUtils.SingleBorderUp) |
							SquareAttackedSlider(square, -8, BitboardUtils.SingleBorderDown) |
							SquareAttackedSlider(square, -1, BitboardUtils.SingleBorderRight) |
							SquareAttackedSlider(square, +1, BitboardUtils.SingleBorderLeft);

				RookMask[i] = SquareAttackedSlider(square, +8, BitboardUtils.SingleBorderUp, true) |
							SquareAttackedSlider(square, -8, BitboardUtils.SingleBorderDown, true) |
							SquareAttackedSlider(square, -1, BitboardUtils.SingleBorderRight, true) |
							SquareAttackedSlider(square, +1, BitboardUtils.SingleBorderLeft, true);

				Bishop[i] = SquareAttackedSlider(square, +9, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderLeft) |
							SquareAttackedSlider(square, +7, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderRight) |
							SquareAttackedSlider(square, -7, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderLeft) |
							SquareAttackedSlider(square, -9, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderRight);

				BishopMask[i] = SquareAttackedSlider(square, +9, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderLeft, true) |
							SquareAttackedSlider(square, +7, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderRight, true) |
							SquareAttackedSlider(square, -7, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderLeft, true) |
							SquareAttackedSlider(square, -9, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderRight, true);


				// And now generate magics
				int rookPositions = (1 << _rookShifts[i]);
				RookMagic[i] = new ulong[rookPositions];
				for (int j = 0; j < rookPositions; j++)
				{
					ulong pieces = BitboardMagicAttacks.GeneratePieces(j, _rookShifts[i], RookMask[i]);
					int magicIndex = Transform(pieces, _magicRookNumbers[i], _rookShifts[i]);
					RookMagic[i][magicIndex] = GetRookShiftAttacks(square, pieces);
				}

				int bishopPositions = (1 << _bishopShifts[i]);
				BishopMagic[i] = new ulong[bishopPositions];
				for (int j = 0; j < bishopPositions; j++)
				{
					ulong pieces = BitboardMagicAttacks.GeneratePieces(j, _bishopShifts[i], BishopMask[i]);
					int magicIndex = Transform(pieces, _magicBishopNumbers[i], _bishopShifts[i]);
					BishopMagic[i][magicIndex] = GetBishopShiftAttacks(square, pieces);
				}

				// Move onto the next square.
				square <<= 1;
				i++;
			}

			_initialized = true;
		}


		/// <summary>
		/// Determine the squares that can be reached (i.e. attacked) from the specified square in a given direction.
		/// </summary>
		/// <remarks>
		/// This method is called when determining potential attacking moves for non-sliding pieces (i.e. those having restricted movement such as
		/// those that can only move by one square when attacking [e.g. Pawns or Kings] or have other restricted movement [e.g. Knights]).
		/// 
		/// The shift parameter indicates the direction of movement and is used to shift the bitboard pattern in order to reach the square
		/// that is being attacked (e.g. a shift value of +8 is a move one square up, -8 is a move one square down, +9 is a move up one row and right one column, etc.)
		/// </remarks>
		/// <param name="square">The bitboard value for the square being considered.</param>
		/// <param name="shift">The amount to shift the bitboard value for the direction of movement.</param>
		/// <param name="border">The border bitboard value that is used to constrain the movement in the given direction (in order to
		///   prevent attempted moves off the edge of the board).</param>
		/// <returns>The bitboard representing the square(s) that can be attacked in the direction indicated by the shift.</returns>
		private static ulong SquareAttacked(ulong square, int shift, ulong border)
		{
			if ((square & border) == 0)
			{
				if (shift > 0)
				{
					square <<= shift;
				}
				else
				{
					square >>= -shift;
				}
				return square;
			}
			return 0;
		}

		/// <summary>
		/// Determine the squares that can be reached (i.e. attacked) from the specified square in a given direction.
		/// </summary>
		/// <remarks>
		/// This method is called when determining potential attacking moves for sliding pieces (i.e. those that can move several squares in
		/// a single move [e.g. Bishops, Rooks, or Queens]. Note that Queens are handled as a combination of Bishop and Rook moves.
		/// 
		/// The shift parameter indicates the direction of movement and is used to shift the bitboard pattern in order to reach the squares
		/// that are being attacked (e.g. a shift value of +8 is for upward verical moves, -8 is for downward vertical moves, +9 is for diagonal up and right moves, etc.)
		/// </remarks>
		/// <param name="square">The bitboard value for the square being considered.</param>
		/// <param name="shift">The amount to shift the bitboard value for the direction of movement.</param>
		/// <param name="border">The border bitboard value that is used to constrain the movement in the given direction (in order to
		///   prevent attempted moves off the edge of the board).</param>
		/// <param name="maskBorder"><c>true</c> if border squares should be masked out of the resulting bitboard, otherwise <c>false</c>.</param>
		/// <returns>The bitboard representing the square(s) that can be attacked in the direction indicated by the shift.</returns>
		private static ulong SquareAttackedSlider(ulong square, int shift, ulong border, bool maskBorder = false)
		{
			ulong ret = 0;
			while ((square & border) == 0)
			{
				if (shift > 0)
				{
					square <<= shift;
				}
				else
				{
					square >>= -shift;
				}
				if (!maskBorder || (square & border) == 0)
				{
					ret |= square;
				}
			}
			return ret;
		}

		/// <summary>
		/// Determines if a position on the board is being attacked for a given side.
		/// </summary>
		/// <param name="b">The state of the board.</param>
		/// <param name="squareIndex">Zero-based position on the board (in the range 0 to 63).</param>
		/// <param name="white"><c>true</c> when checking if white is being attacked, otherwise <c>false</c>.</param>
		/// <returns><c>true</c> if the position on the board is being attacked, otherwise <c>false</c>.</returns>
		private static bool IsIndexAttacked(Board b, byte squareIndex, bool white)
		{
			if (squareIndex < 0 || squareIndex > 63)
			{
				// Invalid board position.
				return false;
			}

			ulong others = (white ? b.BlackPieces : b.WhitePieces);
			ulong all = b.AllPieces;

			if (((white ? BitboardMagicAttacks.WhitePawn[squareIndex] : BitboardMagicAttacks.BlackPawn[squareIndex])
					& (b.WhitePawns | b.BlackPawns) & others) != 0)
			{
				return true;
			}
			if ((BitboardMagicAttacks.King[squareIndex] & (b.WhiteKing | b.BlackKing) & others) != 0)
			{
				return true;
			}
			if ((BitboardMagicAttacks.Knight[squareIndex] & (b.WhiteKnights | b.BlackKnights) & others) != 0)
			{
				return true;
			}
			if ((GetRookAttacks(squareIndex, all) & ((b.WhiteRooks | b.BlackRooks) | (b.WhiteQueens | b.BlackQueens)) & others) != 0)
			{
				return true;
			}
			if ((GetBishopAttacks(squareIndex, all) & ((b.WhiteBishops | b.BlackBishops) | (b.WhiteQueens | b.BlackQueens)) & others) != 0)
			{
				return true;
			}

			return false;
		}

		private static int Transform(ulong b, ulong magic, byte bits) => (int)((b * magic) >> (64 - bits));

		private static ulong GeneratePieces(int index, int bits, ulong mask)
		{
			ulong result = 0L;
			for (int i = 0; i < bits; i++)
			{
				ulong  lsb = BitOperations.LowestSetBit(mask);
				mask ^= lsb;
				if ((index & (1 << i)) != 0)
				{
					result |= lsb;
				}
			}
			return result;
		}

		private static ulong GetRookShiftAttacks(ulong square, ulong all)
		{
			return SquareAttackedAux(square, all, +8, BitboardUtils.SingleBorderUp) |
					SquareAttackedAux(square, all, -8, BitboardUtils.SingleBorderDown) |
					SquareAttackedAux(square, all, -1, BitboardUtils.SingleBorderRight) |
					SquareAttackedAux(square, all, +1, BitboardUtils.SingleBorderLeft);
		}

		private static ulong GetBishopShiftAttacks(ulong square, ulong all)
		{
			return SquareAttackedAux(square, all, +9, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderLeft) |
					SquareAttackedAux(square, all, +7, BitboardUtils.SingleBorderUp | BitboardUtils.SingleBorderRight) |
					SquareAttackedAux(square, all, -7, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderLeft) |
					SquareAttackedAux(square, all, -9, BitboardUtils.SingleBorderDown | BitboardUtils.SingleBorderRight);
		}

		private static ulong SquareAttackedAux(ulong square, ulong all, int shift, ulong border)
		{
			ulong ret = 0;
			while ((square & border) == 0)
			{
				if (shift > 0)
				{
					square <<= shift;
				}
				else
				{
					square >>= -shift;
				}
				ret |= square;

				if ((square & all) != 0)
				{
					break;
				}
			}
			return ret;
		}

		#endregion
	}
}
