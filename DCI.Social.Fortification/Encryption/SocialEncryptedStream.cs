using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Encryption;
public class SocialEncryptedStream : IDisposable
{
    private readonly Aes _symmetricAlgorithm;
    private readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();
    private const int IvLengthPrefixLength = 10;

    public SocialEncryptedStream(byte[]? key = null)
    {
        _symmetricAlgorithm = Aes.Create();
        if (key != null)
            _symmetricAlgorithm.Key = key;
    }


    public byte[] Key => _symmetricAlgorithm.Key;

    public (byte[] EncryptedBytes, byte[] IV) Encrypt(byte[] input)
    {
        using (var output = new MemoryStream())
        {
            var iv = CreateIV(input.Length);

            using (var cryptoStream = new CryptoStream(output, _symmetricAlgorithm.CreateEncryptor(_symmetricAlgorithm.Key, iv), CryptoStreamMode.Write))
                cryptoStream.Write(input, 0, input.Length);
            var returnee = output.ToArray();
            return (returnee, iv);

        }
    }

    public string EncryptForTransport(string input)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var (encBytes, iv) = Encrypt(inputBytes);
        var base64encBytes = Convert.ToBase64String(encBytes);
        var base64iv = Convert.ToBase64String(iv);
        var ivLength = base64iv.Length;
        var ivLengthString = ivLength.ToString();
        for (var i = 0; ivLengthString.Length < IvLengthPrefixLength; i++)
            ivLengthString = "0" + ivLengthString;

        var returnee = $"{ivLengthString}{base64iv}{base64encBytes}";
        return returnee;
    }

    public string EncryptForTransport<T>(T input) where T : class
    {
        var serialized = JsonSerializer.Serialize(input)!;
        var returnee = EncryptForTransport(serialized);
        return returnee;
    }


    public byte[] Decrypt(byte[] input, byte[] iv)
    {
        using var output = new MemoryStream(input);
        using var cryptoStream = new CryptoStream(output, _symmetricAlgorithm.CreateDecryptor(_symmetricAlgorithm.Key, iv), CryptoStreamMode.Read);
        using var reader = new StreamReader(cryptoStream);
        var readString = reader.ReadToEnd();
        var returnee = Encoding.UTF8.GetBytes(readString);
        return returnee;
    }


    public string DecryptFromTransport(string input)
    {
        var ivLengthString = input.Substring(0, IvLengthPrefixLength);
        var ivLength = Convert.ToInt32(ivLengthString);
        var ivBase64String = input.Substring(IvLengthPrefixLength, ivLength);
        var lastIndexOfIv = IvLengthPrefixLength + ivLength;
        var encBase64String = input.Substring(lastIndexOfIv, input.Length - lastIndexOfIv);
        var ivBytes = Convert.FromBase64String(ivBase64String);
        var encBytes = Convert.FromBase64String(encBase64String);
        var decBytes = Decrypt(encBytes, ivBytes);
        var returnee = Encoding.UTF8.GetString(decBytes);
        return returnee;
    }

    public T DeserializeFromTransport<T>(string input) where T : class
    {
        var serialized = DecryptFromTransport(input);
        var returnee = JsonSerializer.Deserialize<T>(serialized)!;
        return returnee;
    }




    private byte[] CreateIV(int length)
    {
        length = length - length % 16;
        length = length + 16;
        var returnee = new byte[16];
        _random.GetBytes(returnee);
        return returnee;
    }



    public void Dispose()
    {
        _symmetricAlgorithm.Dispose();
    }
}
