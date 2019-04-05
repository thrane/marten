using System;
using System.Linq;
using System.Reflection;
using Marten.Schema;

namespace Marten.Linq.Fields
{
    public class DefaultFieldSource : IFieldSource
    {
        public bool TryResolve(string dataLocator, StoreOptions options, ISerializer serializer, Type documentType,
            MemberInfo[] members, out IField field)
        {
            if (members.Length == 1)
            {
                field = new JsonLocatorField(dataLocator, options,serializer.EnumStorage, serializer.Casing, members.Single());
            }
            else
            {
                field = new JsonLocatorField(dataLocator, serializer.EnumStorage, serializer.Casing, members);
            }
            
            

            return true;
        }
    }
}