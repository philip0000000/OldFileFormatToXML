using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OldFileFormatToXML
{
    /// <summary>
    /// Responsible for all commandline operations
    /// </summary>
    class CommandLine
    {
        /// <summary>
        /// Displays help information
        /// </summary>
        public static void Help()
        {
            string version = "";
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            if (v != null)
            {
                version = $"v{v.Major}.{v.Minor}";
            }

            // Print help message
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Name + " " + version);
            Console.WriteLine();
            Console.WriteLine(Strings.HELP_MSG);
        }

        /// <summary>
        /// Parsing command line arguments and store input file and maybe output file if specified.
        /// </summary>
        /// <param name="args">Console input</param>
        /// <param name="InputFile">Name of input file</param>
        /// <param name="OutputFile">Name of output file</param>
        public static void Parse(string[] args, ref string InputFile, ref string OutputFile)
        {
            if (args.Length == 1)
            {
                InputFile = args[0];

                // Add first part of input file name to the new file name
                if (args[0].Contains('.'))
                {
                    OutputFile = args[0].Split('.')[0];
                }
                else
                {
                    OutputFile = args[0];
                }

                // Add XML filename extension
                OutputFile += Constants.XML.FILENAME_EXTENSION;
            }
            else //args.Length == 2
            {
                InputFile = args[0];
                OutputFile = args[1];
            }
        }

        /// <summary>
        /// Print the error that have appeared after parsing successfully old file format to XML.
        /// </summary>
        public static void PrintErrors(int NumberOfEmptyLines, int NumberOfUnresolvedLines, int NumberOfLineConflicts, string InputFile)
        {
            if (NumberOfEmptyLines > 0)
            {
                Console.WriteLine($"ERROR! The old file format file {InputFile} had {NumberOfEmptyLines} empty lines!");
            }
            if (NumberOfUnresolvedLines > 0)
            {
                Console.WriteLine($"ERROR! The old file format file {InputFile} had {NumberOfUnresolvedLines} unresolved lines!");
            }
            if (NumberOfLineConflicts > 0)
            {
                Console.WriteLine($"ERROR! The old file format file {InputFile} had {NumberOfLineConflicts} line conflicts!");
            }
        }

        /// <summary>
        /// Print the message to error output stream.
        /// </summary>
        /// <param name="message"></param>
        public static void PrintErrorMessage(string message)
        {
            TextWriter errorWriter = Console.Error;
            errorWriter.WriteLine(message);
        }
    }
}
