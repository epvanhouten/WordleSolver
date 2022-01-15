using WordleSolver.FitnessFunction;

namespace WordleSolver;

public static class GuessGenerator
{
    public static IComparer<GuessTuple> FitnessFunction { get; set; } = new HybridStrategy();

    private static IEnumerable<GuessTuple> GetGuess(IEnumerable<string> wordsToTest, WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        var testList = wordsToTest.ToList();
        var guesses = new List<GuessTuple>(testList.Count());
        var remainingAnswers = currentWordLists.ApplyConstraints(knownConstraints).ToList();
        foreach (var proposedGuess in testList)
        {
            var resultingAnswerCounts = new List<Tuple<int, bool>>(currentWordLists.LegalAnswers.Count);
            foreach (var assumedSolution in remainingAnswers)
            {
                var response = GameResponse.TestGuess(proposedGuess, assumedSolution);
                var newConstraint = AnswerConstraints.FromResponse(proposedGuess, response);
                newConstraint = AnswerConstraints.MergeConstraints(newConstraint, knownConstraints);
                var resultingWordList = currentWordLists.ApplyConstraints(newConstraint).ToList();

                resultingAnswerCounts.Add(new Tuple<int, bool>(resultingWordList.Count, response.IsVictory()));
            }

            if (resultingAnswerCounts.Count == 0)
            {
                continue;
            }

            var averageResult = resultingAnswerCounts.Average(t => t.Item1);
            var worstCase = resultingAnswerCounts.Max(t => t.Item1);
            var winRate = (double)resultingAnswerCounts.Count(t => t.Item2) / resultingAnswerCounts.Count;
            guesses.Add(new GuessTuple
            {
                Guess = proposedGuess,
                AverageAnswerListLength = averageResult,
                WorstCase = worstCase,
                WinRate = winRate,
            });
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
