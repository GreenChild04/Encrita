using encryption;
using cstore;
using encrita;

namespace back
{
    public class Account {
        public byte[] publicKey;
        private byte[] privatekey;
        private string discordtoken;

        public Account(string filename, string password) {
            // Temporary varibles to unlock important ones
            Stone.log("Loading account long term memory...");
            LongTerm longTerm = CStore.loadObj<LongTerm>(filename);
            Stone.log("Successfully loaded account long term memory!");
            verifyPassword(longTerm.passhash, password, () => throw new Exception("[Account (undesired)] Error: tried to login to account with wrong password (there was no previous check)"));

            // Important
            this.publicKey = longTerm.publicKey;
            Stone.log("Loaded public key", "Account");
            this.privatekey = Symmetric.decrypt(longTerm.privateKey, password);
            Stone.log("Loaded private key", "Account");
            this.discordtoken = System.Text.Encoding.UTF8.GetString(Symmetric.decrypt(longTerm.discordToken, password));
            Stone.log("Loaded discord bot token", "Account");
        }

        // Static stuff
        public static void mintInvite(string password, string token) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token);
            File.WriteAllBytes("invite.eib", Symmetric.encrypt(bytes, password));
        }

        public static void verifyPassword(byte[][] desired, string password, Action except) {
            if (!PassHashing.hash(password, desired[1])[0].SequenceEqual(desired[0])) except();
        }

        public static void init(string filename, string password, string rawDiscordToken) {
            Stone.log("Initialising account...");
            byte[][] passhash = PassHashing.hash(password);
            Asymmetric.generateKeys(out byte[] publicKey, out byte[] rawPrivateKey);
            byte[] privateKey = Symmetric.encrypt(rawPrivateKey, password);
            byte[] discordToken = Symmetric.encrypt(System.Text.Encoding.UTF8.GetBytes(rawDiscordToken), password);
            LongTerm longTerm = new LongTerm {
                passhash = passhash,
                publicKey = publicKey,
                privateKey = privateKey,
                discordToken = discordToken,
            }; CStore.store(filename, longTerm);
            Stone.log("Initialised account");
        }

        private struct LongTerm {
            public byte[][] passhash;
            public byte[] publicKey;
            public byte[] privateKey;
            public byte[] discordToken;
        }
    }
}