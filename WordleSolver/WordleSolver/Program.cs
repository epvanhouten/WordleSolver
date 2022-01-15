// See https://aka.ms/new-console-template for more information

using Spectre.Console.Cli;
using WordleSolver;

var app = new CommandApp<InteractiveSolverCommand>();
return await app.RunAsync(args);