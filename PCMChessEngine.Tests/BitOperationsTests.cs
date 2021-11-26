using PendleCodeMonkey.ChessEngineLib;
using Xunit;

namespace PendleCodeMonkey.ChessEngine.Tests
{
	public class BitOperationsTests
	{
		[Theory]
		[InlineData(0x0000000000000000, 0x0000000000000000)]
		[InlineData(0x0000000000000f00, 0x0000000000000100)]
		[InlineData(0x0000000000001800, 0x0000000000000800)]
		[InlineData(0x8000000000000000, 0x8000000000000000)]
		[InlineData(0xFFFFFFFFFFFFFFFF, 0x0000000000000001)]
		public void LowestOneBit_ReturnsCorrectValue(ulong inputValue, ulong expectedResult)
		{
			var value = BitOperations.LowestSetBit(inputValue);
			Assert.Equal(expectedResult, value);
		}

		[Theory]
		[InlineData(0x0000000000000000, 0x0000000000000000)]
		[InlineData(0x0000000000000f00, 0x0000000000000800)]
		[InlineData(0x0000000000001800, 0x0000000000001000)]
		[InlineData(0x8000000000000000, 0x8000000000000000)]
		[InlineData(0xFFFFFFFFFFFFFFFF, 0x8000000000000000)]
		public void HighestOneBit_ReturnsCorrectValue(ulong inputValue, ulong expectedResult)
		{
			var value = BitOperations.HighestSetBit(inputValue);
			Assert.Equal(expectedResult, value);
		}

		[Theory]
		[InlineData(0x0000000000000000, -1)]
		[InlineData(0x0000000000000f00, 8)]
		[InlineData(0x0000000000001800, 11)]
		[InlineData(0x8000000000000000, 63)]
		[InlineData(0xFFFFFFFFFFFFFFFF, 0)]
		public void CountTrailingZeroes_ReturnsCorrectValue(ulong inputValue, int expectedResult)
		{
			var value = BitOperations.CountTrailingZeroes(inputValue);
			Assert.Equal(expectedResult, value);
		}

		[Theory]
		[InlineData(0x0000000000000000, 0)]
		[InlineData(0x0000000000000f00, 4)]
		[InlineData(0x0000000000001800, 2)]
		[InlineData(0x8000000000000000, 1)]
		[InlineData(0xFFFFFFFFFFFFFFFF, 64)]
		[InlineData(0xAAAAAAAAAAAAAAAA, 32)]
		[InlineData(0x5555555555555555, 32)]
		[InlineData(0xA0A0A0A0A0A0A0A0, 16)]
		public void BitCount_ReturnsCorrectValue(ulong inputValue, int expectedResult)
		{
			var value = BitOperations.BitCount(inputValue);
			Assert.Equal(expectedResult, value);
		}
	}
}
