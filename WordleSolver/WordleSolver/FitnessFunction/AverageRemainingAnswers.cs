namespace WordleSolver.FitnessFunction;

public class AverageRemainingAnswers : IComparer<GuessTuple>
{
    public int Compare(GuessTuple? x, GuessTuple? y)
    {
        if (x is null && y is null)
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        if (x.AverageAnswerListLength < y.AverageAnswerListLength)
        {
            return 1;
        }

        if (x.AverageAnswerListLength > y.AverageAnswerListLength)
        {
            return -1;
        }

        return 0;
    }
}