using System;
using System.Collections.Generic;
using System.Reflection;

namespace Harmony
{
	/// <summary>A access cache for speeding up reflections</summary>
	public class AccessCache
	{
		Dictionary<Type, Dictionary<string, FieldInfo>> fields = new Dictionary<Type, Dictionary<string, FieldInfo>>();
		Dictionary<Type, Dictionary<string, PropertyInfo>> properties = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
		readonly Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>> methods = new Dictionary<Type, Dictionary<string, Dictionary<int, MethodBase>>>();

		/// <summary>Gets field information</summary>
		/// <param name="type">The type</param>
		/// <param name="name">The name</param>
		/// <returns>The field information</returns>
		///
		[UpgradeToLatestVersion(1)]
		public FieldInfo GetFieldInfo(Type type, string name)
		{
			Dictionary<string, FieldInfo> fieldsByType = null;
			if (fields.TryGetValue(type, out fieldsByType) == false)
			{
				fieldsByType = new Dictionary<string, FieldInfo>();
				fields.Add(type, fieldsByType);
			}

			FieldInfo field = null;
			if (fieldsByType.TryGetValue(name, out field) == false)
			{
				field = AccessTools.DeclaredField(type, name);
				fieldsByType.Add(name, field);
			}
			return field;
		}

		/// <summary>Gets property information</summary>
		/// <param name="type">The type</param>
		/// <param name="name">The name</param>
		/// <returns>The property information</returns>
		///
		public PropertyInfo GetPropertyInfo(Type type, string name)
		{
			Dictionary<string, PropertyInfo> propertiesByType = null;
			if (properties.TryGetValue(type, out propertiesByType) == false)
			{
				propertiesByType = new Dictionary<string, PropertyInfo>();
				properties.Add(type, propertiesByType);
			}

			PropertyInfo property = null;
			if (propertiesByType.TryGetValue(name, out property) == false)
			{
				property = AccessTools.DeclaredProperty(type, name);
				propertiesByType.Add(name, property);
			}
			return property;
		}

		static int CombinedHashCode(IEnumerable<object> objects)
		{
			var hash1 = (5381 << 16) + 5381;
			var hash2 = hash1;
			var i = 0;
			foreach (var obj in objects)
			{
				if (i % 2 == 0)
					hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ obj.GetHashCode();
				else
					hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ obj.GetHashCode();
				++i;
			}
			return hash1 + (hash2 * 1566083941);
		}

		/// <summary>Gets method information</summary>
		/// <param name="type">		 The type</param>
		/// <param name="name">		 The name</param>
		/// <param name="arguments">The arguments</param>
		/// <returns>The method information</returns>
		///
		public MethodBase GetMethodInfo(Type type, string name, Type[] arguments)
		{
			Dictionary<string, Dictionary<int, MethodBase>> methodsByName = null;
			methods.TryGetValue(type, out methodsByName);
			if (methodsByName == null)
			{
				methodsByName = new Dictionary<string, Dictionary<int, MethodBase>>();
				methods[type] = methodsByName;
			}

			Dictionary<int, MethodBase> methodsByArguments = null;
			methodsByName.TryGetValue(name, out methodsByArguments);
			if (methodsByArguments == null)
			{
				methodsByArguments = new Dictionary<int, MethodBase>();
				methodsByName[name] = methodsByArguments;
			}

			MethodBase method = null;
			var argumentsHash = CombinedHashCode(arguments);
			if (methodsByArguments.TryGetValue(argumentsHash, out method) == false)
			{
				method = AccessTools.Method(type, name, arguments);
				methodsByArguments.Add(argumentsHash, method);
			}

			return method;
		}
	}
}