using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace QuickSubtitleTranslator.Common
{
    public class MySubtitleItem
    {
        public int StartTime { get; protected set; }
        public int EndTime { get; protected set; }

        readonly IList<string> _lines;
        public IReadOnlyList<string> Lines { get { return _lines.ToImmutableList(); } }
        public MySubtitleItem(int s, int e, IReadOnlyList<string> l)
        {
            StartTime = s;
            EndTime = e;
            _lines = l?.ToList() ?? throw new ArgumentNullException(nameof(l));
        }

        public MySubtitleItem(int s, int e)
        {
            StartTime = s;
            EndTime = e;
            _lines = new List<string>();
        }

        public void AddLine(string line)
        {
            _lines.Add(line);
        }
    }
}
