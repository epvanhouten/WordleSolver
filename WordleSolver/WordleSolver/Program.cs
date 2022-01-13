// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using WordleSolver;

var wordLists = await JsonSerializer.DeserializeAsync<WordLists>(File.OpenRead("WordList.json"));

if (wordLists == null)
{
    Console.WriteLine("Failed to read WordList.json. Aborting.");
    return;
}

var startingGuesses = new string[] { "raise" };
var startingResponses = Array.Empty<string>();
if (args.Length != 0)
{
    startingGuesses = new string[args.Length / 2];
    startingResponses = new string[args.Length / 2];
    for (var argsIndex = 0; argsIndex < args.Length; argsIndex++)
    {
        if (argsIndex % 2 == 0)
        {
            startingGuesses[argsIndex / 2] = args[argsIndex];
        }
        else
        {
            startingResponses[argsIndex / 2] = args[argsIndex];
        }
    }
}

IAnswerConstraints constraints = new AnswerConstraints();
var possibleAnswers = wordLists.LegalAnswers;
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
    var newConstraints = AnswerConstraints.Parse(nextGuess, line);
    constraints = constraints.MergeConstraints(newConstraints);
    possibleAnswers = wordLists.ApplyConstraints(constraints).ToList();

    if (possibleAnswers.Count <= 10)
    {
        Console.WriteLine($"Remaining answers: {string.Join(", ", possibleAnswers)}");
    }
}

Console.WriteLine($"Answer: {possibleAnswers.Single()}");
