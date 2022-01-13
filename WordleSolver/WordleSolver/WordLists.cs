namespace WordleSolver;

public class WordLists
{
    public List<string>? LegalAnswers { get; init; }
    public List<string>? LegalGuesses { get; init; }

    public IEnumerable<string> ApplyConstraints(IAnswerConstraints newConstraint)
    {
        return LegalAnswers.Where(newConstraint.MatchesConstraint);
    }
}