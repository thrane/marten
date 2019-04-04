using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Marten.Schema
{
    public interface IField
    {
        MemberInfo[] Members { get; }
        string MemberName { get; }

        /// <summary>
        /// Postgresql locator that also casts the raw string data to the proper Postgresql type
        /// </summary>
        string TypedLocator { get; }

        /// <summary>
        /// Postgresql locator that returns the raw string value within the JSONB document
        /// </summary>
        string RawLocator { get; }

        string ColumnName { get; }

        object GetValue(Expression valueExpression);

        /// <summary>
        /// The .Net type of this IField
        /// </summary>
        Type FieldType { get; }
        
        [Obsolete]
        bool ShouldUseContainmentOperator();
        string LocatorFor(string rootTableAlias);
    }
}