using PendleCodeMonkey.ChessEngineLib;
using System.Drawing;
using System.Windows.Forms;

namespace PendleCodeMonkey.ChessEngineWinFormsApp
{
	/// <summary>
	/// A chess board control.
	/// </summary>
	public class ChessBoardCtrl : Panel
	{
		public const int PIECE_SIZE = 45;
		public const int PIECE_MARGIN = 4;
		public const int BOARD_MARGIN = 20;

		// Index of each piece's image (in the _imagePieces image list)
		private const int BLACK_BISHOP_PIECE = 0;
		private const int BLACK_KING_PIECE = 1;
		private const int BLACK_KNIGHT_PIECE = 2;
		private const int BLACK_PAWN_PIECE = 3;
		private const int BLACK_QUEEN_PIECE = 4;
		private const int BLACK_ROOK_PIECE = 5;
		private const int WHITE_BISHOP_PIECE = 6;
		private const int WHITE_KING_PIECE = 7;
		private const int WHITE_KNIGHT_PIECE = 8;
		private const int WHITE_PAWN_PIECE = 9;
		private const int WHITE_QUEEN_PIECE = 10;
		private const int WHITE_ROOK_PIECE = 11;
		private const int NO_PIECE = -1;                // indicates that there is no piece on the square

		private ImageList _imagePieces = null;
		private ChessEngine _engine = null;             // the chess engine

		private readonly bool _playerIsWhite = true;                // indicates if the human is playing white.

		// The colours to use for the light and dark squares on the board.
		private readonly Color _light_square_colour = Color.FromArgb(241, 233, 231);
		private readonly Color _dark_square_colour = Color.FromArgb(157, 82, 79);

		// The colours to be used for square highlighting (when a piece is being dragged around) and for the highlighting of
		// the current selected suggested move.
		private readonly Color _highlightedSquareFillColour = Color.Gold;
		private readonly Color _highlightedSquareBorderColour = Color.DarkGoldenrod;
		private readonly Color _moveHighlightColour = Color.MediumSeaGreen;

		public delegate void MouseEventDelegate(int row, int col);


		private MouseEventDelegate _mouseDownDel = null;
		private MouseEventDelegate _mouseUpDel = null;
		private MouseEventDelegate _mouseMoveDel = null;

		private int _highlightedRow = -1;
		private int _highlightedCol = -1;

		private int _highlightedMoveStartRow = -1;
		private int _highlightedMoveStartCol = -1;
		private int _highlightedMoveEndRow = -1;
		private int _highlightedMoveEndCol = -1;

		private Point _highlightMoveArrowStart = new Point();
		private Point _highlightMoveArrowEnd = new Point();

		private int _draggedPieceIndex = NO_PIECE;
		private int _draggedPieceOrigin = -1;
		private int _draggedPieceXOffset = 0;
		private int _draggedPieceYOffset = 0;
		private Point _currentDragPosition = new Point();

		private int _computerMovingPieceIndex = NO_PIECE;
		private int _computerMoveOrigin = -1;
		public Point _computerMovingPiecePos = new Point();

		/// <summary>
		/// Gets or sets a value indicating if the game has ended.
		/// </summary>
		public bool GameHasEnded { get; set; }

		public void SetImageList(ImageList images)
		{
			_imagePieces = images;
		}

		internal void SetChessEngine(ChessEngine engine)
		{
			_engine = engine;

			DoubleBuffered = true;
		}

		public void AttachEventHandlers(MouseEventDelegate mouseDownDel, MouseEventDelegate mouseUpDel, MouseEventDelegate mouseMoveDel)
		{
			_mouseDownDel = mouseDownDel;
			_mouseUpDel = mouseUpDel;
			_mouseMoveDel = mouseMoveDel;

			this.MouseDown += ChessBoardCtrl_MouseDown;
			this.MouseUp += ChessBoardCtrl_MouseUp;
			this.MouseMove += ChessBoardCtrl_MouseMove;
			this.MouseLeave += ChessBoardCtrl_MouseLeave;
		}

		void ChessBoardCtrl_MouseLeave(object sender, System.EventArgs e)
		{
			if (_draggedPieceIndex != NO_PIECE)
			{
				_draggedPieceIndex = NO_PIECE;
				_highlightedRow = -1;
				_highlightedCol = -1;
				Invalidate();
			}
		}

		public void DetachEventHandlers()
		{
			this.MouseDown -= ChessBoardCtrl_MouseDown;
			this.MouseUp -= ChessBoardCtrl_MouseUp;
			this.MouseMove -= ChessBoardCtrl_MouseMove;
		}

		public int[] GetStartAndEndPositions(int from, int to)
		{
			int fromRow = from / 8;
			int fromCol = from % 8;
			int toRow = to / 8;
			int toCol = to % 8;

			int startX = (fromCol * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN);
			int startY = ((7 - fromRow) * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN);
			int endX = (toCol * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN);
			int endY = ((7 - toRow) * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN);

			return new int[] { startX, startY, endX, endY };
		}

		void ChessBoardCtrl_MouseMove(object sender, MouseEventArgs e)
		{
			if (GameHasEnded)
			{
				return;
			}

			if (e.X > BOARD_MARGIN && e.Y > BOARD_MARGIN &&
				e.X < Size.Width - BOARD_MARGIN && e.Y < Size.Height - BOARD_MARGIN)
			{
				int row = (e.Y - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));
				int col = (e.X - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));

				// If we are actually dragging a piece
				if (_draggedPieceIndex != NO_PIECE)
				{
					if (_mouseMoveDel != null)
					{
						_mouseMoveDel(row, col);
					}

					// Update the current position
					_currentDragPosition = new Point(e.X, e.Y);

					// and invalidate the control to draw the piece being dragged.
					Invalidate();
				}
			}
			else
			{
				if (_draggedPieceIndex != NO_PIECE)
				{
					_highlightedRow = -1;
					_highlightedCol = -1;
					Invalidate();
				}
			}
		}

		void ChessBoardCtrl_MouseUp(object sender, MouseEventArgs e)
		{
			if (GameHasEnded)
			{
				return;
			}

			if (e.X > BOARD_MARGIN && e.Y > BOARD_MARGIN &&
				e.X < Size.Width - BOARD_MARGIN && e.Y < Size.Height - BOARD_MARGIN)
			{
				int row = (e.Y - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));
				int col = (e.X - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));

				if (_mouseUpDel != null)
				{
					_mouseUpDel(row, col);
				}
				_draggedPieceIndex = NO_PIECE;
			}
			else
			{
				_draggedPieceIndex = NO_PIECE;
				_highlightedRow = -1;
				_highlightedCol = -1;
				Invalidate();
			}
		}

		void ChessBoardCtrl_MouseDown(object sender, MouseEventArgs e)
		{
			if (GameHasEnded)
			{
				return;
			}

			if (e.X > BOARD_MARGIN && e.Y > BOARD_MARGIN &&
				e.X < Size.Width - BOARD_MARGIN && e.Y < Size.Height - BOARD_MARGIN)
			{
				int row = (e.Y - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));
				int col = (e.X - BOARD_MARGIN) / (PIECE_SIZE + (PIECE_MARGIN * 2));

				// Get the index of the piece that we are attempting to move and
				// the offset into the square of the current cursor position.
				_draggedPieceIndex = GetPieceAtSquare((7 - row), col);
				if (_draggedPieceIndex != NO_PIECE)
				{
					if ((_playerIsWhite && (_draggedPieceIndex >= WHITE_BISHOP_PIECE && _draggedPieceIndex <= WHITE_ROOK_PIECE)) ||
						(!_playerIsWhite && (_draggedPieceIndex >= BLACK_BISHOP_PIECE && _draggedPieceIndex <= BLACK_ROOK_PIECE)))
					{
						_draggedPieceOrigin = ((7 - row) * 8) + col;
						_draggedPieceXOffset = (e.X - BOARD_MARGIN) - (col * (PIECE_SIZE + (PIECE_MARGIN * 2)));
						_draggedPieceYOffset = (e.Y - BOARD_MARGIN) - (row * (PIECE_SIZE + (PIECE_MARGIN * 2)));

						if (_mouseDownDel != null)
						{
							_mouseDownDel(row, col);
						}
					}
					else
					{
						_draggedPieceIndex = NO_PIECE;
					}
				}
			}
		}

		public void SetHighlightedSquare(int row, int col)
		{
			_highlightedRow = row;
			_highlightedCol = col;
			Invalidate();
		}

		public void SetHighlightedMoveSquares(int fromRow, int fromCol, int toRow, int toCol)
		{
			_highlightedMoveStartRow = fromRow;
			_highlightedMoveStartCol = fromCol;
			_highlightedMoveEndRow = toRow;
			_highlightedMoveEndCol = toCol;
			Invalidate();
		}

		public void SetupComputerMovingPiece(int from)
		{
			_computerMoveOrigin = from;
			int fromRow = from / 8;
			int fromCol = from % 8;
			_computerMovingPieceIndex = GetPieceAtSquare(fromRow, fromCol);
		}

		public void FinishComputerMovingPiece()
		{
			_computerMovingPieceIndex = NO_PIECE;
		}

		public void UpdateComputerMovingPiecePosition(int x, int y)
		{
			_computerMovingPiecePos = new Point(x, y);
			Invalidate();
		}

		/// <summary>
		/// Draw the piece image at the specified position
		/// </summary>
		/// <param name="gr">The Graphics object to be rendered to.</param>
		/// <param name="pieceIndex">The index of the pisce.</param>
		/// <param name="position">The position on the board (0 is the bottom left square, 63 is the top right square)</param>
		private void DrawImage(Graphics gr, int pieceIndex, int position)
		{
			if (pieceIndex >= 0 && pieceIndex <= WHITE_ROOK_PIECE &&
				position >= 0 && position < 64)
			{
				int row = (position / 8);
				int col = position % 8;
				int xPos = col * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN;
				int yPos = (7 - row) * (PIECE_SIZE + (PIECE_MARGIN * 2)) + PIECE_MARGIN + BOARD_MARGIN;
				gr.DrawImage((Bitmap)_imagePieces.Images[((int)pieceIndex)], new Point(xPos, yPos));
			}
		}

		private void DrawImageAtPosition(Graphics gr, int pieceIndex, Point pos)
		{
			if (pieceIndex >= 0 && pieceIndex <= WHITE_ROOK_PIECE)
			{
				int x = pos.X;
				int y = pos.Y;

				gr.DrawImage((Bitmap)_imagePieces.Images[((int)pieceIndex)], new Point(x, y));
			}
		}

		private void DrawBoard(Graphics g)
		{
			// Render the board
			int squareSize = PIECE_SIZE + (PIECE_MARGIN * 2);
			using (Brush brush1 = new SolidBrush(_light_square_colour))
			{
				using Brush brush2 = new SolidBrush(_dark_square_colour);
				using Brush brush3 = new SolidBrush(_highlightedSquareFillColour);
				using Pen highlightPen = new Pen(_highlightedSquareBorderColour, 8.0f);
				_highlightMoveArrowStart = new Point();
				_highlightMoveArrowEnd = new Point();
				using Pen highlightMovePen = new Pen(Color.FromArgb(192, _moveHighlightColour), 4.0f);
				for (int row = 0; row < 8; ++row)
				{
					for (int col = 0; col < 8; ++col)
					{
						Rectangle rect = new Rectangle((col * squareSize) + BOARD_MARGIN, (row * squareSize) + BOARD_MARGIN, squareSize, squareSize);
						if (row != _highlightedRow || col != _highlightedCol)
						{
							g.FillRectangle(((row + col) % 2 == 0) ? brush1 : brush2, rect);
						}
						else
						{
							g.FillRectangle(brush3, rect);
						}

						if (row == _highlightedRow && col == _highlightedCol)
						{
							Rectangle r2 = new Rectangle(rect.Location, rect.Size);
							r2.Inflate(-4, -4);
							g.DrawRectangle(highlightPen, r2);
						}
						if (row == _highlightedMoveStartRow && col == _highlightedMoveStartCol)
						{
							_highlightMoveArrowStart = new Point(rect.Left + (squareSize / 2), rect.Top + (squareSize / 2));
							Rectangle r2 = new Rectangle(rect.Location, rect.Size);
							r2.Inflate(-4, -4);
							g.DrawRectangle(highlightMovePen, r2);
						}
						if (row == _highlightedMoveEndRow && col == _highlightedMoveEndCol)
						{
							_highlightMoveArrowEnd = new Point(rect.Left + (squareSize / 2), rect.Top + (squareSize / 2));
							Rectangle r2 = new Rectangle(rect.Location, rect.Size);
							r2.Inflate(-4, -4);
							g.DrawRectangle(highlightMovePen, r2);
						}
					}
				}
			}

			// Draw a border around the board.
			using (Pen pen = new Pen(Color.Gray, (float)BOARD_MARGIN))
			{
				Rectangle rect = new Rectangle(new Point(0, 0), new Size(this.Size.Width - 1, this.Size.Height - 1));
				rect.Inflate(-BOARD_MARGIN / 2, -BOARD_MARGIN / 2);
				g.DrawRectangle(pen, rect);
			}

			// Draw rank and file labels within the border area.
			string files = "abcdefgh";
			string ranks = "87654321";
			using Font drawFont = new Font("Arial", 12);
			using SolidBrush drawBrush = new SolidBrush(Color.White);
			int y = 0;
			int yBottom = this.Size.Height - BOARD_MARGIN - 1;
			int x = BOARD_MARGIN + (squareSize / 2) - 8;
			StringFormat drawFormat = new StringFormat();
			for (int i = 0; i < 8; i++)
			{
				string file = files.Substring(i, 1);
				g.DrawString(file, drawFont, drawBrush, x, y, drawFormat);
				g.DrawString(file, drawFont, drawBrush, x, yBottom, drawFormat);
				x += squareSize;
			}
			y = BOARD_MARGIN + (squareSize / 2) - 8;
			int xRight= this.Size.Width - BOARD_MARGIN - 1;
			for (int i = 0; i < 8; i++)
			{
				string rank = ranks.Substring(i, 1);
				g.DrawString(rank, drawFont, drawBrush, 3, y, drawFormat);
				g.DrawString(rank, drawFont, drawBrush, xRight + 3, y, drawFormat);
				y += squareSize;
			}

		}

		/// <summary>
		/// Get the type of piece at the specified row and column.
		/// </summary>
		/// <param name="row">Row index (0 is bottom row)</param>
		/// <param name="col">Column index (0 is left hand column)</param>
		/// <returns>The index of the piece at the specified location.</returns>
		private int GetPieceAtSquare(int row, int col)
		{
			int squarePos = (row * 8) + col;
			char c = _engine.Get(squarePos);
			int pieceIndex = NO_PIECE;
			switch (c)
			{
				case 'P':
					pieceIndex = WHITE_PAWN_PIECE;
					break;
				case 'K':
					pieceIndex = WHITE_KING_PIECE;
					break;
				case 'N':
					pieceIndex = WHITE_KNIGHT_PIECE;
					break;
				case 'B':
					pieceIndex = WHITE_BISHOP_PIECE;
					break;
				case 'Q':
					pieceIndex = WHITE_QUEEN_PIECE;
					break;
				case 'R':
					pieceIndex = WHITE_ROOK_PIECE;
					break;
				case 'p':
					pieceIndex = BLACK_PAWN_PIECE;
					break;
				case 'k':
					pieceIndex = BLACK_KING_PIECE;
					break;
				case 'n':
					pieceIndex = BLACK_KNIGHT_PIECE;
					break;
				case 'b':
					pieceIndex = BLACK_BISHOP_PIECE;
					break;
				case 'q':
					pieceIndex = BLACK_QUEEN_PIECE;
					break;
				case 'r':
					pieceIndex = BLACK_ROOK_PIECE;
					break;
			}

			return pieceIndex;
		}

		private void DrawPieces(Graphics g)
		{
			if (_imagePieces != null)
			{
				// Work through each row and column of the board and draw the piece that is on each square.
				// Starting at the bottom left corner (NOTE: row 0 is the BOTTOM row)
				for (int row = 0; row < 8; row++)
				{
					for (int col = 0; col < 8; col++)
					{
						int squarePos = (row * 8) + col;
						int pieceIndex = GetPieceAtSquare(row, col);
						if (pieceIndex != NO_PIECE)
						{
							// If we're dragging a piece or the computer is moving one of its pieces then don't show it in its original location.
							if (!(_draggedPieceIndex != NO_PIECE && _draggedPieceOrigin == squarePos) &&
								!(_computerMovingPieceIndex != NO_PIECE && _computerMoveOrigin == squarePos))
							{
								// There is a piece on this square so draw its image.
								DrawImage(g, pieceIndex, squarePos);
							}
						}
					}
				}
			}
		}

		private void DrawHighlightedMovingPiece(Graphics g)
		{
			if (_imagePieces != null)
			{
				int squarePos = ((7 - _highlightedMoveStartRow) * 8) + _highlightedMoveStartCol;
				int pieceIndex = GetPieceAtSquare((7 - _highlightedMoveStartRow), _highlightedMoveStartCol);
				if (pieceIndex != NO_PIECE)
				{
					// If we're dragging a piece or the computer is moving one of its pieces then don't show it in its original location.
					if (!(_draggedPieceIndex != NO_PIECE && _draggedPieceOrigin == squarePos) &&
						!(_computerMovingPieceIndex != NO_PIECE && _computerMoveOrigin == squarePos))
					{
						// There is a piece on this square so draw its image.
						DrawImage(g, pieceIndex, squarePos);
					}
				}
			}
		}

		/// <summary>
		/// Paint the content of the control.
		/// </summary>
		/// <param name="e">A System.Windows.Forms.PaintEventArgs that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			// Call the OnPaint method of the base class.
			base.OnPaint(e);

			// Render the board
			DrawBoard(e.Graphics);

			// Draw the pieces
			DrawPieces(e.Graphics);

			// If we are highlighting one of the found moves then draw the arrow from the starting square
			// of the move to the end square and then re-draw the moving piece (so it appears above the start
			// of the arrow).
			if (_highlightedMoveStartRow >= 0 && _highlightedMoveStartCol >= 0)
			{
				if (!_highlightMoveArrowStart.IsEmpty && !_highlightMoveArrowEnd.IsEmpty)
				{
					using Pen highlightMovePen = new Pen(Color.FromArgb(192, _moveHighlightColour), 4.0f);
					highlightMovePen.Width = 8.0f;
					highlightMovePen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
					e.Graphics.DrawLine(highlightMovePen, _highlightMoveArrowStart, _highlightMoveArrowEnd);
				}

				DrawHighlightedMovingPiece(e.Graphics);
			}

			// Draw the piece that is being dragged (if any)
			if (_draggedPieceIndex != NO_PIECE)
			{
				int x = _currentDragPosition.X - _draggedPieceXOffset;
				int y = _currentDragPosition.Y - _draggedPieceYOffset;

				DrawImageAtPosition(e.Graphics, _draggedPieceIndex, new Point(x + PIECE_MARGIN, y + PIECE_MARGIN));
			}

			// Draw the computer's moving piece
			if (_computerMovingPieceIndex != NO_PIECE)
			{
				DrawImageAtPosition(e.Graphics, _computerMovingPieceIndex, _computerMovingPiecePos);
			}
		}
	}
}
