using System;

namespace ExcelDatabase.Editor.Parser
{
    public class ParserException : Exception
    {
        public readonly string TableName;

        public ParserException(string tableName, string message) : base(message)
        {
            TableName = tableName;
        }
    }
}