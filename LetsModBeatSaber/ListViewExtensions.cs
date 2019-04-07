using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LetsModBeatSaber
{
    public static class ListViewExtensions
    {
        public static void SetEnabled(this ListViewItem item, bool enabled)
        {
            if (enabled)
            {
                item.Font = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Regular);
                item.ForeColor = Color.Black;
            }
            else
            {
                item.Font = new Font(FontFamily.GenericSansSerif, 8.5f, FontStyle.Italic);
                item.ForeColor = Color.DarkGray;
            }
        }

        public static ListViewGroup GetOrCreateGroup(this ListView item, string groupName)
        {
            for (int i = 0; i < item.Groups.Count; i++)
            {
                if (string.Compare(item.Groups[i].Name, groupName) == 0)
                    return item.Groups[i];
            }

            return item.Groups.Add(groupName, groupName);
        }
    }
}
