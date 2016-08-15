using System;
using System.Globalization;


namespace TinyJSON
{
    public sealed class ProxyNumber : Variant
    {
        private static readonly char[] floatingPointCharacters = new char[] { '.', 'e' };
        private IConvertible m_Value;


        public ProxyNumber( IConvertible value )
        {
            if (value is string)
            {
                this.m_Value = Parse( value as string );
            }
            else
            {
                this.m_Value = value;
            }
        }

        public override object value
        {
            get
            {
                return m_Value;
            }
        }


        private IConvertible Parse( string value )
        {
            if (value.IndexOfAny( floatingPointCharacters ) == -1)
            {
                if (value[0] == '-')
                {
                    Int64 parsedValue;
                    if (Int64.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ))
                    {
                        return parsedValue;
                    }
                }
                else
                {
                    UInt64 parsedValue;
                    if (UInt64.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ))
                    {
                        return parsedValue;
                    }
                }
            }

            Decimal decimalValue;
            if (Decimal.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimalValue ))
            {
                // Check for decimal underflow.
                if (decimalValue == Decimal.Zero)
                {
                    Double parsedValue;
                    if (Double.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ))
                    {
                        if (parsedValue != 0.0)
                        {
                            return parsedValue;
                        }
                    }
                }
                return decimalValue;
            }
            else
            {
                Double parsedValue;
                if (Double.TryParse( value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out parsedValue ))
                {
                    return parsedValue;
                }
            }

            return 0;
        }


        public override bool ToBoolean( IFormatProvider provider )
        {
            return m_Value.ToBoolean( provider );
        }


        public override byte ToByte( IFormatProvider provider )
        {
            return m_Value.ToByte( provider );
        }


        public override char ToChar( IFormatProvider provider )
        {
            return m_Value.ToChar( provider );
        }


        public override decimal ToDecimal( IFormatProvider provider )
        {
            return m_Value.ToDecimal( provider );
        }


        public override double ToDouble( IFormatProvider provider )
        {
            return m_Value.ToDouble( provider );
        }


        public override short ToInt16( IFormatProvider provider )
        {
            return m_Value.ToInt16( provider );
        }


        public override int ToInt32( IFormatProvider provider )
        {
            return m_Value.ToInt32( provider );
        }


        public override long ToInt64( IFormatProvider provider )
        {
            return m_Value.ToInt64( provider );
        }


        public override sbyte ToSByte( IFormatProvider provider )
        {
            return m_Value.ToSByte( provider );
        }


        public override float ToSingle( IFormatProvider provider )
        {
            return m_Value.ToSingle( provider );
        }


        public override string ToString( IFormatProvider provider )
        {
            return m_Value.ToString( provider );
        }


        public override ushort ToUInt16( IFormatProvider provider )
        {
            return m_Value.ToUInt16( provider );
        }


        public override uint ToUInt32( IFormatProvider provider )
        {
            return m_Value.ToUInt32( provider );
        }


        public override ulong ToUInt64( IFormatProvider provider )
        {
            return m_Value.ToUInt64( provider );
        }
    }
}

