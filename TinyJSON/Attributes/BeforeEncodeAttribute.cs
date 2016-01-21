using System;

namespace TinyJSON
{
  /// <summary>
  /// Mark methods to be called before an object is encoded.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public class BeforeEncodeAttribute : Attribute
  {
  }
}
