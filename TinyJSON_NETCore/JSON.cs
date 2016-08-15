using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#if !NETCORE
using UnityEngine.Scripting;
#endif

namespace TinyJSON
{
#if !NETCORE
	[Preserve]
#endif
	public static class JSON
	{
		static readonly Type INCLUDE_ATTRIBUTE_TYPE = typeof(IncludeAttribute);
		static readonly Type EXCLUDE_ATTRIBUTE_TYPE = typeof(ExcludeAttribute);
		internal static readonly Type ALIAS_ATTRIBUTE_TYPE = typeof(AliasAttribute);
		public static readonly BindingFlags INSTANCE_BINDING_FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static Variant Load(string json)
		{
			if (json == null)
			{
				throw new ArgumentNullException("json");
			}

			return Decoder.Decode(json);
		}


		public static string Dump(object data)
		{
			return Dump(data, EncodeOptions.Default);
		}


		public static string Dump(object data, EncodeOptions options)
		{
			// Invoke methods tagged with [BeforeEncode] attribute.
			if (data != null)
			{
#if !NETCORE
				var type = data.GetType();
				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in type.GetMethods(instanceBindingFlags))
					{
						if (method.GetCustomAttributes(false).AnyOfType(typeof(BeforeEncodeAttribute)))
						{
							if (method.GetParameters().Length == 0)
							{
								method.Invoke(data, null);
							}
						}
					}
				}
#else
				var dataType = data.GetType();
				var type = dataType.GetTypeInfo();				

				if (!(type.IsEnum || type.IsPrimitive || type.IsArray))
				{
					foreach (var method in dataType.GetMethods(instanceBindingFlags))
					{
						if (method.GetCustomAttributes(false).AnyOfType(typeof(BeforeEncodeAttribute)))
						{
							if (method.GetParameters().Length == 0)
							{
								method.Invoke(data, null);
							}
						}
					}
				}
#endif

			}

			return Encoder.Encode(data, options);
		}


		public static void MakeInto<T>(Variant data, out T item)
		{
			item = DecodeType<T>(data);
		}


		private static Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
		private static Type FindType(string fullName)
		{
			if (fullName == null)
			{
				return null;
			}

			Type type;
			if (typeCache.TryGetValue(fullName, out type))
			{
				return type;
			}
			
			type = Type.GetType(fullName);

			if (type != null)
			{
				typeCache.Add(fullName, type);
				return type;
			}

			return null;
		}

#if !NETCORE
	[Preserve]
#endif
		private static T DecodeType<T>(Variant data)
		{
			if (data == null)
			{
				return default(T);
			}

			Type type = typeof(T);
#if NETCORE
			TypeInfo tInfo = type.GetTypeInfo();
#endif

#if !NETCORE
			if (type.IsEnum)
#else
			if (tInfo.IsEnum)
#endif
			{
				return (T)Enum.Parse(type, data.ToString());
			}

#if !NETCORE
			if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
#else
			if (tInfo.IsPrimitive || type == typeof(string) || type == typeof(decimal))
#endif
			{
				return (T)Convert.ChangeType(data, type);
			}

			if (type.IsArray)
			{
				if (type.GetArrayRank() == 1)
				{
					var makeFunc = decodeArrayMethod.MakeGenericMethod(new Type[] { type.GetElementType() });
					return (T)makeFunc.Invoke(null, new object[] { data });
				}
				else
				{
					var arrayData = data as ProxyArray;
					var arrayRank = type.GetArrayRank();
					var rankLengths = new int[arrayRank];
					if (arrayData.CanBeMultiRankArray(rankLengths))
					{
						var array = Array.CreateInstance(type.GetElementType(), rankLengths);
						var makeFunc = decodeMultiRankArrayMethod.MakeGenericMethod(new Type[] { type.GetElementType() });
						try
						{
							makeFunc.Invoke(null, new object[] { arrayData, array, 1, rankLengths });
						}
						catch (Exception e)
						{
							throw new DecodeException("Error decoding multidimensional array. Did you try to decode into an array of incompatible rank or element type?", e);
						}
						return (T)Convert.ChangeType(array, typeof(T));
					}
					throw new DecodeException("Error decoding multidimensional array; JSON data doesn't seem fit this structure.");
#pragma warning disable 0162
					return default(T);
#pragma warning restore 0162
				}
			}

//#if !NETCORE
			if (typeof(IList).IsAssignableFrom(type))
			{
				var makeFunc = decodeListMethod.MakeGenericMethod(type.GetGenericArguments());
				return (T)makeFunc.Invoke(null, new object[] { data });
			}
//#else
//			if (typeof(IList).GetTypeInfo().IsAssignableFrom(type))
//			{
//				var makeFunc = decodeListMethod.MakeGenericMethod(tInfo.GetGenericArguments());
//				return (T)makeFunc.Invoke(null, new object[] { data });
//			}
//#endif

//#if !NETCORE
			if (typeof(IDictionary).IsAssignableFrom(type))
			{
				var makeFunc = decodeDictionaryMethod.MakeGenericMethod(type.GetGenericArguments());
				return (T)makeFunc.Invoke(null, new object[] { data });
			}
//#else
//			if (typeof(IDictionary).GetTypeInfo().IsAssignableFrom(type))
//			{
//				var makeFunc = decodeDictionaryMethod.MakeGenericMethod(tInfo.GetGenericArguments());
//				return (T)makeFunc.Invoke(null, new object[] { data });
//			}
//#endif

			// At this point we should be dealing with a class or struct.
			T instance;
			var proxyObject = data as ProxyObject;
			if (proxyObject == null)
			{
				throw new InvalidCastException("ProxyObject expected when decoding into '" + type.FullName + "'.");
			}

			// If there's a type hint, use it to create the instance.
			var typeHint = proxyObject.TypeHint;
			if (typeHint != null && typeHint != type.FullName)
			{
				var makeType = FindType(typeHint);
				if (makeType == null)
				{
					throw new TypeLoadException("Could not load type '" + typeHint + "'.");
				}
				else
				{
//#if !NETCORE
					if (type.IsAssignableFrom(makeType))
//#else
//					if (tInfo.IsAssignableFrom(makeType))
//#endif
					{
						instance = (T)Activator.CreateInstance(makeType, nonPublic: true);
						type = makeType;
					}
					else
					{
						throw new InvalidCastException("Cannot assign type '" + typeHint + "' to type '" + type.FullName + "'.");
					}
				}
			}
			else
			{
				// We don't have a type hint, so just instantiate the type we have.
				instance = (T)Activator.CreateInstance(typeof(T), nonPublic: true);
			}

			List<MemberInfo> members = new List<MemberInfo>();
//#if !NETCORE
			members.AddRange(type.GetProperties(JSON.INSTANCE_BINDING_FLAGS));
			members.AddRange(type.GetFields(JSON.INSTANCE_BINDING_FLAGS));
//#else
//			members.AddRange(tInfo.GetProperties(JSON.INSTANCE_BINDING_FLAGS));
//			members.AddRange(tInfo.GetFields(JSON.INSTANCE_BINDING_FLAGS));
//#endif
			for (int i = 0; i < members.Count; i++)
			{
				MemberInfo member = members[i];
				FieldInfo fieldInfo = null;
				PropertyInfo propertyInfo = null;

				string memberName = member.Name;

				int shouldEncode = -1;

#if !NETCORE
				Attribute[] attributes = Attribute.GetCustomAttributes(member, inherit: true);
#else
				IEnumerable attributes = member.GetCustomAttributes(true);
#endif

				foreach (var att in attributes)
				{
					if (att is ExcludeAttribute)
					{
						shouldEncode = 0;
						break;
					}

					if (att is IncludeAttribute)
					{
						shouldEncode = 1;
						continue;
					}

					if (att is AliasAttribute)
					{
						memberName = ((AliasAttribute)att).alias;
						continue;
					}
				}

				//We only want to encode the member if we have the key. 
				if (proxyObject.ContainsKey(memberName))
				{

					MethodInfo MakeMethod = null;

					if (member is FieldInfo)
					{
						fieldInfo = member as FieldInfo;

						if (shouldEncode == -1)
						{
							//This only happens if no attribute was found that modifies if it should be encoded or not. 
							if (fieldInfo.IsPublic)
							{
								shouldEncode = 1;
							}
						}

						if (shouldEncode == 1)
						{
							//We are going to encode this field
							MakeMethod = decodeTypeMethod.MakeGenericMethod(new Type[] { fieldInfo.FieldType });

#if !NETCORE
							if (type.IsValueType)
#else
							if (tInfo.IsValueType)
#endif
							{
								object instanceRef = (object)instance;
								fieldInfo.SetValue(instanceRef, MakeMethod.Invoke(obj: null, parameters: new object[] { proxyObject[memberName] }));
								instance = (T)instanceRef;
							}
							else
							{
								fieldInfo.SetValue(instance, MakeMethod.Invoke(null, new object[] { proxyObject[memberName] }));
							}

						}
					}
					else if (member is PropertyInfo)
					{
						propertyInfo = member as PropertyInfo;

						if (shouldEncode == 1 && propertyInfo.CanWrite)
						{
							//We are going to encode this property
							MakeMethod = decodeTypeMethod.MakeGenericMethod(new Type[] { propertyInfo.PropertyType });

#if !NETCORE
							if (type.IsValueType)
#else
							if (tInfo.IsValueType)
#endif
							{
								object instanceRef = (object)instance;
								propertyInfo.SetValue(instanceRef, MakeMethod.Invoke(obj: null, parameters: new object[] { proxyObject[memberName] }), null);
								instance = (T)instanceRef;
							}
							else
							{
								propertyInfo.SetValue(instance, MakeMethod.Invoke(obj: null, parameters: new object[] { proxyObject[memberName] }), null);
							}


						}
					}
				}
			}

//#if !NETCORE
			// Invoke methods tagged with [AfterDecode] attribute. 
			foreach (var method in type.GetMethods(instanceBindingFlags))
//#else
//			foreach (var method in tInfo.GetMethods(instanceBindingFlags))
//#endif
			{
				if (method.GetCustomAttributes(false).AnyOfType(typeof(AfterDecodeAttribute)))
				{
					if (method.GetParameters().Length == 0)
					{
						method.Invoke(instance, null);
					}
					else
					{
						method.Invoke(instance, new object[] { data });
					}
				}
			}

			return instance;
		}


		private static List<T> DecodeList<T>(Variant data)
		{
			var list = new List<T>();

			foreach (var item in data as ProxyArray)
			{
				list.Add(DecodeType<T>(item));
			}

			return list;
		}

#if !NETCORE
		[Preserve]
#endif
		private static Dictionary<K, V> DecodeDictionary<K, V>(Variant data)
		{
			var dict = new Dictionary<K, V>();
			var type = typeof(K);

			foreach (var pair in data as ProxyObject)
			{
#if !NETCORE
				var k = (K)(type.IsEnum ? Enum.Parse(type, pair.Key) : Convert.ChangeType(pair.Key, type));
#else
				var k = (K)(type.GetTypeInfo().IsEnum ? Enum.Parse(type, pair.Key) : Convert.ChangeType(pair.Key, type));
#endif
				var v = DecodeType<V>(pair.Value);
				dict.Add(k, v);
			}

			return dict;
		}

#if !NETCORE
		[Preserve]
#endif
		private static T[] DecodeArray<T>(Variant data)
		{
			var arrayData = data as ProxyArray;
			var arraySize = arrayData.Count;
			var array = new T[arraySize];

			int i = 0;
			foreach (var item in data as ProxyArray)
			{
				array[i++] = DecodeType<T>(item);
			}

			return array;
		}

#if !NETCORE
		[Preserve]
#endif
		private static void DecodeMultiRankArray<T>(ProxyArray arrayData, Array array, int arrayRank, int[] indices)
		{
			var count = arrayData.Count;
			for (int i = 0; i < count; i++)
			{
				indices[arrayRank - 1] = i;
				if (arrayRank < array.Rank)
				{
					DecodeMultiRankArray<T>(arrayData[i] as ProxyArray, array, arrayRank + 1, indices);
				}
				else
				{
					array.SetValue(DecodeType<T>(arrayData[i]), indices);
				}
			}
		}


		private static BindingFlags instanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		private static BindingFlags staticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
//#if !NETCORE
		private static MethodInfo decodeTypeMethod				= typeof(JSON).GetMethod("DecodeType", staticBindingFlags);
		private static MethodInfo decodeListMethod				= typeof(JSON).GetMethod("DecodeList", staticBindingFlags);
		private static MethodInfo decodeDictionaryMethod		= typeof(JSON).GetMethod("DecodeDictionary", staticBindingFlags);
		private static MethodInfo decodeArrayMethod				= typeof(JSON).GetMethod("DecodeArray", staticBindingFlags);
		private static MethodInfo decodeMultiRankArrayMethod	= typeof(JSON).GetMethod("DecodeMultiRankArray", staticBindingFlags);
//#else
//		private static MethodInfo decodeTypeMethod				= typeof(JSON).GetTypeInfo().GetMethod("DecodeType", staticBindingFlags);
//		private static MethodInfo decodeListMethod				= typeof(JSON).GetTypeInfo().GetMethod("DecodeList", staticBindingFlags);
//		private static MethodInfo decodeDictionaryMethod		= typeof(JSON).GetTypeInfo().GetMethod("DecodeDictionary", staticBindingFlags);
//		private static MethodInfo decodeArrayMethod				= typeof(JSON).GetTypeInfo().GetMethod("DecodeArray", staticBindingFlags);
//		private static MethodInfo decodeMultiRankArrayMethod	= typeof(JSON).GetTypeInfo().GetMethod("DecodeMultiRankArray", staticBindingFlags);
//#endif

#if !NETCORE
		[Preserve]
#endif
		public static void SupportTypeForAOT<T>()
		{
			DecodeType<T>(null);
			DecodeList<T>(null);
			DecodeArray<T>(null);
			DecodeDictionary<Int16, T>(null);
			DecodeDictionary<UInt16, T>(null);
			DecodeDictionary<Int32, T>(null);
			DecodeDictionary<UInt32, T>(null);
			DecodeDictionary<Int64, T>(null);
			DecodeDictionary<UInt64, T>(null);
			DecodeDictionary<Single, T>(null);
			DecodeDictionary<Double, T>(null);
			DecodeDictionary<Decimal, T>(null);
			DecodeDictionary<Boolean, T>(null);
			DecodeDictionary<String, T>(null);
			Variant.CombineInto<ProxyObject>(null);
			Variant.CombineInto<ProxyArray>(null);
		}

#if !NETCORE
		[Preserve]
#endif
		private static void SupportValueTypesForAOT()
		{
			SupportTypeForAOT<Int16>();
			SupportTypeForAOT<UInt16>();
			SupportTypeForAOT<Int32>();
			SupportTypeForAOT<UInt32>();
			SupportTypeForAOT<Int64>();
			SupportTypeForAOT<UInt64>();
			SupportTypeForAOT<Single>();
			SupportTypeForAOT<Double>();
			SupportTypeForAOT<Decimal>();
			SupportTypeForAOT<Boolean>();
			SupportTypeForAOT<String>();
		}
	}
}

