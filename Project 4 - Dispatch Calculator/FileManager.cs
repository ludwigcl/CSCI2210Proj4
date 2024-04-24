using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_4___Dispatch_Calculator
{
    /// <summary>
    /// Manages file operations for storing and retrieving calculator variables.
    /// </summary>
    public class FileManager
    {
        private string defaultFilePath = "calculator_variables.txt";

        /// <summary>
        /// Saves calculator variables to a specified file or a default file.
        /// </summary>
        /// <param name="variables">The dictionary of variable names and their values to save.</param>
        /// <param name="filePath">The file path where the variables should be saved. If null or empty, uses the default file path.</param>
        public void SaveVariables(Dictionary<string, double> variables, string filePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = defaultFilePath;
            }

            var lines = variables.Select(kvp => $"{kvp.Key}={kvp.Value}");
            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// Loads calculator variables from a specified file or a default file.
        /// </summary>
        /// <param name="filePath">The file path from which to load the variables. If null or empty, uses the default file path.</param>
        /// <returns>A dictionary containing the variable names and their loaded values.</returns>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
        public Dictionary<string, double> LoadVariables(string filePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                filePath = defaultFilePath;
            }

            var variables = new Dictionary<string, double>();
            if (File.Exists(filePath))
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2 && double.TryParse(parts[1], out double value))
                    {
                        variables[parts[0]] = value;
                    }
                }
            }
            else
            {
                throw new FileNotFoundException($"The file {filePath} was not found.");
            }
            return variables;
        }
    }
}
