using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

#pragma warning disable SYSLIB0041

namespace encryption
{
    public static class PassHashing {
        private const int iterations = 100000;
        private const int saltSize = 16;
        private const int hashSize = 20;

        public static byte[][] hash(string password) {
            byte[] salt = new byte[saltSize];
            RandomNumberGenerator.Fill(salt);
            return hash(password, salt);
        }

        public static byte[][] hash(string password, byte[] salt) =>
            new byte[][] {new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, iterations).GetBytes(hashSize), salt};
    }

    public static class Asymmetric {
        public static void generateKeys(out byte[] publicKey, out byte[] privateKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            publicKey = rsa.ExportRSAPublicKey();
            privateKey = rsa.ExportRSAPrivateKey();
        }

        public static byte[] encrypt(byte[] bytes, byte[] publicKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPublicKey(publicKey, out _);
            return rsa.Encrypt(bytes, false);
        }

        public static byte[] decrypt(byte[] encrypted, byte[] privateKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            return rsa.Decrypt(encrypted, false);
        }

        public static byte[] sign(byte[] bytes, byte[] privateKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] hash = SHA256.HashData(bytes);
            rsa.ImportRSAPrivateKey(privateKey, out _);
            return rsa.Encrypt(bytes, false);
        }

        public static bool verify(byte[] data, byte[] signature, byte[] publicKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportRSAPublicKey(publicKey, out _);
            byte[] oldHash = rsa.Decrypt(signature, false);
            byte[] hash = SHA256.HashData(data);
            return oldHash.SequenceEqual(hash);
        }
    }

    public static class Symmetric {
        public static byte[] encrypt(byte[] data, string passphrase) {
            byte[] pass = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
            using (Aes aes = Aes.Create()) {
                aes.Key = pass;
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (MemoryStream ms = new MemoryStream()) {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
                        cs.Write(data, 0, data.Length);
                    } return ms.ToArray();
                }
            }
        }

        public static byte[] decrypt(byte[] encrypted, string passphrase) {
            byte[] pass = SHA256.HashData(Encoding.UTF8.GetBytes(passphrase));
            using (Aes aes = Aes.Create()) {
                aes.Key = pass;
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream ms = new MemoryStream()) {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read)) {
                        byte[] data = new byte[encrypted.Length];
                        cs.Read(data, 0, data.Length);
                        return data;
                    }
                }
            }
        }
    }
}