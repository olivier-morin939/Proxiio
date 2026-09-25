using ServiceContracts;
using Microsoft.AspNetCore.DataProtection;

namespace Services
{
    public class EncryptionsService : IEncryptionsService
    {
        private readonly IDataProtector _protector;


        public EncryptionsService(IDataProtectionProvider provider)
        {
            
            _protector = provider.CreateProtector("Proxiio.Registering.Password.V1");
        }

        // Method to encrypt plain text data
        public Task<string> EncryptData(string plainText)
        {
            return Task.FromResult(_protector.Protect(plainText));
        }

        // Method to decrypt the encrypted data
        public Task<string> DecryptData(string encryptedData)
        {
            try
            {
                return Task.FromResult(_protector.Unprotect(encryptedData));
            }
            catch (Exception ex)
            {
                // If decryption fails (e.g., data is tampered or invalid), handle the exception
                return Task.FromResult($"Decryption failed: {ex.Message}");
            }
        }

    }
}
