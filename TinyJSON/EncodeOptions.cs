using System;


namespace TinyJSON
{
    [Flags]
    public enum EncodeOptions
    {
        /// <summary>
        /// Does not use any special logic when encoding.
        /// </summary>
        None = 0,
        /// <summary>
        /// The encoders output will be formatted to be more human readable. 
        /// </summary>
        PrettyPrint = 1 << 1,

        /// <summary>
        /// Tells the encoder to ignore writing TypeHints. 
        /// </summary>
        NoTypeHints = 1 << 2,

        /// <summary>
        /// If you want to encode objects faster you can disable the reflection 
        /// look up for Attributes. This will ignore all attributes in the class
        /// being decoded
        /// </summary>
        IgnoreAttributes = 1 << 3,
    }
}

