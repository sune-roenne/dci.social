using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Encryption;
public interface IFortificationEncryptionService
{
    Task<string> EncryptStringForTransport(string input);
    Task<string> EncryptForTransport<T>(T input) where T : class;

    Task<string> DecryptStringFromTransport(string input);
    Task<T> DecryptFromTransport<T>(string input) where T : class;

    Task UpdateWithKey(byte[] key);

    Task<string> EncryptSymmetricKey();
    Task DecryptSymmetricKey(string input);

    bool IsInitiatedWithSymmetricKey { get; }


}
