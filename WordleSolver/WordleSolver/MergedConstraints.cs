namespace WordleSolver;

public class MergedConstraints : IAnswerConstraints
{
    private readonly IAnswerConstraints[] _answerConstraints;

    public MergedConstraints(IEnumerable<IAnswerConstraints> answerConstraints)
    {
        _answerConstraints = answerConstraints.ToArray();
    }

    public bool MatchesConstraint(string answerToTest)
    {
        for (var constraintIndex = 0; constraintIndex < _answerConstraints.Length; constraintIndex++)
        {
            if (!_answerConstraints[constraintIndex].MatchesConstraint(answerToTest))
            {
                return false;
            }
        }

        return true;
    }

    public IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints)
    {
        return new MergedConstraints(_answerConstraints.Append(newConstraints));
    }
}