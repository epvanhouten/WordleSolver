namespace WordleSolver;

public interface IAnswerConstraints
{
    bool MatchesConstraint(string answerToTest);
    IAnswerConstraints MergeConstraints(IAnswerConstraints newConstraints);
}