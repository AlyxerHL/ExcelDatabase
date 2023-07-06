using System;

namespace ExcelDatabase.Editor.Parser
{
    public class ParseException : Exception
    {
        public string tableName { get; }

        public ParseException(string tableName, string message)
            : base(message)
        {
            this.tableName = tableName;
        }
    }
}
