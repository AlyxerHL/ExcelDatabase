using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ExcelDatabase.Editor.Library.Convert
{
    public static class Builder
    {
        public static string BuildScript(TableParser.Table table, IEnumerable<Validator.Col> cols)
        {
            var builder = new StringBuilder(Config.tableTemplate).Replace(
                Config.tableVariable,
                table.name
            );

            foreach (var col in cols)
            {
                var isNullable = col.cells.Any((cell) => cell.Value is null);
                var colTemplate = col.typeSpec switch
                {
                    Validator.Col.TypeSpec.Convert when col.isArray
                        => isNullable ? Config.convertNullArrCol : Config.convertArrCol,
                    Validator.Col.TypeSpec.Convert
                        => isNullable ? Config.convertNullCol : Config.convertCol,
                    _ when col.isArray
                        => isNullable ? Config.generalNullArrCol : Config.generalArrCol,
                    _ => isNullable ? Config.generalNullCol : Config.generalCol
                };

                builder
                    .Replace(Config.colTemplate, colTemplate + Config.colTemplate)
                    .Replace(
                        Config.typeVariable,
                        col.typeSpec == Validator.Col.TypeSpec.Variable ? "string" : col.type
                    )
                    .Replace(Config.nameVariable, col.name);
            }

            builder.Replace(Config.colTemplate, string.Empty);
            return builder.ToString();
        }

        public static string BuildJson(IEnumerable<Validator.Col> cols)
        {
            var ids = cols.First().cells.Keys;
            var json = ids.Select(
                (id) =>
                    cols.Where((col) => col.cells[id] is not null)
                        .ToDictionary(
                            (col) =>
                                col.typeSpec == Validator.Col.TypeSpec.Convert
                                    ? '_' + col.name
                                    : col.name,
                            (col) => col.cells[id]
                        )
            );

            return JsonConvert.SerializeObject(json, Formatting.Indented);
        }
    }
}
