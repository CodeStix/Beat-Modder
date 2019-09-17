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
using Stx.BeatModsAPI;

namespace Stx.BeatModder
{
    public partial class FormModInfo : Form
    {
        private Mod mod;
        private InstalledMod localMod;

        private static bool showAdvanced = false;

        public FormModInfo(Mod mod, InstalledMod localMod = null)
        {
            this.localMod = localMod;
            this.mod = mod;

            InitializeComponent();

            if (mod != null)
            {
                textBoxName.Text = mod.Name + Environment.NewLine + "\tby " + mod.author.username;
                textBoxDescription.Text = mod.description + Environment.NewLine + Environment.NewLine + "Category: " + mod.category;
                labelVersion.Text = mod.Version;
                labelGameVersion.Text = "game " + mod.gameVersion;
            }

            if (localMod != null)
            {
                textBoxBinaryFile.Text = $"Binary file: { localMod.binaryFile.file }";

                if(mod == null)
                {
                    textBoxName.Text = localMod.Name;
                    linkLabel.Visible = false;
                    buttonDirectDownload.Enabled = false;
                    textBoxDescription.Text = "No description.";
                    labelVersion.Text = localMod.Version;
                    labelGameVersion.Text = "game " + localMod.gameVersion;
                }
            }

            CreateTree();
        }

        private void CreateTree()
        {
            treeViewAdvanced.Nodes.Clear();

            if (localMod != null)
            {
                JObject rootMod = JObject.Parse(JsonConvert.SerializeObject(localMod));
                TreeNode rootNodeMod = treeViewAdvanced.Nodes.Add("Local mod");
                rootNodeMod.ForeColor = Color.ForestGreen;
                Node(rootNodeMod, rootMod);
            }

            if (mod != null)
            {
                JObject rootMod = JObject.Parse(JsonConvert.SerializeObject(mod));
                TreeNode rootNodeMod = treeViewAdvanced.Nodes.Add("Mod");
                rootNodeMod.ForeColor = Color.DarkGray;
                Node(rootNodeMod, rootMod);
            }
        }

        private void Node(TreeNode parent, JToken further)
        {
            if (further is JArray)
            {
                foreach (JToken b in (JArray)further)
                {
                    parent.Nodes.Add(b.ToString());
                }
            }
            else
            {
                foreach (JToken token in further.Children())
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
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
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
            Process.Start(mod.GetBestDownloadFor(BeatSaberInstalledType.Steam).DownloadUrl);
        }
    }
}
