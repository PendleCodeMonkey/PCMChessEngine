namespace PendleCodeMonkey.ChessEngineLib
{
	/// <summary>
	/// Bit operations (such as methods to count bits in a ulong, locate first and last 1 bit, etc.)
	/// </summary>
	public class BitOperations
	{
		#region data

		private static readonly int[] _index64 = new int[] {
				0, 47,  1, 56, 48, 27,  2, 60,
			   57, 49, 41, 37, 28, 16,  3, 61,
			   54, 58, 35, 52, 50, 42, 21, 44,
			   38, 32, 29, 23, 17, 11,  4, 62,
			   46, 55, 26, 59, 40, 36, 15, 53,
			   34, 51, 20, 43, 31, 22, 10, 45,
			   25, 39, 14, 33, 19, 30,  9, 24,
			   13, 18,  8, 12,  7,  6,  5, 63
			};


		#endregion

		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BitOperations"/> class.
		/// </summary>
		public BitOperations()
		{
		}

		#endregion

		#region methods

		/// <summary>
		/// Retrieve the lowest set bit of the supplied value.
		/// </summary>
		/// <param name="value">The value for which we want the lowest set bit.</param>
		/// <returns>A 64-bit value containing only the lowest set bit.</returns>
		public static ulong LowestSetBit(ulong value)
		{
			ulong val = value;
			val |= (val << 1);
			val |= (val << 2);
			val |= (val << 4);
			val |= (val << 8);
			val |= (val << 16);
			val |= (val << 32);
			return val - (val << 1);
		}

		/// <summary>
		/// Retrieve the highest set bit of the supplied value.
		/// </summary>
		/// <param name="value">The value for which we want the highest set bit.</param>
		/// <returns>A 64-bit value containing only the highest set bit.</returns>
		public static ulong HighestSetBit(ulong value)
		{
			ulong val = value;
			val |= (val >> 1);
			val |= (val >> 2);
			val |= (val >> 4);
			val |= (val >> 8);
			val |= (val >> 16);
			val |= (val >> 32);
			return val - (val >> 1);
		}

		/// <summary>
		/// Count the number of trailing zero bits in the specified 64-bit value.
		/// </summary>
		/// <remarks>
		/// Uses a De Bruijn sequence for improved performance.
		/// </remarks>
		/// <param name="input">The 64-bit value for which the nnumber of trailing zero bits should be determined.</param>
		/// <returns>The number of trailing zero bits in the specified 64-bit value.</returns>
		public static int CountTrailingZeroes(ulong input)
		{
			const ulong debruijn64 = 0x03f79d71b4cb0a89ul;
			return input == 0 ? -1 : _index64[((input ^ (input - 1)) * debruijn64) >> 58];
		}

		/// <summary>
		/// Determine the number of set bits in the supplied 64-bit value.
		/// </summary>
		/// <param name="value">The 64-bit value for which the number of set bits should be determined.</param>
		/// <returns>The number of set bits in the supplied 64-bit value.</returns>
		public static int BitCount(ulong value)
		{
			byte[] bitCounts = new byte[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
											1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
											1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
											1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
											2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
											3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
											3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
											4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8 };

			int count = 0;

			ulong val = value;
			for (int i = 0; i < 8; ++i)
			{
				count += bitCounts[val & 0xff];
				val >>= 8;
			}

			return count;
		}

		#endregion
	}
}
