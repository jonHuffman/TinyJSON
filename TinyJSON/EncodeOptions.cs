using System;


namespace TinyJSON
{
    [Flags]
    public enum EncodeOptions
    {
        /// <summary>
        /// Does not use any special logic when encoding.
        /// </summary>
        Default = 0,
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
        /// being encoded and instead only serialize public fields.
        /// </summary>
        IgnoreAttributes = 1 << 3,

        /// <summary>
        /// TinyJson supports writing Arrays as combinable. This writes and @index key for each array. 
        /// This allows you to take one json files and combine it with a second one. If not @index keys
        /// exist the two arrays will just be combined. 
        /// </summary>
        Combinable = 1 << 4,

        /// <summary>
        /// By default TinyJSON converts enums to string values. This option will instead have enums converted to integers
        /// </summary>
        EnumsAsInts = 1 << 5,
    }
}

