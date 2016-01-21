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


        StringBuilder builder;
        EncodeOptions options;
        int indent;


        Encoder(EncodeOptions options)
        {
            this.options = options;
            builder = new StringBuilder();
            indent = 0;
        }


        public static string Encode(object obj)
        {
            return Encode(obj, EncodeOptions.None);
        }


        public static string Encode(object obj, EncodeOptions options)
        {
            var instance = new Encoder(options);
            instance.EncodeValue(obj, false);
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


        void EncodeValue(object value, bool forceTypeHint)
        {
            Array asArray;
            IList asList;
            IDictionary asDict;
            string asString;

            if (value == null)
            {
                builder.Append("null");
            }
            else
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
            {
                EncodeOther(value, forceTypeHint);
            }
        }


        void EncodeObject(object value, bool forceTypeHint)
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

            #region -= Fields =-
            bool shouldTypeHint = false;
            bool shouldEncode = false;

            FieldInfo[] fields = type.GetFields(JSON.INSTANCE_BINDING_FLAGS);

            for (int i = 0; i < fields.Length; i++)
            {
                shouldTypeHint = false;
                shouldEncode = fields[i].IsPublic || encodePrivateVariables;
                string fieldName = fields[i].Name;


                if (!ignoreAttributes)
                {
                    Attribute[] attributes = Attribute.GetCustomAttributes(fields[i], inherit: true);

                    for (int x = 0; x < attributes.Length; x++)
                    {
                        if (attributes[x] is ExcludeAttribute)
                        {

                            shouldEncode = false;
                            break;
                        }

                        if (attributes[x] is IncludeAttribute)
                        {
                            shouldEncode = true;
                            continue;
                        }

                        if (attributes[x] is AliasAttribute)
                        {
                            fieldName = ((AliasAttribute)attributes[x]).alias;
                            continue;
                        }


                        if (attributes[x] is TypeHintAttribute)
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
                    EncodeValue(fields[i].GetValue(value), shouldTypeHint);
                    firstItem = false;
                }
            }

            #endregion

            #region -= Properties 
            // Properties can only be include with Attributes so we can skip them if Attributes are ignored
            if (!ignoreAttributes)
            {
                PropertyInfo[] properties = type.GetProperties(JSON.INSTANCE_BINDING_FLAGS);

                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].CanRead)
                    {
                        shouldEncode = false;
                        shouldTypeHint = false;
                        string propertyName = properties[i].Name;

                        Attribute[] attributes = Attribute.GetCustomAttributes(properties[i], inherit: true);

                        for (int x = 0; x < attributes.Length; x++)
                        {
                            if (attributes[x] is ExcludeAttribute)
                            {
                                shouldEncode = false;
                                break;
                            }

                            if (attributes[x] is IncludeAttribute)
                            {
                                shouldEncode = true;
                                continue;
                            }

                            if (attributes[x] is AliasAttribute)
                            {
                                propertyName = ((AliasAttribute)attributes[x]).alias;
                                continue;
                            }

                            if (attributes[x] is TypeHintAttribute)
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
                            EncodeValue(properties[i].GetValue(value, null), shouldTypeHint);
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
                    EncodeValue(value[e], forceTypeHint);
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
                foreach (object obj in value)
                {
                    AppendComma(firstItem);
                    EncodeValue(obj, forceTypeHint);
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
                EncodeArrayRank(value, 0, indices, forceTypeHint);
            }
        }


        void EncodeArrayRank(Array value, int rank, int[] indices, bool forceTypeHint)
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
                    EncodeValue(value.GetValue(indices), forceTypeHint);
                }
            }
            else
            {
                for (int i = min; i <= max; i++)
                {
                    indices[rank] = i;
                    AppendComma(i == min);
                    EncodeArrayRank(value, rank + 1, indices, forceTypeHint);
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


        void EncodeOther(object value, bool forceTypeHint)
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
                EncodeObject(value, forceTypeHint);
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

