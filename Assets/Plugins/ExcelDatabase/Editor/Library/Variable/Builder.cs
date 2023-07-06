using System.Collections.Generic;
using System.Text;

namespace ExcelDatabase.Editor.Library.Variable
{
    public static class Builder
    {
        public static string BuildScript(TableParser.Table table, IEnumerable<Validator.Row> rows)
        {
            var builder = new StringBuilder(Config.table).Replace(Config.TableVariable, table.name);

            foreach (var row in rows)
            {
                builder
                    .Replace(Config.RowTemplate, Config.row + Config.RowTemplate)
                    .Replace(Config.TypeVariable, row.type)
                    .Replace(Config.NameVariable, row.name)
                    .Replace(
                        Config.ValueVariable,
                        row.type switch
                        {
                            "float" => row.value + 'f',
                            "bool" => row.value.ToLower(),
                            _ => row.value
                        }
                    );
            }

            builder.Replace(Config.RowTemplate, string.Empty);
            return builder.ToString();
        }
    }
}
