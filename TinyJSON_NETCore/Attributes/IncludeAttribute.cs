using System;

namespace TinyJSON
{
    /// <summary>
    /// Mark members that should be included. 
    /// Public fields are included by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class IncludeAttribute : Attribute
    {

    }
}
