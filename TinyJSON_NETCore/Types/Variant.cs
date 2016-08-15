using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace TinyJSON
{
    public abstract class Variant : IConvertible
    {
        protected static IFormatProvider formatProvider = new NumberFormatInfo();


        public void Make<T>(out T item)
        {
            JSON.MakeInto<T>(this, out item);
        }


        public T Make<T>()
        {
            T item;
            JSON.MakeInto<T>(this, out item);
            return item;
        }


        public virtual TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public abstract object value
        {
            get;
        }

        /// <summary>
        /// Takes an array of Variants that and tries to combine them into one 
        /// class. All combining class must be the same root type
        /// <see cref="ProxyObject"/> or <see cref="ProxyArray"/>. Classes are 
        /// merged one at a time starting from the top of the array and moving down. 
        /// </summary>
        /// <typeparam name="T">The type of Variant you want to combine.</typeparam>
        /// <param name="targets">The list of Variants objects that 
        /// you want to  combine"/></param>
        /// <returns>The combine <typeparamref name="T"/> object.</returns>
        public static T CombineInto<T>(params Variant[] targets) where T : Variant
        {
            if (targets.Length == 0)
            {
                // If we don't have any we can't combine them.
                return null;
            }

            if (targets.Length == 1)
            {
                // Nothing to combine so we just return the only one. 
                return (T)targets[0];
            }

            T @base = (T)targets[0];

            for (int i = 1; i < targets.Length; i++)
            {
                @base = (T)CombineVariant(@base, targets[i]);
            }
            return @base;
        }

        protected static Variant CombineVariant(Variant startingVariant, Variant combineWith)
        {
            Type type = null;

            if (startingVariant != null)
            {
                type = startingVariant.GetType();
            }
            else if (combineWith != null)
            {
                type = combineWith.GetType();
            }
            else
            {
                throw new ArgumentException("Both values are null for combine");
            }

            if (type == typeof(ProxyObject))
            {
                ProxyObject combineObject = combineWith as ProxyObject;
                ProxyObject startingObject = startingVariant as ProxyObject;

                foreach (var item in combineObject)
                {
                    if (startingObject.ContainsKey(item.Key))
                    {
                        //It had the value so we have to loop over it. 
                        startingObject[item.Key] = CombineVariant(startingObject[item.Key], item.Value);
                    }
                    else
                    {
                        //It did not have a value so we can just copy the whole tree
                        startingObject[item.Key] = item.Value;
                    }
                }
            }
            else if (type == typeof(ProxyArray))
            {
                ProxyArray combineArray = combineWith as ProxyArray;
                ProxyArray startingArray = startingVariant as ProxyArray;

                for (int i = 0; i < combineArray.Count; i++)
                {
                    if (combineArray[i] is ProxyObject)
                    {
                        ProxyObject arrayObject = combineArray[i] as ProxyObject;

                        if (arrayObject != null)
                        {
                            if (arrayObject.ContainsKey(ProxyArray.CombineHintName))
                            {
                                int @Index = arrayObject[ProxyArray.CombineHintName];

                                Variant indexVariant = FindIndex(startingArray, @Index);

                                if (indexVariant == null)
                                {
                                    startingArray.Add(arrayObject);
                                }
                                else
                                {
                                    indexVariant = CombineVariant(indexVariant, arrayObject);
                                }
                            }
                        }
                    }
                }
            }
            else if (type == typeof(ProxyNumber))
            {
                startingVariant = combineWith;
            }
            else if (type == typeof(ProxyString))
            {
                startingVariant = combineWith;
            }

            return startingVariant;
        }

        private static Variant FindIndex(ProxyArray array, int index)
        {
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is ProxyObject)
                {
                    ProxyObject proxy = array[i] as ProxyObject;

                    if (proxy.ContainsKey(ProxyArray.CombineHintName))
                    {
                        int @Index = proxy[ProxyArray.CombineHintName];

                        if (@Index == index)
                        {
                            return proxy;
                        }
                    }
                }
            }
            return null;
        }


        public string Dump(EncodeOptions options = EncodeOptions.Default)
        {
            return Encoder.Encode(this, options);
        }

        public virtual object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to " + conversionType.Name);
        }


        public virtual DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to DateTime");
        }


        public virtual bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Boolean");
        }

        public virtual byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Byte");
        }


        public virtual char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Char");
        }


        public virtual decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Decimal");
        }


        public virtual double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Double");
        }


        public virtual short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Int16");
        }


        public virtual int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Int32");
        }


        public virtual long ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Int64");
        }


        public virtual sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to SByte");
        }


        public virtual float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to Single");
        }


        public virtual string ToString(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to String");
        }


        public virtual ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to UInt16");
        }


        public virtual uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to UInt32");
        }


        public virtual ulong ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert " + this.GetType() + " to UInt64");
        }


        public override string ToString()
        {
            return ToString(formatProvider);
        }

        public virtual Variant this[string key]
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public virtual Variant this[int index]
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public static implicit operator Boolean(Variant variant)
        {
            return variant.ToBoolean(formatProvider);
        }


        public static implicit operator Single(Variant variant)
        {
            return variant.ToSingle(formatProvider);
        }


        public static implicit operator Double(Variant variant)
        {
            return variant.ToDouble(formatProvider);
        }


        public static implicit operator UInt16(Variant variant)
        {
            return variant.ToUInt16(formatProvider);
        }


        public static implicit operator Int16(Variant variant)
        {
            return variant.ToInt16(formatProvider);
        }


        public static implicit operator UInt32(Variant variant)
        {
            return variant.ToUInt32(formatProvider);
        }


        public static implicit operator Int32(Variant variant)
        {
            return variant.ToInt32(formatProvider);
        }


        public static implicit operator UInt64(Variant variant)
        {
            return variant.ToUInt64(formatProvider);
        }


        public static implicit operator Int64(Variant variant)
        {
            return variant.ToInt64(formatProvider);
        }


        public static implicit operator Decimal(Variant variant)
        {
            return variant.ToDecimal(formatProvider);
        }


        public static implicit operator String(Variant variant)
        {
            return variant.ToString(formatProvider);
        }
    }
}

