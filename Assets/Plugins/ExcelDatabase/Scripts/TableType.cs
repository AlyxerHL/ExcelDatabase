using Newtonsoft.Json;

namespace ExcelDatabase.Scripts
{
    [JsonObject(MemberSerialization.Fields)]
    public class TableType
    {
        public readonly string ID;
    }
}
