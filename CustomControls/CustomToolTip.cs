using System;
using Win32Lib;
using Drawing.ThemeRoutines;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CustomControls
{
  // Describes the possible tooltip styles available
  public enum ToolStyle
  {
    UxTheme,
    Flat,
  }

  // internal interface for a class providing a wndproc
  internal interface IWndProcProvider
  {
    void WndProc(ref Message msg, ToolTipNativeWindow wnd);
  }

  // internal class to handle the native window and set its wndproc provider
  internal class ToolTipNativeWindow : NativeWindow
  {
    IWndProcProvider m_WndProcProv; // object providing the wndproc

    internal ToolTipNativeWindow(IWndProcProvider prov)
      : base()
    {
      m_WndProcProv = prov;
    }

    protected override void WndProc(ref Message m)
    {
      m_WndProcProv.WndProc(ref m, this);
    }
  }

  public class CustomToolTip : Component, IWndProcProvider
  {
    private int BORDERWIDTH = 1;  // constant border width
    private int PADDING = 4;  // how much padding we want on between text and the edges

    private IntPtr m_OpenToolTipTheme;  // cached theme ptr
    private Rectangle m_ClientRectangle;  // the bounds of our tooltip window
    private CreateParams m_CreateParams;  // the styles, etc. of the tooltip window

    // tooltip's text
    private string m_Text;
    public string Text
    {
      get { return m_Text; }
      set
      {
        if (value != m_Text)
          SetText(value);
      }
    }

    // the font for the control
    private Font m_Font;
    public Font Font
    {
      get { return m_Font; }
      set { SetFont(value); }
    }

    // tooltip's current style
    private ToolStyle m_Style;
    public ToolStyle Style
    {
      get { return m_Style; }
      set { m_Style = value; }
    }

    // control the tool is attached to
    private Control m_Parent;
    public Control Parent
    {
      get { return m_Parent; }
      set { m_Parent = value; }
    }

    // tooltip's backcolor (if running flat theme)
    private Color m_BackColor;
    public Color BackColor
    {
      get { return m_BackColor; }
      set { m_BackColor = value; }
    }

    // tooltip's forecolor
    private Color m_ForeColor;
    public Color ForeColor
    {
      get { return m_ForeColor; }
      set { m_ForeColor = value; }
    }

    // tooltips's border color (if running flat theme)
    private Color m_BorderColor;
    public Color BorderColor
    {
      get { return m_BorderColor; }
      set { m_BorderColor = value; }
    }

    // the maximum width of the tooltip
    private int m_MaxWidth;
    public int MaxWidth
    {
      get { return m_MaxWidth; }
      set { m_MaxWidth = value; }
    }

    // determines if the tooltip follows the mouse until it leaves the control
    private bool m_MouseTrack;
    public bool MouseTrack
    {
      get { return m_MouseTrack; }
      set { m_MouseTrack = value; }
    }

    // how long it takes for the control to show itself (with a minimum of 100)
    public int ShowDelay
    {
      get { return m_ShowTimer.Interval; }
      set
      {
        if (value >= 100)
          m_ShowTimer.Interval = value;
        else
          m_ShowTimer.Interval = 100;
      }
    }

    // returns whether the tooltip is visible
    private bool m_Visible;
    public bool Visible
    {
      get { return m_Visible; }
    }

    private ToolTipNativeWindow m_ToolTipWnd; // the tooltip's native window
    private Timer m_ShowTimer;  // timer to determine when to display
    private Point m_LastCursorPos;  // the position collected from the last wm_mousemove message

    private bool m_bDisposed; // whether the control has been disposed
    public bool IsDisposed
    {
      get { return m_bDisposed; }
    }

    [DllImport("uxtheme.dll", ExactSpelling = true)]
    private extern static Int32 CloseThemeData(IntPtr hTheme);

    [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
    private extern static IntPtr OpenThemeData(IntPtr hWnd, string classList);

    [DllImport("uxtheme.dll", ExactSpelling = true)]
    private extern static int IsThemeActive();

    // constructor
    public CustomToolTip(Control parent)
    {
      m_bDisposed = false;

      m_ToolTipWnd = new ToolTipNativeWindow(this);

      m_Parent = parent;
      m_Font = m_Parent.Font;
      m_Visible = false;

      m_Text = "";

      m_BackColor = SystemColors.Info;
      m_BorderColor = SystemColors.ActiveBorder;
      m_ForeColor = SystemColors.InfoText;
      m_Style = ToolStyle.Flat;

      m_ShowTimer = new Timer();
      m_ShowTimer.Stop();
      m_ShowTimer.Interval = 2000;

      m_MaxWidth = 500;

      CreateToolTipWnd();

      m_ShowTimer.Tick += new EventHandler(m_ShowTimer_Tick);
    }

    ~CustomToolTip()
    {
      Dispose(false);
    }

    protected override void Dispose(bool disposing)
    {
      // if not disposed
      if (!m_bDisposed)
      {
        // if disposing
        if (disposing)
        {
          // if we have a theme open, close it
          if (m_OpenToolTipTheme != IntPtr.Zero)
          {
            // TODO: Determine why ThemeRoutines.CloseThemeData cannot be used
            CloseThemeData(m_OpenToolTipTheme);
          }

          // dispose our timer and remove the reference to the parent
          m_ShowTimer.Dispose();
          m_Parent = null;

          // destroy the tooltip's window handle
          if (m_ToolTipWnd != null)
            m_ToolTipWnd.DestroyHandle();

          // we have now disposed the object
          m_bDisposed = true;
        }
      }
      
      // let the base object handle its disposal
      base.Dispose(disposing);
    }

    public new void Dispose()
    {
      // we are now disposing for real
      Dispose(true);

      // suppress garbage collection
      GC.SuppressFinalize(this);
    }

    private void CreateToolTipWnd()
    {
      // set up the windows style creation
      m_CreateParams = new CreateParams();
      m_CreateParams.ClassStyle = (int)ClassStyles.OwnDC | (int)ClassStyles.VerticalRedraw
        | (int)ClassStyles.HorizontalRedraw | (int)ClassStyles.DropShadow;
      m_CreateParams.Caption = null;
      m_CreateParams.ExStyle = (int)WindowStylesEx.WS_EX_TOPMOST | (int) WindowStylesEx.WS_EX_NOACTIVATE;
      m_CreateParams.Parent = m_Parent.Handle;
      m_CreateParams.Height = 0;
      m_CreateParams.Width = 0;
      unchecked { m_CreateParams.Style = (int)WindowStyles.WS_POPUP; }
    }

    void IWndProcProvider.WndProc(ref Message msg, ToolTipNativeWindow wnd)
    {
      switch ((WindowsMessages)msg.Msg)
      {
        case WindowsMessages.WM_PAINT:
          // paint if we get this message
          PaintToolTip(ref msg);
          break;
        default:
          // otherwise, let the default wndproc handle it
          wnd.DefWndProc(ref msg);
          break;
      }
    }

    // Get the current theme
    public IntPtr GetToolTipTheme()
    {
      // if we havent opened the theme, and its active, then open it
      if (m_OpenToolTipTheme == IntPtr.Zero && IsThemeActive() == 1)
      {
        Debug.WriteLine(DateTime.Now + ": Opening ToolTip Theme.");
        m_OpenToolTipTheme = OpenThemeData(m_ToolTipWnd.Handle, "TOOLTIP");
      }

      // return the open theme
      return m_OpenToolTipTheme;
    }

    private void SetupWindow()
    {
      Debug.WriteLine(DateTime.Now + ": Setting up window.");

      // get the windows graphics object
      Graphics GFX = Graphics.FromHwnd(IntPtr.Zero);
      SizeF TextRect;

      // measure the size of the string based on the maximum width
      TextRect = GFX.MeasureString(m_Text, m_Font, m_MaxWidth, StringFormat.GenericTypographic);

      // create the new client rectangle based on the size of the text rectangle, then inflate it by the padding
      m_ClientRectangle = new Rectangle(0, 0, (int)TextRect.Width + 1, (int)TextRect.Height + 1);
      m_ClientRectangle.Inflate(PADDING, PADDING);

      // if the window handle is not null
      if (m_ToolTipWnd.Handle != IntPtr.Zero)
      {
        // if we are using uxthemes
        if (m_Style == ToolStyle.UxTheme)
        {
          // grab the theme and graphics handle
          IntPtr pTheme = GetToolTipTheme();
          IntPtr hdc = GFX.GetHdc();

          // if the pointer is not null
          if (pTheme != IntPtr.Zero)
          {
            // create a RECT to pass to the theme routine and create a region pointer
            RECT ClipRect = new RECT(0, 0, m_ClientRectangle.Width, m_ClientRectangle.Height);

            IntPtr pRegion = IntPtr.Zero;

            // attempt to get the region of the theme
            if (ThemeRoutines.GetThemeBackgroundRegion(pTheme, hdc, (int)ToolTipPart.Standard,
              (int)ToolTipStandardState.TTSS_NORMAL, ref ClipRect, out pRegion) == 0)
            {
              // if we were successful, go ahead and set the window region, this will give us rounded edges
              Win32.SetWindowRgn(m_ToolTipWnd.Handle, pRegion, true);

              // TODO: Determine if not releasing the region object pointed to will cause memory leaks (likely use deleteobject)
            }
            else
            {
              // otherwise, fall to the failsafe, assume the entire rectangle is visible, set that as the region
              Win32.SetWindowRgn(m_ToolTipWnd.Handle, ref ClipRect, true);
            }
          }

          // release the hdc
          GFX.ReleaseHdc(hdc);
        }
      }

      // dispose the graphics object
      GFX.Dispose();
    }

    public void SetText(string value)
    {
      // if the value passed is null, assume empty string
      if (value == null)
        value = "";

      // if a handle exists, hide the tooltip
      if (m_ToolTipWnd.Handle != IntPtr.Zero)
        HideToolTip();

      // assign our new text value
      m_Text = value;

      // if its empty
      if (value == string.Empty)
      {
        // and we have a window handle, destroy it (we dont want to have a bunch of windows allocated that are not being used)
        if (m_ToolTipWnd.Handle != IntPtr.Zero)
        {
          m_ToolTipWnd.DestroyHandle();
          Debug.WriteLine(DateTime.Now + ": ToolTip Wnd Handle Destroyed.");
        }
      }
      else
      {
        // otherwise, if we dont have a window handle, create one
        if (m_ToolTipWnd.Handle == IntPtr.Zero)
        {
          Debug.WriteLine(DateTime.Now + ": ToolTip Wnd Handle Created.");
          m_ToolTipWnd.CreateHandle(m_CreateParams);
        }

        // set up the window
        SetupWindow();
      }
    }

    private void SetFont(Font value)
    {
      // attempt to hide the tool tip if we have a handle
      if (m_ToolTipWnd.Handle != IntPtr.Zero)
        HideToolTip();

      // set the font value, and set up the window
      m_Font = value;
      SetupWindow();
    }

    private void PaintToolTip(ref Message msg)
    {
      try
      {
        // get the open them and graphics object
        Debug.WriteLine(DateTime.Now + ": Painting Tooltip.");
        Graphics GFX = Graphics.FromHwnd(m_ToolTipWnd.Handle);
        IntPtr pTheme = GetToolTipTheme();

        // if we are using uxtheme and have an open theme available
        if (m_Style == ToolStyle.UxTheme && pTheme != IntPtr.Zero)
        {
          // get the clip rectangle from the client rectangle
          RECT ClipRect = new RECT(0, 0, m_ClientRectangle.Width, m_ClientRectangle.Height);

          IntPtr hdc = GFX.GetHdc();

          //DrawingUtils.GDI.GDI.SetBkMode(hdc, 1);

          // draw the themed background using uxtheme
          ThemeRoutines.DrawThemeBackground(pTheme, hdc, (int)ToolTipPart.Standard, (int)ToolTipStandardState.TTSS_NORMAL,
            ref ClipRect, ref ClipRect);

          // TODO: Draw the themed text as well

          GFX.ReleaseHdc(hdc);
        }
        else
        {
          // otherwise, this is for flat style and fail safe

          // get a pen with the border color and clear the background using the backcolor
          Pen brderPen = new Pen(m_BorderColor, BORDERWIDTH);
          GFX.Clear(m_BackColor);

          // draw the border and dispose the pen
          GFX.DrawRectangle(brderPen, 0, 0, m_ClientRectangle.Width - BORDERWIDTH, m_ClientRectangle.Height - BORDERWIDTH);
          brderPen.Dispose();
        }

        // get the text bounds by using the client rectangle and deflating it by the padding width
        Rectangle TxtBounds = new Rectangle(-1, -1, m_ClientRectangle.Width, m_ClientRectangle.Height);
        TxtBounds.Inflate(-PADDING, -PADDING);

        // get a new brush using the forecolor and draw the string
        Brush bshFrgrnd = new SolidBrush(m_ForeColor);
        GFX.DrawString(m_Text, m_Font, bshFrgrnd, TxtBounds);

        // clean up what we used
        bshFrgrnd.Dispose();
        GFX.Dispose();

        // return the hresult of zero and validate the entire rectangle
        msg.Result = IntPtr.Zero;
        Win32.ValidateRect(msg.HWnd, IntPtr.Zero);
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
    }

    public void ShowToolTip()
    {
      // set the windows position based on the mouse cursor and ensure its top most
      Win32.SetWindowPos(m_ToolTipWnd.Handle, new IntPtr((int)InsertAfter.HWND_TOPMOST), Control.MousePosition.X + 15,
        Control.MousePosition.Y + 15, m_ClientRectangle.Width, m_ClientRectangle.Height, SetWindowPosFlags.DoNotActivate);

      // show the window and set the visible flag to true
      Win32.ShowWindow(m_ToolTipWnd.Handle, ShowWindowCommand.ShowNoActivate);
      m_Visible = true;
    }

    private void m_ShowTimer_Tick(object sender, EventArgs e)
    {
      Debug.WriteLine(DateTime.Now + ": ToolTip Timer Expired");
      // if we have a window, show it. Stop the show timer as well
      if (m_ToolTipWnd.Handle != IntPtr.Zero)
        ShowToolTip();
      m_ShowTimer.Stop();
    }

    private void HideToolTip()
    {
      Debug.WriteLine(DateTime.Now + ": Hiding Tool Tip.");
      // if we have a window, hide it. set our visible flag to false
      if(m_ToolTipWnd.Handle != IntPtr.Zero)
        Win32.ShowWindow(m_ToolTipWnd.Handle, ShowWindowCommand.Hide);
      m_Visible = false;
    }

    public bool HandleMessage(ref Message msg)
    {
      switch ((WindowsMessages)msg.Msg)
      {
          // any type of mouse or key movement should cause the tool to disappear
        case WindowsMessages.WM_RBUTTONUP:
        case WindowsMessages.WM_LBUTTONUP:
        case WindowsMessages.WM_RBUTTONDOWN:
        case WindowsMessages.WM_LBUTTONDOWN:
        case WindowsMessages.WM_KEYDOWN:
        case WindowsMessages.WM_KEYUP:
        case WindowsMessages.WM_MOUSELEAVE:
          // if we have a handle
          if (m_ToolTipWnd.Handle != IntPtr.Zero)
          {
            // hide the tool tip and stop our show timer
            HideToolTip();
            m_ShowTimer.Stop();
            // if the mouse cursor is inside the parent control, restart the timer
            if (m_Parent.ClientRectangle.Contains(m_Parent.PointToClient(Control.MousePosition)))
              m_ShowTimer.Start();
            else
              m_LastCursorPos = new Point(-1, -1);
          }
          break;
          // assess the mouse move
        case WindowsMessages.WM_MOUSEMOVE:
          // if we have have a handle and the mouse actually moved..
          if (m_ToolTipWnd.Handle != IntPtr.Zero && Control.MousePosition != m_LastCursorPos)
          {
            // store the new location
            m_LastCursorPos = Control.MousePosition;
            // if visible
            if (m_Visible)
            {
              // if we arent using mouse track
              if (!m_MouseTrack)
              {
                // hide the tooltip, stop the timer and restart it if the cursor is in our control
                HideToolTip();
                m_ShowTimer.Stop();
                if (m_Parent.ClientRectangle.Contains(m_Parent.PointToClient(Control.MousePosition)))
                  m_ShowTimer.Start();
              }
              else
                // otherwise, just reshow the tooltip
                ShowToolTip();
            }
            else
            {
              // else, just stop the timer
              m_ShowTimer.Stop();
              // if we have a string
              if (m_Text != string.Empty)
              {
                // if the cursor is in the paret control, restart the timer
                if (m_Parent.ClientRectangle.Contains(m_Parent.PointToClient(Control.MousePosition)))
                {
                  m_ShowTimer.Start();
                }
              }
            }
          }
          break;
        default:
          return false;
      }

      return true;
    }
  }
}
