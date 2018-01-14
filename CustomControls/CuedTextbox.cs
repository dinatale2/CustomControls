using System;
using System.Text;
using Win32Lib;
using System.Runtime.InteropServices;

namespace CustomControls
{
	public class CuedTextbox : ColorTextBox
	{
		private string _strCueText;
		public string CueText
		{
			get { return _strCueText; }
			set { SetCueText(value); }
		}

		private bool _bCueOnFocus;
		public bool CueOnFocus
		{
			get { return _bCueOnFocus; }
			set
			{
				if (_bCueOnFocus != value)
				{
					_bCueOnFocus = value;
					SetCueText(_strCueText, true);
				}
			}
		}

		public CuedTextbox()
			: base()
		{
			_bCueOnFocus = false;
		}

		private void SetCueText(string text)
		{
			SetCueText(text, false);
		}

		private void SetCueText(string text, bool force)
		{
			if (force || _strCueText != text)
			{
				_strCueText = text;
				IntPtr pString = IntPtr.Zero;
				int cueOnFocus = _bCueOnFocus ? 1 : 0;

				if (text == null)
				{
					text = "";
				}

				GCHandle GCH = GCHandle.Alloc(text, GCHandleType.Pinned);
				pString = GCH.AddrOfPinnedObject();
				Win32.SendMessage(this.Handle, (int)WindowsMessages.EM_SETCUEBANNER, cueOnFocus, pString);
				GCH.Free();

				this.Invalidate();
			}
		}
	}
}
