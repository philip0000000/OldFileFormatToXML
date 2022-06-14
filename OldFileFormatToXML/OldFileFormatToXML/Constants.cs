namespace OldFileFormatToXML
{
    // Constant values the program uses are defined here.

    class Constants
    {
        // for old file format
        public struct OLD_FILE_FORMAT
        {
            public const string DATA_SEGREGATION_MARK = "|";

            public const string P_DATA_MARK = "P";
            public const string T_DATA_MARK = "T";
            public const string A_DATA_MARK = "A";
            public const string F_DATA_MARK = "F";

            public const string EOF = null;
            public const string DATA_MARK_ERROR = ""; // string.Empty
        }

        // for XML
        public struct XML
        {
            public const string FILENAME_EXTENSION = ".xml";
        }
    }
}
