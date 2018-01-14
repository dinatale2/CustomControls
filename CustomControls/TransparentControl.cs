using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Win32Lib;

namespace CustomControls
{
    public abstract class TransparentControl : Control
    {
        // overriding the base controls createparams
        protected override CreateParams CreateParams
        {
            get
            {
                // if we are not in design mode, just append the Transparent ExStyle and return it
                CreateParams cp = base.CreateParams;

                if (!DesignMode)
                    cp.ExStyle |= (int)WindowStylesEx.WS_EX_TRANSPARENT;

                return cp;
            }
        }

        // constructor
        public TransparentControl()
          : base()
        {
            SetStyle(ControlStyles.Opaque, true);
        }

        // override the paintbackground to have it paint nothing
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Don't paint background if not design mode
        }

        // any control derived from this MUST implement it, therefore, make it abstract.
        protected abstract void DrawControl(Graphics GFX);

        // the paint handle will simply call DrawControl so any control derived, will just have to implement that
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            DrawControl(e.Graphics);
        }

    }
}
