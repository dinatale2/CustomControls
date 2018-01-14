using System.Windows.Forms;
using System;

namespace CustomControls
{
    public class TransparentHost : Form
    {
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            // Add the properties that cause a visual change here
            e.Control.TextChanged += onPropertyChange;
            e.Control.ForeColorChanged += onPropertyChange;
            e.Control.BackColorChanged += onPropertyChange;
        }

        private void onPropertyChange(object sender, EventArgs e)
        {
            if (!(sender is Control))
                return;

            Control control = (Control)sender;
            this.Invalidate(control.Bounds, true);
        }
    }
}
