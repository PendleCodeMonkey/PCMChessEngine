namespace PendleCodeMonkey.ChessEngineWinFormsApp
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.SuggestMovesButton = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.InfoLabel1 = new System.Windows.Forms.Label();
			this.InfoLabel2 = new System.Windows.Forms.Label();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.board = new PendleCodeMonkey.ChessEngineWinFormsApp.ChessBoardCtrl();
			this.listBoxSuggestedMoves = new System.Windows.Forms.ListBox();
			this.MakeMoveButton = new System.Windows.Forms.Button();
			this.NewGameButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// SuggestMovesButton
			// 
			this.SuggestMovesButton.Location = new System.Drawing.Point(615, 151);
			this.SuggestMovesButton.Margin = new System.Windows.Forms.Padding(2);
			this.SuggestMovesButton.Name = "SuggestMovesButton";
			this.SuggestMovesButton.Size = new System.Drawing.Size(174, 32);
			this.SuggestMovesButton.TabIndex = 0;
			this.SuggestMovesButton.Text = "FIND MOVES";
			this.SuggestMovesButton.UseVisualStyleBackColor = true;
			this.SuggestMovesButton.Click += new System.EventHandler(this.SuggestMovesButton_Click);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "BBishop.gif");
			this.imageList1.Images.SetKeyName(1, "BKing.gif");
			this.imageList1.Images.SetKeyName(2, "BKnight.gif");
			this.imageList1.Images.SetKeyName(3, "BPawn.gif");
			this.imageList1.Images.SetKeyName(4, "BQueen.gif");
			this.imageList1.Images.SetKeyName(5, "BRook.gif");
			this.imageList1.Images.SetKeyName(6, "WBishop.gif");
			this.imageList1.Images.SetKeyName(7, "WKing.gif");
			this.imageList1.Images.SetKeyName(8, "WKnight.gif");
			this.imageList1.Images.SetKeyName(9, "WPawn.gif");
			this.imageList1.Images.SetKeyName(10, "WQueen.gif");
			this.imageList1.Images.SetKeyName(11, "WRook.gif");
			// 
			// InfoLabel1
			// 
			this.InfoLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.InfoLabel1.ForeColor = System.Drawing.Color.Maroon;
			this.InfoLabel1.Location = new System.Drawing.Point(570, 15);
			this.InfoLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.InfoLabel1.Name = "InfoLabel1";
			this.InfoLabel1.Size = new System.Drawing.Size(233, 33);
			this.InfoLabel1.TabIndex = 3;
			this.InfoLabel1.Text = "It\'s your move";
			this.InfoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// InfoLabel2
			// 
			this.InfoLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
			this.InfoLabel2.Location = new System.Drawing.Point(588, 48);
			this.InfoLabel2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
			this.InfoLabel2.Name = "InfoLabel2";
			this.InfoLabel2.Size = new System.Drawing.Size(229, 37);
			this.InfoLabel2.TabIndex = 4;
			this.InfoLabel2.Text = "info2";
			this.InfoLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// imageList2
			// 
			this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList2.Images.SetKeyName(0, "BlackBishop.gif");
			this.imageList2.Images.SetKeyName(1, "BlackKing.gif");
			this.imageList2.Images.SetKeyName(2, "BlackKnight.gif");
			this.imageList2.Images.SetKeyName(3, "BlackPawn.gif");
			this.imageList2.Images.SetKeyName(4, "BlackQueen.gif");
			this.imageList2.Images.SetKeyName(5, "BlackRook.gif");
			this.imageList2.Images.SetKeyName(6, "WhiteBishop.gif");
			this.imageList2.Images.SetKeyName(7, "WhiteKing.gif");
			this.imageList2.Images.SetKeyName(8, "WhiteKnight.gif");
			this.imageList2.Images.SetKeyName(9, "WhitePawn.gif");
			this.imageList2.Images.SetKeyName(10, "WhiteQueen.gif");
			this.imageList2.Images.SetKeyName(11, "WhiteRook.gif");
			// 
			// board
			// 
			this.board.GameHasEnded = false;
			this.board.Location = new System.Drawing.Point(14, 15);
			this.board.Margin = new System.Windows.Forms.Padding(2);
			this.board.Name = "board";
			this.board.Size = new System.Drawing.Size(378, 405);
			this.board.TabIndex = 1;
			// 
			// listBoxSuggestedMoves
			// 
			this.listBoxSuggestedMoves.FormattingEnabled = true;
			this.listBoxSuggestedMoves.ItemHeight = 15;
			this.listBoxSuggestedMoves.Location = new System.Drawing.Point(614, 188);
			this.listBoxSuggestedMoves.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.listBoxSuggestedMoves.Name = "listBoxSuggestedMoves";
			this.listBoxSuggestedMoves.Size = new System.Drawing.Size(174, 244);
			this.listBoxSuggestedMoves.TabIndex = 5;
			this.listBoxSuggestedMoves.SelectedIndexChanged += new System.EventHandler(this.listBoxSuggestedMoves_SelectedIndexChanged);
			// 
			// MakeMoveButton
			// 
			this.MakeMoveButton.Enabled = false;
			this.MakeMoveButton.Location = new System.Drawing.Point(615, 436);
			this.MakeMoveButton.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MakeMoveButton.Name = "MakeMoveButton";
			this.MakeMoveButton.Size = new System.Drawing.Size(174, 30);
			this.MakeMoveButton.TabIndex = 6;
			this.MakeMoveButton.Text = "MAKE SELECTED MOVE";
			this.MakeMoveButton.UseVisualStyleBackColor = true;
			this.MakeMoveButton.Click += new System.EventHandler(this.MakeMoveButton_Click);
			// 
			// NewGameButton
			// 
			this.NewGameButton.BackColor = System.Drawing.Color.YellowGreen;
			this.NewGameButton.Location = new System.Drawing.Point(617, 100);
			this.NewGameButton.Name = "NewGameButton";
			this.NewGameButton.Size = new System.Drawing.Size(171, 34);
			this.NewGameButton.TabIndex = 7;
			this.NewGameButton.Text = "NEW GAME";
			this.NewGameButton.UseVisualStyleBackColor = false;
			this.NewGameButton.Click += new System.EventHandler(this.NewGameButton_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(814, 500);
			this.Controls.Add(this.NewGameButton);
			this.Controls.Add(this.MakeMoveButton);
			this.Controls.Add(this.listBoxSuggestedMoves);
			this.Controls.Add(this.InfoLabel2);
			this.Controls.Add(this.InfoLabel1);
			this.Controls.Add(this.board);
			this.Controls.Add(this.SuggestMovesButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Margin = new System.Windows.Forms.Padding(2);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "PCM CHESS";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button SuggestMovesButton;
		private ChessBoardCtrl board;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Label InfoLabel1;
		private System.Windows.Forms.Label InfoLabel2;
		private System.Windows.Forms.ImageList imageList2;
		private System.Windows.Forms.ListBox listBoxSuggestedMoves;
		private System.Windows.Forms.Button MakeMoveButton;
		private System.Windows.Forms.Button NewGameButton;
	}
}

