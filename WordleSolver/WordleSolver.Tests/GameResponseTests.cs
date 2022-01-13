using NUnit.Framework;

namespace WordleSolver.Tests;

[TestFixture]
public class GameResponseTests
{
    [Test]
    public void DoubleLetterWithExactMatchGivesGreenAndBlack()
    {
        var response = GameResponse.TestGuess("enter", "abbey");
        var actualResponse = GameResponse.Parse("xxxgx");

        for (var position = 0; position < response.PositionHints.Count; position++)
        {
            Assert.IsTrue(response.PositionHints[position] == actualResponse.PositionHints[position]);
        }
    }

    [Test]
    public void SingleContainedCharactersGivesYellow()
    {
        var response = GameResponse.TestGuess("raise", "abbey");
        var actualResponse = GameResponse.Parse("xyxxy");
        
        for (var position = 0; position < response.PositionHints.Count; position++)
        {
            Assert.IsTrue(response.PositionHints[position] == actualResponse.PositionHints[position]);
        }
    }

    [Test]
    public void ExactMatchReturnsAllGreen()
    {
        var response = GameResponse.TestGuess("abbey", "abbey");
        var actualResponse = GameResponse.Parse("ggggg");
        
        for (var position = 0; position < response.PositionHints.Count; position++)
        {
            Assert.IsTrue(response.PositionHints[position] == actualResponse.PositionHints[position]);
        }
    }
}