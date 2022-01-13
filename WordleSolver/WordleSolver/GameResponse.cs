namespace WordleSolver;

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