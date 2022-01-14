using WordleSolver.FitnessFunction;

namespace WordleSolver;

public static class GuessGenerator
{
    public static IComparer<GuessTuple> FitnessFunction { get; set; } = new HybridStrategy();

    private static GuessTuple GetGuess(IEnumerable<string> wordsToTest, WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        GuessTuple? guess = null;
        var remainingAnswers = currentWordLists.ApplyConstraints(knownConstraints).ToList();
        foreach (var proposedGuess in wordsToTest)
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

            var averageResult = resultingAnswerCounts.Average(t => t.Item1);
            var worstCase = resultingAnswerCounts.Max(t => t.Item1);
            var winRate = (double)resultingAnswerCounts.Count(t => t.Item2) / resultingAnswerCounts.Count;
            var newGuess = new GuessTuple
            {
                Guess = proposedGuess,
                AverageAnswerListLength = averageResult,
                WorstCase = worstCase,
                WinRate = winRate,
            };

            if (FitnessFunction.Compare(newGuess, guess) > 0)
            {
                if (guess != null)
                {
                    Console.WriteLine($"{newGuess} improves on {guess}.");
                }

                guess = newGuess;
            }
        }

        return guess ?? throw new Exception("Didn't find a guess.");
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

        GuessTuple? bestGuess = null;
        foreach (var task in taskList.Cast<Task<GuessTuple>>())
        {
            var chunkGuess = await task;

            if (bestGuess == null)
            {
                bestGuess = chunkGuess;
            }
            else
            {
                if (FitnessFunction.Compare(chunkGuess, bestGuess) > 0)
                {
                    Console.WriteLine($"{chunkGuess} improves on {bestGuess}.");
                    bestGuess = chunkGuess;
                }
                else
                {
                    Console.WriteLine($"{bestGuess} is better than {chunkGuess}.");
                }
            }
        }

        return bestGuess ?? throw new Exception("Did not find guess.");
    }
}
