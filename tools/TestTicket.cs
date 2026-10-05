using System;
using System.Collections.Generic;
using RinAccountManager;
public class TestTicket {
  public static void Main() {
    List<Account> accs = AccountStore.Load();
    for (int i = 0; i < accs.Count; i++) {
      Account a = accs[i];
      Console.WriteLine("== " + a.Alias + " ==");
      string csrf, e1;
      bool ok1 = RobloxApi.TryGetCsrfToken(a.Cookie, out csrf, out e1);
      Console.WriteLine("csrf_ok=" + ok1 + " err=" + e1 + " len=" + (csrf == null ? 0 : csrf.Length));
      if (!ok1) continue;
      string ticket, e2;
      bool ok2 = RobloxApi.TryGetAuthTicket(a.Cookie, csrf, out ticket, out e2);
      Console.WriteLine("ticket_ok=" + ok2 + " err=" + e2 + " len=" + (ticket == null ? 0 : ticket.Length));
    }
  }
}
