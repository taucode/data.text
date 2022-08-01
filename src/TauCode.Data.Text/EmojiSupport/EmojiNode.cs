namespace TauCode.Data.Text.EmojiSupport
{
    internal class EmojiNode
    {
        private Emoji? _emoji;
        private readonly Dictionary<char, EmojiNode> _followers;

        internal EmojiNode(char? c)
        {
            this.Char = c;
            _followers = new Dictionary<char, EmojiNode>();
        }

        internal char? Char { get; }

        internal Emoji? Emoji
        {
            get => _emoji;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (_emoji != null)
                {
                    throw new InvalidOperationException("Emoji already set.");
                }

                _emoji = value;
            }
        }

        internal IReadOnlyDictionary<char, EmojiNode> Followers => _followers;

        internal void AddEmoji(Emoji emoji)
        {
            this.CheckIsRoot();
            this.AddEmojiTail(emoji, 0);
        }

        internal TextDataExtractionResult TryExtract(ReadOnlySpan<char> input, out Emoji emoji)
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applicable to the root node.");
            }

            if (input.Length == 0)
            {
                emoji = default;
                return new TextDataExtractionResult(0, TextDataExtractionErrorCodes.UnexpectedEnd);
            }

            Emoji? lastEmoji = null;
            int? lastSuccessfulOffset = null;

            var offset = 0;
            var current = this;

            while (true)
            {
                if (offset == input.Length)
                {
                    if (lastEmoji == null)
                    {
                        emoji = default;
                        return new TextDataExtractionResult(offset, TextDataExtractionErrorCodes.UnexpectedEnd);
                    }
                    else
                    {
                        emoji = lastEmoji.Value;
                        return new TextDataExtractionResult(lastSuccessfulOffset!.Value + 1, null);
                    }
                }

                var c = input[offset];

                var follower = current._followers.GetValueOrDefault(c);

                if (follower == null)
                {
                    if (current.Emoji.HasValue)
                    {
                        emoji = current.Emoji.Value;
                        return new TextDataExtractionResult(offset, null);
                    }
                    else
                    {
                        if (lastEmoji == null)
                        {
                            emoji = default;
                            return new TextDataExtractionResult(offset, TextDataExtractionErrorCodes.NonEmojiCharacter);
                        }
                        else
                        {
                            emoji = lastEmoji.Value;
                            return new TextDataExtractionResult(lastSuccessfulOffset!.Value + 1, null);
                        }
                    }
                }
                else
                {
                    if (follower.Emoji.HasValue)
                    {
                        lastEmoji = follower.Emoji.Value;
                        lastSuccessfulOffset = offset;
                    }

                    offset++;

                    if (offset == input.Length)
                    {
                        if (lastEmoji == null)
                        {
                            emoji = default;
                            return new TextDataExtractionResult(offset, TextDataExtractionErrorCodes.UnexpectedEnd);
                        }
                        else
                        {
                            emoji = lastEmoji.Value;
                            return new TextDataExtractionResult(lastSuccessfulOffset!.Value + 1, null);
                        }
                    }

                    current = follower;
                }
            }
        }

        private void AddEmojiTail(Emoji emoji, int offset)
        {
            var c = emoji.Value[offset];
            var follower = this.GetOrCreateFollower(c);

            if (offset == emoji.Value.Length - 1)
            {
                follower.Emoji = emoji;
            }
            else
            {
                follower.AddEmojiTail(emoji, offset + 1);
            }
        }

        private EmojiNode GetOrCreateFollower(char c)
        {
            var follower = _followers.GetValueOrDefault(c);
            if (follower == null)
            {
                follower = new EmojiNode(c);
                _followers.Add(c, follower);
            }

            return follower;
        }

        internal bool HasPath(ReadOnlySpan<char> path, int length)
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applicable to the root node.");
            }

            var cleanPath = path[..Math.Min(path.Length, length)];

            return this.HasPathInternal(cleanPath);
        }

        private bool HasPathInternal(ReadOnlySpan<char> cleanPath)
        {
            if (cleanPath.Length == 0)
            {
                return true;
            }

            var follower = _followers.GetValueOrDefault(cleanPath[0]);
            if (follower == null)
            {
                return false;
            }

            return follower.HasPathInternal(cleanPath[1..]);
        }

        private void CheckIsRoot()
        {
            if (this.Char != null)
            {
                throw new InvalidOperationException("Only applies to the root node."); // should never happen
            }
        }
    }
}
