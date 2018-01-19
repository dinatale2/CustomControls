using System.Drawing;
using System.Windows.Forms;
using System;
using Drawing.ThemeRoutines;
using System.ComponentModel;

namespace SQL_QueryRunner
{
    public class CustomTabControl : TabControl
    {
        private UxThemeManager m_ThemeManager;

        // TODO: Implement proper sizing of uxtheme parts (GetThemeSize)

        public event CancelEventHandler TabClosing;

        public CustomTabControl() : base()
        {
            m_ThemeManager = new UxThemeManager(this);
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.IsDisposed)
                {
                    m_ThemeManager.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        private Rectangle GetCloseBtnRect(int index)
        {
            return GetCloseBtnRect(GetTabRect(index));
        }

        private Rectangle GetCloseBtnRect(Rectangle rTab)
        {
            return new Rectangle(new Point(rTab.Right - 13 - 2, rTab.Bottom - 13 - ((rTab.Height - 13) / 2)), new Size(13, 13));
        }

        private int MouseDownIndex = -1;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (GetCloseBtnRect(SelectedIndex).Contains(e.Location))
            {
                MouseDownIndex = SelectedIndex;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseDownIndex < 0)
                return;

            Invalidate(GetCloseBtnRect(MouseDownIndex));
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (MouseDownIndex < 0)
                return;

            if (GetCloseBtnRect(MouseDownIndex).Contains(e.Location))
            {
                CancelEventArgs args = new CancelEventArgs(false);
                TabClosing?.Invoke(this, args);

                if (!args.Cancel)
                    TabPages.RemoveAt(MouseDownIndex);
            }

            MouseDownIndex = -1;
            Invalidate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            Rectangle rTab = this.GetTabRect(e.Index);
            Rectangle rClose = GetCloseBtnRect(rTab);
            Rectangle rText = new Rectangle(new Point(rTab.Left + 2, rTab.Top + 2), new Size(rTab.Width - 13 - 6, rTab.Height - 4));

            if (e.Graphics.ClipBounds.Contains(rClose))
            {
                IntPtr pHdc = e.Graphics.GetHdc();
                Point p = PointToClient(Control.MousePosition);
                if (e.Index == MouseDownIndex)
                {
                    if (rClose.Contains(p))
                        m_ThemeManager.DrawThemeBackground(
                            UxThemeElements.WINDOW, pHdc, (int)WindowParts.SmallCloseButton,
                            (int)CloseButtonState.Hot, ref rClose, IntPtr.Zero);
                    else
                        m_ThemeManager.DrawThemeBackground(
                            UxThemeElements.WINDOW, pHdc, (int)WindowParts.SmallCloseButton,
                            (int)CloseButtonState.Pressed, ref rClose, IntPtr.Zero);
                }
                else
                {
                    if (rClose.Contains(p))
                        m_ThemeManager.DrawThemeBackground(
                            UxThemeElements.WINDOW, pHdc, (int)WindowParts.SmallCloseButton,
                            (int)CloseButtonState.Hot, ref rClose, IntPtr.Zero);
                    else
                        m_ThemeManager.DrawThemeBackground(
                            UxThemeElements.WINDOW, pHdc, (int)WindowParts.SmallCloseButton,
                            (int)CloseButtonState.Normal, ref rClose, IntPtr.Zero);
                }
                e.Graphics.ReleaseHdc(pHdc);
            }

            if (e.Graphics.ClipBounds.Contains(rText))
            {
                StringFormat sf = new StringFormat();
                sf.FormatFlags |= StringFormatFlags.NoWrap;
                sf.Trimming |= StringTrimming.EllipsisCharacter;
                e.Graphics.DrawString(TabPages[e.Index].Text, Font, new SolidBrush(DefaultForeColor), rText, sf);
            }

#if DEBUG
            e.Graphics.FillRectangle(Brushes.Green, rClose);
            e.Graphics.FillRectangle(Brushes.Red, rText);
#endif
        }
    }
}