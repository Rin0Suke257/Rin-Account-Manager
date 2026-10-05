using System;
using System.Windows.Forms;
using RinAccountManager;
public class TestGamesForm {
  [STAThread]
  public static void Main() {
    Application.EnableVisualStyles();
    Application.SetCompatibleTextRenderingDefault(false);
    Application.Run(new GamesForm("", ""));
  }
}
