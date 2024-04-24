using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4___Dispatch_Calculator
{
    /// <summary>
    /// Provides a user interface for interacting with the Calculator class.
    /// </summary>
    public class Menu
    {
        private Calculator calculator;
        private Dictionary<string, Action> commandDispatchTable;


        /// <summary>
        /// Initializes a new instance of the Menu class, creates a Calculator instance, and displays initial instructions.
        /// </summary>
        public Menu()
        {
            calculator = new Calculator();
            InitializeCommandDispatchTable();
            DisplayInstructions();
        }

        private void InitializeCommandDispatchTable()
        {
            commandDispatchTable = new Dictionary<string, Action>
            {
                {"exit", () => { }},  // This is a placeholder to gracefully exit the loop in Run
                {"save to file", HandleSaveToFile},
                {"load from file", HandleLoadFromFile},
                {"undo", HandleUndo},
                {"fib", HandleFibonacci},
                {"clear", HandleClear},
                {"list variables", HandleListVariables},
                {"list", HandleListVariables}  // Alias for list variables
            };
        }

        /// <summary>
        /// Displays instructions for using the calculator to the console. This includes available operations and commands.
        /// </summary>
        private void DisplayInstructions()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Calculator is ready.");
            Console.ResetColor();

            Console.WriteLine("\nOperations:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  +  Add         -  Subtract      *  Multiply");
            Console.WriteLine("  /  Divide      %  Modulo        ^  Exponentiate");
            Console.ResetColor();

            Console.WriteLine("\nCommands:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  ans            Use the last calculated answer in your expression");
            Console.WriteLine("  save [varName] Save the last answer as a named variable");
            Console.WriteLine("  load [varName] Load a saved variable into the current expression");
            Console.WriteLine("  save to file   Save all variables to a file");
            Console.WriteLine("  load from file Load variables from a file");
            Console.WriteLine("  list           List all saved variables");
            Console.WriteLine("  undo           Undo the last calculation");
            Console.WriteLine("  fib            Calculate Fibonacci number of the last answer");
            Console.WriteLine("  clear          Reset the calculator state to zero");
            Console.WriteLine("  exit           Quit the calculator program");
            Console.ResetColor();
        }


        /// <summary>
        /// Starts the command loop, processing user input commands until 'exit' is entered.
        /// </summary>
        public void Run()
        {
            string input = "";
            while (true)
            {
                Console.Write("> ");
                input = Console.ReadLine().ToLower();

                if (input.StartsWith("save "))
                {
                    HandleSaveVariable(input);
                    continue;
                }
                else if (input.StartsWith("load "))
                {
                    HandleLoadVariable(input);
                    continue;
                }

                if (commandDispatchTable.TryGetValue(input, out Action commandAction))
                {
                    if (input == "exit") break;
                    commandAction.Invoke();
                }
                else
                {
                    HandleEvaluateExpression(input);
                }
            }
        }

        private void HandleSaveToFile()
        {
            Console.WriteLine("Enter the file path to save or press Enter to use default:");
            string filePath = Console.ReadLine();
            calculator.SaveVariablesToFile(filePath);
        }

        private void HandleLoadFromFile()
        {
            Console.WriteLine("Enter the file path to load or press Enter to use default:");
            string filePath = Console.ReadLine();
            calculator.LoadVariablesFromFile(filePath);
        }

        private void HandleUndo()
        {
            calculator.Undo();
            Console.WriteLine("Undid the last operation.");
        }

        private void HandleFibonacci()
        {
            try
            {
                var result = calculator.Fibonacci((int)calculator.LastAnswer);
                Console.WriteLine($"Fibonacci of last answer: {result}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error calculating Fibonacci: " + ex.Message);
                Console.ResetColor();
            }
        }

        private void HandleClear()
        {
            calculator.ClearState();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Calculator state cleared.");
            Console.ResetColor();
            DisplayInstructions();
        }

        private void HandleSaveVariable(string input)
        {
            var name = input.Substring(5);
            try
            {
                calculator.SaveVariable(name);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Saved current answer as '{name}'.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private void HandleLoadVariable(string input)
        {
            var name = input.Substring(5);
            try
            {
                var value = calculator.LoadVariable(name);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Loaded '{name}' with value: {value}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }

        private void HandleListVariables()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(calculator.ListVariables());
            Console.ResetColor();
        }

        private void HandleEvaluateExpression(string input)
        {
            try
            {
                var result = calculator.Evaluate(input);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Result: " + result);
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }


    }
}
