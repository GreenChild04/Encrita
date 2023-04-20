using encrita;
using Terminal.Gui;

namespace back
{
    public static class Boot {
        public static void boot() {
            try {boot(true);}
            catch (Exception e) {
                Stone.log("An unhandled c# exception has occured!");
                Stone.log($"[Unhandled Exception] {e.GetType().FullName}: ({e.Message}) Stack Trace:\n{e.StackTrace}");
                errorUI(e);
            }
        }

        private static void boot(bool isPrivate) { // Private layer of boot with no exception catching
            Application.Init();
            Colors.Error = new ColorScheme() {Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black)};
            Stone.log("\n\n(===)", "[(===)]");
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

        private static void quit() {
            Stone.log("User aborting program...", "User");
            Application.Shutdown();
            Stone.log("Aborted program successfully");
        }

        private static void errorUI(Exception e) {
            Window window = new Window("Unhandled Exception") {
                X = Pos.Center(),
                Y = Pos.Center(),
                Height = Dim.Fill() - 5,
                Width = Dim.Fill() - 10,
                ColorScheme = Stone.windowColour,
            }; Label title = new Label("*An Unhandled Error has Occured*") {
                X = Pos.Center(),
                Y = 1,
                ColorScheme = Stone.bold,
            }; TextView text = new TextView {
                Text = "This page should never show up normally, if it has, then a bug in the app's code has occured, what you should do is create a new 'issue' on this project's github (https://github.com/GreenChild04/Encrita) with the title of \"Unexpected Error\" and then, for the body, paste a copy of the last entry of the 'runtime.log' file. Afterwards, just close and open the app again :D.",
                WordWrap = true,
                Width = Dim.Fill() - 2,
                Height = 7,
                X = Pos.Center(),
                Y = 2,
                CanFocus = false,
                ReadOnly = true,
                ColorScheme = Stone.windowColour,
            }; Button button = new Button("Quit") {
                X = Pos.Center(),
                Y = 11,
                IsDefault = true,
            }; button.Clicked += () => front.UiUtils.confirm("Quit?", "Please make sure you filed an issue report on this project's github, it really helps out the developers! :D", () => {Application.Shutdown(); Stone.log("Exiting Encrita..."); Environment.Exit(1);});
            window.Add(title, text, button);
            Application.Run(window);
        }
    }
}