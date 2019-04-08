using System.Reflection;

namespace Marten.Linq.Fields
{
    public class JSONBField : FieldBase
    {
        public JSONBField(string dataLocator, Casing casing, MemberInfo[] members) : base(dataLocator, "jsonb", casing, members)
        {
            RawLocator = $"CAST({RawLocator} as {PgType})";
            TypedLocator = RawLocator;
        }
    }
}