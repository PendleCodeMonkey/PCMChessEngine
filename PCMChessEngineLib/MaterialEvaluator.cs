namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Class that implements very simple evaluation based on adding up the material value
	/// assigned to each piece.
	/// </summary>
	internal class MaterialEvaluator : IEvaluator
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MaterialEvaluator"/> class.
		/// </summary>
		public MaterialEvaluator()
		{
		}

		#endregion

		#region methods

		/// <summary>
		/// Calculates an estimation of the value of the specified board from the perspective
		/// of the side to move.
		/// </summary>
		/// <remarks>
		/// Positive values indicate an advantage to the side to move.
		/// A zero value indicates an estimated draw.
		/// Negative values indicate position that is disadvantageous to the side to move. 
		/// Note: A _very_ large negative value is returned in the case of checkmate.
		/// </remarks>
		/// <param name="b">The board state to be evaluated.</param>
		/// <returns>An estimated value of the board according to the side to move.</returns>
		public int Eval(Board b)
		{
			if (b.IsMate())
			{
				return int.MinValue + 2;
			}
			if (b.IsDraw())
			{
				return 0;
			}

			// Calculate the material value of each side's pieces (passing true to the method calls so that pawns are
			// included in the calculation)
			int whiteMaterialValue = b.WhitePieceMaterial(true);
			int blackMaterialValue = b.BlackPieceMaterial(true);
			return b.WhiteToMove ? (whiteMaterialValue - blackMaterialValue) : (blackMaterialValue - whiteMaterialValue);
		}

		#endregion
	}

}
