using System;
using System.Windows.Forms;
using System.Drawing;
using System.Text;

namespace CustomControls
{
  // TODO: Need to add presets for things like mac os and firefox load wheels
  public class LoadingCircle : Control
  {
    private Timer m_AnimationTimer;
    private Color m_Color;
    private int m_SpokeCount;
    private int m_InnerCircleRadius;
    private int m_OuterCircleRadius;
    private int m_SpokeThickness;
    private int m_Progress;
    private int m_MinimumAlpha;
    private Color[] m_Colors;
    private double[] m_Angles;

    private const double dblDegreeCircle = 360;
    private const double dblDegreeHalfCircle = dblDegreeCircle / 2;

    public Color SpokeColor
    {
      get { return m_Color; }
      set
      {
        if (m_Color != value)
        {
          m_Color = value;
          m_Colors = GetSpokeColors(m_Color, m_SpokeCount);
          Invalidate();
        }
      }
    }

    public int SpokeCount
    {
      get { return m_SpokeCount; }
      set
      {
        int nSpokeCount = Math.Max(value, 2);
        if (m_SpokeCount != nSpokeCount)
        {
          m_SpokeCount = nSpokeCount;
          m_Angles = GetAngles(m_SpokeCount);
          m_Colors = GetSpokeColors(m_Color, m_SpokeCount);
          Invalidate();
        }
      }
    }

    public int InnerCircleRadius
    {
      get { return m_InnerCircleRadius; }
      set
      {
        m_InnerCircleRadius = Math.Max(0, value);
        Invalidate();
      }
    }

    public int OuterCircleRadius
    {
      get { return m_OuterCircleRadius; }
      set
      {
        m_OuterCircleRadius = Math.Max(1, value);
        Invalidate();
      }
    }

    public int SpokeThickness
    {
      get { return m_SpokeThickness; }
      set
      {
        m_SpokeThickness = Math.Max(1, value);
        Invalidate();
      }
    }

    public int RotationSpeed
    {
      get { return m_AnimationTimer.Interval; }
      set
      {
        if (value > 0)
        {
          m_AnimationTimer.Interval = value;
        }
      }
    }

    public int MinimumAlpha
    {
      get { return m_MinimumAlpha; }
      set
      {
        int nMinAlpha = Math.Min(value, 255);
        nMinAlpha = Math.Max(nMinAlpha, 0);
        if (m_MinimumAlpha != nMinAlpha)
        {
          m_MinimumAlpha = nMinAlpha;
          m_Colors = GetSpokeColors(m_Color, m_SpokeCount);
          Invalidate();
        }
      }
    }

    public LoadingCircle()
      : base()
    {
      m_AnimationTimer = new Timer();
      m_Color = Color.DarkGray;
      m_InnerCircleRadius = 6;
      m_OuterCircleRadius = 11;
      m_SpokeCount = 12;
      m_SpokeThickness = 2;
      m_Progress = 0;
      m_MinimumAlpha = 75;

      m_Angles = GetAngles(m_SpokeCount);
      m_Colors = GetSpokeColors(m_Color, m_SpokeCount); 
      m_AnimationTimer.Tick += new EventHandler(m_AnimationTimer_Tick);

      SetStyle(
        ControlStyles.AllPaintingInWmPaint |
        ControlStyles.OptimizedDoubleBuffer |
        ControlStyles.UserPaint,
        true
      );
    }

    void m_AnimationTimer_Tick(object sender, EventArgs e)
    {
      m_Progress = ++m_Progress % m_SpokeCount;
      Invalidate();
    }

    public void ToggleTimer()
    {
      if (m_AnimationTimer.Enabled)
      {
        m_AnimationTimer.Stop();
      }
      else
      {
        m_Progress = 0;
        m_AnimationTimer.Start();
      }

      Invalidate();
    }

    private double[] GetAngles(int nNumSpokes)
    {
      double[] Angles = new double[nNumSpokes];
      double dblAngle = dblDegreeCircle / nNumSpokes;

      for (int i = nNumSpokes - 1; i >= 0; i--)
        Angles[i] = (i == (nNumSpokes - 1)) ? 0 : Angles[i + 1] + dblAngle;

      return Angles;
    }

    private Color[] GetSpokeColors(Color clr, int nNumSpokes)
    {
      byte byteIncrement = (byte)(byte.MaxValue / m_SpokeCount);
      int nCurrDarkness = byte.MaxValue;
      Color[] newClrs = new Color[nNumSpokes];

      for (int i = 0; i < nNumSpokes; i++)
      {
        nCurrDarkness = i * byteIncrement;
        nCurrDarkness %= byte.MaxValue;
        nCurrDarkness = Math.Max(nCurrDarkness, m_MinimumAlpha);
        newClrs[i] = Color.FromArgb(nCurrDarkness, m_Color);
      }

      return newClrs;
    }

    private PointF GetCenter()
    {
      return new PointF(this.Width / 2, this.Height / 2 - 1);
    }

    private PointF GenerateCoordinate(PointF ptCircleCenter, int nRadius, double dblAngle)
    {
      double dblNewAngle = Math.PI * dblAngle / dblDegreeHalfCircle;
      return new PointF(ptCircleCenter.X + (float)Math.Sin(dblNewAngle) * nRadius, ptCircleCenter.Y + (float)Math.Cos(dblNewAngle) * nRadius);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);

      if (m_SpokeCount > 0)
      {
        PointF ptCenter = GetCenter();
        Graphics gfx = e.Graphics;
        gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        int nPosition = m_Progress;
        for (int i = 0; i < m_SpokeCount; i++, nPosition++)
        {
          using (Pen p = new Pen(m_AnimationTimer.Enabled ? m_Colors[i] : m_Color, m_SpokeThickness))
          {
            p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            p.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            nPosition = nPosition % m_SpokeCount;
            gfx.DrawLine(p,
              GenerateCoordinate(ptCenter, m_InnerCircleRadius, m_Angles[nPosition]),
              GenerateCoordinate(ptCenter, m_OuterCircleRadius, m_Angles[nPosition]));
          }
        }
      }
    }
  }
}
