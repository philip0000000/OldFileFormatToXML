namespace OldFileFormatToXML
{
    // These 4 struct encapsulate data from the old file format in the respected order of P, T, A and F.
    // While OldFileFormatData struct contains all these 4 struct for better organisation of data.

    public struct Pdata
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public void Clear()
        {
            Firstname = string.Empty;
            Lastname = string.Empty;
        }
    }
    public struct Tdata
    {
        public string Mobile { get; set; }
        public string Landline { get; set; }
        public void Clear()
        {
            Mobile = string.Empty;
            Landline = string.Empty;
        }
    }
    public struct Adata
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public void Clear()
        {
            Street = string.Empty;
            City = string.Empty;
            Zip = string.Empty;
        }
    }
    public struct Fdata
    {
        public string Name { get; set; }
        public string Born { get; set; }
        public void Clear()
        {
            Name = string.Empty;
            Born = string.Empty;
        }
    }

    struct OldFileFormatData
    {
        public Pdata P;
        public Tdata T;
        public Adata A;
        public Fdata F;
    }
}
