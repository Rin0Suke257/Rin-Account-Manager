using System;
using System.Threading;

namespace RinAccountManager
{
    // Giu mutex giong MultiBloxy de cho phep mo nhieu client.
    // Thu thuat: giu 1 MUTEX ten "ROBLOX_singletonEvent" de chan Roblox tao EVENT cung ten
    // (2 loai WaitHandle khac nhau khong duoc trung ten).
    // Neu Roblox (ke ca tray background) dang chay va giu san EVENT thi new Mutex se throw,
    // phai CloseSingletonHandles hoac tat Roblox truoc roi thu lai.
    public static class MultiRoblox
    {
        private static Mutex eventMutex = null;
        private static Mutex gameMutex = null;

        public static bool IsEnabled
        {
            get { return eventMutex != null; }
        }

        public static bool TryEnable(out string error)
        {
            error = null;
            try
            {
                if (eventMutex == null)
                {
                    bool createdNew;
                    eventMutex = new Mutex(false, "ROBLOX_singletonEvent", out createdNew);
                }
            }
            catch (Exception ex)
            {
                Cleanup();
                error = "Khong giu duoc ROBLOX_singletonEvent (Roblox dang chay?). Chi tiet: " + ex.Message;
                return false;
            }
            try
            {
                if (gameMutex == null)
                {
                    bool createdNew2;
                    gameMutex = new Mutex(false, "ROBLOX_singletonMutex", out createdNew2);
                }
            }
            catch (Exception ex)
            {
                Cleanup();
                error = "Khong giu duoc ROBLOX_singletonMutex: " + ex.Message;
                return false;
            }
            return true;
        }

        public static bool Enable()
        {
            string err;
            return TryEnable(out err);
        }

        private static void Cleanup()
        {
            try
            {
                if (eventMutex != null)
                {
                    try { eventMutex.Close(); } catch { }
                    eventMutex = null;
                }
            }
            catch { eventMutex = null; }
            try
            {
                if (gameMutex != null)
                {
                    try { gameMutex.Close(); } catch { }
                    gameMutex = null;
                }
            }
            catch { gameMutex = null; }
        }

        public static void Disable()
        {
            Cleanup();
        }
    }
}
