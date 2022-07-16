using System.Collections.Generic;
using TauCode.Data.Text.Exceptions;
using TauCode.Data.Text.TextDataExtractors;

namespace TauCode.Data.Text.EmailAddressSupport
{
    internal class EmailAddressExtractionContext
    {
        private readonly List<Segment> _localPartSegments;
        private readonly List<Segment> _domainSegments;

        internal EmailAddressExtractionContext(TerminatingDelegate terminator)
        {
            this.Terminator = terminator;

            _localPartSegments = new List<Segment>();
            _domainSegments = new List<Segment>();
            HostNameExtractor = new HostNameExtractor(this.Terminator);
        }

        internal readonly HostNameExtractor HostNameExtractor;

        internal int Position;

        internal readonly TerminatingDelegate Terminator;

        internal int? AtSymbolIndex; // index of '@' in span

        internal IReadOnlyList<Segment> LocalPartSegments => _localPartSegments;
        internal int LocalPartLength { get; private set; }
        internal void AddLocalPartSegment(Segment segment)
        {
            _localPartSegments.Add(segment);
            this.LocalPartLength += segment.Length;
        }

        internal IReadOnlyList<Segment> DomainSegments => _domainSegments;
        internal int DomainLength { get; private set; }
        internal void AddDomainSegment(Segment segment)
        {
            _domainSegments.Add(segment);
            this.DomainLength += segment.Length;
        }

        internal SegmentType? GetLastLocalPartSegmentType()
        {
            if (this.LocalPartSegments.Count == 0)
            {
                return null;
            }

            return this.LocalPartSegments[^1].Type;
        }

        internal SegmentType? GetLastDomainSegmentType()
        {
            if (this.DomainSegments.Count == 0)
            {
                return null;
            }

            return this.DomainSegments[^1].Type;
        }

        /// <summary>
        /// Gets index of character where domain starts
        /// </summary>
        /// <returns>Index of character where domain starts</returns>
        internal int GetDomainStartIndex()
        {
            if (this.AtSymbolIndex.HasValue)
            {
                return this.AtSymbolIndex.Value + 1;
            }

            throw new TextDataExtractionException(
                Helper.GetErrorMessage(TextDataExtractionErrorCodes.InternalError),
                TextDataExtractionErrorCodes.InternalError,
                0);
        }

        internal Segment? GetIPHostNameSegment()
        {
            if (_domainSegments.Count == 0)
            {
                return null;
            }

            var segment = _domainSegments[0];
            if (segment.IPHostName == null)
            {
                return null;
            }

            return _domainSegments[0];
        }

        internal HostName? GetIPHostName() => GetIPHostNameSegment()?.IPHostName;
    }
}
