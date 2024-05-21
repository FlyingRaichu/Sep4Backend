using System.Security.Cryptography;

namespace IoTInterfacing.Encryption;

public class EncryptionHandler : IEncryptionHandler
{
    public byte[] PrivateKey { get; private set; }
    public byte[] PublicKey { get; private set; }
    private byte[] _aesKey;

    public EncryptionHandler()
    {
        (PublicKey, PrivateKey) = GenerateEccKeys();
        _aesKey = new byte[32];
    }
    // Method for generating ECC keys
    private static (byte[] PublicKey, byte[] PrivateKey) GenerateEccKeys()
    {
        using var ecdh = ECDiffieHellman.Create();
        var publicKey = ecdh.ExportSubjectPublicKeyInfo();
        var privateKey = ecdh.ExportECPrivateKey();

        return (publicKey, privateKey);
    }

    // Method for deriving shared secret and AES key
    public void DeriveSharedSecret(byte[] theirPublicKey)
    {
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportSubjectPublicKeyInfo(theirPublicKey, out _);

        var sharedSecret = ecdh.DeriveKeyMaterial(ecdh.PublicKey);
        _aesKey = sharedSecret;
    }

    // Method for AES decryption
    public string DecryptAes(byte[] cipherText, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = _aesKey;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream(cipherText);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);

        return sr.ReadToEnd();
    }

    public byte[] EncryptAes(string plainText, out byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = _aesKey;
        aes.GenerateIV();
        iv = aes.IV;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);

        sw.Write(plainText);
        sw.Close();

        return ms.ToArray();
    }
}
