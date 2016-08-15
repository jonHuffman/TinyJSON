using System;
using System.Collections;
using System.Reflection;
using System.Text;


namespace TinyJSON
{
	public sealed class Encoder
	{
		static readonly Type includeAttrType = typeof(IncludeAttribute);
		static readonly Type excludeAttrType = typeof(ExcludeAttribute);
		static readonly Type typeHintAttrType = typeof(TypeHintAttribute);


		private StringBuilder builder;
		private EncodeOptions options;
		private int indent;

		Encoder(EncodeOptions options)
		{
			this.options = options;
			builder = new StringBuilder();
			indent = 0;
		}

		public static string Encode(object obj)
		{
			return InternalEncode(obj, EncodeOptions.Default, skipOverVariants: false);
		}

		public static string Encode(object obj, EncodeOptions options)
		{
			return InternalEncode(obj, options, skipOverVariants: false);
		}

		private static string InternalEncode(object obj, EncodeOptions options, bool skipOverVariants)
		{
			var instance = new Encoder(options);
			int combineIndex = instance.combinable ? 0 : -1;
			instance.EncodeValue(obj, false, combineIndex);
			return instance.builder.ToString();
		}


		bool prettyPrintEnabled
		{
			get
			{
				return ((options & EncodeOptions.PrettyPrint) == EncodeOptions.PrettyPrint);
			}
		}


		bool typeHintsEnabled
		{
			get
			{
				return ((options & EncodeOptions.NoTypeHints) != EncodeOptions.NoTypeHints);
			}
		}

		bool ignoreAttributes
		{
			get
			{
				return ((options & EncodeOptions.IgnoreAttributes) == EncodeOptions.IgnoreAttributes);
			}
		}


		bool encodePrivateVariables
		{
			get
			{
				return ((options & EncodeOptions.EncodePrivateVariables) == EncodeOptions.EncodePrivateVariables);
			}
		}

		bool combinable
		{
			get
			{
				return ((options & EncodeOptions.Combinable) == EncodeOptions.Combinable);
			}
		}


		void EncodeValue(object value, bool forceTypeHint, int combineIndex)
		{
			Array asArray;
			Variant asVariant;
			IList asList;
			IDictionary asDict;
			string asString;

			if (value == null)
			{
				builder.Append("null");
			}
			else
			{
				if ((asString = value as string) != null)
				{
					EncodeString(asString);
				}
				else
				if (value is bool)
				{
					builder.Append(value.ToString().ToLower());
				}
				else
				if (value is Enum)
				{
					EncodeString(value.ToString());
				}
				else
				if ((asArray = value as Array) != null)
				{
					EncodeArray(asArray, forceTypeHint);
				}
				else
				if ((asList = value as IList) != null)
				{
					EncodeList(asList, forceTypeHint);
				}
				else
				if ((asDict = value as IDictionary) != null)
				{
					EncodeDictionary(asDict, forceTypeHint);
				}
				else
				if (value is char)
				{
					EncodeString(value.ToString());
				}
				else
				if ((asVariant = value as Variant) != null)
				{
					EncodeValue(asVariant.value, forceTypeHint, combineIndex);
				}
				else
				{
					EncodeOther(value, forceTypeHint, combineIndex);
				}
			}
		}


		void EncodeObject(object value, bool forceTypeHint, int combineIndex)
		{
			Type type = value.GetType();

			AppendOpenBrace();

			forceTypeHint = forceTypeHint || typeHintsEnabled;

			bool firstItem = !forceTypeHint;
			if (forceTypeHint)
			{
				if (prettyPrintEnabled)
				{
					AppendIndent();
				}
				EncodeString(ProxyObject.TypeHintName);
				AppendColon();
				EncodeString(type.FullName);
				firstItem = false;
			}

			if (combineIndex > -1)
			{
				if (prettyPrintEnabled)
				{
					AppendIndent();
				}
				EncodeString(ProxyArray.CombineHintName);
				AppendColon();
				EncodeOther(combineIndex, forceTypeHint, combineIndex);
				combineIndex++;
				firstItem = false;
			}

			#region -= Fields =-
			bool shouldTypeHint = false;
			bool shouldEncode = false;

//#if !NETCORE
			FieldInfo[] fields = type.GetFields(JSON.INSTANCE_BINDING_FLAGS);
//#else
//			FieldInfo[] fields = type.GetTypeInfo().GetFields(JSON.INSTANCE_BINDING_FLAGS);
//#endif

			for (int i = 0; i < fields.Length; i++)
			{
				shouldTypeHint = false;
				shouldEncode = fields[i].IsPublic || encodePrivateVariables;
				string fieldName = fields[i].Name;


				if (!ignoreAttributes)
				{
#if !NETCORE
					Attribute[] attributes = Attribute.GetCustomAttributes(fields[i], inherit: true);
#else
					IEnumerable attributes = fields[i].GetCustomAttributes(true);
#endif

					foreach (var att in attributes)
					{
						if (att is ExcludeAttribute)
						{

							shouldEncode = false;
							break;
						}

						if (att is IncludeAttribute)
						{
							shouldEncode = true;
							continue;
						}

						if (att is AliasAttribute)
						{
							fieldName = ((AliasAttribute)att).alias;
							continue;
						}


						if (att is TypeHintAttribute)
						{
							shouldTypeHint = true;
							continue;
						}
					}
				}

				if (shouldEncode)
				{
					AppendComma(firstItem);
					EncodeString(fieldName);
					AppendColon();
					EncodeValue(fields[i].GetValue(value), shouldTypeHint, combineIndex);
					firstItem = false;
				}
			}

#endregion

#region -= Properties 
			// Properties can only be include with Attributes so we can skip them if Attributes are ignored
			if (!ignoreAttributes)
			{
//#if !NETCORE
				PropertyInfo[] properties = type.GetProperties(JSON.INSTANCE_BINDING_FLAGS);
//#else
//				PropertyInfo[] properties = type.GetTypeInfo().GetProperties(JSON.INSTANCE_BINDING_FLAGS);
//#endif

				for (int i = 0; i < properties.Length; i++)
				{
					if (properties[i].CanRead)
					{
						shouldEncode = false;
						shouldTypeHint = false;
						string propertyName = properties[i].Name;

#if !NETCORE
						Attribute[] attributes = Attribute.GetCustomAttributes(properties[i], inherit: true);
#else
						IEnumerable attributes = properties[i].GetCustomAttributes(true);
#endif

						foreach (var att in attributes)
						{
							if (att is ExcludeAttribute)
							{
								shouldEncode = false;
								break;
							}

							if (att is IncludeAttribute)
							{
								shouldEncode = true;
								continue;
							}

							if (att is AliasAttribute)
							{
								propertyName = ((AliasAttribute)att).alias;
								continue;
							}

							if (att is TypeHintAttribute)
							{
								shouldTypeHint = true;
								continue;
							}
						}


						if (shouldEncode)
						{
							AppendComma(firstItem);
							EncodeString(propertyName);
							AppendColon();
							EncodeValue(properties[i].GetValue(value, null), shouldTypeHint, -1);
							firstItem = false;
						}
					}

				}
			}
#endregion

			AppendCloseBrace();
		}


		void EncodeDictionary(IDictionary value, bool forceTypeHint)
		{
			if (value.Count == 0)
			{
				builder.Append("{}");
			}
			else
			{
				AppendOpenBrace();

				var firstItem = true;
				foreach (object e in value.Keys)
				{
					AppendComma(firstItem);
					EncodeString(e.ToString());
					AppendColon();
					EncodeValue(value[e], forceTypeHint, -1);
					firstItem = false;
				}

				AppendCloseBrace();
			}
		}


		void EncodeList(IList value, bool forceTypeHint)
		{
			if (value.Count == 0)
			{
				builder.Append("[]");
			}
			else
			{
				AppendOpenBracket();

				var firstItem = true;

				for (int i = 0; i < value.Count; i++)
				{
					AppendComma(firstItem);
					EncodeValue(value[i], forceTypeHint, combinable ? i : -1);
					firstItem = false;
				}

				AppendCloseBracket();
			}
		}


		void EncodeArray(Array value, bool forceTypeHint)
		{
			if (value.Rank == 1)
			{
				EncodeList(value, forceTypeHint);
			}
			else
			{
				var indices = new int[value.Rank];
				EncodeArrayRank(value, 0, indices, forceTypeHint, 0);
			}
		}


		void EncodeArrayRank(Array value, int rank, int[] indices, bool forceTypeHint, int combineIndex)
		{
			AppendOpenBracket();

			var min = value.GetLowerBound(rank);
			var max = value.GetUpperBound(rank);

			if (rank == value.Rank - 1)
			{
				for (int i = min; i <= max; i++)
				{
					indices[rank] = i;
					AppendComma(i == min);
					EncodeValue(value.GetValue(indices), forceTypeHint, combinable ? i : -1);
				}
			}
			else
			{
				for (int i = min; i <= max; i++)
				{
					indices[rank] = i;
					AppendComma(i == min);
					EncodeArrayRank(value, rank + 1, indices, forceTypeHint, combinable ? i : -1);
				}
			}

			AppendCloseBracket();
		}



		void EncodeString(string value)
		{
			builder.Append('\"');

			char[] charArray = value.ToCharArray();
			foreach (var c in charArray)
			{
				switch (c)
				{
					case '"':
						builder.Append("\\\"");
						break;

					case '\\':
						builder.Append("\\\\");
						break;

					case '\b':
						builder.Append("\\b");
						break;

					case '\f':
						builder.Append("\\f");
						break;

					case '\n':
						builder.Append("\\n");
						break;

					case '\r':
						builder.Append("\\r");
						break;

					case '\t':
						builder.Append("\\t");
						break;

					default:
						int codepoint = Convert.ToInt32(c);
						if ((codepoint >= 32) && (codepoint <= 126))
						{
							builder.Append(c);
						}
						else
						{
							builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
						}
						break;
				}
			}

			builder.Append('\"');
		}


		void EncodeOther(object value, bool forceTypeHint, int combineIndex)
		{
			if (value is float ||
				value is double ||
				value is int ||
				value is uint ||
				value is long ||
				value is sbyte ||
				value is byte ||
				value is short ||
				value is ushort ||
				value is ulong ||
				value is decimal)
			{
				builder.Append(value.ToString());
			}
			else
			{
				EncodeObject(value, forceTypeHint, combineIndex);
			}
		}


#region Helpers

		void AppendIndent()
		{
			for (int i = 0; i < indent; i++)
			{
				builder.Append('\t');
			}
		}


		void AppendOpenBrace()
		{
			builder.Append('{');

			if (prettyPrintEnabled)
			{
				builder.Append(Environment.NewLine);
				indent++;
			}
		}


		void AppendCloseBrace()
		{
			if (prettyPrintEnabled)
			{
				builder.Append(Environment.NewLine);
				indent--;
				AppendIndent();
			}

			builder.Append('}');
		}


		void AppendOpenBracket()
		{
			builder.Append('[');

			if (prettyPrintEnabled)
			{
				builder.Append(Environment.NewLine);
				indent++;
			}
		}


		void AppendCloseBracket()
		{
			if (prettyPrintEnabled)
			{
				builder.Append(Environment.NewLine);
				indent--;
				AppendIndent();
			}

			builder.Append(']');
		}


		void AppendComma(bool firstItem)
		{
			if (!firstItem)
			{
				builder.Append(',');

				if (prettyPrintEnabled)
				{
					builder.Append(Environment.NewLine);
				}
			}

			if (prettyPrintEnabled)
			{
				AppendIndent();
			}
		}


		void AppendColon()
		{
			builder.Append(':');

			if (prettyPrintEnabled)
			{
				builder.Append(' ');
			}
		}

#endregion
	}
}

