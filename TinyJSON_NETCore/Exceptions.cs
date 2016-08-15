using System;

namespace TinyJSON
{
  /// <summary>
  /// This is thrown when there has been an error decoding a type.
  /// </summary>
  public sealed class DecodeException : Exception
  {
    public DecodeException(string message)
        : base(message)
    {
    }


    public DecodeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
  }
}
