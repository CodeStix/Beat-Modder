using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace LetsModBeatSaber
{
    public partial class FormModInfo : Form
    {
        private Mod mod;

        private static bool showAdvanced = false;

        public FormModInfo(Mod mod)
        {
            InitializeComponent();

            this.mod = mod;
            textBoxName.Text = mod.name + Environment.NewLine + "\tby " + mod.author.username;
            textBoxDescription.Text = mod.description + Environment.NewLine + Environment.NewLine + "Category: " + mod.category;
            labelVersion.Text = mod.version;

            CreateTree();
        }

        private void CreateTree()
        {
            treeViewAdvanced.Nodes.Clear();

            JObject root = JObject.Parse(JsonConvert.SerializeObject(mod));

            TreeNode rootNode = treeViewAdvanced.Nodes.Add("Properties");

            Node(rootNode, root);
        }

        private void Node(TreeNode parent, JToken further)
        {
            foreach(JToken token in further.Children())
            {
                if (token is JObject)
                {
                    TreeNode node = parent.Nodes.Add(token.Type.ToString());

                    Node(node, token);
                }
                else if (token is JProperty)
                {
                    JProperty prop = (JProperty)token;

                    TreeNode node;
                    if (prop.Value is JValue)
                        node = parent.Nodes.Add(prop.Name + ": " + prop.Value.ToString());
                    else
                        node = parent.Nodes.Add(prop.Name);

                    if (prop.Value is JObject || prop.Value is JArray)
                        Node(node, prop.Value);
                }
            }
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(mod.link);
        }

        private void buttonAdvancedInfo_Click(object sender, EventArgs e)
        {
            showAdvanced = !showAdvanced;

            UpdateSize();
        }

        private void UpdateSize()
        {
            Size s = Size;
            s.Height = showAdvanced ? 561 : 254;
            Size = s;

            buttonAdvancedInfo.Text = showAdvanced ? "Advanced /\\" : "Advanced \\/";
        }

        private void FormModInfo_Load(object sender, EventArgs e)
        {
            UpdateSize();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBoxAdvancedSelected.Text = e.Node.Text;
        }

        private void buttonDirectDownload_Click(object sender, EventArgs e)
        {
            Process.Start(mod.GetBestDownloadFor(ModDownloadType.Steam).DownloadUrl);
        }
    }
}
