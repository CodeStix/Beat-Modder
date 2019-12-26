using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.BeatModder
{
    public partial class FormFindFile : Form
    {
        public string fileToFind;
        public List<string> searchLocations;

        public string SelectedFile { get; private set; }
        public string SelectedDirectory => new FileInfo(SelectedFile).Directory.FullName;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public FormFindFile(string fileToFind, List<string> searchLocations, string info = "Please selected the right file for {0}:")
        {
            this.fileToFind = fileToFind;
            this.searchLocations = searchLocations;

            InitializeComponent();

            labelInfo.Text = string.Format(info, fileToFind);
            this.Text = $"Finding location of { fileToFind } ...";
        }


        private void FormFindFile_Load(object sender, EventArgs e)
        {
            SearchForFileAsync(cancellationTokenSource.Token);
        }

        public Task SearchForFileAsync(CancellationToken token)
        {
            return Task.Run(() =>
            {
                SetStatus($"Looking for any file named '{ fileToFind }' ...", false);

                BeginInvoke(new MethodInvoker(() => 
                {
                    listBox.Items.Clear();
                    listBox.Enabled = true;
                    buttonRefresh.Enabled = false;
                }));

                Thread.Sleep(500);

                ParallelLoopResult plr = Parallel.ForEach(searchLocations, (loc) =>
                {
                    foreach (string file in FileUtil.FindFile(loc, fileToFind))
                    {
                        BeginInvoke(new MethodInvoker(() =>
                        {
                            listBox.Items.Add(file);
                            SetStatus($"Found { fileToFind }: '{ file }'", false);
                        }));

                        if (token.IsCancellationRequested)
                            break;
                    }
                });
                
                while (!plr.IsCompleted && !token.IsCancellationRequested) ;

                SetStatus($"Done looking for { fileToFind }. Found { listBox.Items.Count } occurrences.", true);

                if (token.IsCancellationRequested)
                    return;

                BeginInvoke(new MethodInvoker(() =>
                {
                    if (listBox.Items.Count == 0)
                    {
                        listBox.Enabled = false;
                        listBox.Items.Add($"No location for '{ fileToFind }' was found,");
                        listBox.Items.Add("please locate it yourself using the 'Browse' button.");
                    }

                    buttonRefresh.Enabled = true;
                }));

            }, token);
        }

        private void SetStatus(string status, bool done)
        {
            if (!InvokeRequired)
            {
                statusLabel.Text = $"Status: { status }";
                progressBar.Visible = !done;
                return;
            }

            BeginInvoke(new MethodInvoker(() =>
            {
                statusLabel.Text = $"Status: { status }";
                progressBar.Visible = !done;
            }));
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            DialogResult = DialogResult.Cancel;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SelectedFile = listBox.SelectedItem.ToString();

            cancellationTokenSource.Cancel();
            DialogResult = DialogResult.OK;
        }

        private void ListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            buttonChoose.Enabled = listBox.SelectedIndex >= 0;
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = $"Please select the location of '{ fileToFind }'.";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string file = Path.Combine(fbd.SelectedPath, fileToFind);
                if (!File.Exists(file))
                {
                    DialogResult result = MessageBox.Show($"The required file '{ fileToFind }' was not found in the selected folder.\n\n" +
                        $"Press Retry to specify the folder containing '{ fileToFind }'.\n" +
                        $"Press Cancel to cancel.", "Required file not found.", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation);

                    if (result == DialogResult.Retry)
                        buttonBrowse.PerformClick(); 

                    return;
                }

                SelectedFile = file;

                cancellationTokenSource.Cancel();
                DialogResult = DialogResult.OK;
            }
        }

        private void ButtonRefresh_Click(object sender, EventArgs e)
        {
            SearchForFileAsync(cancellationTokenSource.Token);
        }
    }
}
