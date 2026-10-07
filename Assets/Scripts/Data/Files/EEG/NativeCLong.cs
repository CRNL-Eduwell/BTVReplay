using System;
using System.Runtime.InteropServices;

namespace Tools.DLL
{
    /// <summary>
    /// Size of the C <c>long</c> type on the running platform. Windows is LLP64 (<c>long</c> stays
    /// 32 bits even on x64); macOS and Linux are LP64 (<c>long</c> is pointer-sized). A native
    /// function returning <c>long</c> therefore needs an <c>int</c> extern on Windows and a
    /// <c>long</c> extern elsewhere. .NET 6 solves this with CLong, which Unity's .NET Standard 2.1
    /// profile does not have.
    /// </summary>
    public static class NativeCLong
    {
        /// <summary>
        /// True when the C <c>long</c> of the running process is 32 bits wide.
        /// </summary>
        public static readonly bool Is32Bit = SizeInBytes(RuntimeInformation.IsOSPlatform(OSPlatform.Windows), IntPtr.Size) == 4;

        /// <summary>
        /// Width in bytes of the C <c>long</c> for a platform: 4 on Windows, the pointer size elsewhere.
        /// </summary>
        public static int SizeInBytes(bool isWindows, int pointerSize)
        {
            return isWindows ? 4 : pointerSize;
        }
    }
}
