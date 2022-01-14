namespace WordleSolver.FitnessFunction;

public class HybridStrategy : IComparer<GuessTuple>
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

        if (x.WorstCase < y.WorstCase)
        {
            return 1;
        }

        if (x.WorstCase > y.WorstCase)
        {
            return -1;
        }

        if (x.WinRate > y.WinRate)
        {
            return 1;
        }

        if (x.WinRate < y.WinRate)
        {
            return -1;
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