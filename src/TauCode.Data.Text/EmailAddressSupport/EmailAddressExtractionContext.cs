using System;
using System.Collections.Generic;

namespace TauCode.Data.Text.EmailAddressSupport
{
    internal class EmailAddressExtractionContext
    {
        private readonly List<Segment> _localPartSegments;
        private readonly List<Segment> _domainSegments;

        internal EmailAddressExtractionContext(TerminatingDelegate terminatingPredicate)
        {
            this.TerminatingPredicate = terminatingPredicate;

            _localPartSegments = new List<Segment>();
            _domainSegments = new List<Segment>();
        }

        internal int Position;

        internal readonly TerminatingDelegate TerminatingPredicate;

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


        internal bool IsAtTerminatingChar(ReadOnlySpan<char> input)
        {
            return this.TerminatingPredicate(input, this.Position);
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

            throw Helper.CreateException(ExtractionErrorTag.InternalError, 0);
        }

        internal HostName? GetIPHostName()
        {
            if (_domainSegments.Count == 0)
            {
                return null;
            }

            return _domainSegments[0].IPHostName;
        }

        internal bool GotLabelOrPeriod()
        {
            if (_domainSegments.Count == 0)
            {
                return false;
            }

            var type = _domainSegments[0].Type;
            return
                type == SegmentType.Label ||
                type == SegmentType.Period ||
                false;
        }
    }
}
