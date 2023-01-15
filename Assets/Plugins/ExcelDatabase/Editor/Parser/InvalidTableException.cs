using System;

namespace ExcelDatabase.Editor.Parser
{
    public class InvalidTableException : Exception
    {
        public InvalidTableException(string tableName, string message) : base($"{tableName}: {message}")
        {
        }
    }
}