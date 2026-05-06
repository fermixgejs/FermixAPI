namespace FermixAPI.Hints.Core.Interface
{
    using FermixAPI.Hints.Core.Enum;

    internal interface IFontTool
    {
        float GetCharWidth(char c, float fontSize, TextStyle style);
    }
}
