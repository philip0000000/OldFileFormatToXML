using System;
using System.Xml;
using System.IO;

namespace OldFileFormatToXML
{
    /* (for reference)
    P|förnamn|efternamn             P|firstname|lastname (person)
    T|mobilnummer|fastnätsnummer    T|mobile|landline    (phone)
    A|gata|stad|postnummer          A|street|city|zip    (address)
    F|namn|födelseår                F|name|born          (family)

    F|namn|födelseår                F|name|born
    T|mobilnummer|fastnätsnummer    T|mobile|landline
    A|gata|stad|postnummer          A|street|city|zip

    P kan följas av T, A och F (P can be followed by T, A and F)
    F kan följas av T och A    (F can be followed by T and A)

    P<TAF>
    F<TA>

    P|Elof|Sundin
    T|073-101801|018-101801
    A|S:t Johannesgatan 16|Uppsala|75330
    F|Hans|1967
    A|Frodegatan 13B|Uppsala|75325
    F|Anna|1969
    T|073-101802|08-101802
    P|Boris|Johnson
    A|10 Downing Street|London
    <people>
    <person>
        <firstname>Elof</firstname>
        <lastname>Sundin</lastname>
        <address>
            <street>S:t Johannesgatan 16</street>
            <city>Uppsala</city>
            <zip>75330</zip>
        </address>
        <phone>
            <mobile>073-101801</mobile>
            <landline>018-101801</landline>
        </phone>
        <family>
            <name>Hans</name>
            <born>1967</born>
            <address>...</address>
        </family>
        <family>...</family>
    </person>
    <person>...</person>
    </people>
    */

    /// <summary>
    /// Responsible for converting old file format file data
    /// to new XML data to be placed inside the new XML file.
    /// </summary>
    public class XmlParse
    {
        OldFileFormatData OldFileFormatData;

        // I/O
        XmlWriter OutputStream;
        StreamReader InputStream;

        // counting error info
        public int NumberOfEmptyLines { get; private set; }
        public int NumberOfUnresolvedLines { get; private set; }
        public int NumberOfLineConflicts { get; private set; } // if a line of data is correct but is in the wrong place

        public XmlParse()
        {
            NumberOfEmptyLines = 0;
            NumberOfUnresolvedLines = 0;
            NumberOfLineConflicts = 0;
        }

        /// <summary>
        /// Parse the input file in old file format to be converted to XML and
        /// inserted into output file.
        /// </summary>
        /// <param name="InputFile">old file format file</param>
        /// <param name="OutputFile">file name of the file that will get new XML format from InputFile</param>
        public void ParseFile(string InputFile, string OutputFile)
        {
            try
            {
                InitReadFile(InputFile);                            // check the input file is ok
                DeleteFile(OutputFile);                             // delete the output file if it exist

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    // Add indent to XML
                    //Indent = true,
                    //IndentChars = "\t"
                };

                using (OutputStream = XmlWriter.Create(OutputFile, settings)) // file to contain new XML
                using (InputStream = new StreamReader(InputFile))   // create an instance of StreamReader to read from a file
                {
                    PrintDocType();                                 // write the DocumentType node

                    // create the new XML
                    PrintXMLStartElement();
                    FindFirstPLine();                               // go to first P data in old file format
                    CreateAllPersonElements();                      // create all P data to XML elements
                    PrintXMLEndElement();
                }
            }
            catch (Exception ex)
            {
                // let the user know what went wrong
                CommandLine.PrintErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Check the input file before using it when converting old file format to XML.
        /// </summary>
        void InitReadFile(string InputFile)
        {
            // 0. Get info about the file
            FileInfo fInfo = new FileInfo(InputFile);

            // 1. Dose the file not exist. If yes, throw a exception
            if (fInfo.Exists == false)
            {
                throw new FileNotFoundException();
            }

            // 2. Is the file empty. If yes, throw a exception
            if (fInfo.Length == 0)
            {
                throw new Exception("File is empty");
            }
        }
        void DeleteFile(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Find line with P data in old file format and treat all other data as error.
        /// </summary>
        void FindFirstPLine()
        {
            string LineData;
            bool FoundPDataMark = false;

            do
            {
                LineData = InputStream.ReadLine();

                // if LineData is EOF, we could not find any line with P data, throw a exception
                if (LineData == Constants.OLD_FILE_FORMAT.EOF)
                {
                    throw new Exception("ERROR! Wrong formatted file, could not find any start(P) data in the file that was part of the old file format");
                }

                string DataMark = GetFromLineOldFileFormatDataMark(in LineData);
                switch (DataMark)
                {
                    default:
                        break;
                    case Constants.OLD_FILE_FORMAT.T_DATA_MARK:
                    case Constants.OLD_FILE_FORMAT.A_DATA_MARK:
                    case Constants.OLD_FILE_FORMAT.F_DATA_MARK:
                        NumberOfLineConflicts++;
                        break;
                    case Constants.OLD_FILE_FORMAT.P_DATA_MARK:
                        FoundPDataMark = true;
                        SaveOldFileFormatPData(ref LineData);
                        break;
                }
            } while (!FoundPDataMark);
        }

        /// <summary>
        /// Create all XML element of P data in old file format.
        /// </summary>
        void CreateAllPersonElements()
        {
            string LineData;
            bool WeAreAtTheEndOfTheFile = false;

            PrintOldFileFormatPDataInXMLStart();

            do
            {
                LineData = InputStream.ReadLine();

                // if LineData is EOF, end loop, no more P data to create
                if (LineData == Constants.OLD_FILE_FORMAT.EOF)
                {
                    WeAreAtTheEndOfTheFile = true;
                }
                else
                {
                    string DataMark = GetFromLineOldFileFormatDataMark(in LineData);
                    switch (DataMark)
                    {
                        case Constants.OLD_FILE_FORMAT.P_DATA_MARK:
                            PrintOldFileFormatPDataInXMLEnd();
                            SaveOldFileFormatPData(ref LineData);
                            PrintOldFileFormatPDataInXMLStart();
                            break;
                        case Constants.OLD_FILE_FORMAT.T_DATA_MARK:
                            SaveOldFileFormatTData(ref LineData);
                            PrintOldFileFormatTDataInXML();
                            break;
                        case Constants.OLD_FILE_FORMAT.A_DATA_MARK:
                            SaveOldFileFormatAData(ref LineData);
                            PrintOldFileFormatADataInXML();
                            break;
                        case Constants.OLD_FILE_FORMAT.F_DATA_MARK:
                            bool EndLoop = false;
                            do
                            {
                                SaveOldFileFormatFData(ref LineData);
                                PrintOldFileFormatFDataInXMLStart();

                                LineData = CreateFElementInXML();

                                if (LineData == Constants.OLD_FILE_FORMAT.EOF) // Check if EOF. If yes, end this loop and main loop
                                {
                                    EndLoop = true;
                                    WeAreAtTheEndOfTheFile = true;
                                }
                                else
                                {
                                    string DataMarkFromReturnValue = GetFromLineOldFileFormatDataMark(in LineData);

                                    switch (DataMarkFromReturnValue)
                                    {
                                        case Constants.OLD_FILE_FORMAT.P_DATA_MARK: // new P data
                                            PrintOldFileFormatFDataInXMLEnd();
                                            PrintOldFileFormatPDataInXMLEnd();

                                            SaveOldFileFormatPData(ref LineData);
                                            PrintOldFileFormatPDataInXMLStart();

                                            EndLoop = true;
                                            break;
                                        case Constants.OLD_FILE_FORMAT.F_DATA_MARK: // new F data
                                            PrintOldFileFormatFDataInXMLEnd();
                                            break;
                                        default:
                                            PrintOldFileFormatFDataInXMLEnd();
                                            EndLoop = true;
                                            break;
                                    }
                                }
                            } while (!EndLoop);
                            break;
                        default:
                            break;
                    }
                }
            } while (!WeAreAtTheEndOfTheFile);

            PrintOldFileFormatPDataInXMLEnd();
        }
        /// <summary>
        /// Create the F data in old file format in XML, return data when end-of-file(EOF), P data or F data has been found.
        /// </summary>
        /// <returns>
        /// Data that dose not belong to the F data element, can be EOF, P data or F data.
        /// </returns>
        string CreateFElementInXML()
        {
            string returnValue = Constants.OLD_FILE_FORMAT.EOF; // default value is EOF of old data format
            string LineData;
            string DataMark;
            bool NotEndLoop = true;

            do
            {
                LineData = InputStream.ReadLine();
                if (LineData == Constants.OLD_FILE_FORMAT.EOF)
                {
                    // end of file has been reached, end loop
                    PrintOldFileFormatFDataInXMLEnd();
                    NotEndLoop = false;
                }
                else
                {
                    DataMark = GetFromLineOldFileFormatDataMark(in LineData);
                    switch (DataMark)
                    {
                        case Constants.OLD_FILE_FORMAT.P_DATA_MARK: // new P data
                            NotEndLoop = false;
                            returnValue = LineData;
                            break;
                        case Constants.OLD_FILE_FORMAT.F_DATA_MARK: // new F data
                            NotEndLoop = false;
                            returnValue = LineData;
                            break;
                        case Constants.OLD_FILE_FORMAT.T_DATA_MARK:
                            SaveOldFileFormatTData(ref LineData);
                            PrintOldFileFormatTDataInXML();
                            break;
                        case Constants.OLD_FILE_FORMAT.A_DATA_MARK:
                            SaveOldFileFormatAData(ref LineData);
                            PrintOldFileFormatADataInXML();
                            break;
                        default:
                            break;
                    }
                }
            } while (NotEndLoop);

            return returnValue;
        }

        /// <summary>
        /// Get the data mark(in the old file system marks) from the LineData and check if
        /// LineData has incorrect data mark or is empty, and increase respected error number.
        /// </summary>
        string GetFromLineOldFileFormatDataMark(in string LineData)
        {
            // check if LineData is not empty
            if (LineData.Length == 0)
            {
                NumberOfEmptyLines++; // keep count of error
            }
            else
            {
                // check that 2nd mark in the data is a data segregation mark in old file format
                if (LineData.Length >= 2 && LineData[1].ToString() == Constants.OLD_FILE_FORMAT.DATA_SEGREGATION_MARK)
                {
                    // choose what data mark the input data has
                    switch (LineData[0].ToString())
                    {
                        case Constants.OLD_FILE_FORMAT.P_DATA_MARK:
                            return Constants.OLD_FILE_FORMAT.P_DATA_MARK;
                        case Constants.OLD_FILE_FORMAT.T_DATA_MARK:
                            return Constants.OLD_FILE_FORMAT.T_DATA_MARK;
                        case Constants.OLD_FILE_FORMAT.A_DATA_MARK:
                            return Constants.OLD_FILE_FORMAT.A_DATA_MARK;
                        case Constants.OLD_FILE_FORMAT.F_DATA_MARK:
                            return Constants.OLD_FILE_FORMAT.F_DATA_MARK;
                        default:
                            NumberOfUnresolvedLines++; // keep count of error
                            break;
                    }
                }
                else
                {
                    NumberOfUnresolvedLines++; // keep count of error
                }
            }

            return Constants.OLD_FILE_FORMAT.DATA_MARK_ERROR;
        }

        // These 4 function save the data from the referenced string parameter into OldFileFormatData structure
        void SaveOldFileFormatPData(ref string LineData)
        {
            // Split string and insert to P struct of OldFileFormatData. If value dose not exist, insert empty string.
            string[] DataArray = LineData.Split(new string[] { Constants.OLD_FILE_FORMAT.DATA_SEGREGATION_MARK }, StringSplitOptions.None);
            if (DataArray.Length >= 3)
                OldFileFormatData.P.Lastname = !(string.IsNullOrEmpty(DataArray[2])) ? DataArray[2] : string.Empty;
            else
                OldFileFormatData.P.Lastname = string.Empty;
            if (DataArray.Length >= 2)
                OldFileFormatData.P.Firstname = !(string.IsNullOrEmpty(DataArray[1])) ? DataArray[1] : string.Empty;
            else
                OldFileFormatData.P.Firstname = string.Empty;
        }
        void SaveOldFileFormatTData(ref string LineData)
        {
            // Split string and insert to T struct of OldFileFormatData. If value dose not exist, insert empty string.
            string[] DataArray = LineData.Split(new string[] { Constants.OLD_FILE_FORMAT.DATA_SEGREGATION_MARK }, StringSplitOptions.None);
            if (DataArray.Length >= 3)
                OldFileFormatData.T.Landline = !(string.IsNullOrEmpty(DataArray[2])) ? DataArray[2] : string.Empty;
            else
                OldFileFormatData.T.Landline = string.Empty;
            if (DataArray.Length >= 2)
                OldFileFormatData.T.Mobile = !(string.IsNullOrEmpty(DataArray[1])) ? DataArray[1] : string.Empty;
            else
                OldFileFormatData.T.Mobile = string.Empty;
        }
        void SaveOldFileFormatAData(ref string LineData)
        {
            // Split string and insert to A struct of OldFileFormatData. If value dose not exist, insert empty string.
            string[] DataArray = LineData.Split(new string[] { Constants.OLD_FILE_FORMAT.DATA_SEGREGATION_MARK }, StringSplitOptions.None);
            if (DataArray.Length >= 4)
                OldFileFormatData.A.Zip = !(string.IsNullOrEmpty(DataArray[3])) ? DataArray[3] : string.Empty;
            else
                OldFileFormatData.A.Zip = string.Empty;
            if (DataArray.Length >= 3)
                OldFileFormatData.A.City = !(string.IsNullOrEmpty(DataArray[2])) ? DataArray[2] : string.Empty;
            else
                OldFileFormatData.A.City = string.Empty;
            if (DataArray.Length >= 2)
                OldFileFormatData.A.Street = !(string.IsNullOrEmpty(DataArray[1])) ? DataArray[1] : string.Empty;
            else
                OldFileFormatData.A.Street = string.Empty;
        }
        void SaveOldFileFormatFData(ref string LineData)
        {
            // Split string and insert to F struct of OldFileFormatData. If value dose not exist, insert empty string.
            string[] DataArray = LineData.Split(new string[] { Constants.OLD_FILE_FORMAT.DATA_SEGREGATION_MARK }, StringSplitOptions.None);
            if (DataArray.Length >= 3)
                OldFileFormatData.F.Born = !(string.IsNullOrEmpty(DataArray[2])) ? DataArray[2] : string.Empty;
            else
                OldFileFormatData.F.Born = string.Empty;
            if (DataArray.Length >= 2)
                OldFileFormatData.F.Name = !(string.IsNullOrEmpty(DataArray[1])) ? DataArray[1] : string.Empty;
            else
                OldFileFormatData.F.Name = string.Empty;
        }

        // These 9 function use the data in InputStream to generate XML elements in OutputStream,
        // data in InputStream is interpreted as old file format data.
        void PrintDocType()
        {
            OutputStream.WriteStartDocument(true); // add 1st the 'standalone' directive, so to ignore DTD
            OutputStream.WriteDocType("people", null, null, Strings.DTD_OF_NEW_XML_FILE);
        }
        void PrintXMLStartElement()
        {
            OutputStream.WriteStartElement("people");
        }
        void PrintXMLEndElement()
        {
            // Write the close tag for the root element and flush buffer
            OutputStream.WriteEndElement();
            OutputStream.Flush();
        }
        void PrintOldFileFormatPDataInXMLStart()
        {
            OutputStream.WriteStartElement("person");

            OutputStream.WriteElementString("firstname", OldFileFormatData.P.Firstname);
            OutputStream.WriteElementString("lastname", OldFileFormatData.P.Lastname);

            OldFileFormatData.P.Clear();
        }
        void PrintOldFileFormatPDataInXMLEnd()
        {
            OutputStream.WriteEndElement();
        }
        void PrintOldFileFormatTDataInXML()
        {
            OutputStream.WriteStartElement("phone");

            OutputStream.WriteElementString("mobile", OldFileFormatData.T.Mobile);
            OutputStream.WriteElementString("landline", OldFileFormatData.T.Landline);

            OutputStream.WriteEndElement();

            OldFileFormatData.T.Clear();
        }
        void PrintOldFileFormatADataInXML()
        {
            OutputStream.WriteStartElement("address");

            OutputStream.WriteElementString("street", OldFileFormatData.A.Street);
            OutputStream.WriteElementString("city", OldFileFormatData.A.City);
            OutputStream.WriteElementString("zip", OldFileFormatData.A.Zip);

            OutputStream.WriteEndElement();

            OldFileFormatData.A.Clear();
        }
        void PrintOldFileFormatFDataInXMLStart()
        {
            OutputStream.WriteStartElement("family");

            OutputStream.WriteElementString("name", OldFileFormatData.F.Name);
            OutputStream.WriteElementString("born", OldFileFormatData.F.Born);

            OldFileFormatData.F.Clear();
        }
        void PrintOldFileFormatFDataInXMLEnd()
        {
            OutputStream.WriteEndElement();
        }
    }
}
