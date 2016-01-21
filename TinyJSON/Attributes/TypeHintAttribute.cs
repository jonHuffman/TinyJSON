using System;

namespace TinyJSON
{
  /// <summary>
  /// Mark members to force type hinting even when EncodeOptions.NoTypeHints is set.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
  public class TypeHintAttribute : Attribute
  {
  }
}
