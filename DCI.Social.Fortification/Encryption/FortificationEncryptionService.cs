using DCI.Social.Fortification.Configuration;
using DCI.Social.Fortification.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Encryption;
internal class FortificationEncryptionService : IFortificationEncryptionService
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
    private SocialEncryptedStream _stream = new SocialEncryptedStream();
    private readonly RSA _trustedRsaPublicKey;
    private readonly RSA? _trustedRsaPrivateKey;
    private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;
    private readonly IServiceScopeFactory _scopeFactory;

    private bool _receivedSymmetricKey = false;
    public bool IsInitiatedWithSymmetricKey => _receivedSymmetricKey;

    public string CurrentSymmetricKey => Convert.ToBase64String(_stream.Key);

    public FortificationEncryptionService(IOptions<FortificationConfiguration> options, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _trustedRsaPublicKey = RSA.Create();
        var publicKeyString = options.Value.TrustedCertificateFile.ReadCertificateFile();
        Log($"Using HQ certificate file: {options.Value.TrustedCertificateFile}");
        Log(publicKeyString);
        var certBytes = Convert.FromBase64String(publicKeyString);
        var cert = new X509Certificate2(certBytes);
        _trustedRsaPublicKey = cert.GetRSAPublicKey()!;
        if(options.Value.TrustedPrivateKeyFile != null)
        {
            var privateKeyString = options.Value.TrustedPrivateKeyFile.ReadCertificateFile();
            var privateKeyBytes = Convert.FromBase64String(privateKeyString);
            _trustedRsaPrivateKey = RSACng.Create();
            _trustedRsaPrivateKey.ImportPkcs8PrivateKey(privateKeyBytes, out _);

        }

    }

    public async Task<T> DecryptFromTransport<T>(string input) where T : class =>
        await Locked(() => _stream.DeserializeFromTransport<T>(input), nameof(DecryptFromTransport));


    public async Task<string> DecryptStringFromTransport(string input) => 
        await Locked(() => _stream.DecryptFromTransport(input), nameof(DecryptStringFromTransport));

    public async Task<string> EncryptForTransport<T>(T input) where T : class =>
        await Locked(() => _stream.EncryptForTransport(input), nameof(EncryptForTransport));

    public async Task<string> EncryptStringForTransport(string input) => 
        await Locked(() => _stream.EncryptForTransport(input), nameof(EncryptStringForTransport));

    public async Task UpdateWithKey(byte[] key) => 
        await LockedAction(() => { 
            _stream = new SocialEncryptedStream(key);
            _receivedSymmetricKey = true;
        
        });


    private async Task<T> LockedAsync<T>(Func<Task<T>> toPerform, string? logString)
    {
        if (logString != null)
            Log(logString);
        await _lock.WaitAsync();
        try
        {
            var result = await toPerform();
            return result;
        }
        finally
        {
            _lock.Release();
        }
    }

    private Task<T> Locked<T>(Func<T> toPerform, string? logString) => 
        LockedAsync(() => Task.FromResult(toPerform()), logString);



    private async Task LockedAction(Action toPerform) =>
        _ = await LockedAsync(async () =>
        {
            await Task.CompletedTask;
            toPerform();
            return 0;
        }, logString: null);

    public Task<string> EncryptSymmetricKey()
    {
        var encryptedBytes = _trustedRsaPublicKey.Encrypt(_stream.Key, Padding);
        var asBase64 = Convert.ToBase64String(encryptedBytes);
        return Task.FromResult(asBase64);
    }

    public async Task DecryptSymmetricKey(string input)
    {
        if(_trustedRsaPrivateKey != null)
        {
            var asBytes = Convert.FromBase64String(input);
            var decrypted = _trustedRsaPrivateKey.Decrypt(asBytes, Padding);
            await UpdateWithKey(decrypted);
        }
    }

    private void Log(string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FortificationEncryptionService>>();
        logger.LogInformation(message);
    }

}


