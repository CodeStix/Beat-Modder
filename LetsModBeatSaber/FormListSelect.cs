using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Stx.BeatModder
{
    public partial class FormListSelect : Form
    {
        public object[] items;

        private object result = null;

        public object Result
        {
            get
            {
                return result;
            }
        }

        public FormListSelect(string mainInstruction, string title = "Select an item...", params object[] items)
        {
            InitializeComponent();

            this.items = items;
            label1.Text = mainInstruction;
            Text = title;
        }

        private void FormListSelect_Load(object sender, EventArgs e)
        {
            listBox.Items.AddRange(items);
        }

        private void listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonOk.Enabled = listBox.SelectedIndex >= 0;
            
            if (listBox.SelectedIndex >= 0)
            {
                result = listBox.SelectedItem;
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            result = null;
            DialogResult = DialogResult.Cancel;
        }
    }
}
