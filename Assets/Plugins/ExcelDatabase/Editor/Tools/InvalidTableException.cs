using System;

namespace ExcelDatabase.Editor.Tools
{
    public class InvalidTableException : Exception
    {
        public readonly string TableName;

        public InvalidTableException(string tableName, string message) : base(message)
        {
            TableName = tableName;
        }

        public InvalidTableException(string tableName, string message, Exception inner) : base(message, inner)
        {
            TableName = tableName;
        }
    }
}