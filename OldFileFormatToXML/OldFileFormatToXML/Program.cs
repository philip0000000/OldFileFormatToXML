using System;

namespace OldFileFormatToXML
{
    class Program
    {
        /// <summary>
        /// Main entry point of OldFileFormatToXML
        /// Parses arguments and runs operations
        /// </summary>
        /// <param name="args">Program arguments</param>
        private static void Main(string[] args)
        {
            // Check if no arguments or more then 2
            if (args.Length == 0 || args.Length > 2)
            {
                CommandLine.Help();
                Environment.Exit(0);
            }

            // Parse command line arguments
            CommandLine.Parse(args, out string InputFile, out string OutputFile);

            // Create the xml object were all the parseing is happening from old file format to XML
            XmlParse xml = new XmlParse();

            // Parse the old file format to XML from a old file that exist to a new file that is being created
            xml.ParseFile(InputFile, OutputFile);
            // Print error that occured when parsing the old file format to XML
            CommandLine.PrintErrors(xml.NumberOfEmptyLines, xml.NumberOfUnresolvedLines, xml.NumberOfLineConflicts, InputFile);

            Environment.Exit(0);
        }
    }
}
