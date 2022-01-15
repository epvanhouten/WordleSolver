using WordleSolver.FitnessFunction;

namespace WordleSolver;

public static class GuessGenerator
{
    public static IComparer<GuessTuple> FitnessFunction { get; set; } = new TwoMoveWinRateStrategy();

    private static IEnumerable<GuessTuple> GetGuess(IEnumerable<string> wordsToTest, WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        var testList = wordsToTest.ToList();
        var guesses = new List<GuessTuple>(testList.Count);
        var remainingAnswers = currentWordLists.ApplyConstraints(knownConstraints).ToList();
        foreach (var proposedGuess in testList)
        {
            var resultingAnswerCounts = new List<GuessTrial>(currentWordLists.LegalAnswers.Count);
            foreach (var assumedSolution in remainingAnswers)
            {
                var response = GameResponse.TestGuess(proposedGuess, assumedSolution);
                var newConstraint = AnswerConstraints.FromResponse(proposedGuess, response);
                newConstraint = AnswerConstraints.MergeConstraints(newConstraint, knownConstraints);
                var resultingWordList = currentWordLists.ApplyConstraints(newConstraint).ToList();

                resultingAnswerCounts.Add(new GuessTrial
                {
                    RemainingAnswers = resultingWordList.Count,
                    IsWin = response.IsVictory()
                });
            }

            if (resultingAnswerCounts.Count == 0)
            {
                continue;
            }

            guesses.Add(new GuessTuple(proposedGuess, resultingAnswerCounts));
        }

        return guesses;
    }

    public static async Task<GuessTuple> GetGuessAsync(WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        var taskList = new List<Task>();
        foreach (var chunk in currentWordLists.LegalGuesses.Chunk((int)Math.Ceiling((double)currentWordLists.LegalGuesses.Count / (Environment.ProcessorCount * 2))))
        {
            var chunkTask = Task.Run(() => GetGuess(chunk, currentWordLists, knownConstraints));
            taskList.Add(chunkTask);
        }

        await Task.WhenAll(taskList);

        var allGuesses = Enumerable.Empty<GuessTuple>();
        foreach (var task in taskList.Cast<Task<IEnumerable<GuessTuple>>>())
        {
            var chunkGuess = await task;

            allGuesses = allGuesses.Concat(chunkGuess);
        }

        return allGuesses.Max(FitnessFunction) ?? throw new Exception("Did not find guess.");
    }
}
