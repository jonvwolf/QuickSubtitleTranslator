using QuickSubtitleTranslator.Common;
using SubtitlesParser.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuickSubtitleTranslator
{
    /// <summary>
    /// 
    /// </summary>
    public static class SubtitleItemExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static MySubtitleItem CopyToMySubtitleItem(this SubtitleItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return new MySubtitleItem(s: item.StartTime, e: item.EndTime, l: item.Lines.ToList());
        }
    }
}
