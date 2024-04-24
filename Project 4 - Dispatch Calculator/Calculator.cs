using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace Project_4___Dispatch_Calculator
{
    /// <summary>
    /// Manages calculations, variable storage, and undo functionality.
    /// </summary>
    public class Calculator
    {
        public double LastAnswer { get; private set; }
        private Dictionary<string, double> variables = new Dictionary<string, double>();
        private Stack<double> history = new Stack<double>();
        private FileManager fileManager = new FileManager();

        /// <summary>
        /// Initializes the calculator with initial history state.
        /// </summary>
        public Calculator()
        {
            history.Push(0);  // Initialize history with an initial state.
        }

        /// <summary>
        /// Resets the calculator's state and history.
        /// </summary>
        public void ClearState()
        {
            LastAnswer = 0;
            history.Clear();
            history.Push(LastAnswer);
        }

        /// <summary>
        /// Reverts to the previous calculation result, if available.
        /// </summary>
        public void Undo()
        {
            if (history.Count > 1)  // Ensure there is a previous state to revert to.
            {
                history.Pop();
                LastAnswer = history.Peek();
            }
        }

        /// <summary>
        /// Saves all current variables to a specified file or the default file.
        /// </summary>
        /// <param name="filePath">Optional file path for saving variables.</param>
        public void SaveVariablesToFile(string filePath = null)
        {
            // Set default file path if none provided or if it's blank after trimming.
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "calculator_variables.txt";
            }
            // Ensure we trim any leading/trailing whitespace in the filePath.
            filePath = filePath.Trim();

            fileManager.SaveVariables(variables, filePath);
            Console.WriteLine($"Variables saved to {filePath}.");
        }

        /// <summary>
        /// Loads variables from a specified file or the default file.
        /// </summary>
        /// <param name="filePath">Optional file path for loading variables.</param>
        public void LoadVariablesFromFile(string filePath = null)
        {
            // Set default file path if none provided or if it's blank after trimming.
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = "calculator_variables.txt";
            }
            // Ensure we trim any leading/trailing whitespace in the filePath.
            filePath = filePath.Trim();

            try
            {
                variables = fileManager.LoadVariables(filePath);
                if (variables.ContainsKey("ans"))
                {
                    LastAnswer = variables["ans"];
                }
                Console.WriteLine($"Variables loaded from {filePath}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Saves the last result as a variable with a specified name.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        public void SaveVariable(string name)
        {
            if (!IsValidName(name))
            {
                throw new ArgumentException("Variable name is not valid.");
            }
            variables[name] = LastAnswer;
        }

        /// <summary>
        /// Loads a variable by name into the calculator's current state.
        /// </summary>
        /// <param name="name">Name of the variable to load.</param>
        /// <returns>The value of the loaded variable.</returns>
        public double LoadVariable(string name)
        {
            if (!variables.TryGetValue(name, out double value))
            {
                throw new KeyNotFoundException($"Variable '{name}' not found.");
            }
            LastAnswer = value;
            return value;
        }

        /// <summary>
        /// Lists all saved variables and their values.
        /// </summary>
        /// <returns>A formatted string of all variables.</returns>
        public string ListVariables()
        {
            if (variables.Count == 0)
            {
                return "No variables saved.";
            }

            var result = new StringBuilder("Saved Variables:\n");
            foreach (var variable in variables)
            {
                result.AppendLine($"{variable.Key} = {variable.Value}");
            }
            return result.ToString();
        }

        /// <summary>
        /// Validates the variable name.
        /// </summary>
        /// <param name="name">Name to validate.</param>
        /// <returns>True if the name is valid, otherwise false.</returns>
        private bool IsValidName(string name)
        {
            return Regex.IsMatch(name, "^[a-z]+$");
        }

        /// <summary>
        /// Evaluates a mathematical expression.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The result of the evaluation.</returns>
        public double Evaluate(string expression)
        {
            expression = Regex.Replace(expression, @"\s+", "");
            expression = expression.Replace("ans", LastAnswer.ToString());

            // Replace known variables with their values
            foreach (var variable in variables)
            {
                expression = expression.Replace(variable.Key, variable.Value.ToString());
            }

            // Check for unknown variables after known substitutions
            var match = Regex.Match(expression, @"[a-zA-Z]+");
            if (match.Success)
            {
                throw new ArgumentException($"Unknown variable or function: '{match.Value}'");
            }

            // Custom parsing to handle exponentiation
            if (expression.Contains("^"))
            {
                var parts = expression.Split('^');
                if (parts.Length == 2 && double.TryParse(parts[0], out double baseValue) && double.TryParse(parts[1], out double exponent))
                {
                    LastAnswer = Exponentiate(baseValue, (int)exponent);
                    history.Push(LastAnswer);
                    return LastAnswer;
                }
                else
                {
                    throw new ArgumentException("Invalid syntax for exponentiation. Ensure it's in the format 'base^exponent'.");
                }
            }

            try
            {
                var table = new DataTable();
                LastAnswer = Convert.ToDouble(table.Compute(expression, ""));
            }
            catch (Exception ex)
            {
                // Provide a more generic error message for other syntax errors
                throw new ArgumentException($"Error in expression. Please check the syntax: {ex.Message}");
            }
            history.Push(LastAnswer);
            return LastAnswer;
        }

        /// <summary>
        /// Calculates the result of raising a base number to the power of an exponent.
        /// </summary>
        /// <param name="baseValue">The base number.</param>
        /// <param name="exponent">The exponent to which the base number is raised.</param>
        /// <returns>The result of the exponentiation.</returns>
        /// <remarks>
        /// Handles negative exponents by returning the reciprocal of the base raised to the absolute value of the exponent.
        /// </remarks>
        private double Exponentiate(double baseValue, int exponent)
        {
            double result = 1;
            for (int i = 1; i <= Math.Abs(exponent); i++)
            {
                result *= baseValue;
            }
            return exponent < 0 ? 1 / result : result;
        }

        /// <summary>
        /// Evaluates expressions given in Reverse Polish Notation (RPN).
        /// </summary>
        /// <param name="tokens">An array of strings representing tokens in RPN order.</param>
        /// <returns>The result of the RPN expression.</returns>
        /// <exception cref="ArgumentException">Thrown when an unsupported operator is encountered.</exception>
        /// <remarks>
        /// Supports basic arithmetic operations (+, -, *, /) and exponentiation (^).
        /// </remarks>
        public double EvaluateRPN(string[] tokens)
        {
            Stack<double> stack = new Stack<double>();
            foreach (var token in tokens)
            {
                if (double.TryParse(token, out double num))
                {
                    stack.Push(num);
                }
                else
                {
                    double b = stack.Pop();
                    double a = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(a + b); break;
                        case "-": stack.Push(a - b); break;
                        case "*": stack.Push(a * b); break;
                        case "/": stack.Push(a / b); break;
                        case "^": stack.Push(Math.Pow(a, b)); break;
                        default: throw new ArgumentException($"Unsupported operator: {token}");
                    }
                }
            }

            LastAnswer = stack.Peek();
            history.Push(LastAnswer);
            return LastAnswer;
        }

        /// <summary>
        /// Computes the nth Fibonacci number.
        /// </summary>
        /// <param name="n">The position in the Fibonacci sequence to compute.</param>
        /// <returns>The nth Fibonacci number.</returns>
        /// <remarks>
        /// The Fibonacci sequence is a series of numbers where each number is the sum of the two preceding ones, usually starting with 0 and 1.
        /// </remarks>
        public double Fibonacci(int n)
        {
            if (n <= 1) return n;
            int a = 0, b = 1;
            for (int i = 2; i <= n; i++)
            {
                int temp = b;
                b = a + b;
                a = temp;
            }
            return b;
        }
    }
}
