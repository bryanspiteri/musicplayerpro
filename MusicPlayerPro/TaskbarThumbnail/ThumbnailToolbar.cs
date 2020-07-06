//--------------------------------------------------------------------
// <copyright file="ThumbnailToolbar.cs" company="MyAPKapp">
//     Copyright (c) MyAPKapp. All rights reserved.
// </copyright>                                                                
//--------------------------------------------------------------------
// This open-source project is licensed under Apache License 2.0
//--------------------------------------------------------------------

using System;
using System.Windows.Forms;


namespace MusicPlayerPro
{
    /// <summary>
    /// <code>ThumbnailToolbar</code> is a nativewindow, the purpose is return a WndProc'd Handle
    /// </summary>
    internal class ThumbnailToolbar : NativeWindow
    {

        private ThumbnailButton[] buttons;

        public ThumbnailToolbar(IntPtr Handle, ThumbnailButton[] buttons)
        {
            this.buttons = buttons;
            AssignHandle(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_COMMAND && NativeMethods.GetHiWord(m.WParam.ToInt64(), 16) == NativeMethods.THUMBBUTTON.Clicked)
            {
                int buttonId = NativeMethods.GetLoWord(m.WParam.ToInt64());
                foreach (ThumbnailButton button in buttons)
                {
                    if (button.Id == buttonId)
                    {
                        button.FireClickEvent();
                    }
                }
            }
            base.WndProc(ref m);
        }

    }
}