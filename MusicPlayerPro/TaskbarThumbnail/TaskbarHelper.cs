//--------------------------------------------------------------------
// <copyright file="TaskbarHelper.cs" company="MyAPKapp">
//     Copyright (c) MyAPKapp. All rights reserved.
// </copyright>                                                                
//--------------------------------------------------------------------
// This open-source project is licensed under Apache License 2.0
//--------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MusicPlayerPro
{
	/// <summary>
	/// The <see cref="TaskbarHelper"/> is the class for Windows 7 taskbar features
	/// </summary>
	public sealed class TaskbarHelper
	{

		private static NativeMethods.ITaskbarList3 taskbar;
		private IntPtr _MainHandle;
		private bool buttonsAdded;
		private static TaskbarHelper helper;

		private TaskbarHelper()
		{
			taskbar = (NativeMethods.ITaskbarList3)new TaskbarInstance();
			taskbar.HrInit();
		}

		internal NativeMethods.ITaskbarList3 NativeInterface
		{
			get
			{
				return taskbar;
			}
		}

		/// <summary>
		/// Set progress bar value in the taskbar item of your application
		/// </summary>
		/// <param name="Handle">The associated handle</param>
		/// <param name="value">The current value</param>
		/// <param name="max">The maximum value</param>
		public void SetProgressValue(IntPtr Handle, double value, double max)
		{
			taskbar.SetProgressValue(Handle, (ulong)value, (ulong)max);
		}

		/// <summary>
		/// Set progress bar value in the taskbar item of your application
		/// </summary>
		/// <param name="value">The current value</param>
		/// <param name="max">The maximum value</param>
		public void SetProgressValue(double value, double max)
		{
			SetProgressValue(MainHandle, value, max);
		}

		/// <summary>
		/// Set progress state in the taskbar item of your application
		/// </summary>
		/// <param name="Handle">The associated handle</param>
		/// <param name="state">The progress state</param>
		public void SetProgressState(IntPtr Handle, TaskBarProgressState state)
		{
			taskbar.SetProgressState(Handle, state);
		}

		/// <summary>
		/// Set progress state in the taskbar item of your application
		/// </summary>
		/// <param name="state">The progress state</param>
		public void SetProgressState(TaskBarProgressState state)
		{
			SetProgressState(MainHandle, state);
		}

		/// <summary>
		/// Set an icon that overlays the main task icon
		/// </summary>
		/// <param name="Handle">The associated handle</param>
		/// <param name="icon">The overlay icon</param>
		/// <param name="description">The description or title for accessibility</param>
		public void SetOverlayIcon(IntPtr Handle, Icon icon, string description)
		{
			taskbar.SetOverlayIcon(Handle, icon.Handle, description);
		}

		/// <summary>
		/// Set an icon that overlays the main task icon
		/// </summary>
		/// <param name="icon">The overlay icon</param>
		/// <param name="description">The description or title for accessibility</param>
		public void SetOverlayIcon(Icon icon, string description)
		{
			SetOverlayIcon(MainHandle, icon, description);
		}

		/// <summary>
		/// Add buttons below thumbnail preview
		/// </summary>
		/// <param name="Handle">The associated handle</param>
		/// <param name="buttons">Buttons to add, the limit is 7 buttons</param>
		public void AddThumbnailButtons(IntPtr Handle, params ThumbnailButton[] buttons)
		{
			if (!buttonsAdded)
			{
				buttonsAdded = true;
				if (buttons == null || buttons.Length <= 0)
				{
					throw new ArgumentException("Buttons array is empty", "buttons");
				}
				if (buttons.Length > 7)
				{
					throw new ArgumentException("Buttons amount reaches limit of 7 buttons", "buttons");
				}
				if (Handle == null)
				{
					throw new NullReferenceException("Form Handle is required");
				}
				List<NativeMethods.THUMBBUTTON> nativeButtons = new List<NativeMethods.THUMBBUTTON>();
				ThumbnailToolbar WndProcdHandle = new ThumbnailToolbar(Handle, buttons);
				foreach (ThumbnailButton button in buttons)
				{
					button.ParentHandle = WndProcdHandle.Handle;
					nativeButtons.Add(button.NativeButton);
				}
				NativeMethods.THUMBBUTTON[] nativeBtns = nativeButtons.ToArray();
				taskbar.ThumbBarAddButtons(Handle, nativeBtns.Length, nativeBtns);
			}
		}

		[Guid("56FDF344-FD6D-11d0-958A-006097C9A090")]
		[ClassInterface(ClassInterfaceType.None)]
		[ComImport()]
		private class TaskbarInstance { }

		/// <summary>
		/// Gets the instance of the taskbar, if Windows version is earlier than Windows 7, an exception will be throwed. Call <see cref="IsSupported"/> property to check if version is Windows 7 or later
		/// </summary>
		public static TaskbarHelper Instance
		{
			get
			{
				if (!IsSupported)
				{
					throw new Exception("Windows 7 or newer is required!");
				}
				if (helper == null) helper = new TaskbarHelper();
				return helper;
			}
		}

		/// <summary>
		/// Check if Windows version supports taskbar methods.
		/// </summary>
		public static bool IsSupported
		{
			get
			{
				return Environment.OSVersion.Version >= new Version(6, 1);
			}
		}

		private IntPtr MainHandle
		{
			get
			{
				if (_MainHandle == IntPtr.Zero)
				{
					Process process = Process.GetCurrentProcess();
					if (process == null || process.MainWindowHandle == IntPtr.Zero)
					{
						throw new InvalidOperationException("A main valid window is required");
					}
					_MainHandle = process.MainWindowHandle;
				}
				return _MainHandle;
			}
		}

	}
}
