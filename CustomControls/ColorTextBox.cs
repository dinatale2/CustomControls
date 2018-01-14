using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace CustomControls
{
	public class ColorTextBox : TextBox
	{
		private bool m_MouseOver;

		[Browsable(false)]
		public new Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}

		[Browsable(false)]
		public new Color ForeColor
		{
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		private Color m_BackColor;
		public Color StandBackColor
		{
			get { return m_BackColor; }
			set
			{
				if (m_BackColor != value)
				{
					m_BackColor = value;
					SetBackColor();
				}
			}
		}

		private Color m_ForeColor;
		public Color StandForeColor
		{
			get { return m_ForeColor; }
			set
			{
				if (m_ForeColor != value)
				{
					m_ForeColor = value;
					SetForeColor();
				}
			}
		}

		private Color m_FocusBackColor;
		public Color FocusBackColor
		{
			get { return m_FocusBackColor; }
			set
			{
				if (m_FocusBackColor != value)
				{
					m_FocusBackColor = value;
					SetForeColor();
				}
			}
		}

		private Color m_FocusForeColor;
		public Color FocusForeColor
		{
			get { return m_FocusForeColor; }
			set
			{
				if (m_FocusForeColor != value)
				{
					m_FocusForeColor = value;
					SetForeColor();
				}
			}
		}

		private Color m_MouseOverBackColor;
		public Color MouseOverBackColor
		{
			get { return m_MouseOverBackColor; }
			set
			{
				if (m_MouseOverBackColor != value)
				{
					m_MouseOverBackColor = value;
					SetBackColor();
				}
			}
		}

		private Color m_MouseOverForeColor;
		public Color MouseOverForeColor
		{
			get { return m_MouseOverForeColor; }
			set
			{
				if (m_MouseOverForeColor != value)
				{
					m_MouseOverForeColor = value;
					SetForeColor();
				}
			}
		}

		public new bool ReadOnly
		{
			get { return base.ReadOnly; }
			set
			{
				if (base.ReadOnly != value)
				{
					base.ReadOnly = value;
					SetForeColor();
					SetBackColor();
				}
			}
		}

		public new bool Enabled
		{
			get { return base.Enabled; }
			set
			{
				if (base.Enabled != value)
				{
					base.Enabled = value;
					SetBackColor();
					SetForeColor();
				}
			}
		}

		public ColorTextBox()
			: base()
		{
			m_BackColor = Color.White;
			m_MouseOverBackColor = Color.White;
			m_FocusBackColor = Color.White;

			m_ForeColor = Color.Black;
			m_MouseOverForeColor = Color.Black;
			m_FocusForeColor = Color.Black;
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);

			m_MouseOver = true;
			SetBackColor();
			SetForeColor();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			SetBackColor();
			SetForeColor();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);

			SetBackColor();
			SetForeColor();
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			m_MouseOver = false;
			SetBackColor();
			SetForeColor();
		}

		private void SetBackColor()
		{
			Color newColor = m_BackColor;

			if (!ReadOnly && m_MouseOver && !m_MouseOverBackColor.IsEmpty)
				newColor = m_MouseOverBackColor;

			if (!ReadOnly && this.Focused && !m_FocusBackColor.IsEmpty)
				newColor = m_FocusBackColor;

			base.BackColor = newColor;
		}

		private void SetForeColor()
		{
			Color newColor = m_ForeColor;

			if (!ReadOnly && m_MouseOver && !m_MouseOverForeColor.IsEmpty)
				newColor = m_MouseOverForeColor;

			if (!ReadOnly && this.Focused && !m_FocusForeColor.IsEmpty)
				newColor = m_FocusForeColor;

			base.ForeColor = newColor;
		}
	}
}