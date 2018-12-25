using System.Reflection;
using System.Windows.Forms;

namespace VRCLogViewer
{
    public class VRCLocationInfo
    {
        public string WorldId = string.Empty;
        public string InstanceId = string.Empty;
        public string InstanceInfo = string.Empty;
    };

    public static class Utils
    {
        public static void SetDoubleBuffering(Control control)
        {
            var type = control.GetType();
            type.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(control, true, null);
            type.GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true });
        }

        public static bool ParseLocation(string idWithTags, out VRCLocationInfo info, bool strict = true)
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
            info = new VRCLocationInfo();
            var tags = idWithTags.Split('~');
            var a = tags[0].Split(new char[] { ':' }, 2);
            if (!string.IsNullOrEmpty(a[0]))
            {
                if (a.Length == 2)
                {
                    if (!string.IsNullOrEmpty(a[1]) &&
                        !"local".Equals(a[0]))
                    {
                        var type = "public";
                        info.WorldId = a[0];
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
                            info.InstanceId = a[1] + tag;
                        }
                        else
                        {
                            info.InstanceId = a[1];
                        }
                        info.InstanceInfo = $"#{a[1]} {type}";
                        return true;
                    }
                }
                else if (!strict && !("offline".Equals(a[0]) || "private".Equals(a[0])))
                {
                    info.WorldId = a[0];
                    return true;
                }
            }
            return false;
        }
    }
}