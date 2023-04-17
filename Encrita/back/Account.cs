using encryption;

namespace back
{
    public class Account {
        public static void mintInvite(string password, string token) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(token);
            File.WriteAllBytes("invite.eib", Symmetric.encrypt(bytes, password));
        }
    }
}