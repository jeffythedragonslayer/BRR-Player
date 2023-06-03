using System.Drawing;
using System.Windows.Forms;

namespace AM4Play
{
    class VerticalLine : Control
    {
        public VerticalLine()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor
                | ControlStyles.UserPaint
                | ControlStyles.AllPaintingInWmPaint, true);
        }

        Color color = Color.Gray;

        public override Color ForeColor
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(color), ClientRectangle);
        }
    }
}
