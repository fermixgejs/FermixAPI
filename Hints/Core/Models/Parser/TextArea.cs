namespace FermixAPI.Hints.Core.Models.Parser
{
    internal class TextArea
    {
        public float Top { get; set; }

        public float Bottom { get; set; }

        public float Left { get; set; }

        public float Right { get; set; }

        public bool HasIntersection(TextArea area)
        {
            return !(Left >= area.Right || area.Left >= Right || Top >= area.Bottom || area.Top >= Bottom);
        }
    }
}
