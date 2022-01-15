using System.Text.Json;
using Spectre.Console.Cli;

namespace WordleSolver;

public class InteractiveSolverCommand : AsyncCommand<InteractiveSolverCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--word-list")]
        public string WordListPath { get; init; } = "WordList.json";

        [CommandArgument(0, "[GuessResponseSequence]")]
        public string[] GuessResponseSequence { get; init; } = new[] { "raise" };
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var wordLists = await JsonSerializer.DeserializeAsync<WordLists>(File.OpenRead(settings.WordListPath));

        if (wordLists == null)
        {
            Console.WriteLine("Failed to read WordList.json. Aborting.");
            return -1;
        }

        var startingGuesses = new string[(int)Math.Ceiling((double)settings.GuessResponseSequence.Length / 2)];
        var startingResponses = new string[settings.GuessResponseSequence.Length / 2];
        for (var argsIndex = 0; argsIndex < settings.GuessResponseSequence.Length; argsIndex++)
        {
            if (argsIndex % 2 == 0)
            {
                startingGuesses[argsIndex / 2] = settings.GuessResponseSequence[argsIndex];
            }
            else
            {
                startingResponses[argsIndex / 2] = settings.GuessResponseSequence[argsIndex];
            }
        }

        IAnswerConstraints constraints = new AnswerConstraints();
        var possibleAnswers = wordLists.LegalAnswers ?? throw new Exception("Legal answers failed to load");
        for (var answerCycle = 0; answerCycle < startingGuesses.Length; answerCycle++)
        {
            Console.WriteLine($"Guess: {startingGuesses[answerCycle]}");
            if (startingResponses.Length > answerCycle)
            {
                Console.WriteLine($"Response: {startingResponses[answerCycle]}");
                var newConstraints = AnswerConstraints.Parse(startingGuesses[answerCycle], startingResponses[answerCycle]);
                constraints = constraints.MergeConstraints(newConstraints);
                possibleAnswers = wordLists.ApplyConstraints(constraints).ToList();
                Console.WriteLine($"Number of possible answers: {possibleAnswers.Count}");
            }
            else
            {
                Console.WriteLine("Enter result: x = 'black', y = 'yellow', g = 'green'");
                var line = Console.ReadLine();
                var newConstraints = AnswerConstraints.Parse(startingGuesses[answerCycle], line);
                constraints = constraints.MergeConstraints(newConstraints);
            }
        }

        while (possibleAnswers.Count != 1)
        {
            var nextGuess = await GuessGenerator.GetGuessAsync(wordLists, constraints);
            Console.WriteLine($"Guess: {nextGuess}");
            Console.WriteLine("Enter result: x = 'black', y = 'yellow', g = 'green'");
            var line = Console.ReadLine();
            var newConstraints = AnswerConstraints.Parse(nextGuess.Guess, line);
            constraints = constraints.MergeConstraints(newConstraints);
            possibleAnswers = wordLists.ApplyConstraints(constraints).ToList();

            if (possibleAnswers.Count <= 10)
            {
                Console.WriteLine($"Remaining answers: {string.Join(", ", possibleAnswers)}");
            }
        }

        Console.WriteLine($"Answer: {possibleAnswers.Single()}");
        return 0;
    }
}