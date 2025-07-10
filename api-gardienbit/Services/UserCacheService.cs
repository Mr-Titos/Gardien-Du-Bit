using api_gardienbit.Models;
using api_gardienbit.Repositories;
using common_gardienbit.DTO.Exception;
using common_gardienbit.DTO.PwdPackage;
using System.Security.Cryptography;
using System.Text;

namespace api_gardienbit.Services
{
    public class UserCacheService(VaultSessionRepository vaultSessionRepository)
    {
        public readonly int minSessionAlive = 15;

        private async Task<VaultSession?> GetUserVaultSession(Client client, Guid vaultId, bool isTotpRequest)
        {
            var vs = await vaultSessionRepository.GetVaultSessionByVaultAndClient(vaultId, client);
            if (vs == null)
                return null;

            VaultSession? vsToSend = null;
            if (vs.VasLastActivityDate.AddMinutes(minSessionAlive) < DateTime.UtcNow)
                await vaultSessionRepository.DeleteObject(vs.VasId);
            else
            {
                // If VauTOTP is null, it means that the vault is not a TOTP vault
                // If VasIsTotpResolved is true, it means that the TOTP has already been resolved
                // If isTotpRequest is true, it means that the user is trying to access the vault to complete TOTP
                if (vs.VasVault.VauTOTP == null || vs.VasIsTotpResolved == true || isTotpRequest == true)
                {
                    await UpdateActivityVaultSession(vs);
                    vsToSend = vs;
                }
            }
            return vsToSend;
        }

        public async Task<VaultSession?> GetUserVaultSession(Client client, Vault vault)
        {
            return await this.GetUserVaultSession(client, vault.VauId, false);
        }

        public async Task<VaultSession?> GetUserVaultSession(Client client, Guid vaultId)
        {
            return await this.GetUserVaultSession(client, vaultId, false);
        }

        public async Task<VaultSession?> GetUserVaultSessionTotp(Client client, Guid vaultId)
        {
            return await this.GetUserVaultSession(client, vaultId, true);
        }

        public async Task<VaultSession> CreateUserVaultSession(Client client, Vault vault)
        {
            RSA rsa = RSA.Create(2048);
            byte[] privateKey = rsa.ExportRSAPrivateKey();
            byte[] publicKey = rsa.ExportSubjectPublicKeyInfo(); // export public key in SPKI format for JS support

            RSA rsa2 = RSA.Create(2048);
            byte[] encryptionKeyPrivate = rsa2.ExportPkcs8PrivateKey();
            byte[] encryptionKeyPublic = rsa2.ExportSubjectPublicKeyInfo();

            var vs = await vaultSessionRepository.CreateObject(new VaultSession()
            {
                VasClient = client,
                VasVault = vault,
                VasPrivateKey = privateKey,
                VasPublicKey = publicKey,
                VasEncryptionKeyPrivate = encryptionKeyPrivate,
                VasEncryptionKeyPublic = encryptionKeyPublic,
                VasIsTotpResolved = false,
                VasEntryDate = DateTime.UtcNow,
                VasLastActivityDate = DateTime.UtcNow,
            });

            return vs ?? throw new Exception("Error while creating user-vault session");
        }

        private async Task UpdateActivityVaultSession(VaultSession vs)
        {
            vs.VasLastActivityDate = DateTime.UtcNow;
            await vaultSessionRepository.UpdateObject(vs.VasId, vs);
        }

        public async Task UpdateTotpVaultSession(VaultSession vs, bool isTotpResolved)
        {
            vs.VasIsTotpResolved = isTotpResolved;
            await vaultSessionRepository.UpdateObject(vs.VasId, vs);
        }

        public void EncryptFieldPwdPackageDTOVaultSession(VaultSession vs, FieldPwdPackageDTO data, out FieldPwdPackageDTO encryptedData)
        {
            byte[] puKey = vs.VasPublicKey ?? throw new UserVaultSessionExpiredException();

            try
            {
                RSA rsa = RSA.Create(2048);
                rsa.ImportSubjectPublicKeyInfo(puKey, out _);
                byte[] ivBytes = Encoding.UTF8.GetBytes(data.EnfInitialisationVectorBase64 ?? throw new ArgumentNullException());
                string ivString = Convert.ToBase64String(rsa.Encrypt(ivBytes, RSAEncryptionPadding.OaepSHA256));
                byte[] cipherBytes = Encoding.UTF8.GetBytes(data.EnfCipherTextBase64 ?? throw new ArgumentNullException());
                string cipherString = Convert.ToBase64String(rsa.Encrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256));
                byte[] tagBytes = Encoding.UTF8.GetBytes(data.EnfAuthTagBase64 ?? throw new ArgumentNullException());
                string tagString = Convert.ToBase64String(rsa.Encrypt(tagBytes, RSAEncryptionPadding.OaepSHA256));
                encryptedData = new FieldPwdPackageDTO()
                {
                    EnfInitialisationVectorBase64 = ivString,
                    EnfCipherTextBase64 = cipherString,
                    EnfAuthTagBase64 = tagString,
                };
            }
            catch (Exception)
            {
                throw new ParsingEncryptedParameterException(["Encrypt_FieldPwdPackageDTO"]);
            }
        }

        public void DecryptFieldPwdPackageDTOVaultSession(VaultSession vs, FieldPwdPackageDTO data, out FieldPwdPackageDTO decryptedData)
        {
            byte[] prKey = vs.VasPrivateKey ?? throw new UserVaultSessionExpiredException();

            try
            {
                RSA rsa = RSA.Create(2048);
                rsa.ImportRSAPrivateKey(prKey, out _);
                byte[] ivBytes = Convert.FromBase64String(data.EnfInitialisationVectorBase64 ?? throw new ArgumentNullException());
                string ivString = Encoding.UTF8.GetString(rsa.Decrypt(ivBytes, RSAEncryptionPadding.OaepSHA256));
                byte[] cipherBytes = Convert.FromBase64String(data.EnfCipherTextBase64 ?? throw new ArgumentNullException());
                string cipherString = Encoding.UTF8.GetString(rsa.Decrypt(cipherBytes, RSAEncryptionPadding.OaepSHA256));
                byte[] tagBytes = Convert.FromBase64String(data.EnfAuthTagBase64 ?? throw new ArgumentNullException());
                string tagString = Encoding.UTF8.GetString(rsa.Decrypt(tagBytes, RSAEncryptionPadding.OaepSHA256));
                decryptedData = new FieldPwdPackageDTO()
                {
                    EnfInitialisationVectorBase64 = ivString,
                    EnfCipherTextBase64 = cipherString,
                    EnfAuthTagBase64 = tagString,
                };
            }
            catch (Exception)
            {
                throw new ParsingEncryptedParameterException(["Decrypt_FieldPwdPackageDTO"]);
            }
        }

        public void DecryptStringVaultSession(VaultSession vs, string data, out string decryptedData)
        {
            byte[] prKey = vs.VasPrivateKey ?? throw new UserVaultSessionExpiredException();

            try
            {
                RSA rsa = RSA.Create(2048);
                rsa.ImportRSAPrivateKey(prKey, out _);
                byte[] ivBytes = Convert.FromBase64String(data);
                decryptedData = Encoding.UTF8.GetString(rsa.Decrypt(ivBytes, RSAEncryptionPadding.OaepSHA256));
            }
            catch (Exception)
            {
                throw new ParsingEncryptedParameterException(["StringVaultSession"]);
            }
        }

        // Each time the route need an authentification through the public / private session key 
        // The dto will contain the vault id in GUID and the vault id encrypted
        // We can verifiy that the user has the good public key with a simple if
        public void AuthenticateVaultSession(VaultSession vs, string encryptedData, Guid authData)
        {
            this.DecryptStringVaultSession(vs, encryptedData, out string? decryptedData);
            if (Guid.TryParse(decryptedData, out Guid idEncrypted) == false)
                throw new UserVaultSessionExpiredException();
            if (idEncrypted != authData)
                throw new UserVaultSessionExpiredException();
        }
    }
}