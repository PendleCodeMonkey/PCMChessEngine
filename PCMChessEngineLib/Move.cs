namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Class that represents a move
	/// </summary>
	/// <remarks>
	/// A move is actually represented by an int; this class just provides functionality to easily access
	/// the various elements of a move.
	///
	/// The integer moves consist of 19 bits, as follows:
	/// 
	///  101 | 0 | 101 | 010101 | 010101 |
	///  flg | c | typ |   to   |  from  |
	/// 
	/// 'flg' indicates special events in a move (such as promotion, castling, or en passant).
	/// 'c' is the capture flag - a boolean (with 1 indicating a capture).
	/// 'typ' is the type of piece that is moving (e.g. Move.PAWN)
	/// 'to' is 6 bits representing the location (in the range 0 to 63) that the piece is moving to
	/// 'from' is 6 bits representing the location (in the range 0 to 63) that the piece is moving from
	/// </remarks>
	public class Move
	{
		#region data

		public const int PAWN = 0;
		public const int KNIGHT = 1;
		public const int BISHOP = 2;
		public const int ROOK = 3;
		public const int QUEEN = 4;
		public const int KING = 5;

		public const int NO_FLAG = 0;
		public const int FLAG_CASTLE_KINGSIDE = 1;
		public const int FLAG_CASTLE_QUEENSIDE = 2;
		public const int FLAG_EN_PASSANT = 3;
		public const int FLAG_PROMOTE_KNIGHT = 4;
		public const int FLAG_PROMOTE_BISHOP = 5;
		public const int FLAG_PROMOTE_ROOK = 6;
		public const int FLAG_PROMOTE_QUEEN = 7;

		#endregion

		public enum Player
		{
			White,
			Black
		}

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Move"/> class.
		/// </summary>
		public Move()
		{
		}

		#endregion

		#region properties


		#endregion

		#region methods

		/// <summary>
		/// Generates a new move.
		/// </summary>
		/// <param name="from">An integer in the range 0 to 63 representing where the piece is moving from.</param>
		/// <param name="to">An integer in the range 0 to 63 representing where the piece is moving to.</param>
		/// <param name="type">An integer representing the type of piece moving (e.g. Move.PAWN, Move.KNIGHT, etc.)</param>
		/// <param name="capture"><c>true</c> if the move is a capture, otherwise <c>false</c>.</param>
		/// <param name="flag">An integer representing any special features about this move (e.g. Move.FLAG_CASTLE_KINGSIDE, FLAG_EN_PASSANT, etc.)</param>
		/// <returns>An integer representing the passed information about a move.</returns>
		public static int GenerateMove(int from, int to, int type, bool capture, int flag)
		{
			return (from) | (to << 6) | (type << 12) | ((capture ? 1 : 0) << 15) | (flag << 16);
		}


		/// <summary>
		/// Gets the integer (in the range 0 to 63) representing where a move began (i.e. its "from" location).
		/// </summary>
		/// <param name="move">An integer representing the move in question.</param>
		/// <returns>The location from which this move began.</returns>
		public static int GetFrom(int move)
		{
			return move & 0x3f;
		}

		/// <summary>
		/// Gets the integer (in the range 0 to 63) representing the destination of a move (i.e. its "to" location).
		/// </summary>
		/// <param name="move">An integer representing the move in question.</param>
		/// <returns>The location to which this move went.</returns>
		public static int GetTo(int move)
		{
			return ((int)((uint)move >> 6)) & 0x3f;
		}

		/// <summary>
		/// Gets the piece that is moving (e.g. Move.PAWN, Move.KNIGHT, etc.)
		/// </summary>
		/// <param name="move">An integer representing the move in question.</param>
		/// <returns>The piece that was moved in the specified move.</returns>
		public static int GetPieceType(int move)
		{
			return ((int)((uint)move >> 12)) & 0x7;
		}

		/// <summary>
		/// Determines if a move was a capture.
		/// </summary>
		/// <param name="move">An integer representing the move in question.</param>
		/// <returns><c>true</c> if the move was a capture, otherwise <c>false</c>.</returns>
		public static bool IsCapture(int move)
		{
			return (((int)((uint)move >> 15)) & 0x1) == 1;
		}

		/// <summary>
		/// Gets any special flag relating to a move (e.g. Move.FLAG_CASTLE_KINGSIDE, ETC.)
		/// </summary>
		/// <param name="move">An integer representing the move in question.</param>
		/// <returns>The flag associated with the specified move.</returns>
		public static int GetFlag(int move)
		{
			return ((int)((uint)move >> 16)) & 0x7;
		}

		/// <summary>
		/// Determines if the specified flag represents a promotion.
		/// </summary>
		/// <param name="flag">An integer representing the flag of a move.</param>
		/// <returns><c>true</c> if the move represents a promotion, otherwise <c>false</c>.</returns>
		public static bool IsPromotion(int flag)
		{
			return flag == FLAG_PROMOTE_KNIGHT || flag == FLAG_PROMOTE_BISHOP
					|| flag == FLAG_PROMOTE_ROOK || flag == FLAG_PROMOTE_QUEEN;
		}

		#endregion
	}
}
