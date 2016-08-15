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


		public override char ToChar(IFormatProvider provider)
		{
			if(m_Value.Length > 0)
			{
				return m_Value[0];
			}
			else
			{
				// Returns a unicode null 
				return '\0';
			}
		}

        public override string ToString( IFormatProvider provider )
        {
            return m_Value;
        }
    }
}

