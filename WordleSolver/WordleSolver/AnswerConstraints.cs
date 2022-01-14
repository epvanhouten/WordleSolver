using Microsoft.Extensions.ObjectPool;

namespace WordleSolver;

public class AnswerConstraints : IAnswerConstraints
{
    private readonly PositionConstraint[] _positionConstraints = new PositionConstraint[GameConstants.WordLength];
    private readonly Dictionary<char, CountRange> _characterCounts = new();

    private class CountRange
    {
        private int _min = 0;
        private int _max = GameConstants.WordLength;

        public void SetUpperLimit()
        {
            _max = _min;
        }

        public void SetLowerLimit()
        {
            _min++;

            if (_min > _max)
            {
                SetUpperLimit();
            }
        }

        public bool InRange(int observedCount)
        {
            return observedCount >= _min &&
                   observedCount <= _max;
        }

        public override string ToString()
        {
            return $"{_min}:{_max}";
        }
    }

    public AnswerConstraints()
    {
        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            _positionConstraints[position] = new PositionConstraint();
        }
    }

    private static void EnsureDictionaryHasKey(IDictionary<char, CountRange> dict, char key)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, new CountRange());
        }
    }

    private static readonly ObjectPool<Dictionary<char, int>> ObservedCharacterCountPool =
        ObjectPool.Create<Dictionary<char, int>>();

    public bool MatchesConstraint(string answerToTest)
    {
        var observedCharacterCounts = ObservedCharacterCountPool.Get();
        observedCharacterCounts.Clear();

        try
        {
            for (var position = 0; position < GameConstants.WordLength; position++)
            {
                if (!_positionConstraints[position].IsCharacterAllowed(answerToTest[position]))
                {
                    return false;
                }

                if (!observedCharacterCounts.ContainsKey(answerToTest[position]))
                {
                    observedCharacterCounts.Add(answerToTest[position], 1);
                }
                else
                {
                    observedCharacterCounts[answerToTest[position]]++;
                }
            }

            return observedCharacterCounts.Where(kvp => _characterCounts.ContainsKey(kvp.Key))
                .All(kvp => _characterCounts[kvp.Key].InRange(kvp.Value));
        }
        finally
        {
            ObservedCharacterCountPool.Return(observedCharacterCounts);
        }
    }

    public void SetExactMatch(int guessPosition, char guessCharacter)
    {
        EnsureDictionaryHasKey(_characterCounts, guessCharacter);
        _characterCounts[guessCharacter].SetLowerLimit();
        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].SetExact(guessCharacter);
    }

    public void SetContains(int guessPosition, char guessCharacter)
    {
        EnsureDictionaryHasKey(_characterCounts, guessCharacter);
        _characterCounts[guessCharacter].SetLowerLimit();

        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].ForbidCharacter(guessCharacter);
    }

    public void SetMissing(char guessCharacter)
    {
        EnsureDictionaryHasKey(_characterCounts, guessCharacter);
        _characterCounts[guessCharacter].SetUpperLimit();

        if (!_characterCounts[guessCharacter].InRange(1))
        {
            for (int position = 0; position < GameConstants.WordLength; position++)
            {
                _positionConstraints[position] = _positionConstraints[position].ForbidCharacter(guessCharacter);
            }
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
