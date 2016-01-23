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

        /// <summary>
        /// Takes one Variant as a starting base and combines it with a second one. Any 
        /// values that are not contained in the first one but are in the second will be added.
        /// If both Variants contains the see keys the first one will have it's values overwritten
        /// by the second one. 
        /// Note:
        /// Arrays can only be combined if there is a @Index key in each object. If none exist all 
        /// arrays will just be added to each other. When creating your own json use the 
        /// <see cref="EncodeOptions.Combinable"/> to make it add indexes. 
        /// </summary>
        /// <param name="startingVariant">The variant to be used as the base.</param>
        /// <param name="combineWith">The one you are merging in</param>
        /// <returns>The combination of the two variants.</returns>
        public static Variant Combine(Variant startingVariant, Variant combineWith)
        {
            if(startingVariant == null)
            {
                throw new ArgumentException("The Starting Variant is null which can not be handled");
            }
            
            if(combineWith == null)
            {
                throw new ArgumentException("The combineWith Variant is null which can not be handled");
            }

            Variant combined = startingVariant;
            return CombineVariant(combined, combineWith); ;
        }

        private static Variant CombineVariant(Variant startingVariant, Variant combineWith)
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

