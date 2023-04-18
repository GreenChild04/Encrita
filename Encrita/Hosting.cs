using encrita;
using Terminal.Gui;

#pragma warning disable CS8600, CS8604

namespace front
{
    public static class Hosting {
        public static void entry() {
            // UI to prompt user
            Window dialog = new Window("Installation Error") {
                X = Pos.Center(),
                Y = Pos.Center(),
                Height = Dim.Fill() - 5,
                Width = Dim.Fill() - 10,
                ColorScheme = Stone.windowColour,
            }; Label label = new Label($"Error: No invite file found! ({Stone.inviteFile})") {
                X = Pos.Center(),
                Y = Pos.Center() - 4,
                ColorScheme = Colors.Error,
            }; Label info = new Label("(To chat you need an invite to a server or you could host your own!)") {
                X = Pos.Center(),
                Y = Pos.Center() - 2,
            }; Label moreInfo = new Label("You can either go find your invite file or host your own server :D") {
                X = Pos.Center(),
                Y = Pos.Center() - 1,
            }; Button quitButton = new Button("Quit") {
                X = Pos.Center() - 20,
                Y = Pos.Center() + 1,
                IsDefault = true,
            }; quitButton.Clicked += () => {Stone.log("User chose to quit application instead of hosting a server", "User"); Application.Shutdown(); Environment.Exit(0);};
            Button hostButton = new Button("Host Server") {
                X = Pos.Center() + 5,
                Y = Pos.Center() + 1,
            }; hostButton.Clicked += () => host();
            dialog.Add(label, info, moreInfo, quitButton, hostButton);
            Application.Run(dialog);
            
        }

        public static void host() {
            // Bot token length is 61
            Application.RequestStop();
            Stone.log("User chose to host their own server", "User");
            Stone.log("Fetching guide on hosting server...");

            // Hosting UI
            Window window = new Window("Server Hosting Guide") {
                X = Pos.Center(),
                Y = Pos.Center(),
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                ColorScheme = Stone.windowColour,
            }; Label welcome = new Label("*Welcome to a step by step guide on hosting your own server!*") {
                X = Pos.Center(),
                ColorScheme = Stone.bold,
            }; Label url = new Label("url: (https://www.writebots.com/discord-bot-token/) hold ctrl and click it") {
                X = Pos.Center(),
                Y = 2,
                ColorScheme = new ColorScheme() {Normal = Application.Driver.MakeAttribute(Terminal.Gui.Color.BrightBlue, Terminal.Gui.Color.Black)},
            }; Label step1 = new Label("1. Follow this guide above (the url) and paste your discord bot token below") {
                Y = 3,
            }; UiUtils.coolInput("Bot Token", 61, 1, 4, window, out TextField field);
            Label warning = new Label("*Warning! Never share your discord token with people you don't trust!*") {
                X = Pos.Center(),
                Y = 6,
                ColorScheme = Colors.Error,
            }; Label step2 = new Label("2. Once you've pasted the token, create a server on discord and invite the bot") {
                Y = 8,
            }; Label step3 = new Label("3. Now hit the 'Finish' button below to exit & create an invite to your server") {
                Y = 9,
            }; Button done = new Button("Finish & Create Invite") {
                X = Pos.Center(),
                Y = 11,
            }; done.Clicked += () => UiUtils.confirm("Finish & Create Invite?", "Are you sure that you want to exit the program and create an invite to your server (you can't undo this)?", () => {
                Stone.log("Hosting Setup Finished");
                string token = field.Text.ToString();

                // Take in passcode
                Stone.log("Asking for invite passcode...");
                Window passwin = new Window("Invite Passcode") {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Width = Dim.Fill() - 10,
                    Height = 10,
                    ColorScheme = Stone.windowColour,
                }; TextView text = new TextView() {
                    Text = "To secure and create your invite, please enter below and memorise a 4 digit passcode that will be required to accept this invite to your server.",
                    X = Pos.Center(),
                    Width = Dim.Fill() - 2,
                    Height = 3,
                    WordWrap = true,
                    ReadOnly = true,
                    CanFocus = false,
                }; UiUtils.coolInput("Passcode", 4, Pos.Center(), 3, passwin, out TextField passcode, true);
                Button button = new Button("Create Invite") {
                    X = Pos.Center(),
                    Y = 5,
                }; passwin.Add(text, button);
                 button.Clicked += () => UiUtils.confirm("Confirm Passcode", $"Are you sure that you want '{passcode.Text.ToString()}' to be your invite file's passcode? (Application will exit after creating invite)", () => {
                    back.Account.mintInvite(passcode.Text.ToString(), token);
                    Stone.log("Minted first invite for this server");
                    Stone.log("Exiting boot...");
                    Application.Shutdown();
                    Environment.Exit(0);
                }); Application.Run(passwin);
            });
            
            window.Add(welcome, url, warning, step1, step2, step3, done);
            Application.Run(window);
        }
    }
}