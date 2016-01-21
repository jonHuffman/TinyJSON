using System;

namespace TinyJSON
{

  [Obsolete("Load has been renamed to AfterDecode please use attribute instead.")]
  public sealed class Load : AfterDecodeAttribute
  {

  }


  /// <summary>
  /// Mark methods to be called after an object is decoded.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method)]
  public class AfterDecodeAttribute : Attribute
  {
  }
}
