using System.Collections.Generic;
using System.Text;

namespace ExcelDatabase.Editor.Library.Enum
{
    public static class Builder
    {
        public static string BuildScript(TableParser.Table table, IEnumerable<Validator.Row> rows)
        {
            var builder = new StringBuilder(Config.table).Replace(Config.tableVariable, table.name);
            string prevGroupValue = null;

            foreach (var row in rows)
            {
                if (prevGroupValue != row.group)
                {
                    prevGroupValue = row.group;
                    builder.Replace(Config.rowTemplate, string.Empty);
                    builder
                        .Replace(Config.groupTemplate, Config.group + Config.groupTemplate)
                        .Replace(Config.groupVariable, row.group);
                }

                builder
                    .Replace(Config.rowTemplate, Config.row + Config.rowTemplate)
                    .Replace(Config.rowVariable, row.enumName);
            }

            builder.Replace(Config.rowTemplate, string.Empty);
            builder.Replace(Config.groupTemplate, string.Empty);
            return builder.ToString();
        }
    }
}
