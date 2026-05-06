namespace FermixAPI.Hints.Core.Enum
{
    using System;

    [Flags]
    internal enum TextStyle
    {
        Normal = 0x0000,
        Bold = 0x0001,
        Italic = 0x0010,
    }
}
