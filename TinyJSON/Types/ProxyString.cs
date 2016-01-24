using System;


namespace TinyJSON
{
    public sealed class ProxyString : Variant
    {
        private string m_Value;

        public override object value
        {
            get
            {
                return m_Value; 
            }
        }

        public ProxyString( string value )
        {
            this.m_Value = value;
        }


        public override string ToString( IFormatProvider provider )
        {
            return m_Value;
        }
    }
}

