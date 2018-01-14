using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms; 
using Drawing.ThemeRoutines;

namespace CustomControls
{
  public class CustomProgressBar : Control
  {
    protected double m_dblMinimum;
    protected double m_dblMaximum;
    protected double m_dblValue;

    protected bool m_bThemed;
    private UxThemeManager m_ThemeManager;

    public double Minimum
    {
      get { return m_dblMinimum; }
      set
      {
        if (m_dblMinimum != value)
        {
          m_dblMinimum = value;

          // ensure that our minimum is above 0, and that our minimum is also below our max
          m_dblMinimum = Math.Max(0, m_dblMinimum);
          m_dblMinimum = Math.Min(m_dblMinimum, m_dblMaximum);

          // ensure our value is within our bounds
          m_dblValue = Math.Max(m_dblMinimum, m_dblValue);

          // invalidate
          this.Invalidate();
        }
      }
    }

    public double Maximum
    {
      get { return m_dblMaximum; }
      set
      {
        if (m_dblMaximum != value)
        {
          m_dblMaximum = value;

          // ensure our max is above 0 then ensure that its above our minium
          m_dblMaximum = Math.Max(0, m_dblMaximum);
          m_dblMaximum = Math.Max(m_dblMinimum, m_dblMaximum);

          // ensure our value is within our bounds
          m_dblValue = Math.Min(m_dblMaximum, m_dblValue);

          // invalidate
          this.Invalidate();
        }
      }
    }

    public double Value
    {
      get { return m_dblValue; }
      set
      {
        if (m_dblValue != value)
        {
          m_dblValue = value;

          //ensure that our value is above or equal to our minimum and below or equal to our max
          m_dblValue = Math.Max(m_dblMinimum, m_dblValue);
          m_dblValue = Math.Min(m_dblMaximum, m_dblValue);

          // invalidate
          this.Invalidate();
        }
      }
    }

    public bool IsThemed
    {
      get { return m_bThemed; }
      set
      {
        if (m_bThemed != value)
        {
          m_bThemed = value;
          this.Invalidate();
        }
      }
    }

    public CustomProgressBar()
    {
      m_dblMaximum = 100;
      m_dblMinimum = 0;
      m_dblValue = 0;

      m_ThemeManager = new UxThemeManager(this);
      m_bThemed = true;

      SetStyle(
        ControlStyles.AllPaintingInWmPaint |
        ControlStyles.OptimizedDoubleBuffer |
        ControlStyles.UserPaint,
        true);
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

    private void DrawNonThemedBar(Graphics g)
    {
      int PenWidth = (int)Pens.White.Width;

      g.DrawLine(Pens.DarkGray,
        new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
        new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top));
      g.DrawLine(Pens.DarkGray,
        new Point(this.ClientRectangle.Left, this.ClientRectangle.Top),
        new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth));
      g.DrawLine(Pens.White,
        new Point(this.ClientRectangle.Left, this.ClientRectangle.Height - PenWidth),
        new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));
      g.DrawLine(Pens.White,
        new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Top),
        new Point(this.ClientRectangle.Width - PenWidth, this.ClientRectangle.Height - PenWidth));

      Brush b = new SolidBrush(this.ForeColor); // Create a brush that will draw the background of the Pbar
      // Create a linear gradient that will be drawn over the background. FromArgb means you can use the Alpha value wich is the transparency
      LinearGradientBrush lb = new LinearGradientBrush(new Rectangle(PenWidth, PenWidth, this.Width - (2 * PenWidth), this.Height - (2 * PenWidth)), Color.FromArgb(225, Color.White),
  Color.FromArgb(75, Color.Transparent), LinearGradientMode.Vertical);

      double dblPercent = (m_dblValue - m_dblMinimum) / (m_dblMaximum - m_dblMinimum);
      // Calculate how much has the Pbar to be filled for "x" %
      int width = (int)(dblPercent * (this.Width - (2 * PenWidth)));
      g.FillRectangle(b, PenWidth, PenWidth, width, this.Height - (2 * PenWidth));
      g.FillRectangle(lb, PenWidth, PenWidth, width, this.Height - (2 * PenWidth));
      b.Dispose();
      lb.Dispose();
    }

    private void DrawThemedBar(Graphics g)
    {
      IntPtr hdc = g.GetHdc();
      Rectangle clientRect = this.ClientRectangle;
      m_ThemeManager.DrawThemeBackground(UxThemeElements.PROGRESS, hdc, 1, 1, ref clientRect, ref clientRect);

      double dblPercent = (m_dblValue - m_dblMinimum) / (m_dblMaximum - m_dblMinimum);
      // Calculate how much has the Pbar to be filled for "x" %
      int width = (int)(dblPercent * (this.Width - 2));

      Rectangle chunkRect = new Rectangle(1, 1, width, this.ClientRectangle.Height - 2);
      m_ThemeManager.DrawThemeBackground(UxThemeElements.PROGRESS, hdc, 3, 1, ref chunkRect, ref chunkRect);
      g.ReleaseHdc(hdc);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      if (m_bThemed && UxThemeManager.VisualStylesEnabled())
        DrawThemedBar(e.Graphics);
      else
        DrawNonThemedBar(e.Graphics);

      base.OnPaint(e);
    }
  }
}
