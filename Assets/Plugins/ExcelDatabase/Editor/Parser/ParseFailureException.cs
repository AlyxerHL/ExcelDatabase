using System;

namespace ExcelDatabase.Editor.Parser
{
    // Q: 파싱에 실패했을 때 던질 예외의 적절한 이름은
    public class ParseFailureException : Exception
    {
        public readonly string TableName;

        public ParseFailureException(string tableName, string message) : base(message)
        {
            TableName = tableName;
        }

        public ParseFailureException(string tableName, string message, Exception inner) : base(message, inner)
        {
            TableName = tableName;
        }
    }
}