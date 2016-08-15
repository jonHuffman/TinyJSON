using System;

namespace TinyJSON
{
    [Obsolete("Skip has been renamed to Exclude please use that attribute instead.", error:true)]
    public sealed class Skip : ExcludeAttribute
    {
    }

    /// <summary>
    /// Mark members that should be excluded.
    /// Private fields and all properties are excluded by default.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExcludeAttribute : Attribute
    {
    }
}
