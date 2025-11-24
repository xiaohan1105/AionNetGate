using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Utilty
{
    public struct WindowInfo
    {
        public IntPtr hWnd;
        public string szWindowName;
        public string szClassName;
    }

    [Serializable]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
