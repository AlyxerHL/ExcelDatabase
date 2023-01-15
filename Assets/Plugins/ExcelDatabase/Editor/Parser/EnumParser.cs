using System.IO;
using System.Text;
using ExcelDatabase.Editor.Config;
using NPOI.SS.UserModel;

namespace ExcelDatabase.Editor.Parser
{
    public class EnumParser
    {
        private readonly StringBuilder _builder;
        private readonly IWorkbook _workbook;
        private readonly string _tableName;

        public EnumParser(IWorkbook workbook, string tableName)
        {
            var tableTemplate = File.ReadAllText(TemplateConfig.Enum.TablePath);
            _builder = new StringBuilder(tableTemplate).Replace(TemplateConfig.Enum.TableVariable, tableName);
            _workbook = workbook;
            _tableName = tableName;
        }

        public void Parse()
        {
            var sheet = ValidateSheet();
        }

        private ISheet ValidateSheet()
        {
            for (var i = 0; i < _workbook.NumberOfSheets; i++)
            {
                var sheet = _workbook.GetSheetAt(i);
                if (!sheet.SheetName.StartsWith(ParserConfig.ExcludePrefix))
                {
                    return sheet;
                }
            }

            throw new InvalidTableException(_tableName, "Sheet not found");
        }
    }
}