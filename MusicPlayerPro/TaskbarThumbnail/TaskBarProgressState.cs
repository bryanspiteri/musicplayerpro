//--------------------------------------------------------------------
// <copyright file="TaskBarProgressState.cs" company="MyAPKapp">
//     Copyright (c) MyAPKapp. All rights reserved.
// </copyright>                                                                
//--------------------------------------------------------------------
// This open-source project is licensed under Apache License 2.0
//--------------------------------------------------------------------


namespace MusicPlayerPro
{
    public enum TaskBarProgressState
    {
        NoProgress = 0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }
}
