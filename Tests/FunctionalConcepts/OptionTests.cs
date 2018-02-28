using System;
using System.Collections.Generic;
using FunctionalConcepts;
using Xunit;

namespace Tests.FunctionalConcepts
 {

    public class OptionTests
    {
        private class DummyClass
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }

        [Fact]
        public void Value_NoValuePresent_ThrowsException()
        {
            // Arrange
            Option<DummyClass> dummyClass = null;

			// Act
            var result = Assert.Throws<InvalidOperationException>(() => (dummyClass.Value));

			// Assert
			Assert.IsType<InvalidOperationException>(result);
        }

        [Fact]
        public void Value_ValuePresent_ReturnsValue()
        {
            // Arrange
            Option<DummyClass> dummyClass = new DummyClass {Value = "Test"};

			// Act
            var result = dummyClass.Value;

			// Assert
			Assert.NotNull(result);
        }

        [Fact]
        public void HasValue_NoValuePresent_ReturnsFalse()
        {
            // Arrange
            Option<DummyClass> dummyClass = null;

			// Act
            var result = dummyClass.HasValue;

			// Assert
			Assert.False(result);
        }

		[Fact]
        public void HasValue_ValuePresent_ReturnsTrue()
        {
            // Arrange
            Option<DummyClass> dummyClass = new DummyClass { Value = "Test" };

            // Act
            var result = dummyClass.HasValue;

			// Assert
			Assert.True(result);
        }

        [Fact]
        public void HasNoValue_NoValuePresent_ReturnsTrue()
        {
            // Arrange
            Option<DummyClass> dummyClass = null;

            // Act
            var result = dummyClass.HasNoValue;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasNoValue_ValuePresent_ReturnsFalse()
        {
            // Arrange
            Option<DummyClass> dummyClass = new DummyClass { Value = "Test" };

            // Act
            var result = dummyClass.HasNoValue;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EqualityOperator_OptionHasSameValue_ReturnsTrue()
        {
            // Arrange
            var dummyClass = new DummyClass {Value = "test"};
            Option<DummyClass> option = dummyClass;

			// Assert
			Assert.True(option == dummyClass);
        }

        [Fact]
        public void EqualityOperator_OptionHasNoValue_ReturnsFalse()
        {
            // Arrange
            var dummyClass = new DummyClass { Value = "test" };
            Option<DummyClass> option = null;

            // Assert
            Assert.False(option == dummyClass);
        }

		[Fact]
        public void EqualityOperator_OptionHasDifferentValue_ReturnsFalse()
        {
            // Arrange
            var dummyClass = new DummyClass { Value = "test" };
            Option<DummyClass> option = new DummyClass { Value = "test1" };

            // Assert
            Assert.False(option == dummyClass);
        }

		[Fact]
        public void InEqualityOperator_OptionHasSameValue_ReturnsFalse()
        {
            // Arrange
            var dummyClass = new DummyClass { Value = "test" };
            Option<DummyClass> option = dummyClass;

            // Assert
            Assert.False(option != dummyClass);
        }

        [Fact]
        public void EqualityOperator_BothOptionHaveSameValues_ReturnTrue()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test1" };
            Option<DummyClass> option2 = option1;

			// Assert
			Assert.True(option1 == option2);
        }

        [Fact]
        public void EqualityOperator_BothOptionHaveDifferentValues_ReturnFalse()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test1" };
            Option<DummyClass> option2 = new DummyClass { Value = "test1" };

            // Assert
            Assert.False(option1 == option2);
        }

        [Fact]
        public void InEqualityOperator_BothOptionHaveSameValues_ReturnFalse()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test1" };
            Option<DummyClass> option2 = option1;

            // Assert
            Assert.False(option1 != option2);
        }

        [Fact]
        public void InEqualityOperator_BothOptionHaveDifferentValues_ReturnTrue()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test1" };
            Option<DummyClass> option2 = new DummyClass { Value = "test1" };

            // Assert
            Assert.True(option1 != option2);
        }

        [Fact]
        public void Equals_ValueIsDifferentType_ReturnsFalse()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass {Value = "test"};
            Option<List<int>> option2 = new List<int>();

			// Assert
			Assert.False(option1.Equals(option2));
        }

        [Fact]
        public void Equals_BothSameTypeNoValues_ReturnsTrue()
        {
			// Arrange
            Option<DummyClass> option1 = null;
            Option<DummyClass> option2 = null;

			// Assert
			Assert.True(option1.Equals(option2));
        }

        [Fact]
        public void Equals_BothSameTypeOneHasValue_ReturnsFalse()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass {Value = "test"};
            Option<DummyClass> option2 = null;

            // Assert
            Assert.False(option1.Equals(option2));
        }

        [Fact]
        public void Equals_BothSameTypeAndHaveSameValue_UseValueEqualityReturnTrue()
        {
            // Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test" };
            Option<DummyClass> option2 = option1;

			// Assert
			Assert.True(option1.Equals(option2));
        }

        [Fact]
        public void ToString_OptionHasNoValue_RetunsNovalue()
        {
			// Arrange
            Option<DummyClass> option1 = null;

			// Assert
            Assert.Equal("No value", option1.ToString());
        }

        [Fact]
        public void ToString_OptionHasValue_RetunsTypeToString()
        {
			// Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test" };

            // Assert
            Assert.Equal("test", option1.ToString());
        }

        [Fact]
        public void Unwrap_OptionHasValue_ReturnsValue()
        {
			// Arrange
            Option<DummyClass> option1 = new DummyClass { Value = "test" };

			// Act
            var result = option1.Unwrap();

			//Assert
			Assert.NotNull(result);
        }

        [Fact]
        public void Unwrap_OptionHasNoValue_ReturnsDefaultValue()
        {
            // Arrange
            Option<DummyClass> option1 = null;

            // Act
            var result = option1.Unwrap();

            //Assert
            Assert.Null(result);
        }

    }
}
