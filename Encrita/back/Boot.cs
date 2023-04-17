using encrita;
using Terminal.Gui;

namespace back
{
    public static class Boot {
        public static void boot() {
            Application.Init();
            Stone.log("\n", "(===)");
            Stone.log($"Booting from directory ({Directory.GetCurrentDirectory()})");
            Stone.log("Encrita Booting...");

            // Pre-boot checks
            if (!File.Exists(Stone.systemFile)) {
                Stone.log($"Error: No system file found! ({Stone.systemFile})");
                Install.install();
            }

            Stone.log("Successfully Booted!");
            Application.Shutdown();
        }
    }
}