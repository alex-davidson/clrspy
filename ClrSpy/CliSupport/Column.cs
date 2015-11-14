namespace ClrSpy.CliSupport
{
    public class Column
    {
        public Column(string header = null)
        {
            Header = header;
        }

        public string Header { get; }
        public int? Padding { get; set; }
        public int? Width { get; set; }
        public bool RightAlign { get; set; }
    }
}