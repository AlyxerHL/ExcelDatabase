using System.IO;
using System.Text;
using ExcelDatabase.Editor.Config;
using NPOI.SS.UserModel;
using UnityEditor;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser
    {
        private StringBuilder _builder;
        private IWorkbook _workbook;
        private string _tableName;

        public EnumParser(IWorkbook workbook, string tableName)
        {
            var tableTemplate = File.ReadAllText(TemplateConfig.Enum.TablePath);
            _builder = new StringBuilder(tableTemplate);
            _workbook = workbook;
            _tableName = tableName;
        }

        public void Parse()
        {
            ProcessTableTemplate();
        }

        private void ProcessTableTemplate()
        {
        }
    }
}