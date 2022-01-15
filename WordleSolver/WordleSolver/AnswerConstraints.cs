namespace WordleSolver;

public class AnswerConstraints : IAnswerConstraints
{
    private readonly PositionConstraint[] _positionConstraints = new PositionConstraint[GameConstants.WordLength];
    private readonly Dictionary<char, CountRange> _characterCounts = new();
    private readonly List<Tuple<string, GameResponse>> _guessResponsePairs = new();

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

        public void Merge(CountRange newRange)
        {
            if (newRange._min > _min)
            {
                _min = newRange._min;
            }

            if (newRange._max < _max)
            {
                _max = newRange._max;
            }
        }
    }

    public AnswerConstraints()
        : this(Enumerable.Empty<Tuple<string, GameResponse>>())
    {

    }

    private AnswerConstraints(IEnumerable<Tuple<string, GameResponse>> guessResponsePairs)
    {
        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            _positionConstraints[position] = new PositionConstraint();
        }

        _guessResponsePairs = guessResponsePairs.ToList();
        foreach (var (guess, response) in _guessResponsePairs)
        {
            var iterationCharacterCounts = new Dictionary<char, CountRange>();
            ApplyGuessResponsePair(guess, response, this, iterationCharacterCounts);
            AddCharacterCountLimits(iterationCharacterCounts);
        }
    }

    private void AddCharacterCountLimits(Dictionary<char, CountRange> newCountRanges)
    {
        foreach (var kvp in newCountRanges)
        {
            if (!_characterCounts.TryAdd(kvp.Key, kvp.Value))
            {
                _characterCounts[kvp.Key].Merge(kvp.Value);
            }
        }
    }

    private static void EnsureDictionaryHasKey(IDictionary<char, CountRange> dict, char key)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, new CountRange());
        }
    }

    public bool MatchesConstraint(string answerToTest)
    {

        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            if (!_positionConstraints[position].IsCharacterAllowed(answerToTest[position]))
            {
                return false;
            }
        }

        foreach (var (targetCharacter, targetCharacterRange) in _characterCounts)
        {
            var firstCharacterIndex = answerToTest.IndexOf(targetCharacter);
            var characterCount = 0;
            if (firstCharacterIndex != -1)
            {

                for (var characterSearch = firstCharacterIndex;
                     characterSearch < answerToTest.Length;
                     characterSearch++)
                {
                    if (answerToTest[characterSearch] == targetCharacter)
                    {
                        characterCount++;
                    }
                }
            }

            if (!targetCharacterRange.InRange(characterCount))
            {
                return false;
            }
        }

        return true;
    }

    private void SetExactMatch(int guessPosition, char guessCharacter, Dictionary<char, CountRange> characterCounts)
    {
        EnsureDictionaryHasKey(characterCounts, guessCharacter);
        characterCounts[guessCharacter].SetLowerLimit();

        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].SetExact(guessCharacter);
    }

    private void SetContains(int guessPosition, char guessCharacter, Dictionary<char, CountRange> characterCounts)
    {
        EnsureDictionaryHasKey(characterCounts, guessCharacter);
        characterCounts[guessCharacter].SetLowerLimit();

        _positionConstraints[guessPosition] = _positionConstraints[guessPosition].ForbidCharacter(guessCharacter);
    }

    private void SetMissing(char guessCharacter, Dictionary<char, CountRange> characterCounts)
    {
        EnsureDictionaryHasKey(characterCounts, guessCharacter);
        characterCounts[guessCharacter].SetUpperLimit();

        if (!characterCounts[guessCharacter].InRange(1))
        {
            for (int position = 0; position < GameConstants.WordLength; position++)
            {
                _positionConstraints[position] = _positionConstraints[position].ForbidCharacter(guessCharacter);
            }
        }
    }

    public static IAnswerConstraints Parse(string guess, string? line)
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

        var response = GameResponse.Parse(line);
        return FromResponse(guess, response);
    }

    public static IAnswerConstraints FromResponse(string guess, GameResponse response)
    {
        return new AnswerConstraints(new[] { new Tuple<string, GameResponse>(guess, response) });
    }

    private static void ApplyGuessResponsePair(string guess,
                                               GameResponse response,
                                               AnswerConstraints constraints,
                                               Dictionary<char, CountRange> characterCounts)
    {
        for (var position = 0; position < GameConstants.WordLength; position++)
        {
            switch (response.PositionHints[position])
            {
                case Hint.Black:
                    constraints.SetMissing(guess[position], characterCounts);
                    break;
                case Hint.Yellow:
                    constraints.SetContains(position, guess[position], characterCounts);
                    break;
                case Hint.Green:
                    constraints.SetExactMatch(position, guess[position], characterCounts);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints)
    {
        if (newConstraints is AnswerConstraints other)
        {
            var mergedResponses = _guessResponsePairs.Concat(other._guessResponsePairs);
            return new AnswerConstraints(mergedResponses);
        }

        return new MergedConstraints(new[] { this, newConstraints });
    }

    public static IAnswerConstraints MergeConstraints(IAnswerConstraints first, IAnswerConstraints second)
    {
        if (first is AnswerConstraints firstConstraint && second is AnswerConstraints secondConstraints)
        {
            var mergedResponses = firstConstraint._guessResponsePairs.Concat(secondConstraints._guessResponsePairs);
            return new AnswerConstraints(mergedResponses);
        }

        return new MergedConstraints(new[] { first, second });
    }
}
