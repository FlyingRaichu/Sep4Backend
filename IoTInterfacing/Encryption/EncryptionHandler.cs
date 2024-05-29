using System.Security.Cryptography;
using System.Text;

namespace IoTInterfacing.Encryption
{
    public class EncryptionHandler : IEncryptionHandler
    {
        public byte[] PublicKey { get;}
        private readonly ECDiffieHellman _eccGenerator;
        private readonly byte[] _aesKey = new byte[16];

        public EncryptionHandler()
        {
            _eccGenerator = ECDiffieHellman.Create();
            PublicKey = _eccGenerator.ExportSubjectPublicKeyInfo();
        }

        // Method for deriving shared secret and AES key
        public void DeriveSharedSecret(byte[] theirPublicKey)
        {
            using var otherEcc = ECDiffieHellman.Create();
            otherEcc.ImportSubjectPublicKeyInfo(theirPublicKey, out _);

            var sharedSecret = _eccGenerator.DeriveKeyMaterial(otherEcc.PublicKey);
            var hashedSecret = SHA1.HashData(sharedSecret);
            Array.Copy(hashedSecret, _aesKey, 16);
        }

        // Method for AES decryption
        public string DecryptAes(byte[] cipherText, byte[] iv)
        {
            using var aes = Aes.Create();
            aes.Key = _aesKey;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream(cipherText);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }

        // Method for AES encryption
        public byte[] EncryptAes(string plainText, out byte[] iv)
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            iv = aes.IV;

            // Combine the sender's public key with the plainText
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var message = new byte[4 + PublicKey.Length + plainTextBytes.Length];

            // Write the plain text bytes
            Buffer.BlockCopy(plainTextBytes, 0, message, 4 + PublicKey.Length, plainTextBytes.Length);

            // Encrypt the combined message
            using var encryptor = aes.CreateEncryptor(_aesKey, aes.IV);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(message, 0, message.Length);
            cryptoStream.FlushFinalBlock();

            return memoryStream.ToArray();
        }
    }
}
