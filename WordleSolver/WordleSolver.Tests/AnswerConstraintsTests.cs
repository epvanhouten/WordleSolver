using System;
using NUnit.Framework;

namespace WordleSolver.Tests;

[TestFixture]
public class AnswerConstraintsTests
{
    [Test]
    public void GuessSequenceDoesNotMatchPreviousGuesses()
    {
        var guessSequence = new[]
        {
            new {Guess = "raise", Response = "xyxxy"},
            new {Guess = "renal", Response = "xyxyx"},
            new {Guess = "enter", Response = "xxxgx"},
            new {Guess = "snarl", Response = "xxyxx"},
        };

        IAnswerConstraints constraint = new AnswerConstraints();

        foreach (var guess in guessSequence)
        {
            var nextConstraint = AnswerConstraints.FromResponse(guess.Guess, GameResponse.Parse(guess.Response));
            Assert.IsFalse(nextConstraint.MatchesConstraint(guess.Guess), $"Failed at {guess.Guess}");
            constraint = AnswerConstraints.MergeConstraints(constraint, nextConstraint);
            Assert.IsFalse(constraint.MatchesConstraint(guess.Guess));
        }

    }
}