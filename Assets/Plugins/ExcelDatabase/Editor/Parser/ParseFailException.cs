using System;

namespace ExcelDatabase.Editor.Parser
{
    public class ParseFailException : Exception
    {
        public readonly string TableName;

        public ParseFailException(string tableName, string message) : base(message)
        {
            TableName = tableName;
        }
    }
}