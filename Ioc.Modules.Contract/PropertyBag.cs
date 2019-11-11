using System;
using System.Collections.Generic;
using System.Text;

namespace Ioc.Modules
{
    public class PropertyBag : IPropertyBag
    {
        private readonly IDictionary<string, object> _properties;

        public PropertyBag()
        {
            _properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public T Get<T>(string name)
        {
            if (name == null)
                name = typeof(T).FullName;

            object result;
            if (_properties.TryGetValue(name, out result)) return (T)result;

            return default(T);
        }

        public void Set<T>(T value, string name)
        {
            if (name == null)
                name = typeof (T).FullName;

            _properties[name] = value;
        }

        public string this[string name]
        {
            get { return Get<string>(name); }
            set { Set(value, name); }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("PropertyBag");
            foreach (var name in _properties.Keys)
            {
                var value = _properties[name];
                sb.Append(name);
                sb.Append(" = ");
                sb.AppendLine(value == null 
                    ? "<null>" 
                    : value.ToString().Replace(Environment.NewLine, Environment.NewLine + "   "));
            }
            return sb.ToString();
        }
    }
}