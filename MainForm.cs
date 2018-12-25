using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace VRCLogViewer
{
    public partial class MainForm : Form
    {
        private DateTime next_ = DateTime.MinValue;
        private string created_ = string.Empty;
        private long position_ = 0;
        private Dictionary<string, string> info_ = new Dictionary<string, string>();
        private string last_world_ = string.Empty;
        private string last_user_ = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Utils.SetDoubleBuffering(listview);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            checkbox.Checked = true;
        }

        private void FetchLog()
        {
            listview.BeginUpdate();
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat\output_log.txt";
            if (File.Exists(path))
            {
                try
                {
                    using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (var stream = new BufferedStream(file, 64 * 1024))
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                var diff = true;
                                var line = string.Empty;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.Length > 34 &&
                                        line[20] == 'L' &&
                                        line[21] == 'o' &&
                                        line[22] == 'g')
                                    {
                                        if (diff)
                                        {
                                            diff = false;
                                            var s = line.Substring(0, 19);
                                            if (created_.Equals(s))
                                            {
                                                stream.Position = position_;
                                            }
                                            else
                                            {
                                                created_ = s;
                                            }
                                        }
                                        if (line[34] == '[')
                                        {
                                            var log = line.Substring(34);
                                            if (log.StartsWith("[VRCPlayer] "))
                                            {
                                                if (log.StartsWith("[VRCPlayer] Switching "))
                                                {
                                                    var s = line.Substring(56);
                                                    var i = s.IndexOf(" to avatar ");
                                                    if (i > 0)
                                                    {
                                                        s = s.Substring(0, i);
                                                        if (!string.IsNullOrEmpty(last_user_))
                                                        {
                                                            info_["user," + s] = last_user_;
                                                            last_user_ = string.Empty;
                                                        }
                                                    }
                                                }
                                                else if (log.StartsWith("[VRCPlayer] Received API user "))
                                                {
                                                    last_user_ = line.Substring(64);
                                                }
                                            }
                                            else if (log.StartsWith("[NetworkManager] OnPlayer"))
                                            {
                                                if (log.StartsWith("[NetworkManager] OnPlayerJoined "))
                                                {
                                                    var s = line.Substring(66);
                                                    var item = new ListViewItem
                                                    {
                                                        Tag = "user," + s,
                                                        Text = line.Substring(5, 11),
                                                        StateImageIndex = 1,
                                                        UseItemStyleForSubItems = false
                                                    };
                                                    item.SubItems.Add(new ListViewItem.ListViewSubItem
                                                    {
                                                        Text = s + " has joined",
                                                        ForeColor = Color.RoyalBlue
                                                    });
                                                    listview.Items.Add(item);
                                                    item.EnsureVisible();
                                                }
                                                else if (log.StartsWith("[NetworkManager] OnPlayerLeft "))
                                                {
                                                    var s = line.Substring(64);
                                                    var item = new ListViewItem
                                                    {
                                                        Tag = "user," + s,
                                                        Text = line.Substring(5, 11),
                                                        StateImageIndex = 2,
                                                        UseItemStyleForSubItems = false
                                                    };
                                                    item.SubItems.Add(new ListViewItem.ListViewSubItem
                                                    {
                                                        Text = s + " has left",
                                                        ForeColor = Color.Gray
                                                    });
                                                    listview.Items.Add(item);
                                                    item.EnsureVisible();
                                                }
                                            }
                                            else if (log.StartsWith("[RoomManager] Joining "))
                                            {
                                                if (log.StartsWith("[RoomManager] Joining or Creating Room: "))
                                                {
                                                    VRCLocationInfo l = null;
                                                    var s = line.Substring(74);
                                                    if (!string.IsNullOrEmpty(last_world_))
                                                    {
                                                        Utils.ParseLocation(last_world_, out l);
                                                        info_["world," + s] = last_world_;
                                                        last_world_ = string.Empty;
                                                    }
                                                    var item = new ListViewItem
                                                    {
                                                        Tag = "world," + s,
                                                        Text = line.Substring(5, 11),
                                                        StateImageIndex = 0,
                                                        UseItemStyleForSubItems = false,
                                                        BackColor = Color.Ivory
                                                    };
                                                    if (l != null)
                                                    {
                                                        s += " " + l.InstanceInfo;
                                                    }
                                                    item.SubItems.Add(new ListViewItem.ListViewSubItem
                                                    {
                                                        Text = s,
                                                        ForeColor = Color.DeepPink,
                                                        BackColor = Color.Ivory
                                                    });
                                                    item.SubItems.Add(s);
                                                    listview.Items.Add(item);
                                                    item.EnsureVisible();
                                                }
                                                else
                                                {
                                                    last_world_ = line.Substring(56);
                                                }
                                            }
                                        }
                                    }
                                }
                                position_ = stream.Position;
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            listview.EndUpdate();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (checkbox.Checked)
            {
                if (DateTime.Now.CompareTo(next_) >= 0)
                {
                    checkbox.Text = "Updating...";
                    FetchLog();
                    next_ = DateTime.Now.AddSeconds(9.5);
                }
                else
                {
                    checkbox.Text = "Update in " + ((next_.Ticks - DateTime.Now.Ticks) / 10000000) + " second(s)";
                }
            }
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
                            if (info_.TryGetValue(tag, out string id))
                            {
                                Process.Start("https://vrchat.net/home/user/" + id);
                            }
                            return;

                        case "world":
                            if (info_.TryGetValue(tag, out string location))
                            {
                                var arg = location.Split(new[] { ':' }, 2);
                                if (arg.Length == 2)
                                {
                                    Process.Start("https://vrchat.net/home/launch?worldId=" + arg[0] + "&instanceId=" + arg[1]);
                                }
                                else
                                {
                                    Process.Start("https://vrchat.net/home/launch?worldId=" + arg[0]);
                                }
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