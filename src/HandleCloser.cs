using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RinAccountManager
{
    // Trich tu MultiBloxy (MIT) cua Zgoly, sua cho tuong thich C# 5 + dong ca Mutex.
    // Dung khi teleport bao loi 773: dong handle singleton trong cac process Roblox.
    public static class HandleCloser
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_HANDLE_INFORMATION
        {
            public uint ProcessId;
            public byte ObjectTypeNumber;
            public byte Flags;
            public ushort Handle;
            public uint Object;
            public uint GrantedAccess;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OBJECT_NAME_INFORMATION
        {
            public UNICODE_STRING Name;
        }

        [DllImport("ntdll.dll")]
        private static extern uint NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, uint systemInformationLength, out uint returnLength);

        [DllImport("ntdll.dll")]
        private static extern uint NtQueryObject(IntPtr handle, int objectInformationClass, IntPtr objectInformation, uint objectInformationLength, out uint returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwOptions);

        private const int SystemHandleInformation = 16;
        private const int ObjectNameInformation = 1;
        private const uint PROCESS_ALL = 0x001F0FFF;
        private const uint DUPLICATE_CLOSE_SOURCE = 0x0001;
        private const uint DUPLICATE_SAME_ACCESS = 0x0002;

        public static int CloseSingletonHandles()
        {
            int closed = 0;
            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");
            foreach (Process process in processes)
            {
                try
                {
                    closed += CloseInProcess(process);
                }
                catch { }
                try { process.Dispose(); } catch { }
            }
            return closed;
        }

        private static int CloseInProcess(Process process)
        {
            int closed = 0;
            uint size = 0x10000;
            IntPtr buffer = Marshal.AllocHGlobal((int)size);
            try
            {
                while (true)
                {
                    uint returnLength;
                    uint status = NtQuerySystemInformation(SystemHandleInformation, buffer, size, out returnLength);
                    if (status == 0xC0000004)
                    {
                        size *= 2;
                        Marshal.FreeHGlobal(buffer);
                        buffer = Marshal.AllocHGlobal((int)size);
                    }
                    else
                    {
                        break;
                    }
                }

                int handleCount = Marshal.ReadInt32(buffer);
                IntPtr ptr = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(int)));
                int structSize = Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION));

                for (int i = 0; i < handleCount; i++)
                {
                    SYSTEM_HANDLE_INFORMATION handleInfo = (SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(ptr, typeof(SYSTEM_HANDLE_INFORMATION));
                    if (handleInfo.ProcessId == (uint)process.Id)
                    {
                        IntPtr processHandle = OpenProcess(PROCESS_ALL, false, process.Id);
                        if (processHandle == IntPtr.Zero)
                        {
                            ptr = new IntPtr(ptr.ToInt64() + structSize);
                            continue;
                        }

                        IntPtr dupHandle;
                        bool success = DuplicateHandle(processHandle, new IntPtr(handleInfo.Handle), GetCurrentProcess(), out dupHandle, 0, false, DUPLICATE_SAME_ACCESS);
                        if (!success)
                        {
                            CloseHandle(processHandle);
                            ptr = new IntPtr(ptr.ToInt64() + structSize);
                            continue;
                        }

                        uint bufferSize = 0x1000;
                        IntPtr nameBuffer = Marshal.AllocHGlobal((int)bufferSize);
                        try
                        {
                            uint retLen;
                            uint status = NtQueryObject(dupHandle, ObjectNameInformation, nameBuffer, bufferSize, out retLen);
                            if (status == 0)
                            {
                                OBJECT_NAME_INFORMATION objectNameInfo = (OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(nameBuffer, typeof(OBJECT_NAME_INFORMATION));
                                if (objectNameInfo.Name.Length > 0 && objectNameInfo.Name.Buffer != IntPtr.Zero)
                                {
                                    string name = Marshal.PtrToStringUni(objectNameInfo.Name.Buffer, objectNameInfo.Name.Length / 2);
                                    if (name != null && name.Contains("ROBLOX_singletonEvent"))
                                    {
                                        IntPtr dummy;
                                        if (DuplicateHandle(processHandle, new IntPtr(handleInfo.Handle), IntPtr.Zero, out dummy, 0, false, DUPLICATE_CLOSE_SOURCE))
                                        {
                                            closed++;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                        finally
                        {
                            Marshal.FreeHGlobal(nameBuffer);
                        }
                        CloseHandle(dupHandle);
                        CloseHandle(processHandle);
                    }
                    ptr = new IntPtr(ptr.ToInt64() + structSize);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
            return closed;
        }
    }
}
