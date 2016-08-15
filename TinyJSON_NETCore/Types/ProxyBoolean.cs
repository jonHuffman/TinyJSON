using System;


namespace TinyJSON
{
    public sealed class ProxyBoolean : Variant
    {
        private bool m_Value;


        public ProxyBoolean( bool value )
        {
            this.m_Value = value;
        }


        public override object value
        {
            get
            {
                return m_Value;
            }
        }

        public override bool ToBoolean( IFormatProvider provider )
        {
            return m_Value;
        }
    }
}

