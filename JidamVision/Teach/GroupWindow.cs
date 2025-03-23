using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JidamVision.Teach
{
    public class GroupWindow : InspWindow
    {
        //#MODEL#1 InspStage에 있던 InspWindowList 위치를 이곳으로 변경
        [XmlElement("InspWindow")]
        public List<InspWindow> Members { get; private set; } = new List<InspWindow>();

        public GroupWindow(string groupId, string groupName)
            : base(Core.InspWindowType.Group, groupName)
        {
            UID = groupId;
            Name = groupName;
        }

        public void AddMember(InspWindow window)
        {
            if (window != null && !Members.Contains(window))
                Members.Add(window);
        }

        public void RemoveMember(InspWindow window)
        {
            if (window != null)
                Members.Remove(window);
        }

        public bool Contains(InspWindow window)
        {
            return Members.Contains(window);
        }

        public void MoveAllWindows(int offsetX, int offsetY)
        {
            foreach (var window in Members)
            {
                var area = window.WindowArea;
                area.X += offsetX;
                area.Y += offsetY;
                window.WindowArea = area;
            }
        }

        public Rectangle GetBoundingRect()
        {
            if (Members.Count == 0)
                return Rectangle.Empty;

            var rects = Members.Select(w => new Rectangle(w.WindowArea.X, w.WindowArea.Y, w.WindowArea.Width, w.WindowArea.Height));
            int minX = rects.Min(r => r.Left);
            int minY = rects.Min(r => r.Top);
            int maxX = rects.Max(r => r.Right);
            int maxY = rects.Max(r => r.Bottom);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public override bool DoInpsect(InspectType inspType)
        {
            foreach (var window in Members)
            {
                window.DoInpsect(inspType);
            }
            return true;
        }
    }
}
