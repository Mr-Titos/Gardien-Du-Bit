using common_gardienbit.DTO.PwdPackage;
using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Models;
using Microsoft.JSInterop;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace IIABlazorWebAssembly.Services
{
    public class EncryptionService(IJSRuntime jsRuntime, APIService apiService, GeneralConfiguration generalConfiguration)
    {
        private readonly IJSRuntime jsRuntime = jsRuntime;
        private readonly APIService apiService = apiService;
        private readonly GeneralConfiguration generalConfiguration = generalConfiguration;

        private List<UserVaultSession> sessions = [];


        async Task<string?> GetEncryptionKeyEncryptedFromStorage(Guid vaultId)
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "encryptionKey-" + vaultId.ToString().ToLower());
        }

        public async Task<string?> GetEncryptionKeyDecryptedFromStorage(Guid vaultId)
        {
            try
            {
                var encryptionKeyForEncryptionKey = await this.GetEncryptionKeyVaultProcedure(vaultId);
                var encryptedEncryptionKey = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", "encryptionKey-" + vaultId.ToString().ToLower());
                if (encryptedEncryptionKey == null || encryptionKeyForEncryptionKey == null)
                    return null;

                return await this.DecryptSessionStringAsync(encryptionKeyForEncryptionKey.vauPrivateKey, encryptedEncryptionKey);
            }
            catch (Exception)
            {
                return null;
            }

        }

        async Task StoreEncryptionKeyToStorage(UserVaultSession session, string clearPassword, KeyVaultDTO encryptionKeyForEncryptionKey)
        {
            string? encryptionKeyBase64 = await this.DeriveKeyFromPassword(session, clearPassword) ?? throw new Exception("Derived Key is null");
            var encryptedEncryptionKeyBase64 = await this.EncryptSessionStringAsync(encryptionKeyForEncryptionKey.vauPublicKey, encryptionKeyBase64);
            await jsRuntime.InvokeVoidAsync("localStorage.setItem", "encryptionKey-" + session.successDTO?.vauId.ToString().ToLower(), encryptedEncryptionKeyBase64);
        }

        async Task DeleteEncryptionKeyFromStorage(UserVaultSession session)
        {
            await jsRuntime.InvokeAsync<string?>("localStorage.removeItem", "encryptionKey-" + session.successDTO?.vauId);
        }

        public async Task<string> DeriveKeyFromPassword(UserVaultSession session, string password)
        {
            // Appel pour générer la clé AES à partir du mot de passe et du sel
            var aesKeyRef = await jsRuntime.InvokeAsync<string>(
                "cryptoHelper.createPasswordKey",
                password,
                Convert.ToBase64String(session.successDTO!.vauSalt)
            );
            return aesKeyRef;
        }

        public async Task<FieldPwdPackageDTO?> EncryptFieldPwdPackageFromDerivedKey(string keyBase64, string? field)
        {
            var encryptedField = await jsRuntime.InvokeAsync<FieldPwdPackageDTO>(
                "cryptoHelper.encryptWithPasswordKey",
                keyBase64,
                field
            );

            return encryptedField;
        }

        public async Task<FieldPwdPackageDTO?> DecryptFieldPwdPackageFromDerivedKey(string keyBase64, FieldPwdPackageDTO field)
        {
            field.EnfDecryptedText = await jsRuntime.InvokeAsync<string?>(
                "cryptoHelper.decryptWithPasswordKey",
                keyBase64,
                field
            );

            return field;
        }

        public byte[] Sha256HashString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] data = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(data);

                return hash;
            }
        }

        public byte[] GenerateSalt(int size = 16)
        {
            byte[] salt = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public byte[] HashPasswordString(string input)
        {
            return this.Sha256HashString(generalConfiguration.StaticSalt + input);
        }

        public async Task<string> EncryptSessionStringAsync(byte[] publicKey, string data)
        {
            string publicKeyBase64 = Convert.ToBase64String(publicKey);
            var result = await jsRuntime.InvokeAsync<string>(
                "cryptoHelper.encryptRSAOAEP",
                publicKeyBase64,
                data
            );
            return result;
        }

        public async Task<string> DecryptSessionStringAsync(byte[] encryptionKey, string data)
        {
            return await jsRuntime.InvokeAsync<string>(
                "cryptoHelper.decryptRSAOAEP",
                Convert.ToBase64String(encryptionKey),
                data
            );
        }

        public async Task<T> DecryptSessionStringObjectAsync<T>(byte[] encryptionKey, T obj)
            where T : class
        {
            foreach (PropertyInfo attribute in obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(string)))
            {
                if (attribute.GetValue(obj) is string attributeValue)
                {
                    attribute.SetValue(obj, await DecryptSessionStringAsync(encryptionKey, attributeValue));
                }
            }
            return obj;
        }

        // Can't encrypt an object with attributes different than string
        public async Task<T> EncryptSessionStringObject<T>(byte[] publicKey, T obj)
            where T : class
        {
            foreach (PropertyInfo attribute in obj.GetType().GetProperties().Where(p => p.PropertyType == typeof(string)))
            {
                if (attribute.GetValue(obj) is string attributeValue)
                {
                    attribute.SetValue(obj, await EncryptSessionStringAsync(publicKey, attributeValue));
                }
            }
            return obj;
        }

        // Check if the user is already logged in to the vault in the local storage
        // If so, then it will request the necessary informations from the back to rebuild its session.
        public async Task<(UserVaultSession?, KeyVaultDTO?)> LoginFromLocalStorage(Guid vaultId)
        {
            var alreadyLoggedIn = await this.GetEncryptionKeyEncryptedFromStorage(vaultId);
            UserVaultSession? session = null;
            KeyVaultDTO? keyVaultDTO = null;
            if (alreadyLoggedIn != null)
            {
                var request = await apiService.GetDataAsync<ResetSessionVaultDTO>("/vaults/resetSession/" + vaultId.ToString());
                if (request.Data != null)
                {
                    if (request.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        session = new UserVaultSession() { successDTO = request.Data.successDTO, usrVauSessionId = Guid.NewGuid() };
                        sessions.Add(session);
                        keyVaultDTO = request.Data.keyVaultDTO;
                    }
                }

            }
            return (session, keyVaultDTO);
        }

        // Login to the Vault and getting the public key of the session
        public async Task<(UserVaultSession?, KeyVaultDTO)> LoginVaultProcedure(string clearPassword, Guid vaultId, bool isTotp)
        {
            var session = sessions.FirstOrDefault(s => s.successDTO?.vauId == vaultId);
            UserVaultSession vaultSession = new UserVaultSession();
            if (session == null)
            {
                var exception1 = new Exception("Error on the login request");

                var hashPassword = Convert.ToBase64String(HashPasswordString(clearPassword));
                var loginVaultDTO = new LoginVaultDTO { vauHash = hashPassword, vauId = vaultId };
                var tupleRequest1 = await apiService.PostDataAsync<LoginSuccessVaultDTO>("/vaults/login", loginVaultDTO);

                if ((int)tupleRequest1.StatusCode > 299)
                    throw exception1;

                vaultSession = new() { successDTO = tupleRequest1.Data ?? throw exception1 };

                sessions.Add(vaultSession);
            }


            // Need the session to be added before getting the encryption key
            // Get the encryption key for the vault for this session (and refresh the session timeout)
            var encryptionKey = isTotp == false ? await GetEncryptionKeyVaultProcedure(vaultId) : null;

            if (session == null && isTotp == false)
            {
                try
                {
                    await this.StoreEncryptionKeyToStorage(vaultSession, clearPassword, encryptionKey!);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error while storing the encryption key in the local storage", ex);
                }

            }
            else if (session == null && isTotp)
            {
                vaultSession.totpData = clearPassword;
            }

            return (session == null ? vaultSession : session, encryptionKey!);
        }

        // Get the Key that will Encrypt the  Vault Key (useful for encrypted entries in the vault) 
        // that is derived from user password and the salt previously gathered
        private async Task<KeyVaultDTO> GetEncryptionKeyVaultProcedure(Guid vaultId)
        {
            var session = sessions.FirstOrDefault(s => s.successDTO?.vauId == vaultId);
            if (session == null)
                throw new Exception("Session not found");

            var exception2 = new Exception("Error on the getEncryptionKey request");
            var encryptedData = await EncryptSessionStringAsync(session.successDTO!.vauPKey, vaultId.ToString());
            var idVaultDTO = new IdVaultDto { vauId = vaultId, vauEncryptedData = encryptedData };
            var tupleRequest2 = await apiService.PostDataAsync<KeyVaultDTO>("/vaults/getKey", idVaultDTO);

            int statusCode = (int)tupleRequest2.StatusCode;
            if (statusCode > 299)
            {
                if (statusCode == 401)
                {
                    await DeleteEncryptionKeyFromStorage(session);
                    throw new MemberAccessException("Session expired");
                }
                else
                {
                    throw exception2;
                }
            }

            return tupleRequest2.Data ?? throw exception2;
        }

        public async Task<KeyVaultDTO> GetEncryptionKeyVaultAfterTotp(Guid vaultId)
        {
            var key = await this.GetEncryptionKeyVaultProcedure(vaultId);
            if (key == null)
                throw new Exception("Key not found");
            try
            {
                var vaultSession = sessions.FirstOrDefault(s => s.successDTO?.vauId == vaultId);
                if (vaultSession != null)
                {
                    await this.StoreEncryptionKeyToStorage(vaultSession, vaultSession?.totpData!, key);
                    vaultSession!.totpData = null;
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Error while storing the encryption key in the local storage", ex);
            }

            return key;
        }
    }
}
