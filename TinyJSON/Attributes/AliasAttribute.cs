using System;

namespace TinyJSON
{
    /// <summary>
    /// An Alias is used to rename the key being decoded or encoded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class AliasAttribute : Attribute
    {

    }
}
