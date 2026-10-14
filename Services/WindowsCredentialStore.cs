using System.Runtime.InteropServices;

namespace DeluxeNET.Services
{
    /// <summary>
    /// Windows Credential Manager read/write (generic credentials).
    /// Secrets never touch a text file.
    /// </summary>
    public static class WindowsCredentialStore
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NativeCredential
        {
            public uint Flags;
            public uint Type;
            [MarshalAs(UnmanagedType.LPWStr)] public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)] public string Comment;
            public long LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)] public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)] public string UserName;
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredWrite(ref NativeCredential cred, uint flags);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredRead(string target, uint type, uint flags, out IntPtr cred);

        [DllImport("advapi32.dll")]
        private static extern void CredFree(IntPtr cred);

        public static string? Read(string target)
        {
            if (!OperatingSystem.IsWindows()) return null;
            try
            {
                if (CredRead(target, 1, 0, out var ptr) && ptr != IntPtr.Zero)
                {
                    try
                    {
                        var c = Marshal.PtrToStructure<NativeCredential>(ptr);
                        if (c.CredentialBlobSize > 0)
                            return Marshal.PtrToStringUni(c.CredentialBlob, (int)(c.CredentialBlobSize / 2));
                    }
                    finally { CredFree(ptr); }
                }
            }
            catch { /* credential store unavailable */ }
            return null;
        }

        public static bool Write(string target, string secret)
        {
            if (!OperatingSystem.IsWindows()) return false;
            var bytes = System.Text.Encoding.Unicode.GetBytes(secret);
            var p = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, p, bytes.Length);
                var cred = new NativeCredential
                {
                    Type = 1,    // CRED_TYPE_GENERIC
                    Persist = 2, // CRED_PERSIST_LOCAL_MACHINE
                    TargetName = target,
                    CredentialBlob = p,
                    CredentialBlobSize = (uint)bytes.Length
                };
                return CredWrite(ref cred, 0);
            }
            catch { return false; }
            finally
            {
                Array.Clear(bytes, 0, bytes.Length);
                Marshal.FreeHGlobal(p);
            }
        }
    }
}
