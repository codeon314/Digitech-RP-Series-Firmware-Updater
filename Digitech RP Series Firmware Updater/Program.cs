namespace NetUpdaterClone;

static class Program
{
    [STAThread]
    static void Main()
    {
        using var mutex = new Mutex(true, @"Local\DigiTechRpUpdater_SingleInstance", out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "DigiTech RP Series Firmware Updater is already running.",
                "DigiTech RP Series Firmware Updater",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.SetColorMode(SystemColorMode.Dark);
        Application.Run(new MainForm());
    }
}
