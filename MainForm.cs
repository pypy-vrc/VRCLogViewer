using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace VRCLogViewer
{
    public partial class MainForm : Form
    {
        public static MainForm Instance { get; private set; } = null;
        private DateTime m_NextUpdateLog = DateTime.MinValue;

        // WINAPI
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public MainForm()
        {
            Instance = this;
            InitializeComponent();
            Utils.SetDoubleBuffering(listview);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            checkbox.Checked = true;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (checkbox.Checked)
            {
                if (DateTime.Now.CompareTo(m_NextUpdateLog) >= 0)
                {
                    checkbox.Text = "Updating...";
                    VRChatLog.Update();
                    m_NextUpdateLog = DateTime.Now.AddSeconds(9);
                }
                else
                {
                    checkbox.Text = "Update in " + ((m_NextUpdateLog.Ticks - DateTime.Now.Ticks) / 10000000) + " second(s)";
                }
            }
            if (checkbox_discord_presence.Checked &&
                FindWindow("UnityWndClass", "VRChat") != IntPtr.Zero)
            {
                if (checkbox_show_location.Checked)
                {
                    Discord.SetPresence(VRChatLog.WorldName, VRChatLog.AccessTag);
                }
                else
                {
                    Discord.SetPresence(string.Empty, string.Empty);
                }
            }
            else
            {
                Discord.RemovePresence();
            }
            Discord.Update();
        }

        public void SetActivity(ActivityInfo info)
        {
            listview.BeginUpdate();
            while (listview.Items.Count >= 500)
            {
                listview.Items.RemoveAt(0);
            }
            switch (info.Type)
            {
                case ActivityType.EnterWorld:
                    {
                        var item = new ListViewItem
                        {
                            Tag = "world," + info.Tag,
                            Text = info.Time,
                            StateImageIndex = 0,
                            UseItemStyleForSubItems = false,
                            BackColor = Color.Ivory
                        };
                        item.SubItems.Add(new ListViewItem.ListViewSubItem
                        {
                            Text = info.Text,
                            ForeColor = Color.DeepPink,
                            BackColor = Color.Ivory
                        });
                        listview.Items.Add(item);
                        item.EnsureVisible();
                    }
                    break;

                case ActivityType.PlayerJoined:
                    {
                        var item = new ListViewItem
                        {
                            Tag = "user," + info.Tag,
                            Text = info.Time,
                            StateImageIndex = 1,
                            UseItemStyleForSubItems = false
                        };
                        item.SubItems.Add(new ListViewItem.ListViewSubItem
                        {
                            Text = info.Text,
                            ForeColor = Color.RoyalBlue
                        });
                        listview.Items.Add(item);
                        item.EnsureVisible();
                    }
                    break;

                case ActivityType.PlayerLeft:
                    {
                        var item = new ListViewItem
                        {
                            Tag = "user," + info.Tag,
                            Text = info.Time,
                            StateImageIndex = 2,
                            UseItemStyleForSubItems = false
                        };
                        item.SubItems.Add(new ListViewItem.ListViewSubItem
                        {
                            Text = info.Text,
                            ForeColor = Color.Gray
                        });
                        listview.Items.Add(item);
                        item.EnsureVisible();
                    }
                    break;
            }
            listview.EndUpdate();
        }

        private void listview_DoubleClick(object sender, EventArgs e)
        {
            if (listview.SelectedItems.Count > 0 &&
                listview.SelectedItems[0].Tag is string tag)
            {
                var a = tag.Split(new[] { ',' }, 2);
                if (a.Length == 2)
                {
                    switch (a[0])
                    {
                        case "user":
                            Process.Start("https://vrchat.net/home/user/" + a[1]);
                            return;

                        case "world":
                            a = a[1].Split(new[] { ':' }, 2);
                            if (a.Length == 2)
                            {
                                Process.Start("https://vrchat.net/home/launch?worldId=" + a[0] + "&instanceId=" + a[1]);
                            }
                            else
                            {
                                Process.Start("https://vrchat.net/home/launch?worldId=" + a[0]);
                            }
                            return;
                    }
                }
                MessageBox.Show(tag);
            }
        }

        private void listview_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.A:
                        listview.BeginUpdate();
                        listview.SelectedItems.Clear();
                        foreach (ListViewItem item in listview.Items)
                        {
                            item.Selected = true;
                        }
                        listview.EndUpdate();
                        break;

                    case Keys.C:
                        if (listview.SelectedItems.Count > 0)
                        {
                            var b = new StringBuilder();
                            foreach (ListViewItem item in listview.SelectedItems)
                            {
                                if (b.Length > 0)
                                {
                                    b.Append("\r\n");
                                }
                                b.Append(item.Text);
                                b.Append(' ');
                                b.Append(item.SubItems[1].Text);
                            }
                            Clipboard.Clear();
                            Clipboard.SetText(b.ToString());
                        }
                        break;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Escape:
                        e.SuppressKeyPress = true;
                        listview.BeginUpdate();
                        listview.SelectedItems.Clear();
                        listview.EndUpdate();
                        break;

                    case Keys.Delete:
                        listview.BeginUpdate();
                        foreach (ListViewItem item in listview.SelectedItems)
                        {
                            item.Remove();
                        }
                        listview.EndUpdate();
                        break;
                }
            }
        }

        private void checkbox_CheckedChanged(object sender, EventArgs e)
        {
            checkbox.Text = checkbox.Checked ? "Update" : "Update Stopped";
        }

        private void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    e.SuppressKeyPress = true;
                    var keyword = textbox.Text;
                    listview.BeginUpdate();
                    {
                        var i = (listview.SelectedItems.Count > 0) ? listview.SelectedItems[0].Index : -1;
                        var n = listview.Items.Count;
                        for (var z = 0; z < n; ++z)
                        {
                            if (++i >= n)
                            {
                                i = 0;
                            }
                            var item = listview.Items[i];
                            if (item.SubItems[1].Text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                listview.SelectedItems.Clear();
                                item.Focused = true;
                                item.Selected = true;
                                item.EnsureVisible();
                                break;
                            }
                        }
                    }
                    listview.EndUpdate();
                    break;

                case Keys.Escape:
                    e.SuppressKeyPress = true;
                    textbox.Visible = false;
                    break;
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                textbox.Visible = true;
                textbox.Focus();
            }
        }

        private void label_author_DoubleClick(object sender, EventArgs e)
        {
            Process.Start("https://gall.dcinside.com/m/list.php?id=vr");
        }
    }
}