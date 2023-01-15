using System;

namespace ExcelDatabase.Editor.Tools
{
    public class InvalidTableException : Exception
    {
        public InvalidTableException(string tableName, string message) : base($"{tableName}: {message}")
        {
        }
    }
}