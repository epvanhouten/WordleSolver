namespace WordleSolver;

public class GuessTuple
{
    public string? Guess { get; init; }
    public double AverageAnswerListLength { get; init; }

    public override string ToString()
    {
        return $"{Guess} (average answers left: {AverageAnswerListLength})";
    }
}