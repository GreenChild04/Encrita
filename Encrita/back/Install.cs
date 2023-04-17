using encrita;
using front;

namespace back
{
    public static class Install {
        public static void install() {
            Stone.log("Installing and configering Encrita...");

            /* Creating system file */
            // Pre-install checks
            if (!File.Exists(Stone.inviteFile)) noInviteFile();
        }
        
        public static void noInviteFile() {
            Stone.log($"Error: No invite file found! ({Stone.inviteFile})");
            Hosting.entry();
        }
    }
}