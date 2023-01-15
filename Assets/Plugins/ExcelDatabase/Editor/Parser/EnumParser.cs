using System.IO;
using System.Text;
using ExcelDatabase.Editor.Config;
using ExcelDatabase.Editor.Tools;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser
    {
        private readonly StringBuilder _builder;
        private readonly TableHandler _tableHandler;
        private readonly string _tableName;

        public EnumParser(IWorkbook workbook, string tableName)
        {
            var tableTemplate = File.ReadAllText(TemplateConfig.Enum.TablePath);
            _builder = new StringBuilder(tableTemplate).Replace(TemplateConfig.Enum.TableVariable, tableName);
            _tableHandler = new TableHandler(workbook);
            _tableName = tableName;
        }

        public void Parse()
        {
        }
    }
}