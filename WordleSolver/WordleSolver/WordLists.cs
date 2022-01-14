namespace WordleSolver;

public class WordLists
{
    public List<string> LegalAnswers { get; init; } = new();

    private readonly List<string> _legalAnswers = new();

    public List<string> LegalGuesses
    {
        get => LegalAnswers.Concat(_legalAnswers).ToList();
        init => _legalAnswers = value;
    }

    public IEnumerable<string> ApplyConstraints(IAnswerConstraints newConstraint)
    {
        return LegalAnswers.Where(newConstraint.MatchesConstraint);
    }
}