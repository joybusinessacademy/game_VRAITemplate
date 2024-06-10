using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVRNodes.Diagnostics
{
    
    public class LogTag
    {
        public static LogTag Make(object tag = null)
        {
            if (null != tag && tag is LogTag)
            {
                return new LogTag() | tag;
            }
            string tagTxt = null == tag ? null : tag.ToString();
            return new LogTag(tagTxt);
        }

        protected HashSet<string> sections = new HashSet<string>();
        public LogTag(string txt = null)
        {
            Add(txt);
        }

        public void Add(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt))
            {
                return;
            }
            sections.Add(txt);
        }

        public void Add(LogTag other)
        {
            if (null == other
                || null == other.sections)
            {
                return;
            }
            foreach(var txt in other.sections)
            {
                Add(txt);
            }
        }

        public void Remove(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return;
            }
            sections.Remove(tag);
        }

        public void Remove(LogTag tag)
        {
            if (null == tag
                || null == tag.sections)
            {
                return;
            }
            foreach(var item in tag.sections)
            {
                sections.Remove(item);
            }
        }

        public void Clear()
        {
            sections.Clear();
        }

        public static LogTag operator |(LogTag a, object b)
        {
            LogTag output = new LogTag();
            if (null != a )
            {
                foreach(var item in a.sections)
                {
                    output.sections.Add(item);
                }
            }

            if (null == b)
            {
                return output;
            }

            if (b is LogTag tagB)
            {
                output.Add(tagB);
            }
            else if (b is string tagStr)
            {
                output.Add(tagStr);
            }
            else if (b is IEnumerable IenumB)
            {
                foreach(var item in IenumB)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    output.Add(item.ToString());
                }
            }
            else
            {
                output.Add(b.ToString());
            }
            return output;
        }

        public static implicit operator string[](LogTag tag)
        {
            if (null == tag)
            {
                return null;
            }
            return tag.sections.ToArray();
        }

        public override string ToString()
        {
            return null == sections ? "" : string.Join("|", sections);
        }
    }
}

