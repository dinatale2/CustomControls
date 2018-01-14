using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CustomControls
{
  public partial class CustomBackground : Control
  {
    // helps determine the thickness of the gradient on the edges
    private int m_BorderWidth;
    public int BorderWidth
    {
      get { return m_BorderWidth; }
      set
      {
        // if the value is less than 15, then we default to 15, otherwise, take the value
        if (value < 15)
          m_BorderWidth = 15;
        else
          m_BorderWidth = value;

        // we have a new minimum size based on our current border size
        base.MinimumSize = new Size(2 * m_BorderWidth, 2 * m_BorderWidth);

        // force the redraw
        this.Invalidate();
      }
    }

    // color to make the "plate" in the background
    private Color m_DrawColor;
    public Color DrawColor
    {
      get { return m_DrawColor; }
      set
      {
        // take on the value, and force a redraw
        m_DrawColor = value;
        this.Invalidate();
      }
    }

    public CustomBackground()
      : base()
    {
      // we need to do all the drawing, redraw on resize, and double buffer to prevent flickering
      this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
      
      // default to our minimum border width
      m_BorderWidth = 15;

      // set the minimum size accordingly
      base.MinimumSize = new Size(2 * m_BorderWidth, 2 * m_BorderWidth);

      // start with white
      m_DrawColor = Color.White;
    }

    // allows for the adjustment of the brightness of a given color by some factor m
    private Color AdjustBrightness(Color color, double m)
    {
      // adjust the red, green, and blue by the given factor
      int r = (int)Math.Max(0, Math.Min(255, Math.Round((double)color.R * m)));
      int g = (int)Math.Max(0, Math.Min(255, Math.Round((double)color.G * m)));
      int b = (int)Math.Max(0, Math.Min(255, Math.Round((double)color.B * m)));

      return Color.FromArgb(r, g, b);
    }

    protected void InvalidateEx()
    {
      if (Parent == null)
        return;
      Rectangle rc = new Rectangle(this.Location, this.Size);
      Parent.Invalidate(rc, true);
    }

    protected override void OnResize(EventArgs e)
    {
      InvalidateEx();
      base.OnResize(e);
    }

    private GraphicsPath CreatePath(Rectangle r)
    {
      // start a new figure
      GraphicsPath path = new GraphicsPath();
      path.StartFigure();

      // left edge
      path.AddLine(r.Left, r.Bottom - m_BorderWidth, r.Left, m_BorderWidth);
      // top left corner
      path.AddArc(r.Left, r.Top, m_BorderWidth, m_BorderWidth, 180, 90);
      // top edge
      path.AddLine(r.Left + m_BorderWidth, r.Top, r.Right - m_BorderWidth, r.Top);
      // top right corner
      path.AddArc(r.Right - m_BorderWidth, r.Top, m_BorderWidth, m_BorderWidth, -90, 90);
      // right edge
      path.AddLine(r.Right, r.Top + m_BorderWidth, r.Right, r.Bottom - m_BorderWidth);
      // bottom right corner
      path.AddArc(r.Right - m_BorderWidth, r.Bottom - m_BorderWidth, m_BorderWidth, m_BorderWidth, 0, 90);
      // bottom edge
      path.AddLine(r.Right - m_BorderWidth, r.Bottom, m_BorderWidth, r.Bottom);
      // bottom left corner
      path.AddArc(r.Left, r.Bottom - m_BorderWidth, m_BorderWidth, m_BorderWidth, 90, 90);

      // close our figure and return
      path.CloseFigure();
      return path;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      // TODO: Determine a way to have this control draw more consistently, maybe use regions instead of using a path
      // Maybe use a path for the rounded corners and linear gradient for the middle.
      Graphics GFX = e.Graphics;

      // make the edges smooth
      GFX.SmoothingMode = SmoothingMode.AntiAlias;
      GFX.InterpolationMode = InterpolationMode.HighQualityBilinear;
      GFX.CompositingQuality = CompositingQuality.HighQuality;

      // Rectangle of the colored portion of the background, then create the path in that region
      Rectangle ColorRect = new Rectangle(10, 0, ClientRectangle.Width - 10, ClientRectangle.Height - 8);
      GraphicsPath ColoredBackDrop = CreatePath(ColorRect);

      // Rectangle of the shadow slightly offset from the colored portion, then create the same path in that rectangle
      Rectangle ShadowRect = new Rectangle(0, 8, ClientRectangle.Width - 8, ClientRectangle.Height - 8);
      GraphicsPath ShadowBack = CreatePath(ShadowRect);

      // Path gradient brush containing the shadow path
      Color ForShadow = Color.FromArgb(85, AdjustBrightness(Parent.BackColor, .65));
      PathGradientBrush ShadowPathGrad = new PathGradientBrush(ShadowBack);
      ShadowPathGrad.CenterColor = ForShadow;

      // gradient blend for shadow color to the parent's backcolor so we can get a "diffusion" at the edges of the shadow
      ColorBlend ToUseForShadow = new ColorBlend();
      ToUseForShadow.Colors = new Color[] { Parent.BackColor, ForShadow, ForShadow };
      ToUseForShadow.Positions = new float[] { 0.0F, .5F * (float)m_BorderWidth / (((float)ShadowRect.Width + (float)ShadowRect.Height) / 2), 1.0F };
      ShadowPathGrad.InterpolationColors = ToUseForShadow;

      // fill the shadow first, because the colored object will go over it
      GFX.FillPath(ShadowPathGrad, ShadowBack);

      // Gradient brush for the colored object
      PathGradientBrush PathGrad = new PathGradientBrush(ColoredBackDrop);
      PathGrad.CenterColor = m_DrawColor;

      // again a color blend, so that way the gradient shows up near the edges of the colored background
      ColorBlend ToUseForBack = new ColorBlend();
      ToUseForBack.Colors = new Color[] { AdjustBrightness(m_DrawColor, .8), m_DrawColor, m_DrawColor };
      ToUseForBack.Positions = new float[] { 0.0F, (float)m_BorderWidth / (((float)ColorRect.Width + (float)ColorRect.Height) / 2), 1.0F };
      PathGrad.InterpolationColors = ToUseForBack;

      // fill the colored path
      GFX.FillPath(PathGrad, ColoredBackDrop);

      // Dispose the path and brushes
      ColoredBackDrop.Dispose();
      ShadowBack.Dispose();
      PathGrad.Dispose();
      ShadowPathGrad.Dispose();

      // call the base paint
      base.OnPaint(e);
    }
  }
}
