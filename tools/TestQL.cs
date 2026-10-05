using System;
using RinAccountManager;
public class TestQL {
  public static void Main() {
    QuickLoginTicket t;
    string err;
    if (!QuickLogin.TryCreate(out t, out err)) {
      Console.WriteLine("CREATE_FAIL:" + err);
      return;
    }
    Console.WriteLine("CREATE_OK code=" + t.Code + " qr=" + (t.QrUrl == null ? "null" : "ok"));
    string acc;
    string st = QuickLogin.GetStatus(t, out acc, out err);
    Console.WriteLine("STATUS=" + st + " acc=" + (acc == null ? "null" : acc) + " err=" + err);
    System.Drawing.Image qr = QuickLogin.LoadQr(t.QrUrl);
    Console.WriteLine("QR=" + (qr == null ? "null" : qr.Width + "x" + qr.Height));
    QuickLogin.Cancel(t);
    System.Threading.Thread.Sleep(1500);
    string st2 = QuickLogin.GetStatus(t, out acc, out err);
    Console.WriteLine("AFTER_CANCEL=" + st2);
  }
}
