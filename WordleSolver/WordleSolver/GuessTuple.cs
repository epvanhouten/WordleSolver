namespace WordleSolver;

public class GuessTuple
{
    public string? Guess { get; init; }
    public double AverageAnswerListLength { get; init; }
    public int WorstCase { get; init; }

    public override string ToString()
    {
        return $"{Guess} (avg: {AverageAnswerListLength:N2} worst: {WorstCase:N0})";
    }
}