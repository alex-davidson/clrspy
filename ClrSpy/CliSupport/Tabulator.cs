using System;
using System.IO;
using System.Linq;

namespace ClrSpy.CliSupport
{
    /// <summary>
    /// Basic tabulator for console output. Formats a series of values into columns.
    /// Terminating line break is not included in the formatted string.
    /// </summary>
    public class Tabulator
    {
        private readonly Column[] columns;
        public Column Defaults { get; } = new Column { Padding = 1 };
        public int LeftPadding { get; set; }

        public Tabulator(params Column[] columns)
        {
            this.columns = columns;
        }

        public string GetHeader()
        {
            var writer = new StringWriter();
            Tabulate(writer, columns.Select(c => c.Header).ToArray<object>());
            return writer.ToString();
        }

        public string Tabulate(params object[] values)
        {
            var writer = new StringWriter();
            Tabulate(writer, values);
            return writer.ToString();
        }

        public void Tabulate(TextWriter writer, params object[] values)
        {
            writer.Write(new String(' ', LeftPadding));
            for(var i = 0; i < values.Length; i++)
            {
                var column = columns.ElementAtOrDefault(i) ?? Defaults;
                writer.Write(FormatCell(column, values[i]));
            }
        }

        private string FormatCell(Column column, object value)
        {
            var width = column.Width ?? Defaults.Width ?? 0;
            var padding = column.Padding ?? Defaults.Padding ?? 0;
            return Align(width, column.RightAlign, value?.ToString()).PadRight(width + padding);
        }
        
        private static string Align(int width, bool right, string value)
        {
            value = value ?? "";
            if(right) return value.PadLeft(width);
            return value.PadRight(width);
        }
    }
}
