using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Marten.Linq;
using Marten.Services.Includes;
using Remotion.Linq;

namespace Marten.Schema
{
    public interface IQueryableDocument
    {
        IWhereFragment FilterDocuments(QueryModel model, IWhereFragment query);

        IWhereFragment DefaultWhereFragment();

        IncludeJoin<TOther> JoinToInclude<TOther>(JoinType joinType, IQueryableDocument other, MemberInfo[] members, Action<TOther> callback);

        [Obsolete("This needs to go away")]
        IField FieldFor(IEnumerable<MemberInfo> members);
        
        IField FieldFor(MemberInfo member);

        IField FieldFor(Expression expression);

        string[] SelectFields();

        PropertySearching PropertySearching { get; }

        DbObjectName Table { get; }

        DuplicatedField[] DuplicatedFields { get; }

        DeleteStyle DeleteStyle { get; }

        Type DocumentType { get; }
    }
}