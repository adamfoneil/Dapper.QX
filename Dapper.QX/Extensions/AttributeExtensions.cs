using System;
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
            var attr = memberInfo.GetCustomAttribute(typeof(T));
            if (attr != null)
            {
                attribute = attr as T;
                return true;
            }

            attribute = null;
            return false;
        }
    }
}
