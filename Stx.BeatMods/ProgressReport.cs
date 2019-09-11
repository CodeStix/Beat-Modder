using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.BeatModsAPI
{
    public class ProgressReport
    {
        public string status;
        public float progress;

        public bool Done
        {
            get
            {
                return progress >= 1f;
            }
        }

        public ProgressReport(string status, float progress)
        {
            this.status = status;
            this.progress = progress;
        }

        public static Progress<ProgressReport> Partial(IProgress<ProgressReport> parentProgress, float progressOffset, float progressPart)
        {
            if (parentProgress == null)
                return new Progress<ProgressReport>();

            return new Progress<ProgressReport>((e) => parentProgress.Report(new ProgressReport(e.status, progressOffset + e.progress * progressPart)));
        }
    }
}
