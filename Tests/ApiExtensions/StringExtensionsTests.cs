using System;
using System.Linq;
using ApiExtensions;
using Xunit;

namespace Tests.ApiExtensions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("Word", "word")]
        [InlineData("Word123Hello", "word123Hello")]
        [InlineData("word", "word")]
        [InlineData("wOrd", "wOrd")]
        public void FirstCharacterToLower_ReturnsExpected(string input, string expectedOutput)
        {
            var output = input.FirstCharacterToLower();
            Assert.Equal(expectedOutput, output);
        }

        [Theory]
        [InlineData("string", "string", true)]
        [InlineData(null, null, true)]
        [InlineData("string", null, false)]
        [InlineData(null, "string", false)]
        public void NullableEquals_RecognisesEquality(string baseString, string compareString, bool expectedEqual)
        {
            var output = StringExtensions.NullableEquals(baseString, compareString);
            Assert.Equal(expectedEqual, output);
        }

        [Theory]
        [InlineData("NoWhiteSpace", "NoWhiteSpace")]
        [InlineData("N o W h i   te S p a  ce", "NoWhiteSpace")]
        public void StripWhiteSpace_DoesStrip(string input, string expectedOutput)
        {
            var output = input.StripWhiteSpace();
            Assert.Equal(expectedOutput, output);
        }

        [Fact]
        public void Lines_CanSplitStringIntoLines()
        {
            var test1 = $@"hello{Environment.NewLine}this is{Environment.NewLine}a{Environment.NewLine}multiline{Environment.NewLine}string";
            var actual = test1.Lines();
            var array = actual as string[] ?? actual.ToArray();
            Assert.Equal(5, array.Count());
            Assert.Equal("multiline", array[3]);
            Assert.Equal("a", array[2]);

        }

        [Fact]
        public void NotEmpty_StringIsEmpty_ThrowsError()
        {
            // Arrange
            var val = "";

            // Act
            Assert.Throws<ArgumentNullException>(() => val.NotEmpty());
        }

        [Fact]
        public void NotEmpty_StringIsNotEmpty_ReturnsValue()
        {
            // Arrange
            var val = "xxx";

            // Act
            var result = val.NotEmpty();

            // Assert
            Assert.Equal(val, result);
        }

        [Fact]
        public void ToEnum_TypeIsNotEnum_ThrowsException()
        {
            var str = "ValueA";
            var ex = Record.Exception(() => str.ToEnum<NotEnum>());

            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void ToEnum_ValueNotInEnum_ReturnsDefault()
        {
            var str = "ValueC";
            var output = str.ToEnum<TestEnum>();

            Assert.Equal(TestEnum.ValueA, output);
        }

        [Fact]
        public void ToEnum_ValueInEnum_ReturnsEnumValue()
        {
            var str = "ValueA";
            var output = str.ToEnum<TestEnum>();

            Assert.Equal(TestEnum.ValueA, output);
        }

        [Fact]
        public void Truncate_ShorterThanLength_ReturnsString()
        {
            var str = "string";
            var output = str.Truncate(10);

            Assert.Equal(str, output);
        }

        [Fact]
        public void Truncate_LongerThanLength_ReturnsTruncatedString()
        {
            var str = "string";
            var output = str.Truncate(5);

            Assert.Equal("strin", output);
        }

        public enum TestEnum
        {
            ValueA,
            ValueB
        }

        public struct NotEnum { }
    }
}
