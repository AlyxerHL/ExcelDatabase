using System;

namespace ExcelDatabase.Editor.Parser {
    public class ParserException : Exception {
        public readonly string TableName;

        public ParserException() { }
        public ParserException(string message) : base(message) { }
        public ParserException(string message, Exception innerException) : base(message, innerException) { }

        public ParserException(string tableName, string message) : base(message) {
            TableName = tableName;
        }

        public ParserException(
            string tableName,
            string message,
            Exception innerException
        ) : base(message, innerException) {
            TableName = tableName;
        }
    }
}
