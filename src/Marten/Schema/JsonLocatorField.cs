using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline.Reflection;
using Marten.Linq;
using Marten.Util;

namespace Marten.Schema
{
    public class JsonLocatorField : Field, IField
    {
        public static JsonLocatorField For<T>(EnumStorage enumStyle, Casing casing, Expression<Func<T, object>> expression)
        {
            var property = ReflectionHelper.GetProperty(expression);

            return new JsonLocatorField("d.data", new StoreOptions(), enumStyle, casing, property);
        }

        private readonly Func<Expression, object> _parseObject = expression => expression.Value();

        public JsonLocatorField(string dataLocator, StoreOptions options, EnumStorage enumStyle, Casing casing, MemberInfo member) : base(enumStyle, member)
        {
            var memberType = member.GetMemberType();
            var memberName = member.Name.FormatCase(casing);

            var isStringEnum = memberType.IsEnum && enumStyle == EnumStorage.AsString;
            if (memberType == typeof(string) || isStringEnum)
            {
                TypedLocator = $"{dataLocator} ->> '{memberName}'";
            }
            else if (TypeMappings.TimespanTypes.Contains(memberType))
            {
                TypedLocator = $"{options.DatabaseSchemaName}.mt_immutable_timestamp({dataLocator} ->> '{memberName}')";
                RawLocator = $"CAST({dataLocator} ->> '{memberName}' as {PgType})";
            }
            else if (TypeMappings.TimespanZTypes.Contains(memberType))
            {
                TypedLocator = $"{options.DatabaseSchemaName}.mt_immutable_timestamptz({dataLocator} ->> '{memberName}')";
                RawLocator = $"CAST({dataLocator} ->> '{memberName}' as {PgType})";
            }
            else if (memberType.IsArray)
            {
                TypedLocator = $"CAST({dataLocator} ->> '{memberName}' as jsonb)";
            }
            else
            {
                TypedLocator = $"CAST({dataLocator} ->> '{memberName}' as {PgType})";
            }

            if (isStringEnum)
            {
                _parseObject = expression =>
                {
                    var raw = expression.Value();
                    return Enum.GetName(FieldType, raw);
                };
            }

            if (RawLocator.IsEmpty())
            {
                RawLocator = TypedLocator;
            }
        }

        public JsonLocatorField(string dataLocator, EnumStorage enumStyle, Casing casing, MemberInfo[] members) : base(enumStyle, members)
        {
            var locator = dataLocator;

            for (int i = 0; i < members.Length - 1; i++)
            {
                locator += $" -> '{members[i].Name.FormatCase(casing)}'";
            }

            locator += $" ->> '{members.Last().Name.FormatCase(casing)}'";

            TypedLocator = FieldType == typeof(string) ? locator : locator.ApplyCastToLocator(enumStyle, FieldType);

            var isStringEnum = FieldType.IsEnum && enumStyle == EnumStorage.AsString;
            if (isStringEnum)
            {
                _parseObject = expression =>
                {
                    var raw = expression.Value();
                    return Enum.GetName(FieldType, raw);
                };
            }
        }

        public string ToComputedIndex(DbObjectName tableName)
        {
            return $"CREATE INDEX {tableName.Name}_{MemberName.ToTableAlias()} ON {tableName.QualifiedName} (({TypedLocator}));";
        }

        public string TypedLocator { get; }
        public string RawLocator { get; }
        public string ColumnName => string.Empty;

        public void WritePatch(DocumentMapping mapping, SchemaPatch patch)
        {
            throw new NotSupportedException();
        }

        public object GetValue(Expression valueExpression)
        {
            return _parseObject(valueExpression);
        }

        public bool ShouldUseContainmentOperator()
        {
            return TypeMappings.ContainmentOperatorTypes.Contains(FieldType);
        }

        public string LocatorFor(string rootTableAlias)
        {
            // Super hokey.
            return TypedLocator.Replace("d.", rootTableAlias + ".");
        }
    }
}