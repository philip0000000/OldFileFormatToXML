namespace OldFileFormatToXML
{
    // Constant strings the program uses are defined here.

    class Strings
    {
        // DTD is valid according to https://www.truugo.com/xml_validator/
        public const string DTD_OF_NEW_XML_FILE =
@"<!ELEMENT people (person*)> <!ELEMENT person (firstname, lastname, phone*, address*, family*)><!ELEMENT firstname (#PCDATA)><!ELEMENT lastname (#PCDATA)><!ELEMENT phone (mobile, landline)><!ELEMENT mobile (#PCDATA)><!ELEMENT landline (#PCDATA)><!ELEMENT address (street, city, zip)><!ELEMENT street (#PCDATA)><!ELEMENT city (#PCDATA)><!ELEMENT zip (#PCDATA)><!ELEMENT family (name, born, address*, phone*)><!ELEMENT name (#PCDATA)><!ELEMENT born (#PCDATA)>";

        // https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/command-line-syntax-key
        public const string HELP_MSG =
@"Description:
        Convert old file format to XML format.

Usage:
        OldFileFormatToXML  <Old file format file> [Name of new XML file]

Options:
        [Name of new XML file]    If no XML file name is provided, the
                                  old file name will be used with XML 
                                  filename extension.

Written by philip0000000
Find the project here [https://github.com/philip0000000/OldFileFormatToXML]";
    }
}
