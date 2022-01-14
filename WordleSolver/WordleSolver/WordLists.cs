namespace WordleSolver;

public class WordLists
{
    private readonly List<string> _legalAnswers = new();
    private readonly List<string> _legalGuesses = new();

    public List<string> LegalAnswers
    {
        get => _legalAnswers;
        init
        {
            _legalAnswers = value;
            _legalGuesses = _legalAnswers.Concat(_legalGuesses).ToList();
        }
    }

    public List<string> LegalGuesses
    {
        get => _legalGuesses;
        init => _legalGuesses = _legalAnswers.Concat(value).ToList();
    }

    public IEnumerable<string> ApplyConstraints(IAnswerConstraints newConstraint)
    {
        return LegalAnswers.Where(newConstraint.MatchesConstraint);
    }
}