namespace PendleCodeMonkey.ChessEngineLib
{
	internal interface IEvaluator
	{
		/// <summary>
		/// Calculates an estimation of the value of the specified board from the perspective
		/// of the side to move.
		/// </summary>
		/// <remarks>
		/// Positive values indicate an advantage to the side to move.
		/// A zero value indicates an estimated draw.
		/// Negative values indicate position that is disadvantageous to the side to move. 
		/// </remarks>
		/// <param name="b">The board state to be evaluated.</param>
		/// <returns>An estimated value of the board according to the side to move.</returns>
		int Eval(Board b);
	}
}
