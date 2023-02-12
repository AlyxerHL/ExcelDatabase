using System;

namespace ExcelDatabase.Editor.Parser
{
    public class ParserException : Exception
    {
        public readonly string TableName;
        public readonly bool Yielding;

        public ParserException(string tableName, string message, bool yielding = false) : base(message)
        {
            TableName = tableName;
            Yielding = yielding;
        }
    }
}