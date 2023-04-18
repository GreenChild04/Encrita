using Discord;
using Terminal.Gui;

#pragma warning disable CS0162

namespace encrita
{
    public static class Stone {
        public const string version = "beta";
        public const string logFile = "runtime.log";
        public const string systemFile = "encrita.eib";
        public const string inviteFile = "invite.eib";
        public const bool keepLog = true;
        public static readonly ColorScheme windowColour = new ColorScheme() {Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.BrightGreen, Terminal.Gui.Color.Black)};
        public static readonly ColorScheme bold = new ColorScheme() {Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.BrightCyan, Terminal.Gui.Color.Black)};

        public static void log(string raw, string pre="Encrita") {
            if (!keepLog) return;
            string msg = $"({DateTime.Now.ToString("HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo)}) [{pre}] {raw}\n";
            // System.Console.Write(msg);
            File.AppendAllText(logFile, msg);
        } public static void log(LogMessage msg) => log(msg.Message, "DiscordAPI");
    }
}