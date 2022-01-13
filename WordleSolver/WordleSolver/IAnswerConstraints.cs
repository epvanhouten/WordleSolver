namespace WordleSolver;

public interface IAnswerConstraints
{
    bool MatchesConstraint(string answerToTest);
    void SetExactMatch(int guessPosition, char guessCharacter);
    void SetContains(int guessPosition, char guessCharacter);
    void SetMissing(char guessCharacter);
    IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints);
}