using DCI.Social.Fortification.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Encryption;
internal class FortificationEncryptionService : IFortificationEncryptionService
{
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
    private SocialEncryptedStream _stream = new SocialEncryptedStream();
    private readonly RSA _trustedRsaPublicKey;
    private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;
    private readonly IServiceScopeFactory _scopeFactory;

    public FortificationEncryptionService(IOptions<FortificationConfiguration> options, IServiceScopeFactory scopeFactory)
    {
        _trustedRsaPublicKey = RSA.Create();
        var publicKeyString = File.ReadAllText(options.Value.TrustedCertificateFile);
        _trustedRsaPublicKey.ImportFromPem(publicKeyString);
        _scopeFactory = scopeFactory;
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
        await LockedAction(() => _stream = new SocialEncryptedStream(key));


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
        var asBytes = Convert.FromBase64String(input);
        var decrypted = _trustedRsaPublicKey.Decrypt(asBytes, Padding);
        await UpdateWithKey(decrypted);
    }

    private void Log(string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FortificationEncryptionService>>();
        logger.LogInformation(message);
    }

}


