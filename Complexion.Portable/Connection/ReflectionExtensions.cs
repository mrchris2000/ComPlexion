using System;
using System.Reflection;

namespace Complexion.Portable.Connection
{
	/// <summary>
	/// Reflection extensions
	/// </summary>
	internal static class ReflectionExtensions
	{
		/// <summary>
		/// Checks a type to see if it derives from a raw generic (e.g. List[[]])
		/// </summary>
		/// <param name="toCheck"></param>
		/// <param name="generic"></param>
		/// <returns></returns>
        internal static bool IsSubclassOfRawGeneric(this Type toCheck, Type generic)
        {
			while (toCheck != typeof(object)) {
				var cur = toCheck.GetTypeInfo().IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
				if (generic == cur) {
					return true;
				}
                toCheck = toCheck.GetTypeInfo().BaseType;
			}
			return false;
		}

        internal static object ChangeType(this object source, Type newType)
		{
			return Convert.ChangeType(source, newType, null);
		}

	    /// <summary>
	    /// Find a value from a System.Enum by trying several possible variants
	    /// of the string value of the enum.
	    /// </summary>
	    /// <param name="type">Type of enum</param>
	    /// <param name="value">Value for which to search</param>
	    /// <returns></returns>
        internal static object FindEnumValue(this Type type, string value)
		{
			return Enum.Parse(type, value, true);
		}
	}
}
