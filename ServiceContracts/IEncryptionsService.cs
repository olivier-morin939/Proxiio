
namespace ServiceContracts
{
    public interface IEncryptionsService
    {

        Task<string> EncryptData(string plainText);

        Task<string> DecryptData(string encryptedData);

    }
}
