namespace WordleSolver;

public class GuessTuple
{
    public string? Guess { get; init; }
    public double AverageAnswerListLength { get; init; }

    public override string ToString()
    {
        return $"{Guess} (average answers left: {AverageAnswerListLength})";
    }
}
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

    private static AnswerConstraints ApplyGuess(string proposedGuess, string assumedSolution)
    {
        var newConstraint = new AnswerConstraints();

        for (var guessPosition = 0; guessPosition < GameConstants.WordLength; guessPosition++)
        {
            var guessCharacter = proposedGuess[guessPosition];

            if (assumedSolution[guessPosition] == guessCharacter)
            {
                newConstraint.SetExactMatch(guessPosition, guessCharacter);
            }
            else if (assumedSolution.Contains(guessCharacter))
            {
                newConstraint.SetContains(guessPosition, guessCharacter);
            }
            else
            {
                newConstraint.SetMissing(guessCharacter);
            }
        }

        return newConstraint;
    }
}

public class GameResponse
{
    private readonly Hint[] _hints;

    public IReadOnlyList<Hint> PositionHints => _hints;

    private GameResponse(Hint[] hints)
    {
        _hints = hints;
    }

    public static GameResponse TestGuess(string guess, string answer)
    {
        var hints = new Hint[GameConstants.WordLength];
        var matchedCharacters = new Dictionary<char, int>(GameConstants.WordLength);

        for (var answerPosition = 0; answerPosition < GameConstants.WordLength; answerPosition++)
        {
            EnsureCharacterInDictionary(matchedCharacters, answer[answerPosition]);
            matchedCharacters[answer[answerPosition]]++;
        }

        for (var assignGreenIterationIndex = 0; assignGreenIterationIndex < GameConstants.WordLength; assignGreenIterationIndex++)
        {
            var guessCharacter = guess[assignGreenIterationIndex];

            if (answer[assignGreenIterationIndex] == guessCharacter)
            {
                matchedCharacters[guessCharacter]--;
                hints[assignGreenIterationIndex] = Hint.Green;
            }
        }

        for (var assignYellowIterationIndex = 0; assignYellowIterationIndex < GameConstants.WordLength; assignYellowIterationIndex++)
        {
            var guessCharacter = guess[assignYellowIterationIndex];

            if (answer[assignYellowIterationIndex] != guessCharacter &&
                matchedCharacters.TryGetValue(guessCharacter, out var outStandingCount) &&
                outStandingCount > 0)
            {
                matchedCharacters[guessCharacter]--;
                hints[assignYellowIterationIndex] = Hint.Yellow;
            }
        }

        return new GameResponse(hints);
    }

    private static void EnsureCharacterInDictionary(Dictionary<char, int> dict, char character)
    {
        if (!dict.ContainsKey(character))
        {
            dict.Add(character, default);
        }
    }

    public static GameResponse Parse(string response)
    {
        var hints = new Hint[GameConstants.WordLength];
        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            hints[position] = response[position] switch
            {
                'x' => Hint.Black,
                'y' => Hint.Yellow,
                'g' => Hint.Green,
                _ => throw new Exception($"{response[position]} is not a valid input")
            };
        }

        return new GameResponse(hints);
    }
}

public enum Hint
{
    Black = 0,
    Yellow,
    Green
}