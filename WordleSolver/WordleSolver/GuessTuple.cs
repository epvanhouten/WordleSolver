namespace WordleSolver;

public class GuessTuple
{
    private readonly GuessTrial[] _trials;

    public GuessTuple(string guess, IEnumerable<GuessTrial> trials)
    {
        Guess = guess;
        _trials = trials.ToArray();

        if (_trials.Length == 0) throw new ArgumentException("Must have at least 1 trial", nameof(trials));
    }

    public string Guess { get; }
    public double AverageAnswerListLength => _trials.Average(t => t.RemainingAnswers);
    public int WorstCase => _trials.Max(t => t.RemainingAnswers);
    public double WinRate => CanWin ? 1.0 / RemainingAnswersCount : 0;

    public int RemainingAnswersCount => _trials.Length;
    public bool CanWin => _trials.Any(t => t.IsWin);

    public override string ToString()
    {
        return $"{Guess} (avg: {AverageAnswerListLength:N2} worst: {WorstCase:N0} win rate: {WinRate:N3})";
    }
}

public struct GuessTrial
{
    public int RemainingAnswers { get; init; }
    public bool IsWin { get; init; }
}