using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Marten.Schema;

namespace Marten.Linq.Fields
{
    public class FieldCollection
    {
        private static readonly IFieldSource[] _defaultFieldSources = new IFieldSource[]
        {
            new DefaultFieldSource(), 
        };
        
        private readonly string _dataLocator;
        private readonly Type _documentType;
        private readonly StoreOptions _options;
        private readonly ConcurrentDictionary<string, IField> _fields = new ConcurrentDictionary<string, IField>();
        private readonly ISerializer _serializer;

        public FieldCollection(string dataLocator, Type documentType, StoreOptions options)
        {
            _dataLocator = dataLocator;
            _documentType = documentType;
            _options = options;
            _serializer = options.Serializer();
        }

        private IEnumerable<IFieldSource> allFieldSources()
        {
            foreach (var source in _options.FieldSources)
            {
                yield return source;
            }

            foreach (var source in _defaultFieldSources)
            {
                yield return source;
            }
        }

        protected void removeIdField()
        {
            var idFields = _fields.Where(x => x.Value is IdField).ToArray();
            foreach (var pair in idFields)
            {
                IField field;
                _fields.TryRemove(pair.Key, out field);
            }
        }

        protected void setField(string name, IField field)
        {
            _fields[name] = field;
        }

        protected IEnumerable<IField> fields()
        {
            return _fields.Values;
        }

        public IField FieldFor(Expression expression)
        {
            return FieldFor(FindMembers.Determine(expression));
        }

        public IField FieldFor(MemberInfo[] members)
        {
            if (members.Count() == 1)
            {
                return FieldFor(members.Single());
            }

            var key = members.Select(x => x.Name).Join("");

            return _fields.GetOrAdd(key,
                _ => resolveField(members));
        }
        
        

        public IField FieldFor(MemberInfo member)
        {
            return _fields.GetOrAdd(member.Name,
                name => resolveField(new []{member}));
        }

        private IField resolveField(MemberInfo[] members)
        {
            foreach (var source in allFieldSources())
            {
                if (source.TryResolve(_dataLocator, _options, _serializer, _documentType, members, out var field))
                {
                    return field;
                }
            }

            return null;
        }

        public IField FieldFor(string memberName)
        {
            return _fields.GetOrAdd(memberName, name =>
            {
                var member = _documentType.GetProperties().FirstOrDefault(x => x.Name == name).As<MemberInfo>() ??
                             _documentType.GetFields().FirstOrDefault(x => x.Name == name);

                if (member == null) return null;




                return resolveField(new MemberInfo[] {member});
            });
        }

    }


}