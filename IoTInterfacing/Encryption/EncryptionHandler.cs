using System.Security.Cryptography;
using System.Text;

namespace IoTInterfacing.Encryption;

public class EncryptionHandler : IEncryptionHandler
{
    private byte[] PublicKey { get;} = GenerateEccKeys();
    private byte[] _aesKey = new byte[32];

    // Method for generating ECC keys
    private static byte[] GenerateEccKeys()
    {
        using var eccGenerator = ECDiffieHellman.Create();
        var publicKey = eccGenerator.ExportSubjectPublicKeyInfo();

        return (publicKey);
    }

    // Method for deriving shared secret and AES key
    public void DeriveSharedSecret(byte[] theirPublicKey)
    {
        using var eccGenerator = ECDiffieHellman.Create();
        eccGenerator.ImportSubjectPublicKeyInfo(theirPublicKey, out _);

        var sharedSecret = eccGenerator.DeriveKeyMaterial(eccGenerator.PublicKey);
        _aesKey = sharedSecret;
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

    public byte[] EncryptAes(string plainText, out byte[] iv)
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        iv = aes.IV;

        // Combine the sender's public key with the plainText
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var message = new byte[4 + PublicKey.Length + plainTextBytes.Length];

        // Write the length of the public key bytes as a 4-byte prefix
        var lengthBytes = BitConverter.GetBytes(PublicKey.Length);
        Buffer.BlockCopy(lengthBytes, 0, message, 0, lengthBytes.Length);

        // Write the public key bytes
        Buffer.BlockCopy(PublicKey, 0, message, 4, PublicKey.Length);

        // // Use a delimiter to separate the public key from the encrypted text //TODO this is possibly an alternative, having a sign be the breakpoint if encoding the public key length isn't enough
        // const byte delimiter = 0x1F;
        // message[4 + publicKeyBytes.Length] = delimiter;

        // Write the encrypted text bytes
        Buffer.BlockCopy(plainTextBytes, 0, message, 4 + PublicKey.Length + 1, plainTextBytes.Length);

        // Encrypt the combined message
        using var encryptor = aes.CreateEncryptor(_aesKey, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        cryptoStream.Write(message, 0, message.Length);
        cryptoStream.FlushFinalBlock();

        return memoryStream.ToArray();
    }

}
