namespace WordleSolver.FitnessFunction;

public abstract class FitnessFunctionBase : IComparer<GuessTuple>
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

        return FitnessFunction(x, y);
    }

    protected abstract int FitnessFunction(GuessTuple x, GuessTuple y);
}