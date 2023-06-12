using System;

namespace ExcelDatabase.Editor.Parser
{
    public class ParserException : Exception
    {
        public string tableName { get; }

        public ParserException() { }

        public ParserException(string message)
            : base(message) { }

        public ParserException(string message, Exception innerException)
            : base(message, innerException) { }

        public ParserException(string tableName, string message)
            : base(message)
        {
            this.tableName = tableName;
        }

        public ParserException(string tableName, string message, Exception innerException)
            : base(message, innerException)
        {
            this.tableName = tableName;
        }
    }
}
