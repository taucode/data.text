using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TauCode.Data.Text.EmailAddressSupport;
using TauCode.Data.Text.EmojiSupport;
using TauCode.Data.Text.Exceptions;
using TauCode.Extensions;

// todo clean
namespace TauCode.Data.Text
{
    public class EmailAddress : IEquatable<EmailAddress>
    {
        #region Constants & Read-only

        public const int MaxEmailAddressLength = 1000; // with all comments and folding whitespaces
        public const int MaxLocalPartLength = 64;
        public const int MaxCleanEmailAddressLength = 254;

        private static readonly HashSet<char> AllowedSymbols;
        private static readonly char[] FoldingWhiteSpaceChars;

        #endregion

        #region Static

        static EmailAddress()
        {
            FoldingWhiteSpaceChars = new[] { '\r', '\n', ' ' };

            AllowedSymbols = new HashSet<char>(new[]
            {
                '-',
                '+',
                '=',
                '!',
                '?',
                '%',
                '~',
                '$',
                '&',
                '/',
                '|',
                '{',
                '}',
                '#',
                '*',
                '^',
                '_',
                '`',
                '\''
            });
        }

        #endregion

        #region Fields

        public readonly string LocalPart;
        public readonly HostName Domain;

        private string _value;
        private bool _valueBuilt;

        #endregion

        #region ctor

        private EmailAddress(string localPart, HostName hostName)
        {
            this.LocalPart = localPart;
            this.Domain = hostName;
        }

        #endregion

        #region Parsing

        public static EmailAddress Parse(ReadOnlySpan<char> input)
        {
            var consumed = TryExtractInternal(
                input,
                out var emailAddress,
                out var exception,
                (span, position) => false);

            if (consumed == 0)
            {
                throw exception;
            }

            return emailAddress;
        }

        public static bool TryParse(
            ReadOnlySpan<char> input,
            out EmailAddress emailAddress,
            out TextDataExtractionException exception)
        {
            var consumed = TryExtract(
                input,
                out emailAddress,
                out exception,
                (span, position) => false);

            return consumed > 0;
        }

        #endregion

        #region Extracting

        // todo: ut
        public static int Extract(
            ReadOnlySpan<char> input,
            out EmailAddress emailAddress,
            TerminatingDelegate terminatingPredicate = null)
        {
            var consumed = TryExtractInternal(
                input,
                out emailAddress,
                out var exception,
                terminatingPredicate);

            if (consumed == 0)
            {
                throw exception;
            }

            return consumed;
        }

        public static int TryExtract(
            ReadOnlySpan<char> input,
            out EmailAddress emailAddress,
            out TextDataExtractionException exception,
            TerminatingDelegate terminatingPredicate = null)
        {
            return TryExtractInternal(
                input,
                out emailAddress,
                out exception,
                terminatingPredicate);
        }

        #endregion

        #region Private

        private static int TryExtractInternal(
            ReadOnlySpan<char> input,
            out EmailAddress emailAddress,
            out TextDataExtractionException exception,
            TerminatingDelegate terminatingPredicate = null)
        {
            terminatingPredicate ??= Helper.DefaultTerminatingPredicate;

            if (input.IsEmpty)
            {
                emailAddress = null;
                exception = Helper.CreateException(ExtractionErrorTag.EmptyInput, null);
                return 0;
            }

            var context = new EmailAddressExtractionContext(terminatingPredicate);

            #region extract local part

            while (true)
            {
                if (context.Position == MaxEmailAddressLength)
                {
                    emailAddress = null;
                    exception = Helper.CreateException(ExtractionErrorTag.InputTooLong, context.Position);
                    return 0;
                }

                var segment = TryExtractLocalPartSegment(input, context, out exception);

                if (segment == null)
                {
                    emailAddress = null;
                    return 0;
                }

                var segmentValue = segment.Value;
                var segmentValueType = segmentValue.Type;

                if (segmentValueType.IsLocalPartSegment())
                {
                    context.AddLocalPartSegment(segmentValue);
                }

                if (context.LocalPartLength > MaxLocalPartLength)
                {
                    // UT tag: 044523bc-6c75-4ef4-bac0-51ec9628fc0a
                    exception = Helper.CreateException(ExtractionErrorTag.LocalPartTooLong, 0);
                    emailAddress = null;
                    return 0;
                }

                if (segmentValueType == SegmentType.At)
                {
                    context.AtSymbolIndex = segmentValue.Start;
                    break;
                }
            }

            #endregion

            #region extract domain & pack result

            while (true)
            {
                // todo: check we are not out of MaxEmailAddressLength (and ut it!)
                if (context.Position == input.Length || context.IsAtTerminatingChar(input))
                {
                    if (context.DomainSegments.Count == 0)
                    {
                        emailAddress = null;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                        return 0;
                    }

                    var lastDomainSegmentType = context.GetLastDomainSegmentType();


                    // got to the end of the email, let's see what we're packing.
                    if (lastDomainSegmentType == SegmentType.Period)
                    {
                        var pos = context.GetDomainStartIndex();
                        exception = Helper.CreateException(ExtractionErrorTag.InvalidDomain, pos);
                        emailAddress = null;
                        return 0;
                    }

                    // local part
                    var localPart = BuildLocalPart(input, context);

                    // domain
                    if (context.DomainLength == 0)
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                        emailAddress = null;
                        return 0;
                    }

                    var domain = BuildDomain(input, context, out exception);

                    if (domain == null)
                    {
                        emailAddress = null;
                        return 0;
                    }

                    if (domain.Value.Kind == HostNameKind.IPv4 && context.GetIPHostName() == null)
                    {
                        // looks like we've got something like joe@1.1.1.1

                        var pos = context.GetDomainStartIndex();
                        exception = Helper.CreateException(ExtractionErrorTag.IPv4MustBeEnclosedInBrackets, pos);
                        emailAddress = null;
                        return 0;
                    }

                    emailAddress = new EmailAddress(localPart, domain.Value);

                    if (emailAddress.ToString().Length > MaxCleanEmailAddressLength)
                    {
                        emailAddress = null;
                        exception = Helper.CreateException(ExtractionErrorTag.EmailAddressTooLong, context.Position);
                        return 0;
                    }

                    return context.Position;
                }

                if (context.Position == MaxEmailAddressLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.InputTooLong, context.Position);
                    emailAddress = null;
                    return 0;
                }

                var segment = TryExtractDomainSegment(
                    input,
                    context,
                    out exception);

                if (segment == null)
                {
                    emailAddress = null;
                    return 0;
                }

                var segmentValue = segment.Value;

                var segmentValueType = segmentValue.Type;

                if (segmentValueType.IsDomainSegment())
                {
                    context.AddDomainSegment(segmentValue);
                }
            }

            #endregion
        }

        private static Segment? TryExtractLocalPartSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var c = input[context.Position];
            var lastLocalPartSegmentType = context.GetLastLocalPartSegmentType();

            if (
                char.IsLetterOrDigit(c) ||
                AllowedSymbols.Contains(c) ||
                c.IsEmojiStartingChar() ||
                false)
            {
                if (
                    lastLocalPartSegmentType == null ||
                    lastLocalPartSegmentType == SegmentType.Period ||
                    false)
                {
                    return TryExtractLocalPartWordSegment(input, context, out exception);
                }

                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                return null;
            }
            else if (c == '.')
            {
                if (
                    lastLocalPartSegmentType == SegmentType.LocalPartWord ||
                    lastLocalPartSegmentType == SegmentType.LocalPartQuotedString ||
                    false)
                {
                    var start = context.Position;
                    context.Position++;
                    exception = null;
                    return new Segment(SegmentType.Period, start, 1, null);
                }
                else
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                    return null;
                }
            }
            else if (c == '@')
            {
                if (lastLocalPartSegmentType == null)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.EmptyLocalPart, context.Position);
                    return null;
                }

                if (
                    lastLocalPartSegmentType == SegmentType.LocalPartWord ||
                    lastLocalPartSegmentType == SegmentType.LocalPartQuotedString ||
                    false)
                {
                    var start = context.Position;
                    context.Position++;
                    exception = null;
                    return new Segment(SegmentType.At, start, 1, null);
                }
                else
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                    return null;
                }
            }
            else if (c == ' ')
            {
                return TryExtractLocalPartSpaceSegment(input, context, out exception);
            }
            else if (c == '\r')
            {
                return TryExtractLocalPartFoldingWhiteSpaceSegment(input, context, out exception);
            }
            else if (c == '"')
            {
                return TryExtractLocalPartQuotedStringSegment(input, context, out exception);
            }
            else if (c == '(')
            {
                return TryExtractCommentSegment(input, context, out exception);
            }

            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
            return null;
        }

        private static Segment? TryExtractDomainSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var c = input[context.Position];
            var lastNonCommentSegmentType = context.GetLastDomainSegmentType();

            if (char.IsLetterOrDigit(c))
            {
                if (context.GetIPHostName() != null)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                    return null;
                }

                // we only want nothing or period before a label
                if (
                    lastNonCommentSegmentType == null ||
                    lastNonCommentSegmentType == SegmentType.Period ||
                    false)
                {
                    return TryExtractLabelSegment(input, context, out exception);
                }
                else
                {
                    exception = Helper.CreateException(ExtractionErrorTag.InvalidDomain, context.Position);
                    return null;
                }
            }
            else if (c == '.')
            {
                if (context.GetIPHostName() != null)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                    return null;
                }

                // we only want label before period segment
                if (lastNonCommentSegmentType == SegmentType.Label)
                {
                    context.Position++;
                    exception = null;
                    return new Segment(SegmentType.Period, (context.Position - 1), 1, null);
                }
                else
                {
                    exception = Helper.CreateException(ExtractionErrorTag.InvalidDomain, context.Position);
                    return null;
                }
            }
            else if (c == '[')
            {
                if (context.GetIPHostName() != null || context.GotLabelOrPeriod())
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                    return null;
                }

                // we only want nothing 'clean' before ip address segment
                // todo: not really. comment can precede IP address. ut this.

                if (lastNonCommentSegmentType == null)
                {
                    if (context.Position < input.Length - 1)
                    {
                        var nextChar = input[context.Position + 1];
                        if (char.IsDigit(nextChar))
                        {
                            return TryExtractIPv4Segment(input, context, out exception);
                        }

                        if (nextChar == 'I') // start of 'IPv6:' signature
                        {
                            return TryExtractIPv6Segment(input, context, out exception);
                        }

                        context.Position++;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                        return null;
                    }
                    else
                    {
                        context.Position++;
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                        return null;
                    }
                }

                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                return null;

            }
            else if (c == '(')
            {
                return TryExtractCommentSegment(input, context, out exception);
            }

            //er-ror = EmailValidationError.UnexpectedCharacter; // todo: terminating char predicate here?

            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
            return null;
        }

        private static Segment? TryExtractCommentSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            context.Position++; // input[start] is '('
            var length = input.Length;

            var depth = 1;

            var escapeMode = false;

            while (true)
            {
                if (context.Position == length)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                    return null;
                }

                if (context.Position == MaxEmailAddressLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.InputTooLong, context.Position);
                    return null;
                }

                var c = input[context.Position];
                if (c == ')')
                {
                    context.Position++;

                    if (escapeMode)
                    {
                        escapeMode = false;
                    }
                    else
                    {
                        depth--;
                        if (depth == 0)
                        {
                            break;
                        }
                    }
                }
                else if (c == '(')
                {
                    context.Position++;

                    if (escapeMode)
                    {
                        escapeMode = false;
                    }
                    else
                    {
                        depth++;
                    }
                }
                else if (c == '\\')
                {
                    context.Position++;
                    escapeMode = !escapeMode;
                }
                else
                {
                    escapeMode = false;

                    var skipped = TrySkipEmoji(input[context.Position..], out var emojiError);
                    if (emojiError != null)
                    {
                        exception = TransformInnerExtractionException(emojiError, context.Position);
                        return null;
                    }

                    if (skipped > 0)
                    {
                        context.Position += skipped;
                        continue;
                    }

                    if (
                        char.IsLetterOrDigit(c) ||
                        AllowedSymbols.Contains(c) ||
                        (
                            (
                                char.IsSymbol(c) ||
                                char.IsSeparator(c)
                            ) &&
                            c >= 0x80
                        ) ||
                        c == ' ' ||
                        c == '@' ||
                        c == '.' ||
                        c == ':' ||
                        c == '\r' || // todo this is wrong: mind folding whitespace
                        c == '\n' || // todo this is wrong: mind folding whitespace
                        c == '"' || // todo: unite into hashSet; todo: ut these chars
                        // todo: '[', ']' are accepted too.
                        false)
                    {
                        context.Position++;
                    }
                    else
                    {
                        // not an allowed char
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                        return null;
                    }
                }
            }

            var delta = context.Position - start;
            exception = null;
            return new Segment(SegmentType.Comment, start, delta, null);
        }

        private static int TrySkipEmoji(
            ReadOnlySpan<char> emojiSpan,
            out TextDataExtractionException exception)
        {
            var skipped = EmojiHelper.Skip(emojiSpan, out var emojiExtractionErrorTag);
            var c = emojiSpan[0];

            switch (emojiExtractionErrorTag)
            {
                case null:
                    // successfully skipped emoji
                    exception = null;
                    return skipped;

                case ExtractionErrorTag.NonEmojiChar:
                    switch (skipped)
                    {
                        case 0:
                            // 0th char was not emoji
                            // do nothing - following code will deal with it
                            exception = null;
                            return 0;

                        case 1:
                            if (c.IsAsciiEmojiStartingChar())
                            {
                                exception = null;
                                return 0;

                                // something like #, *, 0..9
                                // do nothing - following code will deal with it
                            }
                            else
                            {
                                exception = Helper.CreateException(ExtractionErrorTag.BadEmoji, 0); // NOT 'skipped'
                                return 0;
                            }

                        default:
                            exception = Helper.CreateException(ExtractionErrorTag.BadEmoji, 0); // NOT 'skipped'
                            return 0;
                    }

                case ExtractionErrorTag.IncompleteEmoji:
                    exception = Helper.CreateException(ExtractionErrorTag.IncompleteEmoji, 0); // NOT 'skipped'
                    return skipped;

                default:
                    exception = Helper.CreateException(ExtractionErrorTag.InternalError, null); // should never happen
                    return 0;
            }
        }

        #region Local Part Extraction

        private static Segment? TryExtractLocalPartSpaceSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            context.Position++; // input[start] is a proper char since we've got here
            var length = input.Length;

            while (true)
            {
                if (context.Position - start > MaxLocalPartLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.LocalPartTooLong, context.Position);
                    return null;
                }

                if (context.Position == length)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                    return null;
                }

                var c = input[context.Position];

                if (c == ' ')
                {
                    context.Position++;
                    continue;
                }

                // end of white space.
                break;
            }

            exception = null;
            var delta = context.Position - start;
            return new Segment(SegmentType.LocalPartSpace, start, delta, null);
        }

        private static Segment? TryExtractLocalPartFoldingWhiteSpaceSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            context.Position++; // input[start] is a proper char since we've got here
            var length = input.Length;

            var fwsLength = FoldingWhiteSpaceChars.Length;

            int delta;

            while (true)
            {
                if (context.Position == length)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                    return null;
                }

                delta = context.Position - start;

                if (delta == fwsLength)
                {
                    break;
                }

                var c = input[context.Position];
                if (c == FoldingWhiteSpaceChars[delta])
                {
                    context.Position++;
                    continue;
                }

                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                return null;
            }

            exception = null;
            return new Segment(
                SegmentType.LocalPartFoldingWhiteSpace,
                start,
                delta,
                null); // actually, delta MUST be 3.
        }

        private static Segment? TryExtractLocalPartQuotedStringSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            context.Position++; // skip '"'
            var length = input.Length;

            var escapeMode = false;

            while (true)
            {
                if (context.Position - start > MaxLocalPartLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.LocalPartTooLong, context.Position);
                    return null;
                }

                if (context.Position == length)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnclosedQuotedString, context.Position);
                    return null;
                }

                var c = input[context.Position];
                if (c == '"')
                {
                    context.Position++;

                    if (escapeMode)
                    {
                        // no more actions
                    }
                    else
                    {
                        break;
                    }

                    escapeMode = false;
                }
                else if (c == '\0' || c == '\r' || c == '\n')
                {
                    if (escapeMode)
                    {
                        context.Position++;
                    }
                    else
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.UnescapedSpecialCharacter, context.Position);
                        return null;
                    }

                    escapeMode = false;
                }
                
                else if (c == '\\')
                {
                    context.Position++;
                    escapeMode = !escapeMode;
                }
                else
                {
                    escapeMode = false;

                    var skipped = TrySkipEmoji(input[context.Position..], out var emojiError);
                    if (emojiError != null)
                    {
                        exception = TransformInnerExtractionException(emojiError, context.Position);
                        return null;
                    }

                    if (skipped > 0)
                    {
                        context.Position += skipped;
                        continue;
                    }

                    if (
                        char.IsLetterOrDigit(c) ||
                        AllowedSymbols.Contains(c) ||
                        (
                            (
                                char.IsSymbol(c) ||
                                char.IsSeparator(c)
                            ) &&
                            c >= 0x80
                        ) ||
                        c == ' ' ||
                        c == '@' ||
                        c == '.' ||
                        c == ':' ||
                        c == '(' || // todo: unite into hashSet; todo: ut these chars
                        c == ')' || // todo: unite into hashSet; todo: ut these chars
                        c == ']' || // todo: unite into hashSet; todo: ut these chars
                        c == '[' || // todo: unite into hashSet; todo: ut these chars
                        false)
                    {
                        context.Position++;
                    }
                    else
                    {
                        exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                        return null;
                    }
                }
            }

            exception = null;
            var delta = context.Position - start;

            if (delta == 2)
            {
                // todo: ut empty quoted string in the middle of local part
                // empty string
                exception = Helper.CreateException(ExtractionErrorTag.EmptyQuotedString, context.Position - 2);
                context.Position = start;
                return null;
            }

            return new Segment(SegmentType.LocalPartQuotedString, start, delta, null);
        }

        private static Segment? TryExtractLocalPartWordSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;

            var length = input.Length;

            while (true)
            {
                if (context.Position - start > MaxLocalPartLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.LocalPartTooLong, context.Position);
                    return null;
                }

                // todo: local part with comments too long, local part with comments is not too long when extract clean local part.

                if (context.Position == length)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                    return null;
                }

                var c = input[context.Position];

                var skipped = TrySkipEmoji(input[context.Position..], out var emojiError);
                if (emojiError != null)
                {
                    exception = TransformInnerExtractionException(emojiError, context.Position);
                    return null;
                }

                if (skipped > 0)
                {
                    context.Position += skipped;
                    continue;
                }

                if (
                    char.IsLetterOrDigit(c) ||
                    AllowedSymbols.Contains(c))
                {
                    // letter, digit or symbol => go on.
                    context.Position++;
                }
                else
                {
                    // not a char allowed in a word
                    break;
                }
            }

            exception = null;
            var delta = context.Position - start;
            return new Segment(SegmentType.LocalPartWord, start, delta, null);
        }

        #endregion

        #region Domain Extraction

        private static Segment? TryExtractIPv6Segment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            // todo: ut john.doe@[IPv6:::] (success)
            var length = input.Length;
            var start = context.Position;
            context.Position++; // skip '['
            const string prefix = "IPv6:";
            const int prefixLength = 5; // "IPv6:".Length
            const int minRemainingLength =
                prefixLength +
                2 + /* :: */
                1; /* ] */

            var remaining = length - context.Position;

            if (remaining < minRemainingLength)
            {
                exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv6Address, context.Position);
                return null;
            }

            ReadOnlySpan<char> prefixSpan = prefix;
            if (input.Slice(context.Position, prefixLength).Equals(prefixSpan, StringComparison.Ordinal))
            {
                // good.
            }
            else
            {
                exception = Helper.CreateException(ExtractionErrorTag.InvalidIPv6Address, context.Position);
                return null;
            }

            context.Position += prefixLength;

            var ipv6Span = input[context.Position..];

            var consumed = HostName.TryExtract(
                ipv6Span,
                out var hostName,
                out var hostNameError,
                (span, position) => span[position] == ']');

            if (consumed == 0 || hostName.Kind != HostNameKind.IPv6)
            {
                exception = TransformInnerExtractionException(hostNameError, context.Position);
                return null;
            }

            context.Position += consumed; // skip ipv6 address

            if (context.Position == length)
            {
                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                return null;
            }

            var c = input[context.Position];

            if (c == ']')
            {
                context.Position++;

                var segmentLength = context.Position - start;
                var segment = new Segment(SegmentType.IPAddress, start, segmentLength, hostName);

                exception = null;
                return segment;
            }

            // this should never happen, actually.
            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
            return null;

        }

        private static Segment? TryExtractIPv4Segment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            context.Position++; // skip '['

            var hostSpan = input[context.Position..];
            var consumed = HostName.TryExtract(
                hostSpan,
                out var hostName,
                out var hostNameError,
                (span, position) => span[position] == ']');

            if (consumed == 0)
            {
                exception = TransformInnerExtractionException(hostNameError, context.Position);
                return null;
            }

            // we gotta skip ']'
            context.Position += consumed;

            if (context.Position == input.Length)
            {
                // we failed to achieve our ']'
                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedEnd, context.Position);
                return null;
            }

            var c = input[context.Position];

            if (c == ']')
            {
                context.Position++;
                exception = null;

                var length =
                    1 + // '['
                    consumed + // hostName
                    1 + // ']'
                    0;
                var segment = new Segment(SegmentType.IPAddress, start, length, hostName);
                return segment;
            }

            // this should never happen, actually.
            exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
            return null;
        }

        private static Segment? TryExtractLabelSegment(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var start = context.Position;
            var prevChar = input[start];
            context.Position++; // initial char is ok since we've got here
            var length = input.Length;

            while (true)
            {
                // todo: should we be aware of MaxEmailAddressLength? ut this.
                if (context.Position == length)
                {
                    break;
                }

                if (context.Position - start > Helper.MaxAsciiLabelLength)
                {
                    exception = Helper.CreateException(ExtractionErrorTag.DomainLabelTooLong, start);
                    return null;
                }

                var c = input[context.Position];

                if (char.IsLetterOrDigit(c))
                {
                    prevChar = c;
                    context.Position++;
                    continue;
                }

                if (c == '-')
                {
                    if (prevChar == '.')
                    {
                        // '.' cannot be followed by '-'
                        exception = Helper.CreateException(ExtractionErrorTag.InvalidDomain, context.Position);
                        return null;
                    }

                    prevChar = c;
                    context.Position++;
                    continue;
                }

                if (c == '(')
                {
                    // got start of comment
                    break;
                }

                if (c == '.')
                {
                    break;
                }

                if (context.TerminatingPredicate(input, context.Position))
                {
                    break;
                }

                exception = Helper.CreateException(ExtractionErrorTag.UnexpectedChar, context.Position);
                return null;
            }

            if (prevChar == '-')
            {
                // label cannot end with '-'
                exception = Helper.CreateException(ExtractionErrorTag.InvalidDomain, context.Position);
                return null;
            }

            exception = null;
            var delta = context.Position - start;
            return new Segment(SegmentType.Label, start, delta, null);
        }

        #endregion

        #region Building

        private static string BuildLocalPart(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context)
        {
            var localPartArray = new char[context.LocalPartLength];
            var pos = 0;

            foreach (var segment in context.LocalPartSegments)
            {
                var span = input.Slice(segment.Start, segment.Length);
                var destination = new Span<char>(localPartArray, pos, span.Length);
                span.CopyTo(destination);
                pos += span.Length;
            }

            var localPart = new string(localPartArray);
            return localPart;
        }

        private static HostName? BuildDomain(
            ReadOnlySpan<char> input,
            EmailAddressExtractionContext context,
            out TextDataExtractionException exception)
        {
            var contextIPHostName = context.GetIPHostName();

            if (contextIPHostName != null)
            {
                exception = null;
                return contextIPHostName;
            }

            // got 'domain name' host
            var domainArray = new char[context.DomainLength];
            var pos = 0;

            foreach (var segment in context.DomainSegments)
            {
                var span = input.Slice(segment.Start, segment.Length);
                var destination = new Span<char>(domainArray, pos, span.Length);
                span.CopyTo(destination);
                pos += span.Length;
            }

            var domainString = new string(domainArray);
            var parsed = HostName.TryParse(
                domainString,
                out var domain,
                out var hostNameError);

            if (hostNameError != null)
            {
                if (hostNameError.ExtractionError == ExtractionErrorTag.InputTooLong)
                {
                    exception = Helper.CreateException(
                        ExtractionErrorTag.HostNameTooLong,
                        context.GetDomainStartIndex() + hostNameError.Index);
                }
                else
                {
                    exception = TransformInnerExtractionException(hostNameError, context.Position);
                }


                return null;
            }

            exception = null;
            return domain;
        }

        private string BuildValue()
        {
            if (this.Domain.Value == null) // domain is default(HostName), which should not happen, actually
            {
                return null;
            }

            var sb = new StringBuilder();

            sb.Append(this.LocalPart);
            sb.Append("@");

            string format;

            switch (this.Domain.Kind)
            {
                case HostNameKind.Regular:
                case HostNameKind.Internationalized:
                    format = "{0}";
                    break;

                case HostNameKind.IPv4:
                    format = "[{0}]";
                    break;

                case HostNameKind.IPv6:
                    format = "[IPv6:{0}]";
                    break;

                default:
                    throw new FormatException("Cannot build email value.");
            }

            sb.AppendFormat(format, this.Domain.Value);
            var result = sb.ToString();
            return result;
        }

        #endregion

        private static TextDataExtractionException TransformInnerExtractionException(
            TextDataExtractionException exception,
            int index)
        {
            return new TextDataExtractionException(exception.Message, index + exception.Index ?? 0);
        }

        #endregion

        #region IEquatable<EmailAddress> Members

        public bool Equals(EmailAddress other)
        {
            if (other == null)
            {
                return false;
            }

            return
                this.LocalPart == other.LocalPart &&
                this.Domain.Equals(other.Domain);
        }

        #endregion

        #region Overridden

        public override bool Equals(object obj)
        {
            return obj is EmailAddress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.LocalPart, this.Domain);
        }

        public override string ToString()
        {
            if (!_valueBuilt)
            {
                _value = this.BuildValue();
                _valueBuilt = true;
            }

            return _value;
        }

        #endregion
    }
}
