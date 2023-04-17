using Terminal.Gui;

namespace front
{
    public static class UiUtils {
        public static void coolInput(string prompt, int length, Pos x, Pos y, Window window, out TextField field, bool centered=false) {
            string padding = new string(' ', length + 2);
            int offset = centered ? (int) MathF.Round((prompt.Length + 4) / 3.5f): prompt.Length + 4;
            Label label = new Label($"{prompt}: [{padding}]") {
                X = x,
                Y = y,
                ColorScheme = new ColorScheme() {Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.Gray, Terminal.Gui.Color.Black)},
            }; field = new TextField() {
                X = x + offset,
                Y = y,
                Width = length + 1,
            }; field.KeyPress += (e) => {
                if (e.KeyEvent.Key == Key.Enter) e.Handled = true;
            }; window.Add(label, field);
        }

        public static void confirm(string title, string prompt, Action run) {
            if (MessageBox.Query(50, 7, title, prompt, "Cancel", "Confirm") == 1) run();
        }
    }
}