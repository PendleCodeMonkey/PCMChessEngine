using System;
using System.Collections.Generic;
using System.Text;

namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Implementation of the <see cref="Board"/> class.
	/// This class represents the state of the board.
	/// </summary>
	public class Board
	{
		#region data

		/// <summary>
		/// The longest number of moves a game can last.
		/// </summary>
		public static int MAX_GAME_LENGTH = 1024;

		/// <summary>
		/// The maximum number of possible moves there can be in a position.
		/// </summary>
		public static int MAX_MOVES = 1024;

		/// <summary>
		/// Material value applied to each piece type (used by the SEE algorithm)
		/// </summary>
		/// <remarks>
		/// The piece values are as follows:
		///		PAWN = 100
		///		KNIGHT = 325
		///		BISHOP = 325
		///		ROOK = 500
		///		QUEEN = 975
		///		KING = 999999
		/// </remarks>
		private static readonly int[] SEE_PIECE_VALUES = { 100, 325, 325, 500, 975, 999999 };

		private History _history;

		private const ulong WHITE_CASTLE_KINGSIDE_MASK = 0x90UL;                    // bitboard for squares e1 and h1 - initial locations of the white King and white King's Rook.
		private const ulong WHITE_CASTLE_QUEENSIDE_MASK = 0x11UL;                   // bitboard for squares e1 and a1 - initial locations of the white King and white Queen's Rook.
		private const ulong BLACK_CASTLE_KINGSIDE_MASK = 0x9000000000000000UL;      // bitboard for squares e8 and h8 - initial locations of the black King and black King's Rook.
		private const ulong BLACK_CASTLE_QUEENSIDE_MASK = 0x1100000000000000UL;     // bitboard for squares e8 and a8 - initial locations of the black King and black Queen's Rook.


		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Board"/> class.
		/// </summary>
		public Board()
		{
			Initialize();
		}

		/// <summary>
		/// Creates a new board based on the supplied FEN string.
		/// </summary>
		/// <param name="fen">The FEN string to read the board state from.</param>
		public Board(string fen) : this()
		{
			ReadFromFEN(fen);
		}

		#endregion

		#region properties

		// Bitboards for each of white's pieces
		public ulong WhitePawns { get; private set; }
		public ulong WhiteKnights { get; private set; }
		public ulong WhiteBishops { get; private set; }
		public ulong WhiteRooks { get; private set; }
		public ulong WhiteQueens { get; private set; }
		public ulong WhiteKing { get; private set; }

		// Bitboards for each of black's pieces
		public ulong BlackPawns { get; private set; }
		public ulong BlackKnights { get; private set; }
		public ulong BlackBishops { get; private set; }
		public ulong BlackRooks { get; private set; }
		public ulong BlackQueens { get; private set; }
		public ulong BlackKing { get; private set; }

		// Bitboards for all white's pieces, all black's pieces, and all pieces on the board.
		public ulong WhitePieces { get; private set; }
		public ulong BlackPieces { get; private set; }
		public ulong AllPieces { get; private set; }

		public bool WhiteToMove { get; private set; }
		public int EnPassantLoc { get; private set; }
		public int MoveNumber { get; private set; }

		// Flags indicating if kingside or queenside castling is permitted for each side and whether each side has already castled.
		public bool WhiteCastleKingside { get; private set; }
		public bool WhiteCastleQueenside { get; private set; }
		public bool BlackCastleKingside { get; private set; }
		public bool BlackCastleQueenside { get; private set; }
		public bool WhiteHasCastled { get; private set; }
		public bool BlackHasCastled { get; private set; }

		// Zobrist key for the board
		public ulong Key { get; private set; }

		// Initial move number - only updated when the board is being initialized.
		private int InitMoveNumber { get; set; }

		private int FiftyMoveRule { get; set; } = 0;

		public int[] PieceMaterialValues { get => SEE_PIECE_VALUES; }
		#endregion

		#region methods

		/// <summary>
		/// Initialze a new Board. Note that the default board has no pieces; in order to add pieces
		/// then the board state should be initialized (from FEN string for example)
		/// </summary>
		private void Initialize()
		{
			_history = new History();
		}

		/// <summary>
		/// Update the bitboards for all white's pieces, all black's pieces, and all pieces on the board.
		/// </summary>
		private void UpdateSpecialBitboards()
		{
			WhitePieces = WhitePawns | WhiteKnights | WhiteBishops | WhiteRooks | WhiteQueens | WhiteKing;
			BlackPieces = BlackPawns | BlackKnights | BlackBishops | BlackRooks | BlackQueens | BlackKing;

			AllPieces = WhitePieces | BlackPieces;
		}

		/// <summary>
		/// Sets up a position based on a FEN (Forsyth–Edwards Notation) string.
		/// </summary>
		/// <param name="fen">The FEN string to read the board position info from.</param>
		/// <returns><c>true</c> if the FEN string was successfully parsed; otherwise <c>false</c>.</returns>
		public bool ReadFromFEN(string fen)
		{
			string[] tokens = fen.Split(new char[] { '/', ' ' });

			// There needs to be 13 'tokens' for this to be anything like a valid FEN string.
			if (tokens == null || tokens.Length != 13)
			{
				return false;
			}

			List<string> arr = new List<string>(tokens);

			// Parse the square-description portion of the FEN (that is, the first 8 tokens) - these describe the location of the pieces on the board.
			int rank = 7;
			int file;
			for (int i = 0; i < 8; i++)
			{
				file = 0;
				foreach (char c in arr[i].ToCharArray())
				{
					if (char.IsDigit(c))
					{
						// A digit indicates the number of empty squares.
						int value = c - '0';
						file += value;
					}
					else
					{
						ulong square = BitboardUtils.GetSquare(rank, file);

						switch (c)
						{
							case 'p':
								BlackPawns |= square;
								break;
							case 'P':
								WhitePawns |= square;
								break;
							case 'n':
								BlackKnights |= square;
								break;
							case 'N':
								WhiteKnights |= square;
								break;
							case 'b':
								BlackBishops |= square;
								break;
							case 'B':
								WhiteBishops |= square;
								break;
							case 'r':
								BlackRooks |= square;
								break;
							case 'R':
								WhiteRooks |= square;
								break;
							case 'q':
								BlackQueens |= square;
								break;
							case 'Q':
								WhiteQueens |= square;
								break;
							case 'k':
								BlackKing |= square;
								break;
							case 'K':
								WhiteKing |= square;
								break;
						}
						file++;
					}
				}
				rank--;
			}

			UpdateSpecialBitboards();

			// Now we handle determining whose move it is, castling, en-passant, 50 move rule, and the move number.

			// Determine whose turn it is.
			WhiteToMove = arr[8].Equals("w");

			// Determine castling status
			WhiteCastleKingside = arr[9].Contains("K");
			WhiteCastleQueenside = arr[9].Contains("Q");
			BlackCastleKingside = arr[9].Contains("k");
			BlackCastleQueenside = arr[9].Contains("q");

			WhiteHasCastled = false;
			BlackHasCastled = false;

			// En-passant location
			EnPassantLoc = BitboardUtils.AlgebraicToIntLocation(arr[10]);

			// 50 move rule
			int.TryParse(arr[11], out int fiftyMoveRule);
			FiftyMoveRule = fiftyMoveRule;

			// Move number
			int.TryParse(arr[12], out int moveNum);
			MoveNumber = moveNum;
			InitMoveNumber = MoveNumber;

			// Finally, generate the Zobrist hash key for the board.
			Key = Zobrist.GetKeyForBoard(this);

			return true;
		}

		/// <summary>
		/// Converts this board into a human-readable form.
		/// </summary>
		/// <param name="boardOnly">
		///		<c>true</c> if only the board layout should be output or
		///		<c>false</c> if the board layout and additional game state info should be output.
		/// </param>
		/// <returns>A string containing the board in a human-readable form.</returns>
		public string ToHumanReadableString(bool boardOnly = false)
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
					sb.Append($"{GetPieceAt(row * 8 + col)} | ");
				}
				sb.Append($"{row + 1}");
				sb.Append(Environment.NewLine);
				sb.Append("   +---+---+---+---+---+---+---+---+");
				if (row != 0)
				{
					sb.Append(Environment.NewLine);
					sb.Append($" {row} | ");
				}
			}

			sb.Append(Environment.NewLine);
			sb.Append("     a   b   c   d   e   f   g   h");
			sb.Append(Environment.NewLine);
			sb.Append(Environment.NewLine);

			if (!boardOnly)
			{
				sb.Append($"Move number: {MoveNumber}");
				sb.Append(Environment.NewLine);
				sb.Append($"White to move: {BoolToYesNo(WhiteToMove)}");
				sb.Append(Environment.NewLine);
				sb.Append($"White: O-O: {BoolToYesNo(WhiteCastleKingside)}  --- O-O-O: {BoolToYesNo(WhiteCastleQueenside)}");
				sb.Append(Environment.NewLine);
				sb.Append($"Black: O-O: {BoolToYesNo(BlackCastleKingside)}  --- O-O-O: {BoolToYesNo(BlackCastleQueenside)}");
				sb.Append(Environment.NewLine);
				sb.Append($"En Passant: {EnPassantLoc} ({BitboardUtils.IntLocationToAlgebraic(EnPassantLoc)})");
				sb.Append(Environment.NewLine);
				sb.Append($"50 move rule: {FiftyMoveRule}");
				sb.Append(Environment.NewLine);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Helper method that convers a boolean value to a string - "Yes" (for true) or "No" (for false).
		/// </summary>
		/// <param name="value">The boolean value.</param>
		/// <returns>A string representation of the supplied boolean value - "Yes" (for true) or "No" (for false).</returns>
		private string BoolToYesNo(bool value) => value ? "Yes" : "No";

		/// <summary>
		/// Gets the piece that is at a specified location.
		/// </summary>
		/// <param name="loc">An integer (in  the range 0 to 63) representing a location on the board.</param>
		/// <returns>A character representing the piece at the passed location (or a space character if the square is empty).</returns>
		public char GetPieceAt(int loc)
		{
			ulong square = BitboardUtils.GetSquare(loc);

			// Check for a white piece
			if ((WhitePawns & square) != 0L)
			{
				return 'P';
			}
			if ((WhiteKnights & square) != 0L)
			{
				return 'N';
			}
			if ((WhiteBishops & square) != 0L)
			{
				return 'B';
			}
			if ((WhiteRooks & square) != 0L)
			{
				return 'R';
			}
			if ((WhiteQueens & square) != 0L)
			{
				return 'Q';
			}
			if ((WhiteKing & square) != 0L)
			{
				return 'K';
			}

			// Check for a black piece.
			if ((BlackPawns & square) != 0L)
			{
				return 'p';
			}
			if ((BlackKnights & square) != 0L)
			{
				return 'n';
			}
			if ((BlackBishops & square) != 0L)
			{
				return 'b';
			}
			if ((BlackRooks & square) != 0L)
			{
				return 'r';
			}
			if ((BlackQueens & square) != 0L)
			{
				return 'q';
			}
			if ((BlackKing & square) != 0L)
			{
				return 'k';
			}

			return ' ';     // No piece at the specified position on the board.
		}

		/// <summary>
		/// Makes a move, assuming it is a pseudo-legal move-- it is a completely
		/// legal move aside from the possibility of moving into or staying in check.
		/// 
		/// If the passed pseudo-legal move is illegal, the board will automatically
		/// revert itself to its previous state-- asking the board to make illegal
		/// moves will not do anything.
		/// </summary>
		/// <param name="move">Integer representation of the move we're making.</param>
		/// <returns><c>true</c> if the move was completed (i.e. was a legal move), otherwise <c>false</c>.</returns>
		public bool MakeMove(int move)
		{
			// Save the current board information to the history.
			SaveHistory();

			int fromLocation = Move.GetFrom(move);
			int toLocation = Move.GetTo(move);
			ulong from = BitboardUtils.GetSquare(fromLocation);
			ulong to = BitboardUtils.GetSquare(toLocation);
			int moveFlag = Move.GetFlag(move);
			int pieceMoving = Move.GetPieceType(move);
			bool capture = Move.IsCapture(move);

			FiftyMoveRule++;
			MoveNumber++;

			// First of all, check that the piece in the 'from' location actually belongs to the player whose turn it is.
			if (WhiteToMove)
			{
				if ((from & WhitePieces) == 0L)
				{
					return false;
				}
			}
			else
			{
				if ((from & BlackPieces) == 0L)
				{
					return false;
				}
			}

			// Account for captures by clearing the captured square (the 'to' square) from all arrays.
			if (capture)
			{
				// The 50 move rule count gets reset to zero on a capture.
				FiftyMoveRule = 0;

				ulong pieceToRemove = to;
				int pieceToRemoveLocation = toLocation;

				if (moveFlag == Move.FLAG_EN_PASSANT)
				{
					pieceToRemove = (WhiteToMove) ? (to >> 8) : (to << 8);
					pieceToRemoveLocation = (WhiteToMove) ? (toLocation - 8) : (toLocation + 8);
				}

				char pieceRemoved = GetPieceAt(pieceToRemoveLocation);

				if (WhiteToMove)
				{
					// Captured a black piece so clear the captured square in all of black's piece bitboards.
					BlackPawns &= ~pieceToRemove;
					BlackKnights &= ~pieceToRemove;
					BlackBishops &= ~pieceToRemove;
					BlackRooks &= ~pieceToRemove;
					BlackQueens &= ~pieceToRemove;
					BlackKing &= ~pieceToRemove;
				}
				else
				{
					// Captured a white piece so clear the captured square in all of white's piece bitboards.
					WhitePawns &= ~pieceToRemove;
					WhiteKnights &= ~pieceToRemove;
					WhiteBishops &= ~pieceToRemove;
					WhiteRooks &= ~pieceToRemove;
					WhiteQueens &= ~pieceToRemove;
					WhiteKing &= ~pieceToRemove;
				}

				// Update the board's Zobrist hash to account for the removed piece. 
				Key ^= Zobrist.GetKeyForSquare(pieceToRemoveLocation, pieceRemoved);
			}

			// Remove en-passant from board's Zobrist hash key (if it's there)
			if (EnPassantLoc != -1)
			{
				Key ^= Zobrist.EnPassantColumnKeys[BitboardUtils.GetFile(EnPassantLoc)];
			}

			// Clear the en-passant location. Note that it may be re-initialized later; however, from here, the last en-passant doesn't matter.
			EnPassantLoc = -1;

			// We OR the 'to' and 'from' bitboards into a single bitboard value containing both bits; consequently, when we then XOR this with
			// the player's piece bitboard value, it clears the 'from' bit and sets the 'to' bit (effectively making the move in a single XOR operation)
			ulong moveMask = to | from;

			// Handle the moving logic for each type of piece.
			switch (pieceMoving)
			{
				case Move.PAWN:
					// The 50 move rule count gets reset to zero on the movement of a pawn.
					FiftyMoveRule = 0;

					// Check to see if we need to update en-passant square
					if (WhiteToMove && (from << 16 & to) != 0L)
					{
						EnPassantLoc = BitboardUtils.GetLocationFromBitboard(from << 8);
					}
					if (!WhiteToMove && (from >> 16 & to) != 0L)
					{
						EnPassantLoc = BitboardUtils.GetLocationFromBitboard(from >> 8);
					}

					if (EnPassantLoc != -1)
					{
						// En-passant location has been initialized so update the board's Zobrist hash to account for it.
						Key ^= Zobrist.EnPassantColumnKeys[BitboardUtils.GetFile(EnPassantLoc)];
					}

					// Handle any promotions
					if (Move.IsPromotion(moveFlag))
					{
						// Remove the Pawn from the 'from' location
						if (WhiteToMove)
						{
							WhitePawns &= ~from;
							Key ^= Zobrist.GetKeyForSquare(fromLocation, 'P');
						}
						else
						{
							BlackPawns &= ~from;
							Key ^= Zobrist.GetKeyForSquare(fromLocation, 'p');
						}

						// Add the new promoted piece (Queen, Knight, Rook, or Bishop) to the 'to' location (effectively replacing the
						// pawn we have just removed)
						switch (moveFlag)
						{
							case Move.FLAG_PROMOTE_QUEEN:
								if (WhiteToMove)
								{
									WhiteQueens |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'Q');
								}
								else
								{
									BlackQueens |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'q');
								}
								break;
							case Move.FLAG_PROMOTE_KNIGHT:
								if (WhiteToMove)
								{
									WhiteKnights |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'N');
								}
								else
								{
									BlackKnights |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'n');
								}
								break;
							case Move.FLAG_PROMOTE_ROOK:
								if (WhiteToMove)
								{
									WhiteRooks |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'R');
								}
								else
								{
									BlackRooks |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'r');
								}
								break;
							case Move.FLAG_PROMOTE_BISHOP:
								if (WhiteToMove)
								{
									WhiteBishops |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'B');
								}
								else
								{
									BlackBishops |= to;
									Key ^= Zobrist.GetKeyForSquare(toLocation, 'b');
								}
								break;
						}
					}
					else
					{
						// Not a promotion, so just move the pawn.
						if (WhiteToMove)
						{
							WhitePawns ^= moveMask;
							Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'P');
						}
						else
						{
							BlackPawns ^= moveMask;
							Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'p');
						}
					}
					break;
				case Move.KNIGHT:
					if (WhiteToMove)
					{
						WhiteKnights ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'N');
					}
					else
					{
						BlackKnights ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'n');
					}
					break;
				case Move.BISHOP:
					if (WhiteToMove)
					{
						WhiteBishops ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'B');
					}
					else
					{
						BlackBishops ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'b');
					}
					break;
				case Move.ROOK:
					if (WhiteToMove)
					{
						WhiteRooks ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'R');
					}
					else
					{
						BlackRooks ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'r');
					}
					break;
				case Move.QUEEN:
					if (WhiteToMove)
					{
						WhiteQueens ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'Q');
					}
					else
					{
						BlackQueens ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'q');
					}
					break;
				case Move.KING:
					// First we handle any potential castling.
					// rookMoveMask is a bitboard representation of how the rook will move during castling (if at all). Bits are set in this bitboard corresponding to
					// the 'from' and 'to' locations of the Rook's movement so that, when the value is XOR'd with the player's Rooks bitboard, the rook is removed from
					// the 'from' square and added to the 'to' square.
					ulong rookMoveMask = 0UL;
					int rookFromLocation = 0;
					int rookToLocation = 0;
					if (moveFlag == Move.FLAG_CASTLE_KINGSIDE)
					{
						if (WhiteToMove)
						{
							// Set rookFromLocation to 7 (square h1), rookToLocation to 5 (square f1), and rookMoveMask to a bitboard value with bits 5 and 7 set.
							rookMoveMask = 0xa0UL;
							rookFromLocation = 7;
							rookToLocation = 5;
							WhiteHasCastled = true;
						}
						else
						{
							// Set rookFromLocation to 63 (square h8), rookToLocation to 61 (square f8), and rookMoveMask to a bitboard value with bits 61 and 63 set.
							rookMoveMask = 0xa000000000000000UL;
							rookFromLocation = 63;
							rookToLocation = 61;
							BlackHasCastled = true;
						}
					}
					if (moveFlag == Move.FLAG_CASTLE_QUEENSIDE)
					{
						if (WhiteToMove)
						{
							// Set rookFromLocation to 0 (square a1), rookToLocation to 3 (square d1), and rookMoveMask to a bitboard value with bits 0 and 3 set.
							rookMoveMask = 0x9UL;
							rookFromLocation = 0;
							rookToLocation = 3;
							WhiteHasCastled = true;
						}
						else
						{
							// Set rookFromLocation to 56 (square a8), rookToLocation to 59 (square d8), and rookMoveMask to a bitboard value with bits 56 and 59 set.
							rookMoveMask = 0x900000000000000UL;
							rookFromLocation = 56;
							rookToLocation = 59;
							BlackHasCastled = true;
						}
					}
					if (rookMoveMask != 0L)
					{
						// rookMoveMask is non-zero so castling is being carried out; therefore, we need
						// to move the rook (updating the board's Zobrist hash to account for the move)
						if (WhiteToMove)
						{
							WhiteRooks ^= rookMoveMask;
							Key ^= Zobrist.GetKeyForMove(rookFromLocation, rookToLocation, 'R');
						}
						else
						{
							BlackRooks ^= rookMoveMask;
							Key ^= Zobrist.GetKeyForMove(rookFromLocation, rookToLocation, 'r');
						}
					}

					// Now move the King (updating the board's Zobrist hash to account for the move)
					if (WhiteToMove)
					{
						WhiteKing ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'K');
					}
					else
					{
						BlackKing ^= moveMask;
						Key ^= Zobrist.GetKeyForMove(fromLocation, toLocation, 'k');
					}
					break;
			}

			// Update the bitboards that represent all pieces that are currently on the board (i.e. all white's pieces, all black's pieces, and all pieces of either colour)
			UpdateSpecialBitboards();

			// Now update the castling rights (moving a King or a Rook invalidates any future attempts to castle involving those pieces)
			if (WhiteToMove)
			{
				if ((moveMask & WHITE_CASTLE_KINGSIDE_MASK) != 0)       // Check if white King or white King's Rook has moved.
				{
					WhiteCastleKingside = false;
					Key ^= Zobrist.WhiteKingSideCastlingKey;
				}
				if ((moveMask & WHITE_CASTLE_QUEENSIDE_MASK) != 0)     // Check if white King or white Queen's Rook has moved.
				{
					WhiteCastleQueenside = false;
					Key ^= Zobrist.WhiteQueenSideCastlingKey;
				}
			}
			else
			{
				if ((moveMask & BLACK_CASTLE_KINGSIDE_MASK) != 0)      // Check if black King or black King's Rook has moved.
				{
					BlackCastleKingside = false;
					Key ^= Zobrist.BlackKingSideCastlingKey;
				}
				if ((moveMask & BLACK_CASTLE_QUEENSIDE_MASK) != 0)     // Check if black King or black Queen's Rook has moved.
				{
					BlackCastleQueenside = false;
					Key ^= Zobrist.BlackQueenSideCastlingKey;
				}
			}

			// Of course if the move leaves the player's own King in check then it's an illegal move, in which case the move is undone and we return false.
			if (OwnKingIsInCheck())
			{
				UndoMove();
				return false;
			}

			// switch which player's turn it is next (and update the board's Zobrist hash accordingly).
			WhiteToMove = !WhiteToMove;
			Key ^= Zobrist.WhiteMoveKey;

			// Return true to indicate that a valid move has been made.
			return true;
		}

		/// <summary>
		/// Makes a "null move", which essentially amounts to passing one's turn.
		/// En-passant goes away with a null move too.
		/// </summary>
		public void DoNullMove()
		{
			// Save the current board information to the history.
			SaveHistory();
			MoveNumber++;
			if (EnPassantLoc != -1)
			{
				Key ^= Zobrist.EnPassantColumnKeys[BitboardUtils.GetFile(EnPassantLoc)];
			}
			EnPassantLoc = -1;
			WhiteToMove = !WhiteToMove;
			Key ^= Zobrist.WhiteMoveKey;
		}

		/// <summary>
		/// Determines if a position has entered the endgame state.
		/// </summary>
		/// <remarks>
		/// This should not be confused with the actual end of game (which occurs when there
		/// is a checkmate or the game is a draw)
		/// Endgames are boards where both sides have (a) no queen, or (b) a queen and a bishop/knight.
		/// </remarks>
		/// <returns><c>true</c> if it's the endgame, otherwise <c>false</c>.</returns>
		public bool IsEndGame()
		{
			// q == 0 ||
			// .... ((q == 1 && n == 1 && b == 0 && r == 0)
			// .... || (q == 1 && n == 0 && b == 1 && r == 0))
			int queens = BitOperations.BitCount(WhiteQueens);
			int knights = BitOperations.BitCount(WhiteKnights);
			int bishops = BitOperations.BitCount(WhiteBishops);
			int rooks = BitOperations.BitCount(WhiteRooks);
			bool whiteEndgame = (queens == 0 && rooks <= 1) ||
							(queens == 1 && knights == 1 && bishops == 0 && rooks == 0) ||
							(queens == 1 && knights == 0 && bishops == 1 && rooks == 0);

			queens = BitOperations.BitCount(BlackQueens);
			knights = BitOperations.BitCount(BlackKnights);
			bishops = BitOperations.BitCount(BlackBishops);
			rooks = BitOperations.BitCount(BlackRooks);

			bool blackEndgame =
					(queens == 0 && rooks <= 1) ||
							(queens == 1 && knights == 1 && bishops == 0 && rooks == 0) ||
							(queens == 1 && knights == 0 && bishops == 1 && rooks == 0);

			return whiteEndgame && blackEndgame;
		}

		/// <summary>
		/// Determines if the game is over.
		/// </summary>
		/// <returns><c>true</c> if the board is currently in a checkmate or drawn position, otherwise <c>false</c>.</returns>
		public bool IsEndOfGame()
		{
			return IsMate() || IsDraw();
		}

		/// <summary>
		/// Determines if either side is currently in check.
		/// </summary>
		/// <returns><c>true</c> if either side's king is being attacked, otherwise <c>false</c>.</returns>
		public bool IsCheck()
		{
			return BitboardMagicAttacks.IsSquareAttacked(this, WhiteKing, true) ||
					BitboardMagicAttacks.IsSquareAttacked(this, BlackKing, false);
		}


		/// <summary>
		/// Determines if the side to move is currently in checkmate.
		/// </summary>
		/// <remarks>
		/// The side to move is in checkmate if they are in check and there are no legal moves that can be made.
		/// </remarks>
		/// <returns><c>true</c> if the side to move is currently in checkmate, otherwise <c>false</c>.</returns>
		public bool IsMate()
		{
			int[] moves = new int[MAX_MOVES];
			return IsCheck() && (MoveGenerator.GetAllLegalMoves(this, moves) == 0);
		}

		/// <summary>
		/// Determines if the game is a draw.
		/// </summary>
		/// <remarks>
		/// A draw may be due to stalemate, the fifty-move rule, threefold repetition, or
		/// if there is insufficient material (i.e. only Kings remaining) on the board.
		/// </remarks>
		/// <returns><c>true</c> if the current state of the game is a draw, otherwise <c>false</c>.</returns>
		public bool IsDraw()
		{
			// Check for stalemate (i.e. no legal moves can be made but the player is not in check)
			int[] moves = new int[MAX_MOVES];
			if (MoveGenerator.GetAllLegalMoves(this, moves) == 0 && !IsCheck())
			{
				return true;
			}

			// Check for the 50 move rule
			if (FiftyMoveRule >= 50)
			{
				return true;
			}

			// Check for threefold repetition (i.e. if the identical board configuration has already occurred twice before)
			//
			// We start only from MoveNumber - 50mr, and go by twos.
			// We check for identical board configurations by comparing the Zobrist hash keys.
			int reps = 0;
			for (int i = MoveNumber - FiftyMoveRule; i < MoveNumber - 2; i += 2)
			{
				if (_history.KeyHistory[i] == Key)
				{
					reps++;
				}
				if (reps == 2)
				{
					// We have found this board configuration twice before in the recent history of play.
					return true;
				}
			}

			// Check for insufficient material - i.e. check for there being only the white king
			// and black king left on the board.
			if ((WhitePieces & ~WhiteKing) == 0 && (BlackPieces & ~BlackKing) == 0)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Determines the material value of white's pieces.
		/// </summary>
		/// <remarks>
		/// By default this method does not count pawns.
		/// </remarks>
		/// <param name="includePawns"><c>true</c> if pawns should be included in the material calculation, otherwise <c>false</c>.</param>
		/// <returns>The material value of white's pieces</returns>
		public int WhitePieceMaterial(bool includePawns = false)
		{
			return SEE_PIECE_VALUES[Move.KNIGHT] * BitOperations.BitCount(WhiteKnights) +
					SEE_PIECE_VALUES[Move.BISHOP] * BitOperations.BitCount(WhiteBishops) +
					SEE_PIECE_VALUES[Move.ROOK] * BitOperations.BitCount(WhiteRooks) +
					SEE_PIECE_VALUES[Move.QUEEN] * BitOperations.BitCount(WhiteQueens) +
					(includePawns ? SEE_PIECE_VALUES[Move.PAWN] * BitOperations.BitCount(WhitePawns) : 0);
		}

		/// <summary>
		/// Determines the material value of black's pieces.
		/// </summary>
		/// <remarks>
		/// By default this method does not count pawns.
		/// </remarks>
		/// <param name="includePawns"><c>true</c> if pawns should be included in the material calculation, otherwise <c>false</c>.</param>
		/// <returns>The material value of black's pieces</returns>
		public int BlackPieceMaterial(bool includePawns = false)
		{
			return SEE_PIECE_VALUES[Move.KNIGHT] * BitOperations.BitCount(BlackKnights) +
					SEE_PIECE_VALUES[Move.BISHOP] * BitOperations.BitCount(BlackBishops) +
					SEE_PIECE_VALUES[Move.ROOK] * BitOperations.BitCount(BlackRooks) +
					SEE_PIECE_VALUES[Move.QUEEN] * BitOperations.BitCount(BlackQueens) +
					(includePawns ? SEE_PIECE_VALUES[Move.PAWN] * BitOperations.BitCount(BlackPawns) : 0);
		}

		/// <summary>
		/// Determines the material value of the pieces belonging to the side to move.
		/// </summary>
		/// <remarks>
		/// This method does not count pawns.
		/// </remarks>
		/// <returns>The material value of the side to move's pieces.</returns>
		public int MovingSideMaterial()
		{
			return (WhiteToMove) ? WhitePieceMaterial() : BlackPieceMaterial();
		}

		/// <summary>
		/// Reverts the board to its previous state.
		/// </summary>
		public void UndoMove()
		{
			UndoMove(MoveNumber - 1);
		}

		/// <summary>
		/// Reverts the board to the state at the specified move number.
		/// </summary>
		/// <param name="moveNumber">The move number to be reverted to.</param>
		private void UndoMove(int moveNumber)
		{
			if (moveNumber < 0 || moveNumber < InitMoveNumber)
			{
				// Unable to undo so do nothing.
				return;
			}

			WhitePawns = _history.WhitePawnHistory[moveNumber];
			WhiteKnights = _history.WhiteKnightHistory[moveNumber];
			WhiteBishops = _history.WhiteBishopHistory[moveNumber];
			WhiteRooks = _history.WhiteRookHistory[moveNumber];
			WhiteQueens = _history.WhiteQueenHistory[moveNumber];
			WhiteKing = _history.WhiteKingHistory[moveNumber];
			BlackPawns = _history.BlackPawnHistory[moveNumber];
			BlackKnights = _history.BlackKnightHistory[moveNumber];
			BlackBishops = _history.BlackBishopHistory[moveNumber];
			BlackRooks = _history.BlackRookHistory[moveNumber];
			BlackQueens = _history.BlackQueenHistory[moveNumber];
			BlackKing = _history.BlackKingHistory[moveNumber];
			WhitePieces = _history.WhitePiecesHistory[moveNumber];
			BlackPieces = _history.BlackPiecesHistory[moveNumber];
			AllPieces = _history.AllPiecesHistory[moveNumber];
			WhiteToMove = _history.WhiteToMoveHistory[moveNumber];
			FiftyMoveRule = _history.FiftyMoveRuleHistory[moveNumber];
			EnPassantLoc = _history.EnPassantLocHistory[moveNumber];
			WhiteCastleKingside = _history.WhiteCastleKingsideHistory[moveNumber];
			WhiteCastleQueenside = _history.WhiteCastleQueensideHistory[moveNumber];
			BlackCastleKingside = _history.BlackCastleKingsideHistory[moveNumber];
			BlackCastleQueenside = _history.BlackCastleQueensideHistory[moveNumber];
			WhiteHasCastled = _history.WhiteHasCastledHistory[moveNumber];
			BlackHasCastled = _history.BlackHasCastledHistory[moveNumber];
			Key = _history.KeyHistory[moveNumber];
			MoveNumber = moveNumber;
		}


		/// <summary>
		/// Save the current state of both player's pieces etc. to the history.
		/// </summary>
		private void SaveHistory()
		{
			_history.WhitePawnHistory[MoveNumber] = WhitePawns;
			_history.WhiteKnightHistory[MoveNumber] = WhiteKnights;
			_history.WhiteBishopHistory[MoveNumber] = WhiteBishops;
			_history.WhiteRookHistory[MoveNumber] = WhiteRooks;
			_history.WhiteQueenHistory[MoveNumber] = WhiteQueens;
			_history.WhiteKingHistory[MoveNumber] = WhiteKing;
			_history.BlackPawnHistory[MoveNumber] = BlackPawns;
			_history.BlackKnightHistory[MoveNumber] = BlackKnights;
			_history.BlackBishopHistory[MoveNumber] = BlackBishops;
			_history.BlackRookHistory[MoveNumber] = BlackRooks;
			_history.BlackQueenHistory[MoveNumber] = BlackQueens;
			_history.BlackKingHistory[MoveNumber] = BlackKing;
			_history.WhitePiecesHistory[MoveNumber] = WhitePieces;
			_history.BlackPiecesHistory[MoveNumber] = BlackPieces;
			_history.AllPiecesHistory[MoveNumber] = AllPieces;
			_history.WhiteToMoveHistory[MoveNumber] = WhiteToMove;
			_history.FiftyMoveRuleHistory[MoveNumber] = FiftyMoveRule;
			_history.EnPassantLocHistory[MoveNumber] = EnPassantLoc;
			_history.WhiteCastleKingsideHistory[MoveNumber] = WhiteCastleKingside;
			_history.WhiteCastleQueensideHistory[MoveNumber] = WhiteCastleQueenside;
			_history.BlackCastleKingsideHistory[MoveNumber] = BlackCastleKingside;
			_history.BlackCastleQueensideHistory[MoveNumber] = BlackCastleQueenside;
			_history.KeyHistory[MoveNumber] = Key;
		}

		/// <summary>
		/// Determine if the player whose move it is is currently in check.
		/// </summary>
		/// <returns><c>true</c> if the player whose move it is is currently in check, otherwise <c>false</c>.</returns>
		private bool OwnKingIsInCheck()
		{
			return WhiteToMove ? BitboardMagicAttacks.IsSquareAttacked(this, WhiteKing, true) : BitboardMagicAttacks.IsSquareAttacked(this, BlackKing, false);
		}

		/// <summary>
		/// Iterative SEE (Static Exchange Evaluation) algorithm.
		/// </summary>
		/// <remarks>
		/// A Static Exchange Evaluation (SEE) examines the consequence of a series of exchanges on a single square after a given move and
		/// calculates the likely evaluation change (material) to be lost or gained.
		/// </remarks>
		/// <param name="move">The capturing move to be evaluated.</param>
		/// <returns>The value of the resulting tradeoff.</returns>
		public int SEE(int move)
		{
			// Determine the type of piece being captured.
			int pieceCaptured = 0;
			ulong to = BitboardUtils.GetSquare(Move.GetTo(move));
			if ((to & (WhiteKnights | BlackKnights)) != 0)
			{
				pieceCaptured = Move.KNIGHT;
			}
			else if ((to & (WhiteBishops | BlackBishops)) != 0)
			{
				pieceCaptured = Move.BISHOP;
			}
			else if ((to & (WhiteRooks | BlackRooks)) != 0)
			{
				pieceCaptured = Move.ROOK;
			}
			else if ((to & (WhiteQueens | BlackQueens)) != 0)
			{
				pieceCaptured = Move.QUEEN;
			}
			else if (Move.IsCapture(move))
			{
				// By this point, the captured piece must be a pawn.
				pieceCaptured = Move.PAWN;
			}

			return SEE(Move.GetFrom(move), Move.GetTo(move), Move.GetPieceType(move), pieceCaptured);
		}

		/// <summary>
		/// Iterative SEE (Static Exchange Evaluation) algorithm.
		/// Creates a swap-list of best case material gains by traversing a 'square attacked'/'defended by' set
		/// in least valuable piece order (pawn, knight, bishop, rook, queen, then king) with alternating
		/// sides.
		/// The swap-list, a unary tree since there are no branches but just a series of captures, is
		/// negamaxed for a final static exchange evaluation.
		/// </summary>
		/// <param name="fromIndex">The position on the board we are moving from.</param>
		/// <param name="toIndex">The position on the board we are moving to.</param>
		/// <param name="pieceMoved">The type of piece being moved.</param>
		/// <param name="targetPiece">The type of piece being captured by the move.</param>
		/// <returns>The value of the resulting tradeoff.</returns>
		private int SEE(int fromIndex, int toIndex, int pieceMoved, int targetPiece)
		{
			int d = 0;
			int[] seeGain = new int[32];
			ulong mayXray = (WhitePawns | BlackPawns) | (WhiteBishops | BlackBishops) |
							(WhiteRooks | BlackRooks) | (WhiteQueens | BlackQueens);
			ulong fromSquare = BitboardUtils.GetSquare(fromIndex);
			ulong all = AllPieces;
			ulong attacks = BitboardMagicAttacks.GetIndexAttacks(this, toIndex);
			ulong fromCandidates;
			seeGain[d] = SEE_PIECE_VALUES[targetPiece];
			do
			{
				ulong side = ((d % 2 != 0 && WhiteToMove) || (d % 2 == 0 && !WhiteToMove)) ? WhitePieces : BlackPieces;

				d++;
				seeGain[d] = SEE_PIECE_VALUES[pieceMoved] - seeGain[d - 1];
				attacks ^= fromSquare;
				all ^= fromSquare;
				if ((fromSquare & mayXray) != 0)
				{
					attacks |= BitboardMagicAttacks.GetXrayAttacks(this, toIndex, all);
				}

				if ((fromCandidates = attacks & (WhitePawns | BlackPawns) & side) != 0)
				{
					pieceMoved = Move.PAWN;
				}
				else if ((fromCandidates = attacks & (WhiteKnights | BlackKnights) & side) != 0)
				{
					pieceMoved = Move.KNIGHT;
				}
				else if ((fromCandidates = attacks & (WhiteBishops | BlackBishops) & side) != 0)
				{
					pieceMoved = Move.BISHOP;
				}
				else if ((fromCandidates = attacks & (WhiteRooks | BlackRooks) & side) != 0)
				{
					pieceMoved = Move.ROOK;
				}
				else if ((fromCandidates = attacks & (WhiteQueens | BlackQueens) & side) != 0)
				{
					pieceMoved = Move.QUEEN;
				}
				else if ((fromCandidates = attacks & (WhiteKing | BlackKing) & side) != 0)
				{
					pieceMoved = Move.KING;
				}
				fromSquare = BitOperations.LowestSetBit(fromCandidates);

			} while (fromSquare != 0);

			for (int i = 0; i < 5; i++)
			{
				while (d > 1)
				{
					d--;
					seeGain[d - 1] = -Math.Max(-seeGain[d - 1], seeGain[d]);
				}
			}
			return seeGain[0];
		}

		#endregion

		/// <summary>
		/// Implementation of the <see cref="History"/> class.
		/// This class stores the history of game moves (allowing us to Undo moves, etc.)
		/// </summary>
		private class History
		{
			#region constructors

			/// <summary>
			/// Initializes a new instance of the <see cref="History"/> class.
			/// </summary>
			internal History()
			{
				Initialize();
			}

			#endregion

			#region properties

			internal ulong[] WhitePawnHistory { get; private set; }
			internal ulong[] WhiteKnightHistory { get; private set; }
			internal ulong[] WhiteBishopHistory { get; private set; }
			internal ulong[] WhiteRookHistory { get; private set; }
			internal ulong[] WhiteQueenHistory { get; private set; }
			internal ulong[] WhiteKingHistory { get; private set; }
			internal ulong[] BlackPawnHistory { get; private set; }
			internal ulong[] BlackKnightHistory { get; private set; }
			internal ulong[] BlackBishopHistory { get; private set; }
			internal ulong[] BlackRookHistory { get; private set; }
			internal ulong[] BlackQueenHistory { get; private set; }
			internal ulong[] BlackKingHistory { get; private set; }
			internal ulong[] WhitePiecesHistory { get; private set; }
			internal ulong[] BlackPiecesHistory { get; private set; }
			internal ulong[] AllPiecesHistory { get; private set; }
			internal bool[] WhiteToMoveHistory { get; private set; }
			internal int[] FiftyMoveRuleHistory { get; private set; }
			internal int[] EnPassantLocHistory { get; private set; }
			internal bool[] WhiteCastleKingsideHistory { get; private set; }
			internal bool[] WhiteCastleQueensideHistory { get; private set; }
			internal bool[] BlackCastleKingsideHistory { get; private set; }
			internal bool[] BlackCastleQueensideHistory { get; private set; }
			internal bool[] WhiteHasCastledHistory { get; private set; }
			internal bool[] BlackHasCastledHistory { get; private set; }
			internal ulong[] KeyHistory { get; private set; }

			#endregion

			#region methods

			/// <summary>
			/// Initialze the data used to store the history information for the game
			/// </summary>
			private void Initialize()
			{
				WhitePawnHistory = new ulong[MAX_GAME_LENGTH];
				WhiteKnightHistory = new ulong[MAX_GAME_LENGTH];
				WhiteBishopHistory = new ulong[MAX_GAME_LENGTH];
				WhiteRookHistory = new ulong[MAX_GAME_LENGTH];
				WhiteQueenHistory = new ulong[MAX_GAME_LENGTH];
				WhiteKingHistory = new ulong[MAX_GAME_LENGTH];
				BlackPawnHistory = new ulong[MAX_GAME_LENGTH];
				BlackKnightHistory = new ulong[MAX_GAME_LENGTH];
				BlackBishopHistory = new ulong[MAX_GAME_LENGTH];
				BlackRookHistory = new ulong[MAX_GAME_LENGTH];
				BlackQueenHistory = new ulong[MAX_GAME_LENGTH];
				BlackKingHistory = new ulong[MAX_GAME_LENGTH];
				WhitePiecesHistory = new ulong[MAX_GAME_LENGTH];
				BlackPiecesHistory = new ulong[MAX_GAME_LENGTH];
				AllPiecesHistory = new ulong[MAX_GAME_LENGTH];
				WhiteToMoveHistory = new bool[MAX_GAME_LENGTH];
				FiftyMoveRuleHistory = new int[MAX_GAME_LENGTH];
				EnPassantLocHistory = new int[MAX_GAME_LENGTH];
				WhiteCastleKingsideHistory = new bool[MAX_GAME_LENGTH];
				WhiteCastleQueensideHistory = new bool[MAX_GAME_LENGTH];
				BlackCastleKingsideHistory = new bool[MAX_GAME_LENGTH];
				BlackCastleQueensideHistory = new bool[MAX_GAME_LENGTH];
				WhiteHasCastledHistory = new bool[MAX_GAME_LENGTH];
				BlackHasCastledHistory = new bool[MAX_GAME_LENGTH];
				KeyHistory = new ulong[MAX_GAME_LENGTH];
			}

			#endregion
		}
	}
}
