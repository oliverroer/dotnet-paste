using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DotNetPaste {
    class Program {
        static void Main(string[] args) {
            string text = string.Join(" ", args);
            SetText(text);
            GetText();
        }

        public static string GetText() {
            if (!OpenClipboard(IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            IntPtr data = GetClipboardData(CF_UNICODETEXT);

            return null;
        }

        public static void SetText(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (!OpenClipboard(IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            try
            {
                uint bytes = ((uint)text.Length + 1) * 2;
                IntPtr hGlobal = GlobalAlloc(GMEM_MOVABLE, (UIntPtr)bytes);

                if (hGlobal == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    IntPtr source = Marshal.StringToHGlobalUni(text);

                    try
                    {
                        IntPtr target = GlobalLock(hGlobal);

                        if (target == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        try
                        {
                            RtlCopyMemory(target, source, bytes);
                        }
                        finally
                        {
                            GlobalUnlock(target);
                        }

                        if (SetClipboardData(CF_UNICODETEXT, hGlobal) == IntPtr.Zero)
                            throw new Win32Exception(Marshal.GetLastWin32Error());

                        hGlobal = IntPtr.Zero;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(source);
                    }
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                        GlobalFree(hGlobal);
                }
            }
            finally
            {
                CloseClipboard();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        public static extern void RtlCopyMemory(IntPtr dest, IntPtr src, uint count);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVABLE = 0x0002;
    }
}
