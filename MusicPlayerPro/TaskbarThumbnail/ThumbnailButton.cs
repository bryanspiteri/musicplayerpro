//--------------------------------------------------------------------
// <copyright file="ThumbnailButton.cs" company="MyAPKapp">
//     Copyright (c) MyAPKapp. All rights reserved.
// </copyright>                                                                
//--------------------------------------------------------------------
// This open-source project is licensed under Apache License 2.0
//--------------------------------------------------------------------


using System;
using System.Drawing;

namespace MusicPlayerPro
{
	/// <summary>
	/// ThumbnailButton is a button below task thumbnail
	/// </summary>
	public sealed class ThumbnailButton : IDisposable
	{

		private NativeMethods.THUMBBUTTON nativeBtn;
		private bool initialized;
		private static int nextId = 101;
		private string _Tip;
		private Icon _Icon;
		private bool _Enabled;
		private bool _DismissOnClick;
		private bool _Visible;
		private bool _IsInteractive;
		public event EventHandler Click;

		/// <summary>
		/// Initialize the <code>ThumbnailButton</code> button
		/// </summary>
		/// <param name="icon">The button icon</param>
		/// <param name="tip">The tooltip text</param>
		public ThumbnailButton(Icon icon, string tip)
		{
			ParentHandle = Program.Instance.Handle;
			Id = nextId;
			if (nextId == int.MaxValue)
			{
				nextId = 101;
			}
			else
			{
				nextId++;
			}
			Icon = icon;
			Tip = tip;
			Enabled = true;
			nativeBtn = new NativeMethods.THUMBBUTTON();
			BuildNativeButton();
			initialized = true;
		}

		#region Internal and Private Methods

		internal IntPtr ParentHandle { get; set; }
		internal int Id { get; set; }
		internal NativeMethods.THUMBBUTTONFLAGS Flags { get; set; }
		internal NativeMethods.THUMBBUTTON NativeButton
		{
			get
			{
				return nativeBtn;
			}
		}

		internal void FireClickEvent()
		{
			Click?.Invoke(this, EventArgs.Empty);
		}

		private void BuildNativeButton()
		{
			nativeBtn.iId = Id;
			nativeBtn.szTip = Tip;
			nativeBtn.hIcon = (Icon != null) ? Icon.Handle : IntPtr.Zero;
			nativeBtn.dwFlags = Flags;
			nativeBtn.dwMask = NativeMethods.THUMBBUTTONMASK.THB_FLAGS;
			if (Tip != null) nativeBtn.dwMask |= NativeMethods.THUMBBUTTONMASK.THB_TOOLTIP;
			if (Icon != null) nativeBtn.dwMask |= NativeMethods.THUMBBUTTONMASK.THB_ICON;
		}

		private void UpdateChanges()
		{
			if (!initialized || ParentHandle == null) return;
			BuildNativeButton();
			TaskbarHelper.Instance.NativeInterface.ThumbBarUpdateButtons(ParentHandle, 1, new NativeMethods.THUMBBUTTON[] { nativeBtn });
		}

		private bool IsFlagSet(NativeMethods.THUMBBUTTONFLAGS Flags, NativeMethods.THUMBBUTTONFLAGS Flag)
		{
			return (Flags & Flag) != 0;
		}

		private void SetFlag(NativeMethods.THUMBBUTTONFLAGS Flags, NativeMethods.THUMBBUTTONFLAGS Flag, bool addOrRemove)
		{
			if (addOrRemove)
			{
				Flags |= Flag;
			}
			else
			{
				Flags &= ~(Flag);
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// The tooltip of the button
		/// </summary>
		public string Tip
		{
			get
			{
				return _Tip;
			}
			set
			{
				if (_Tip != value)
				{
					_Tip = value;
					UpdateChanges();
				}
			}
		}

		/// <summary>
		/// The icon of the button
		/// </summary>
		public Icon Icon
		{
			get
			{
				return _Icon;
			}
			set
			{
				if (_Icon != value)
				{
					_Icon = value;
					UpdateChanges();
				}
			}
		}

		/// <summary>
		/// If false, button will be grayed out and disabled
		/// </summary>
		public bool Enabled
		{
			get
			{
				return !IsFlagSet(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_DISABLED);
			}
			set
			{
				if (_Enabled != value)
				{
					_Enabled = value;
					SetFlag(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_DISABLED, !value);
					UpdateChanges();
				}
			}
		}

		/// <summary>
		/// If true, thumbnail preview will dismiss on click
		/// </summary>
		public bool DismissOnClick
		{
			get
			{
				return IsFlagSet(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_DISMISSONCLICK);
			}
			set
			{
				if (_DismissOnClick)
				{
					_DismissOnClick = value;
					SetFlag(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_DISMISSONCLICK, value);
					UpdateChanges();
				}
			}
		}

		/// <summary>
		/// If false, button will be hidden
		/// </summary>
		public bool Visible
		{
			get
			{
				return !IsFlagSet(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_HIDDEN);
			}
			set
			{
				if (_Visible != value)
				{
					_Visible = value;
					SetFlag(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_HIDDEN, !value);
					UpdateChanges();
				}
			}
		}

		/// <summary>
		/// If false, button is disabled without being grayed out
		/// </summary>
		public bool IsInteractive
		{
			get
			{
				return !IsFlagSet(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_NONINTERACTIVE);
			}
			set
			{
				if (_IsInteractive != value)
				{
					_IsInteractive = value;
					SetFlag(Flags, NativeMethods.THUMBBUTTONFLAGS.THBF_NONINTERACTIVE, !value);
					UpdateChanges();
				}
			}
		}

		#endregion

		#region Dispose methods

		~ThumbnailButton()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing)
			{
				Icon.Dispose();
				Tip = null;
			}
		}

		#endregion

	}
}