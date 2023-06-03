using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace AM4Play
{
    [ToolboxBitmap(typeof(Label))]
    [Description("A simple but extremely fast control.\r\n " +
        "Believe it or not, a regular label isn't fast enough, even double-buffered.")]
    class FastLabel : Control
    {
        public FastLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.CacheText
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw
                   | ControlStyles.UserPaint
                   | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawString(Text, Font, new SolidBrush(ForeColor), ClientRectangle);
        }
    }
}