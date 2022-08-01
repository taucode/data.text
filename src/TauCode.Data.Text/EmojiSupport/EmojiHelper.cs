using System.Data;
using System.Text;

namespace TauCode.Data.Text.EmojiSupport
{
    internal static class EmojiHelper
    {
        internal static readonly HashSet<char> EmojiStartingChars;
        internal static readonly HashSet<char> AsciiEmojiStartingChars;
        internal static readonly HashSet<char> SingleCharAsciiEmojis;
        internal static readonly Dictionary<string, Emoji> Emojis;
        internal static readonly Dictionary<string, EmojiDescriptor> EmojiDescriptors;

        internal static readonly EmojiNode Root;

        static EmojiHelper()
        {
            var emojis = LoadEmojis();
            Emojis = emojis
                .ToDictionary(x => x.Value, x => x);

            Root = BuildTree(emojis);
            EmojiStartingChars = new HashSet<char>(Root.Followers.Keys);
            AsciiEmojiStartingChars = new HashSet<char>(EmojiStartingChars
                .Where(x => x < 0x100));
            SingleCharAsciiEmojis = new HashSet<char>(emojis
                .Where(x =>
                    x.Value.Length == 1 &&
                    x.Value[0] < 0x100)
                .Select(x => x.Value.Single()));

            EmojiDescriptors = Emojis
                .Values
                .ToDictionary(x => x.Value, x => new EmojiDescriptor(x));

            var tuples = EmojiDescriptors.Values
                .GroupBy(x => x.Emoji.Value.Length)
                .Select(x => Tuple.Create(x.Key, (IList<EmojiDescriptor>)x.ToList()))
                .OrderBy(x => x.Item1)
                .ToList();

            for (var i = 0; i < tuples.Count; i++)
            {
                var tuple = tuples[i];
                var list = tuple.Item2;

                foreach (var emojiDescriptor in list)
                {
                    for (var j = i + 1; j < tuples.Count; j++)
                    {
                        var higherTuple = tuples[j];
                        var higherList = higherTuple.Item2;

                        foreach (var higherEmojiDescriptor in higherList)
                        {
                            if (higherEmojiDescriptor.Emoji.Value.StartsWith(emojiDescriptor.Emoji.Value, StringComparison.Ordinal))
                            {
                                emojiDescriptor.Containers.Add(higherEmojiDescriptor);
                                higherEmojiDescriptor.Contained.Add(emojiDescriptor);
                            }
                        }
                    }
                }
            }
        }

        internal static bool IsEmojiStartingChar(this char c) => EmojiStartingChars.Contains(c);

        private static string? ReadStringFromStream(Stream stream, byte[] buffer)
        {
            // read value
            var intLength = stream.ReadByte();
            if (intLength == -1)
            {
                return null;
            }

            var pos = 0;
            var remaining = intLength;

            while (true)
            {
                var bytesRead = stream.Read(buffer, pos, remaining);

                if (bytesRead == 0)
                {
                    throw new Exception();
                }

                remaining -= bytesRead;

                if (remaining == 0)
                {
                    var value = Encoding.UTF8.GetString(buffer, 0, intLength);
                    return value;
                }
            }
        }

        private static IList<Emoji> LoadEmojis()
        {
            using var stream =
                typeof(EmojiHelper).Assembly.GetManifestResourceStream($"{typeof(Emoji).Namespace}.Resources.emojis.bin")
                ??
                throw new FileNotFoundException("Emoji resource not found.");

            var list = new List<Emoji>();

            var buffer = new byte[1000]; // would be enough

            while (true)
            {
                var value = ReadStringFromStream(stream, buffer);
                if (value == null)
                {
                    break;
                }

                var name = ReadStringFromStream(stream, buffer);
                if (name == null)
                {
                    throw new Exception();
                }

                var emoji = new Emoji(value, name);
                list.Add(emoji);
            }

            return list;
        }

        private static EmojiNode BuildTree(IList<Emoji> emojis)
        {
            var root = new EmojiNode(null);

            foreach (var emoji in emojis)
            {
                root.AddEmoji(emoji);
            }

            return root;
        }
    }
}
