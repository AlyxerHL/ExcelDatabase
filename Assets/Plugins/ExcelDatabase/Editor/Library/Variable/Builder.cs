using System.Collections.Generic;
using System.Text;

namespace ExcelDatabase.Editor.Library.Variable
{
    public static class Builder
    {
        public static string BuildScript(TableParser.Table table, IEnumerable<Validator.Row> rows)
        {
            var builder = new StringBuilder(Config.table).Replace(Config.tableVariable, table.name);

            foreach (var row in rows)
            {
                builder
                    .Replace(Config.rowTemplate, Config.row + Config.rowTemplate)
                    .Replace(Config.typeVariable, row.type)
                    .Replace(Config.nameVariable, row.name)
                    .Replace(
                        Config.valueVariable,
                        row.type switch
                        {
                            "float" => row.value + 'f',
                            "bool" => row.value.ToLower(),
                            _ => row.value
                        }
                    );
            }

            builder.Replace(Config.rowTemplate, string.Empty);
            return builder.ToString();
        }
    }
}
