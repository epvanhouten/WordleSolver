namespace WordleSolver;

public class GuessTuple
{
    public string Guess { get; init; } = string.Empty;
    public double AverageAnswerListLength { get; init; }
    public int WorstCase { get; init; }
    public double WinRate { get; init; }

    public override string ToString()
    {
        return $"{Guess} (avg: {AverageAnswerListLength:N2} worst: {WorstCase:N0} win rate: {WinRate:N3})";
    }
}