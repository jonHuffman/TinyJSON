using System;

namespace TinyJSON
{
    /// <summary>
    /// An Alias is used to rename the key being decoded or encoded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AliasAttribute : Attribute
    {
        private string m_Alias;

        /// <summary>
        /// Gets or sets the Alias for the field or property being encoded / decoded.
        /// </summary>
        public string alias
        {
            get { return m_Alias; }
            set { m_Alias = value; }
        }


        public AliasAttribute(string alias)
        {
            this.alias = alias;
        }
    }
}
