namespace WordleSolver;

public class MergedConstraints : IAnswerConstraints
{
    private readonly IAnswerConstraints[] _answerConstraints;
    private readonly AnswerConstraints _newConstraints = new AnswerConstraints();

    public MergedConstraints(IEnumerable<IAnswerConstraints> answerConstraints)
    {
        _answerConstraints = answerConstraints.ToArray();
    }

    public bool MatchesConstraint(string answerToTest)
    {
        if (!_newConstraints.MatchesConstraint(answerToTest))
        {
            return false;
        }

        for (var constraintIndex = 0; constraintIndex < _answerConstraints.Length; constraintIndex++)
        {
            if (!_answerConstraints[constraintIndex].MatchesConstraint(answerToTest))
            {
                return false;
            }
        }

        return true;
    }

    public void SetExactMatch(int guessPosition, char guessCharacter)
    {
        _newConstraints.SetExactMatch(guessPosition, guessCharacter);
    }

    public void SetContains(int guessPosition, char guessCharacter)
    {
        _newConstraints.SetContains(guessPosition, guessCharacter);
    }

    public void SetMissing(char guessCharacter)
    {
        _newConstraints.SetMissing(guessCharacter);
    }

    public IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints)
    {
        return new MergedConstraints(_answerConstraints.Append(_newConstraints).Append(newConstraints));
    }
}