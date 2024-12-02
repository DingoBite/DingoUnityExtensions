using System;
using System.Linq;
using System.Text;

namespace DingoUnityExtensions.Utils
{
    public static class TypeExtensions
    {
        public static string CSharpName(this Type type, bool isFullName = true)
        {
            var sb = new StringBuilder();
            var name = isFullName ? type.FullName : type.Name;
            if (!type.IsGenericType) return name;
            name = name[..name.IndexOf('`')];
            sb.Append(name);
            var genericArguments = type.GetGenericArguments();
            if (genericArguments.Length == 0 || genericArguments.Any(g => g.FullName == null))
                return name;
            sb.Append("<");
            sb.Append(string.Join(", ", genericArguments.Select(t => t.CSharpName())));
            sb.Append(">");
            return sb.ToString();
        }
    }
}