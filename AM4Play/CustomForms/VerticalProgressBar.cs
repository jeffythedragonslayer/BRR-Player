using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace AM4Play
{
    [ToolboxBitmap(typeof(ProgressBar))]
    public class VerticalProgressBar : Control
    {
        public VerticalProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.SupportsTransparentBackColor, true);
        }

        public int MinValue
        {
            get { return _min; }
            set
            {
                if (_min == _max) return;
                if (_max < _min) return;

                _min = value;
                Invalidate();
            }
        }

        public int MaxValue
        {
            get { return _max; }
            set
            {
                if (_min == _max) return;
                if (_max < _min) return;
                
                _max = value;
                Invalidate();
            }
        }

        public int Value
        {
            get { return _value; }

            set
            {
                if (value < MinValue) value = MinValue;
                if (value > MaxValue) value = MaxValue;

                if (value != _value)
                {
                    _value = value;
                    Invalidate();
                }
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }

            set
            {
                if (_color.ToArgb() != value.ToArgb())
                {
                    _color = value;
                    RedrawColorBuffer();
                    Invalidate();
                }
            }
        }

        public Color EndColor
        {
            get
            {
                return _ecolor;
            }

            set
            {
                if (_ecolor.ToArgb() != value.ToArgb())
                {
                    _ecolor = value;
                    RedrawColorBuffer();
                    Invalidate();
                }
            }
        }

		public bool NotGrayed
		{
			get
			{
				return nograyed;
			}
			set
			{
                if (nograyed != value)
                {
                    nograyed = value;
                    RedrawColorBuffer();
                    Invalidate();
                }
			}
		}

        public bool InvertColors
        {
            get
            {
                return invert;
            }
            set
            {
                if (invert != value)
                {
                    invert = value;
                    RedrawColorBuffer();
                    Invalidate();
                }
            }
        }

        public Color XORColor
        {
            get
            {
                return xorColor;
            }
            set
            {
                if (value.ToArgb() != xorColor.ToArgb())
                {
                    xorColor = value;
                }
            }
        }

        public Color XOREndColor
        {
            get
            {
                return xorEndColor;
            }
            set
            {
                if (value.ToArgb() != xorEndColor.ToArgb())
                {
                    xorEndColor = value;
                }
            }
        }



        // ColorBuffer need to be redrawn if
        // Color, EndColor, NotGrayed and InvertColors is changed,
        // otherwise, not need.

        // Valores
        int _value = 50;
        int _min = 0;
        int _max = 100;

        // Cores
        Color _color = Color.Gray;
        Color _ecolor = Color.Gray;

        Color xorColor = Color.Gray;
        Color xorEndColor = Color.Gray;

        // bools
        bool nograyed = true;
        bool invert = false;

        // buffer
        Bitmap colorBuffer;

        private void RedrawColorBuffer()
        {
            if (colorBuffer != null)
            {
                colorBuffer.Dispose();
                colorBuffer = null;
            }

            int width = ClientSize.Width;
            int height = ClientSize.Height;
            Color startColor = Color;
            Color endColor = EndColor;

            if (width == 0 || height == 0)
            {
                return;
            }

            if (invert)
            {
                startColor = xorColor;
                endColor = xorEndColor;
            }

            if (!nograyed)
            {
                int gray = (int)(startColor.R * 0.3 + startColor.G * 0.59 + startColor.B * 0.11);
                int grayEnd = (int)(endColor.R * 0.3 + endColor.G * 0.59 + endColor.B * 0.11);
                startColor = Color.FromArgb(gray, gray, gray);
                endColor = Color.FromArgb(grayEnd, grayEnd, grayEnd);
            }

            colorBuffer = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            using (Graphics g = Graphics.FromImage(colorBuffer))
            {
                using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0, 0, width, height), startColor, endColor, 90))
                {
                    using (Pen p = new Pen(b, width * 2))
                    {
                        g.DrawLine(p, 0, 0, 0, height);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            float position = (MaxValue - Value) * ((float)Height / MaxValue);

            if (colorBuffer == null)
            {
                RedrawColorBuffer();

                if (colorBuffer == null)
                {
                    return;
                }
            }

            e.Graphics.DrawImage(colorBuffer, 0, position, ClientSize.Width, ClientSize.Height - position);
        }
    }
}
