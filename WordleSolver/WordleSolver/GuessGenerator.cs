namespace WordleSolver;

public static class GuessGenerator
{
    private static GuessTuple GetGuess(IEnumerable<string> wordsToTest, WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        GuessTuple? guess = null;
        var remainingAnswers = currentWordLists.ApplyConstraints(knownConstraints).ToList();
        foreach (var proposedGuess in wordsToTest)
        {
            var resultingAnswerCounts = new List<int>(currentWordLists.LegalAnswers.Count);
            foreach (var assumedSolution in remainingAnswers)
            {
                var response = GameResponse.TestGuess(proposedGuess, assumedSolution);
                var newConstraint = AnswerConstraints.FromResponse(proposedGuess, response);
                newConstraint = AnswerConstraints.MergeConstraints(newConstraint, knownConstraints);
                var resultingWordList = currentWordLists.ApplyConstraints(newConstraint).ToList();

                resultingAnswerCounts.Add(resultingWordList.Count);
            }

            var averageResult = (double)resultingAnswerCounts.Sum() / remainingAnswers.Count;
            if (averageResult < (guess?.AverageAnswerListLength ?? double.MaxValue))
            {
                var newGuess = new GuessTuple
                {
                    Guess = proposedGuess,
                    AverageAnswerListLength = averageResult,
                };
                if (guess != null)
                {
                    Console.WriteLine($"{newGuess} improves on {guess}.");
                }

                guess = newGuess;
            }
        }

        return guess ?? throw new Exception("Didn't find a guess.");
    }

    public static async Task<string> GetGuessAsync(WordLists currentWordLists, IAnswerConstraints knownConstraints)
    {
        var taskList = new List<Task>();
        foreach (var chunk in currentWordLists.LegalAnswers.Chunk((int)Math.Ceiling((double)currentWordLists.LegalAnswers.Count / (Environment.ProcessorCount * 2))))
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
                bestGuess = chunkGuess.AverageAnswerListLength < bestGuess.AverageAnswerListLength
                    ? chunkGuess
                    : bestGuess;
            }
        }

        return bestGuess?.Guess ?? throw new Exception("Did not find guess.");
    }
}
