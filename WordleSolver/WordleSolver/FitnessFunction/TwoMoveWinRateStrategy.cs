namespace WordleSolver.FitnessFunction;

public class TwoMoveWinRateStrategy : FitnessFunctionBase
{
    protected override int FitnessFunction(GuessTuple x, GuessTuple y)
    {
        return BestNextMoveWinRate(x, y);
    }

    private static int BestNextMoveWinRate(GuessTuple left, GuessTuple right)
    {
        var leftTwoMoveWinRate = TwoMoveWinRate(left);
        var rightTwoMoveWinRate = TwoMoveWinRate(right);

        if (leftTwoMoveWinRate > rightTwoMoveWinRate)
        {
            return 1;
        }

        if (leftTwoMoveWinRate < rightTwoMoveWinRate)
        {
            return -1;
        }

        return 0;
    }

    private static double TwoMoveWinRate(GuessTuple guess)
    {
        var thisMoveWinRate = guess.CanWin ? 1.0 / guess.RemainingAnswersCount : 0.0;
        var averageNextMoveWinRate = 1.0 / guess.AverageAnswerListLength;
        var twoMoveWinRate = thisMoveWinRate + (1 - thisMoveWinRate) * averageNextMoveWinRate;
        return twoMoveWinRate;
    }
}