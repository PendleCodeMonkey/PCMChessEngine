using System;
using System.Text;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="BitboardUtils"/> class.
	/// This class implements bitboard utility functionality.
	/// </summary>
	public class BitboardUtils
	{
		#region data

		public static int RANK_1 = 0;
		public static int RANK_2 = 1;
		public static int RANK_3 = 2;
		public static int RANK_4 = 3;
		public static int RANK_5 = 4;
		public static int RANK_6 = 5;
		public static int RANK_7 = 6;
		public static int RANK_8 = 7;
		
		public static int FILE_A = 0;
		public static int FILE_B = 1;
		public static int FILE_C = 2;
		public static int FILE_D = 3;
		public static int FILE_E = 4;
		public static int FILE_F = 5;
		public static int FILE_G = 6;
		public static int FILE_H = 7;

		// Bit table (used by SquareToIndex method)
		private static readonly byte[] bitTable ={ 63, 30, 3, 32, 25, 41, 22, 33,
													15, 50, 42, 13, 11, 53, 19, 34,
													61, 29, 2, 51, 21, 43, 45, 10,
													18, 47, 1, 54, 9, 57, 0, 35,
													62, 31, 40, 4, 49, 5, 52, 26,
													60, 6, 23, 44, 46, 27, 56, 16,
													7, 39, 48, 24, 59, 14, 12, 55,
													38, 28, 58, 20, 37, 17, 36, 8 };

		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BitboardUtils"/> class.
		/// </summary>
		static BitboardUtils()
		{
			Square = new ulong[64];
			MaskRank = new ulong[8];
			MaskFile = new ulong[8];

			for (int i = 0; i < 64; i++)
			{
				Square[i] = 1UL << i;
				MaskRank[GetRank(i)] |= Square[i];
				MaskFile[GetFile(i)] |= Square[i];
			}
		}

		#endregion

		#region properties

		/// <summary>
		/// Array of bitboards representing each of the 64 squares on the board.
		/// </summary>
		/// <remarks>
		/// Square numbering starts at 0 in the bottom left corner (square a1), with the top right square (h8) being number 63.
		/// The square at a1 is square 0, a2 is square 1, b1 is square 8, h8 is square 63.
		/// </remarks>
		public static ulong[] Square { get; }

		/// <summary>
		/// Bitboard values to apply to mask out each rank (i.e. row).
		/// </summary>
		/// <remarks>
		/// MaskRank[i] has bits set to 1 for each square on the i-th rank and bits set to 0 for all other squares.
		/// </remarks>
		public static ulong[] MaskRank { get; }

		/// <summary>
		/// Bitboard values to apply to mask out each file (i.e. column).
		/// </summary>
		/// <remarks>
		/// MaskFile[i] has bits set to 1 for each square on the i-th file and bits set to 0 for all other squares.
		/// </remarks>
		public static ulong[] MaskFile { get; }

		// Single-bit border at each edge of the board (used when generating moves in BitboardMagicAttacks).
		public static ulong SingleBorderDown => 0x00000000000000ffL;
		public static ulong SingleBorderUp => 0xff00000000000000L;
		public static ulong SingleBorderRight => 0x0101010101010101L;
		public static ulong SingleBorderLeft => 0x8080808080808080L;

		// Double thickness border (used when generating Knight moves in BitboardMagicAttacks).
		public static ulong DoubleBorderDown => 0x000000000000ffffL;
		public static ulong DoubleBorderUp => 0xffff000000000000L;
		public static ulong DoubleBorderRight => 0x0303030303030303L;
		public static ulong DoubleBorderLeft => 0xC0C0C0C0C0C0C0C0L;

		#endregion

		#region methods

		/// <summary>
		/// Gets the index location corresponding to the single set bit in a specified bitboard.
		/// </summary>
		/// <param name="bitboard">The bitboard with a single set bit.</param>
		/// <returns>The location (in the range 0 to 63) or -1 if no bit is set in the bitboard.</returns>
		public static int GetLocationFromBitboard(ulong bitboard) => BitOperations.CountTrailingZeroes(bitboard);

		/// <summary>
		/// Gets the file (i.e. column) of the specified board location.
		/// </summary>
		/// <param name="loc">The zero-based board location (value in the range 0 to 63).</param>
		/// <returns>The file (column) of the specified board location.</returns>
		public static int GetFile(int loc) => loc % 8;

		/// <summary>
		/// Gets the rank (i.e. row) of the specified board location.
		/// </summary>
		/// <param name="loc">The zero-based board location (value in the range 0 to 63).</param>
		/// <returns>The rank (row) of the specified board location.</returns>
		public static int GetRank(int loc) => loc / 8;


		/// <summary>
		/// Gets the square at the specified rank and file on the board, returning a bitboard
		/// with a single bit set that corresponds to the location of the square.
		/// </summary>
		/// <param name="rank">The rank (row) of the square's location.</param>
		/// <param name="file">The file (column) of the square's location.</param>
		/// <returns>A bitboard with a bit set at the specified rank and file.</returns>
		public static ulong GetSquare(int rank, int file) => Square[(rank * 8) + file];

		/// <summary>
		/// Gets the bitboard highlighting a single square at the specified position.
		/// </summary>
		/// <param name="position">Zero-based position in the bitboard (value in the range 0 to 63).</param>
		/// <returns>A bitboard with a bit set at the specified position.</returns>
		public static ulong GetSquare(int position) => Square[position];

		/// <summary>
		/// Retrieve the square index for the specified square bitboard value.
		/// </summary>
		/// <remarks>
		/// This method is used by BitboardMagicAttacks.
		/// </remarks>
		/// <param name="square">The bitboard value.</param>
		/// <returns>The corresponding square index.</returns>
		public static byte SquareToIndex(ulong square)
		{
			ulong b = square ^ (square - 1);
			uint fold = (uint)((b & 0xffffffff) ^ (b >> 32));
			return bitTable[(fold * 0x783a9b23) >> 26];
		}

		/// <summary>
		/// Retrieve a string representation of the supplied bitboard.
		/// </summary>
		/// <param name="bitboard">The bitboard to be output in string format.</param>
		/// <returns>The string representation of the supplied bitboard.</returns>
		public static string BitboardToString(ulong bitboard)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("     a   b   c   d   e   f   g   h");
			sb.Append(Environment.NewLine);
			sb.Append("   +---+---+---+---+---+---+---+---+");
			sb.Append(Environment.NewLine);
			sb.Append(" 8 | ");

			for (int row = 7; row >= 0; row--)
			{
				for (int col = 0; col < 8; col++)
				{
					if ((bitboard & BitboardUtils.Square[(row * 8) + col]) != 0UL)
					{
						sb.Append("X | ");
					}
					else
					{
						sb.Append("  | ");
					}
				}
				sb.Append($"{row + 1}{Environment.NewLine}   +---+---+---+---+---+---+---+---+");
				if (row != 0)
				{
					sb.Append($"{Environment.NewLine} {row} | ");
				}
			}

			sb.Append(Environment.NewLine);
			sb.Append("     a   b   c   d   e   f   g   h");
			sb.Append(Environment.NewLine);

			return sb.ToString();
		}

		/// <summary>
		/// Converts a location supplied in algebraic notation (i.e. in the form "a1", "d5", etc.) into
		/// its integer location (in the range 0 to 63)
		/// </summary>
		/// <param name="loc">The location in algebraic notation.</param>
		/// <returns>The zero-based integer representation of the location.</returns>
		public static int AlgebraicToIntLocation(string loc)
		{
			if (loc.Length == 2)
			{
				if (loc[0] >= 'a' && loc[0] <= 'h')
				{
					int col = loc[0] - 'a';
					if (int.TryParse(loc.Substring(1, 1), out int row) && row >= 1 && row <= 8)
					{
						return ((row - 1) * 8) + col;
					}
				}
			}

			// If anything other than a valid location (in algebraic notation) has been supplied then return -1.
			return -1;
		}

		/// <summary>
		/// Converts a zero-based integer location to an algebraic notation string (i.e. in the form "a1", "d5", etc.)
		/// </summary>
		/// <param name="loc">A zero-based location (in the range 0 to 63).</param>
		/// <returns>The location in algebraic notation (or '-' for an invalid location).</returns>
		public static string IntLocationToAlgebraic(int loc)
		{
			if (loc >= 0 && loc < 64)
			{
				char file = (char)(GetFile(loc) + 'a');
				char rank = (char)(GetRank(loc) + '1');
				return new string(new char[] { file, rank });
			}

			return "-";
		}

		/// <summary>
		/// Gets the algebraic file string for a specified zero-based location (in the range 0 to 63).
		/// </summary>
		/// <param name="loc">A zero-based location (in the range 0 to 63).</param>
		/// <returns>A string representing the file of the location in algebraic notation [i.e. "a" - "h"].</returns>
		public static string GetFileString(int loc)
		{
			return "abcdefgh".Substring(GetFile(loc), 1);
		}

		/// <summary>
		/// Gets the algebraic rank string for a specified zero-based location (in the range 0 to 63).
		/// </summary>
		/// <param name="loc">A zero-based location (in the range 0 to 63).</param>
		/// <returns>A string representing the rank of the location in algebraic notation [i.e. "1" - "8"].</returns>
		public static string GetRankString(int loc)
		{
			return "12345678".Substring(GetRank(loc), 1);
		}

		/// <summary>
		/// Returns a string representing the piece that is moving in the supplied integer that represents a move.
		/// </summary>
		/// <param name="move">Integer representation of the move.</param>
		/// <returns>String representing the piece that is moving ("K" for king, "Q" for queen, "R" for rook, "B" for bishop,
		/// "N" for knight, or an empty string for a pawn.</returns>
		public static string GetMovingPiece(int move)
		{
			switch (Move.GetPieceType(move))
			{
				case Move.KING:
					return "K";
				case Move.QUEEN:
					return "Q";
				case Move.ROOK:
					return "R";
				case Move.BISHOP:
					return "B";
				case Move.KNIGHT:
					return "N";
			}
			return "";
		}

		/// <summary>
		/// Convert the specified move into a string representation in the format "PIECE FROM (x/-) TO".
		/// </summary>
		/// <remarks>
		/// For a capture move the string will have an 'x' between the "from" and "to"; otherwise it will have a '-'.
		/// If the move represents kingside or queenside castling, this method will return "0-0" or "0-0-0" respectively.
		/// </remarks>
		/// <param name="move">Integer value representing the move.</param>
		/// <returns>String representation of the move.</returns>
		public static string MoveToString(int move)
		{
			if (Move.GetFlag(move) == Move.FLAG_CASTLE_KINGSIDE)
			{
				return "0-0";
			}
			if (Move.GetFlag(move) == Move.FLAG_CASTLE_QUEENSIDE)
			{
				return "0-0-0";
			}

			string s = string.Empty;

			// Add the piece type (if anything other than a pawn)
			switch (Move.GetPieceType(move))
			{
				case Move.KING:
					s += "K";
					break;
				case Move.QUEEN:
					s += "Q";
					break;
				case Move.ROOK:
					s += "R";
					break;
				case Move.BISHOP:
					s += "B";
					break;
				case Move.KNIGHT:
					s += "N";
					break;
			}
		
			// Add the "from" location (in algebraic notation)
			s += IntLocationToAlgebraic(Move.GetFrom(move));

			// If the move is a capture add "x", otherwise add "-".
			s += Move.IsCapture(move) ? "x" : "-";

			// Add the "to" location (in algebraic notation)
			s += IntLocationToAlgebraic(Move.GetTo(move));

			// Add text for additional flags (e.g. relating to en-passant, promotions, etc.)
			switch (Move.GetFlag(move))
			{
				case Move.FLAG_EN_PASSANT:
					s += " e.p.";
					break;
				case Move.FLAG_PROMOTE_QUEEN:
					s += "=Q";
					break;
				case Move.FLAG_PROMOTE_ROOK:
					s += "=R";
					break;
				case Move.FLAG_PROMOTE_BISHOP:
					s += "=B";
					break;
				case Move.FLAG_PROMOTE_KNIGHT:
					s += "=N";
					break;
			}
		
			return s;
		}

		#endregion
	}
}
