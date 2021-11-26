using PendleCodeMonkey.ChessEngineLib;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PendleCodeMonkey.ChessEngineWinFormsApp
{
	public partial class Form1 : Form
	{
		private readonly ChessEngine _engine = null;
		private Board _board = null;

		private bool _gameHasEnded = false;

		private int _fromRow = -1;
		private int _fromCol = -1;
		private int _toRow = -1;
		private int _toCol = -1;

		private Timer _computerMoveTimer = null;
		private Timer _computerMakeHumansMoveTimer = null;
		private Timer _pieceMoveTimer = null;

		private int _computerMove = 0;
		private int[] _pieceSlidePositions = null;
		private int _pieceSlideCount = 0;

		private List<(int move, string san)> _suggestedMoves = new List<(int, string)>();


		public Form1()
		{
			_board = new Board(ChessEngine.START_FEN);		// The start positions in Forsyth–Edwards Notation ("FEN")
			_engine = new ChessEngine(_board);
			InitializeComponent();
			board.SetImageList(imageList2);
			board.SetChessEngine(_engine);

			board.AttachEventHandlers(MouseDownDelegateHandler, MouseUpDelegateHandler, MouseMoveDelegateHandler);
		}

		public void MouseDownDelegateHandler(int row, int col)
		{
			if (_gameHasEnded)
			{
				return;
			}

			_fromRow = row;
			_fromCol = col;
		}

		public void MouseUpDelegateHandler(int row, int col)
		{
			if (_gameHasEnded)
			{
				return;
			}

			_toRow = row;
			_toCol = col;

			bool cachedWhiteToMove = _board.WhiteToMove;

			// Attempt to make the user-specified move.
			_engine.MakeMove(_fromCol, _fromRow, _toCol, _toRow);

			board.SetHighlightedSquare(-1, -1);         // Clear square highlighting
			_fromCol = _fromRow = -1;

			DetermineGameStatus();

			// If the move has been made (i.e. the players whose turn it is has changed) then
			// kick off a computer move.
			if (_board.WhiteToMove != cachedWhiteToMove)
			{
				board.Invalidate();         // Redraw the board to show the moved piece.

				if (!_gameHasEnded)
				{
					StartComputerMoveTimer();
				}
			}
		}

		public void MouseMoveDelegateHandler(int row, int col)
		{
			if (_gameHasEnded)
			{
				return;
			}

			if (_fromRow != -1 && _fromCol != -1)
			{
				if (row != _fromRow || col != _fromCol)
				{
					board.SetHighlightedSquare(row, col);
				}
				else
				{
					board.SetHighlightedSquare(-1, -1);
				}
			}
		}

		private void StartComputerMoveTimer()
		{
			InfoLabel1.Text = "I'm thinking...";
			InfoLabel1.Invalidate();
			listBoxSuggestedMoves.Items.Clear();
			board.SetHighlightedMoveSquares(-1, -1, -1, -1);

			_computerMoveTimer = new Timer();
			_computerMoveTimer.Interval = 100;
			_computerMoveTimer.Tick += _computerMoveTimer_Tick;
			_computerMoveTimer.Start();
		}

		void _computerMoveTimer_Tick(object sender, EventArgs e)
		{
			_computerMoveTimer.Stop();
			MakeComputerMove();
		}

		private void MakeComputerMove()
		{
			_computerMove = _engine.GetBestEngineMove();
			int from = ChessEngineLib.Move.GetFrom(_computerMove);
			int to = ChessEngineLib.Move.GetTo(_computerMove);

			_pieceSlidePositions = board.GetStartAndEndPositions(from, to);
			board.SetupComputerMovingPiece(from);

			StartPieceMoveTimer();
		}

		private void StartComputerMakeHumansMoveTimer()
		{
			_computerMakeHumansMoveTimer = new Timer();
			_computerMakeHumansMoveTimer.Interval = 100;
			_computerMakeHumansMoveTimer.Tick += ComputerMakeHumansMoveTimer_Tick;
			_computerMakeHumansMoveTimer.Start();
		}

		void ComputerMakeHumansMoveTimer_Tick(object sender, EventArgs e)
		{
			_computerMakeHumansMoveTimer.Stop();
			ComputerMakeHumansMove();
		}

		private void ComputerMakeHumansMove()
		{
			int moveIndex = listBoxSuggestedMoves.SelectedIndex;
			_computerMove = _suggestedMoves[moveIndex].move;
			int from = ChessEngineLib.Move.GetFrom(_computerMove);
			int to = ChessEngineLib.Move.GetTo(_computerMove);

			_pieceSlidePositions = board.GetStartAndEndPositions(from, to);
			board.SetupComputerMovingPiece(from);

			StartComputerMovesHumanPieceTimer();
		}

		private void StartComputerMovesHumanPieceTimer()
		{
			_pieceSlideCount = 50;

			_pieceMoveTimer = new Timer();
			_pieceMoveTimer.Interval = 10;
			_pieceMoveTimer.Tick += ComputerMovesHumanPieceTimer_Tick;
			_pieceMoveTimer.Start();
		}

		void ComputerMovesHumanPieceTimer_Tick(object sender, EventArgs e)
		{
			--_pieceSlideCount;
			if (_pieceSlideCount == 0)
			{
				_pieceMoveTimer.Stop();

				_engine.MakeMove(_computerMove);

				board.FinishComputerMovingPiece();
				InfoLabel2.Text = "";
				InfoLabel2.Invalidate();

				DetermineGameStatus();

				board.Invalidate();         // Redraw the board to show the moved piece.

				if (!_gameHasEnded)
				{
					StartComputerMoveTimer();
				}
			}
			else
			{
				int xPos = _pieceSlidePositions[0] + (((_pieceSlidePositions[2] - _pieceSlidePositions[0]) * (50 - _pieceSlideCount)) / 50);
				int yPos = _pieceSlidePositions[1] + (((_pieceSlidePositions[3] - _pieceSlidePositions[1]) * (50 - _pieceSlideCount)) / 50);

				board.UpdateComputerMovingPiecePosition(xPos, yPos);
			}
		}

		private void StartPieceMoveTimer()
		{
			_pieceSlideCount = 50;

			_pieceMoveTimer = new Timer();
			_pieceMoveTimer.Interval = 10;
			_pieceMoveTimer.Tick += PieceMoveTimer_Tick;
			_pieceMoveTimer.Start();
		}

		void PieceMoveTimer_Tick(object sender, EventArgs e)
		{
			--_pieceSlideCount;
			if (_pieceSlideCount == 0)
			{
				_pieceMoveTimer.Stop();

				_engine.MakeMove(_computerMove);

				board.FinishComputerMovingPiece();
				board.Invalidate();         // Redraw the board to show the moved piece.
				InfoLabel1.Text = "It's your move";
				InfoLabel1.Invalidate();
				listBoxSuggestedMoves.Items.Clear();
				SuggestMovesButton.Enabled = true;

				DetermineGameStatus();
			}
			else
			{
				int xPos = _pieceSlidePositions[0] + (((_pieceSlidePositions[2] - _pieceSlidePositions[0]) * (50 - _pieceSlideCount)) / 50);
				int yPos = _pieceSlidePositions[1] + (((_pieceSlidePositions[3] - _pieceSlidePositions[1]) * (50 - _pieceSlideCount)) / 50);

				board.UpdateComputerMovingPiecePosition(xPos, yPos);
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// Resize the board control to the required size.
			board.Width = board.Height = ((ChessBoardCtrl.PIECE_SIZE + (ChessBoardCtrl.PIECE_MARGIN * 2)) * 8) +
										(ChessBoardCtrl.BOARD_MARGIN * 2);
			InfoLabel2.Text = "";
			board.GameHasEnded = false;
		}

		private void SuggestMovesButton_Click(object sender, EventArgs e)
		{
			SuggestMovesButton.Enabled = false;
			StartSuggestMovesTimer();
		}

		private void StartSuggestMovesTimer()
		{
			InfoLabel2.Text = "Evaluating...";
			InfoLabel2.Invalidate();

			_computerMakeHumansMoveTimer = new Timer();
			_computerMakeHumansMoveTimer.Interval = 100;
			_computerMakeHumansMoveTimer.Tick += SuggestMovesTimer_Tick;
			_computerMakeHumansMoveTimer.Start();
		}

		void SuggestMovesTimer_Tick(object sender, EventArgs e)
		{
			_computerMakeHumansMoveTimer.Stop();
			SuggestMoves();
		}

		private void SuggestMoves()
		{
			_suggestedMoves = _engine.GetSuggestedMoves();
			listBoxSuggestedMoves.Items.Clear();
			foreach (var move in _suggestedMoves)
			{
				listBoxSuggestedMoves.Items.Add(move.san);
			}

			if (listBoxSuggestedMoves.Items.Count > 0)
			{
				listBoxSuggestedMoves.SelectedIndex = 0;
			}
			MakeMoveButton.Enabled = listBoxSuggestedMoves.Items.Count > 0;
			InfoLabel2.Text = "";
		}

		private void MakeMoveButton_Click(object sender, EventArgs e)
		{
			MakeMoveButton.Enabled = false;
			StartComputerMakeHumansMoveTimer();
		}

		private void DetermineGameStatus()
		{
			InfoLabel2.Text = "";

			if (_board.IsCheck())
			{
				InfoLabel2.Text = "*- CHECK! -*";
			}
			if (_engine.WhiteWins)
			{
				InfoLabel2.Text = "WHITE WINS!";
				_gameHasEnded = true;
			}
			if (_engine.BlackWins)
			{
				InfoLabel2.Text = "BLACK WINS!";
				_gameHasEnded = true;
			}
			if (_engine.IsDraw)
			{
				InfoLabel2.Text = "IT'S A DRAW!";
				_gameHasEnded = true;
			}

			board.GameHasEnded = _gameHasEnded;
			if (_gameHasEnded)
			{
				SuggestMovesButton.Enabled = false;
				ClearBoardControlHighlighting();
			}

			InfoLabel2.Invalidate();
		}

		private void ClearBoardControlHighlighting()
		{
			board.SetHighlightedSquare(-1, -1);
			board.SetHighlightedMoveSquares(-1, -1, -1, -1);
			board.Invalidate();
		}

		private void listBoxSuggestedMoves_SelectedIndexChanged(object sender, EventArgs e)
		{
			int moveIndex = listBoxSuggestedMoves.SelectedIndex;
			int move = _suggestedMoves[moveIndex].move;
			int from = ChessEngineLib.Move.GetFrom(move);
			int to = ChessEngineLib.Move.GetTo(move);
			int fromRow = 7 - (from / 8);
			int fromCol = from % 8;
			int toRow = 7 - (to / 8);
			int toCol = to % 8;

			board.SetHighlightedMoveSquares(fromRow, fromCol, toRow, toCol);
		}

		private void NewGameButton_Click(object sender, EventArgs e)
		{
			if (!_gameHasEnded)
			{
				DialogResult dialogResult = MessageBox.Show("Are you sure you want to abandon the current game?", "Start New Game", MessageBoxButtons.YesNo);
				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}
			ClearBoardControlHighlighting();
			_board = _engine.InitBoard();
			_gameHasEnded = false;
			board.GameHasEnded = false;
			SuggestMovesButton.Enabled = true;
			MakeMoveButton.Enabled = false;
			listBoxSuggestedMoves.Items.Clear();
			board.Invalidate();
			InfoLabel2.Text = "";
			InfoLabel2.Invalidate();
		}
	}
}
