using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VRCLogViewer
{
    public enum ActivityType
    {
        None,
        EnterWorld,
        PlayerJoined,
        PlayerLeft
    }

    public class ActivityInfo
    {
        public ActivityType Type = ActivityType.None;
        public string Time = string.Empty;
        public string Text = string.Empty;
        public string Tag = string.Empty;
    }

    //
    // LocationInfo
    //

    public class LocationInfo
    {
        public string WorldId = string.Empty;
        public string InstanceId = string.Empty;
        public string InstanceInfo = string.Empty;

        public static LocationInfo Parse(string idWithTags, bool strict = true)
        {
            // offline
            // private
            // local:0000000000
            // Public       wrld_785bee79-b83b-449c-a3d9-f1c5a29bcd3d:12502
            // Friends+     wrld_785bee79-b83b-449c-a3d9-f1c5a29bcd3d:12502~hidden(usr_4f76a584-9d4b-46f6-8209-8305eb683661)~nonce(79985ba6-8804-49dd-8c8a-c86fe817caca)
            // Friends      wrld_785bee79-b83b-449c-a3d9-f1c5a29bcd3d:12502~friends(usr_4f76a584-9d4b-46f6-8209-8305eb683661)~nonce(13374166-629e-4ac5-afe9-29637719d78c)
            // Invite+      wrld_785bee79-b83b-449c-a3d9-f1c5a29bcd3d:12502~private(usr_4f76a584-9d4b-46f6-8209-8305eb683661)~nonce(6d9b02ca-f32c-4360-b8ac-9996bf12fd74)~canRequestInvite
            // Invite       wrld_785bee79-b83b-449c-a3d9-f1c5a29bcd3d:12502~private(usr_4f76a584-9d4b-46f6-8209-8305eb683661)~nonce(5db0f688-4211-428b-83c5-91533e0a5d5d)
            // wrld_가 아니라 wld_인 것들도 있고 예전 맵들의 경우 아예 o_나 b_인것들도 있음; 그냥 uuid형태인 것들도 있고 개판임
            var tags = idWithTags.Split('~');
            var a = tags[0].Split(new char[] { ':' }, 2);
            if (!string.IsNullOrEmpty(a[0]))
            {
                if (a.Length == 2)
                {
                    if (!string.IsNullOrEmpty(a[1]) &&
                        !"local".Equals(a[0]))
                    {
                        var L = new LocationInfo
                        {
                            WorldId = a[0]
                        };
                        var type = "public";
                        if (tags.Length > 1)
                        {
                            var tag = "~" + string.Join("~", tags, 1, tags.Length - 1);
                            if (tag.Contains("~private("))
                            {
                                if (tag.Contains("~canRequestInvite"))
                                {
                                    type = "invite+"; // Invite Plus
                                }
                                else
                                {
                                    type = "invite"; // Invite Only
                                }
                            }
                            else if (tag.Contains("~friends("))
                            {
                                type = "friends"; // Friends Only
                            }
                            else if (tag.Contains("~hidden("))
                            {
                                type = "friends+"; // Friends of Guests
                            }
                            L.InstanceId = a[1] + tag;
                        }
                        else
                        {
                            L.InstanceId = a[1];
                        }
                        L.InstanceInfo = $"#{a[1]} {type}";
                        return L;
                    }
                }
                else if (!strict && !("offline".Equals(a[0]) || "private".Equals(a[0])))
                {
                    return new LocationInfo()
                    {
                        WorldId = a[0]
                    };
                }
            }
            return null;
        }
    }

    //
    // VRChatLog
    //

    public static class VRChatLog
    {
        public static string AccessTag { get; private set; } = string.Empty;
        public static string WorldName { get; private set; } = string.Empty;
        public static int InRoom { get { return m_InRoom.Count; } }

        private static long m_Position = 0;
        private static string m_FirstLog = string.Empty;
        private static string m_CurrentUser = string.Empty;
        private static string m_Location = string.Empty;
        private static string m_UserID = string.Empty;
        private static Dictionary<string, string> m_InRoom = new Dictionary<string, string>();

        public static void Update()
        {
            try
            {
                var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat");
                if (directory != null && directory.Exists)
                {
                    FileInfo target = null;
                    foreach (var info in directory.GetFiles("output_log_*.txt", SearchOption.TopDirectoryOnly))
                    {
                        if (target == null || info.LastAccessTime.CompareTo(target.LastAccessTime) >= 0)
                        {
                            target = info;
                        }
                    }
                    if (target != null)
                    {
                        using (var file = target.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var stream = new BufferedStream(file, 64 * 1024))
                            {
                                using (var reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    var line = string.Empty;
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line.Length > 34 &&
                                            line[20] == 'L' &&
                                            line[21] == 'o' &&
                                            line[22] == 'g' &&
                                            line[31] == '-')
                                        {
                                            var s = line.Substring(0, 19);
                                            if (m_FirstLog.Equals(s))
                                            {
                                                stream.Position = m_Position;
                                            }
                                            else
                                            {
                                                m_FirstLog = s;
                                            }
                                            Parse(line);
                                            break;
                                        }
                                    }
                                    while ((line = reader.ReadLine()) != null)
                                    {
                                        if (line.Length > 34 &&
                                            line[20] == 'L' &&
                                            line[21] == 'o' &&
                                            line[22] == 'g' &&
                                            line[31] == '-')
                                        {
                                            Parse(line);
                                        }
                                    }
                                    m_Position = stream.Position;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static void Parse(string line)
        {
            try
            {
                if (line[34] == '[')
                {
                    if (line[46] == ']')
                    {
                        var s = line.Substring(34);
                        if (s.StartsWith("[RoomManager] Joining "))
                        {
                            if (s.StartsWith("[RoomManager] Joining or Creating Room: "))
                            {
                                s = line.Substring(74);
                                if (!string.IsNullOrEmpty(m_Location))
                                {
                                    WorldName = s;
                                    var L = LocationInfo.Parse(m_Location);
                                    if (L != null)
                                    {
                                        AccessTag = L.InstanceInfo;
                                        s += " " + L.InstanceInfo;
                                    }
                                    else
                                    {
                                        AccessTag = string.Empty;
                                    }
                                    MainForm.Instance.SetActivity(new ActivityInfo
                                    {
                                        Type = ActivityType.EnterWorld,
                                        Time = line.Substring(11, 8),
                                        Text = s,
                                        Tag = m_Location
                                    });
                                    m_InRoom.Clear();
                                    m_Location = string.Empty;
                                }
                            }
                            else
                            {
                                m_Location = line.Substring(56);
                            }
                        }
                    }
                    else if (line[49] == ']')
                    {
                        var s = line.Substring(34);
                        if (s.StartsWith("[NetworkManager] OnPlayerLeft "))
                        {
                            s = line.Substring(64);
                            if (m_InRoom.TryGetValue(s, out string id))
                            {
                                MainForm.Instance.SetActivity(new ActivityInfo
                                {
                                    Type = ActivityType.PlayerLeft,
                                    Time = line.Substring(11, 8),
                                    Text = s + " has left",
                                    Tag = id
                                });
                                m_InRoom.Remove(s);
                            }
                        }
                    }
                    else if (line[52] == ']')
                    {
                        var s = line.Substring(34);
                        if (s.StartsWith("[VRCFlowManagerVRC] User Authenticated: "))
                        {
                            m_CurrentUser = line.Substring(74);
                        }
                    }
                }
                else if (line[34] == 'R')
                {
                    var s = line.Substring(34);
                    if (s.StartsWith("Received API user "))
                    {
                        m_UserID = line.Substring(52);
                    }
                }
                else if (line[34] == 'S')
                {
                    var s = line.Substring(34);
                    if (s.StartsWith("Switching "))
                    {
                        var i = s.IndexOf(" to avatar ");
                        if (i > 10)
                        {
                            s = s.Substring(10, i - 10);
                            if (!string.IsNullOrEmpty(m_UserID))
                            {
                                if (!m_InRoom.ContainsKey(s))
                                {
                                    MainForm.Instance.SetActivity(new ActivityInfo
                                    {
                                        Type = ActivityType.PlayerJoined,
                                        Time = line.Substring(11, 8),
                                        Text = s + " has joined",
                                        Tag = m_UserID
                                    });
                                }
                                m_InRoom[s] = m_UserID;
                                m_UserID = string.Empty;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}