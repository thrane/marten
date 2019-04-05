using System;
using System.Linq;
using System.Reflection;
using Marten.Util;

namespace Marten.Linq.Fields
{
    public class EnumFieldSource : IFieldSource
    {
        public bool TryResolve(string dataLocator, StoreOptions options, ISerializer serializer, Type documentType,
            MemberInfo[] members, out IField field)
        {
            if (members.Last().GetMemberType().IsEnum)
            {
                field = serializer.EnumStorage == EnumStorage.AsInteger
                    ? (IField) new EnumAsIntegerField(dataLocator, serializer.Casing, members)
                    : new EnumAsStringField(dataLocator, serializer.Casing, members);

                return true;
            }
            else
            {
                field = null;
                return false;
            }
        }
    }
}