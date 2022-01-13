namespace WordleSolver;

public class AnswerConstraints : IAnswerConstraints
{
    private readonly PositionConstraint[] _positionConstraints = new PositionConstraint[GameConstants.WordLength];
    private readonly List<char> _requiredCharacters = new(GameConstants.WordLength);

    public AnswerConstraints()
    {
        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            _positionConstraints[position] = new PositionConstraint();
        }
    }

    public bool MatchesConstraint(string answerToTest)
    {
        for (var requiredCharacterIndex = 0; requiredCharacterIndex < _requiredCharacters.Count; requiredCharacterIndex++)
        {
            if (!answerToTest.Contains(_requiredCharacters[requiredCharacterIndex]))
            {
                return false;
            }
        }

        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            if (!_positionConstraints[position].IsCharacterAllowed(answerToTest[position]))
            {
                return false;
            }
        }

        return true;
    }

    public void SetExactMatch(int guessPosition, char guessCharacter)
    {
        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].SetExact(guessCharacter);
    }

    public void SetContains(int guessPosition, char guessCharacter)
    {
        if (!_requiredCharacters.Contains(guessCharacter))
        {
            _requiredCharacters.Add(guessCharacter);
        }

        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].ForbidCharacter(guessCharacter);
    }

    public void SetMissing(char guessCharacter)
    {
        for (int position = 0; position < GameConstants.WordLength; position++)
        {
            _positionConstraints[position] = _positionConstraints[position].ForbidCharacter(guessCharacter);
        }
    }

    public static AnswerConstraints Parse(string guess, string? line)
    {
        if (line == null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        line = line.Trim();

        if (line.Length != GameConstants.WordLength)
        {
            throw new Exception($"Must provide exactly {GameConstants.WordLength} entries");
        }

        var constraints = new AnswerConstraints();

        for (int position = 0; position < GameConstants.WordLength; position++)
        {
            switch (line[position])
            {
                case 'x':
                    constraints.SetMissing(guess[position]);
                    break;
                case 'y':
                    constraints.SetContains(position, guess[position]);
                    break;
                case 'g':
                    constraints.SetExactMatch(position, guess[position]);
                    break;
                default:
                    throw new Exception($"{line[position]} is not a valid input");
            }
        }

        return constraints;
    }

    public static IAnswerConstraints FromResponse(string guess, GameResponse response)
    {
        var constraints = new AnswerConstraints();

        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            switch (response.PositionHints[position])
            {
                case Hint.Black:
                    constraints.SetMissing(guess[position]);
                    break;
                case Hint.Yellow:
                    constraints.SetContains(position, guess[position]);
                    break;
                case Hint.Green:
                    constraints.SetExactMatch(position, guess[position]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return constraints;
    }

    public IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints)
    {
        return new MergedConstraints(new[] { this, newConstraints });
    }

    public static IAnswerConstraints MergeConstraints(IAnswerConstraints first, IAnswerConstraints second)
    {
        return new MergedConstraints(new[] { first, second });
    }
}
