using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace AM4Player
{
    [ToolboxBitmap(typeof(Label))]
    [Description("Um custom windows forms que aparece 3 flags utilizados na emulação do " +
        "SPC. São eles Echo, Pitch Modulation e Noise")]
    public class DSPFrags : Control
    {
        public DSPFrags()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint
                   | ControlStyles.SupportsTransparentBackColor, true);


            this.BackColor = Color.Transparent;

            this.font = new Font("Lucida Console", 8, FontStyle.Bold);
            this.font2 = new Font("Lucida Console", 9);
            this.size1 = (float)(8.0 * 96.0 / 72.0 / 2.0 + 1.5);
            this.size2 = size1 * 2F + 1.05f;
            this.size3 = size1 * 3F + 1.15f;
            this.chn = "0";
            ++size1;

            // points = pixel * 72 / 96
            // x = y * 72 / 96
            // x = 72y
            //    -----
            //     96
            // 96x = 72y
            // 96x
            // --- = y
            // 72
            // pixels = points * 96 / 2
        }

        Font font, font2;
        float size1, size2, size3;
        string chn;

        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        public int Channel
        {
            get
            {
                return Convert.ToInt32(chn);
            }
            set
            {
                chn = value.ToString();
            }
        }

        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [Description("Especifica se o Echo Flag ficará ativo ou não.")]
        public bool Echo
        {
            get
            {
                return _e == "E";
            }

            set
            {
                if (_e == "E" && value == false)
                {
                    Invalidate();
                }

                if (_e == "-" && value == true)
                {
                    Invalidate();
                }

                if (value)
                {
                    _e = "E";
                    __e = Color.Green;
                }

                else
                {
                    _e = "-";
                    __e = SystemColors.ControlText;
                }
            }
        }

        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [Description("Especifica se o Pitch Modulation Flag ficará ativo ou não.")]
        public bool PMON
        {
            get
            {
                return _p == "P";
            }

            set
            {
                if (_p == "P" && value == false)
                {
                    Invalidate();
                }

                if (_p == "-" && value == true)
                {
                    Invalidate();
                }

                if (value)
                {
                    _p = "P";
                    __p = Color.SteelBlue;
                }

                else
                {
                    _p = "-";
                    __p = SystemColors.ControlText;
                }
            }
        }

        [RefreshProperties(System.ComponentModel.RefreshProperties.All)]
        [Description("Especifica se o Noise Flag ficará ativo ou não.")]
        public bool Noise
        {
            get
            {
                return _n == "N";
            }

            set
            {
                if (_n == "N" && value == false)
                {
                    Invalidate();
                }

                if (_n == "-" && value == true)
                {
                    Invalidate();
                }

                if (value)
                {
                    _n = "N";
                    __n = Color.Chocolate;
                }

                else
                {
                    _n = "-";
                    __n = SystemColors.ControlText;
                }
            }
        }

        string _e = "-";
        string _p = "-";
        string _n = "-";

        Color __e = SystemColors.ControlText;
        Color __p = SystemColors.ControlText;
        Color __n = SystemColors.ControlText;

        protected override void OnPaint(PaintEventArgs e)
        {

            Rectangle cr = ClientRectangle;
            RectangleF cr1 = new RectangleF(cr.X - 2.5f, cr.Y, cr.Width, cr.Height);
            RectangleF cr2 = new RectangleF(cr.X + size1 - 2.5f, cr.Y + 0.75f, cr.Width, cr.Height);
            RectangleF cr3 = new RectangleF(cr.X + size2 - 2.5f, cr.Y + 0.75f, cr.Width, cr.Height);
            RectangleF cr4 = new RectangleF(cr.X + size3 - 2.5f, cr.Y+0.75f, cr.Width, cr.Height);

            e.Graphics.DrawString(chn, font2, SystemBrushes.ControlText, cr1);
            e.Graphics.DrawString(_e, font, new SolidBrush(__e), cr2);
            e.Graphics.DrawString(_p, font, new SolidBrush(__p), cr3);
            e.Graphics.DrawString(_n, font, new SolidBrush(__n), cr4);
        }
    }
}