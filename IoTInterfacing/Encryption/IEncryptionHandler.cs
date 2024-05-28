namespace IoTInterfacing.Encryption;

public interface IEncryptionHandler
{
    byte[] PublicKey { get; }
    void DeriveSharedSecret(byte[] theirPublicKey);
    string DecryptAes(byte[] cipherText, byte[] iv);
    byte[] EncryptAes(string plainText, out byte[] iv);
}