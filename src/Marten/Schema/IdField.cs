using System;
using System.Linq.Expressions;
using System.Reflection;
using Marten.Linq;
using Marten.Util;

namespace Marten.Schema
{
    public class IdField : IField
    {
        private readonly MemberInfo _idMember;

        public IdField(MemberInfo idMember)
        {
            _idMember = idMember;
        }

        public MemberInfo[] Members => new[] {_idMember};
        public string MemberName => _idMember.Name;
        public string TypedLocator { get; } = "d.id";
        public string RawLocator { get; } = "d.id";
        public string ColumnName { get; } = "id";

        public object GetValue(Expression valueExpression)
        {
            return valueExpression.Value();
        }

        public Type FieldType => _idMember.GetMemberType();
        public bool ShouldUseContainmentOperator()
        {
            return false;
        }

        public string LocatorFor(string rootTableAlias)
        {
            return rootTableAlias + ".id";
        }
    }
}