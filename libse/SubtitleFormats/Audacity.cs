using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

	//26.475000	50.941000	aaaaaa
	//56.054000	83.625000	bbbbbb
	//96.041000	121.055000	ccccc

    public class Audacity : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d*.\d\d\d\d\d\d\t\d*.\d\d\d\d\d\d\t.*$", RegexOptions.Compiled);

        public override string Extension => ".txt";

        public override string Name => "Audacity Labels";

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (lines.Count > 0 && lines[0] != null && RegexTimeCodes.Match(lines[0]).Success)
            {
                return true;
            }

            return base.IsMine(lines, fileName);
        }

        public override string ToText(Subtitle subtitle, string title)
        {
            const string paragraphWriteFormat = "{0}\t{1}\t{2}";
            const string timeFormat = "{0:d}.{1:000000}";
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                string startTime = string.Format(timeFormat, p.StartTime.Hours * 60 * 60 + p.StartTime.Minutes * 60 + p.StartTime.Seconds, p.StartTime.Milliseconds*1000);
                string endTime = string.Format(timeFormat, p.EndTime.Hours * 60 * 60 + p.EndTime.Minutes * 60 + p.EndTime.Seconds, p.EndTime.Milliseconds*1000);
                sb.AppendLine(string.Format(paragraphWriteFormat, startTime, endTime, HtmlUtil.RemoveHtmlTags(p.Text.Replace(Environment.NewLine, " "))));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (RegexTimeCodes.Match(line).Success)
                {
                    string[] parts = line.Split(new[] { '\t' }, StringSplitOptions.None);
                    var p = new Paragraph();
                    if (parts.Length > 2 &&
                        GetTimeCode(p.StartTime, parts[0].Trim()) &&
                        GetTimeCode(p.EndTime, parts[1].Trim()))
                    {
                        p.Text = parts[2].Trim();
                        subtitle.Paragraphs.Add(p);
                    }
                }
                else
                {
                    _errorCount += 10;
                }
            }
            subtitle.Renumber();
        }

        private static bool GetTimeCode(TimeCode timeCode, string timeString)
        {
            try
            {
                string[] timeParts = timeString.Split('.');
                int seconds = int.Parse(timeParts[0]);
                timeCode.Hours = seconds / (60 * 60);
                timeCode.Minutes = (seconds - (timeCode.Hours * 60 * 60)) / 60;
                timeCode.Seconds = seconds % 60;
                timeCode.Milliseconds = int.Parse(timeParts[1]) / 1000;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
