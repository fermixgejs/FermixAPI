namespace FermixAPI.Hints.Core.Models.Parser
{
    internal readonly struct CharacterInfo
    {
        public CharacterInfo(char character, float fontSize, float width, float height, float vOffset)
        {
            Character = character;
            FontSize = fontSize;
            Width = width;
            Height = height;
            VOffset = vOffset;
        }

        public char Character { get; }

        public float FontSize { get; }

        public float Width { get; }

        public float Height { get; }

        public float VOffset { get; }
    }
}
