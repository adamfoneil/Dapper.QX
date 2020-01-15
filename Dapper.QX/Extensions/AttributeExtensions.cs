using System;
using System.Linq;
using System.Reflection;

namespace Dapper.QX.Extensions
{
    internal static class AttributeExtensions
    {
        internal static bool HasAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            return HasAttribute<T>(memberInfo, out _);
        }

        internal static bool HasAttribute<T>(this MemberInfo memberInfo, out T attribute) where T : Attribute
        {
            var attrs = memberInfo.GetCustomAttributes(typeof(T), true).OfType<T>();
            if (attrs?.Any() ?? false)
            {
                attribute = attrs.First();
                return true;
            }

            attribute = null;
            return false;
        }

        internal static T GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
        {
            if (HasAttribute(memberInfo, out T result)) return result;
            return null;
        }
    }
}
