using System.Drawing;
using System.Windows.Forms;

namespace CustomControls
{
    public class TransparentLabel : TransparentControl  // want a transparent background, so derive from transparent control
    {
        // the tool tip window associated with this label
        private CustomToolTip m_ToolTip;
        public CustomToolTip ToolTip
        {
            get { return m_ToolTip; }
        }

        // constructor
        public TransparentLabel() : base()
        {
            ForeColor = SystemColors.ControlText; // start with the default system controltext color
            AutoSize = true;  // use auto resize

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);

            // initialize our tooltip
            m_ToolTip = new CustomToolTip(this);
        }

        protected override void DrawControl(Graphics GFX)
        {
            Brush stringBrsh = new SolidBrush(base.ForeColor);

            // if we are using autosize
            if (AutoSize)
            {
                // draw the string
                GFX.DrawString(base.Text, base.Font, stringBrsh, 0, 0);
            }
            else
            {
                // otherwise, we want an ellipsis to show up and attempt to fit the string in our box
                StringFormat sf = new StringFormat();
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.FitBlackBox;

                // draw the text within the bounds of the controls client rectangle
                GFX.DrawString(base.Text, base.Font, stringBrsh, new RectangleF(0, 0, base.Width, base.Height), sf);
            }

            // dispose of the text brush
            stringBrsh.Dispose();
        }

        protected override void WndProc(ref Message m)
        {
            // have the tooltip handle messages
            if (m_ToolTip != null)
                m_ToolTip.HandleMessage(ref m);

            // call the standard wndproc
            base.WndProc(ref m);
        }
    }
}
