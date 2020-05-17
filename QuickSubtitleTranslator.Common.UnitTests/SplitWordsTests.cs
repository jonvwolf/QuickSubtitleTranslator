using System;
using Xunit;

namespace QuickSubtitleTranslator.Common.UnitTests
{
    public class SplitWordsTests
    {
        [Fact]
        public void Test_SplitWords_ShouldWork()
        {
            //arrange
            string workToBreak = "¿Hola? ¿Perdóneme? � Hay una línea, amigo.";
            //act
            var list = LineFormatter.SplitWords(workToBreak, 4, 4);
            //assert
            Assert.NotNull(list);
            Assert.Equal(2, list.Count);
            Assert.Equal(workToBreak, string.Join(" ", list));
        }

        [Fact]
        public void Test_SplitWords_ShouldNotSplit()
        {
            //arrange
            string workToBreak = "¿Hola? ¿Perdóneme? � Hay una línea, amigo.";
            //act
            var list = LineFormatter.SplitWords(workToBreak, 100, 4);
            //assert
            Assert.NotNull(list);
            Assert.Equal(1, list.Count);
            Assert.Equal(workToBreak, list[0]);
        }

    }
}
