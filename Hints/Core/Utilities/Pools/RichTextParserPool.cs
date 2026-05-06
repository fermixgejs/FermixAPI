namespace FermixAPI.Hints.Core.Utilities.Pools
{
    using System.Collections.Concurrent;
    using FermixAPI.Hints.Core.Interface;
    using FermixAPI.Hints.Core.Utilities.Parser;

    internal class RichTextParserPool : IPool<RichTextParser>
    {
        private readonly ConcurrentQueue<RichTextParser> richTextParserQueue = new();

        public static RichTextParserPool Instance { get; } = new();

        public RichTextParser Rent()
        {
            if (richTextParserQueue.TryDequeue(out RichTextParser rtp))
                return rtp;

            return new RichTextParser();
        }

        public void Return(RichTextParser parser)
        {
            parser.ClearStatus();
            richTextParserQueue.Enqueue(parser);
        }
    }
}
