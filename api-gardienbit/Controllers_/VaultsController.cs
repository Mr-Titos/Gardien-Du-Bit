using api_gardienbit.Models;
using api_gardienbit.Repositories;
using api_gardienbit.Services;
using common_gardienbit.DTO.Exception;
using common_gardienbit.DTO.Vault;
using common_gardienbit.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using System.Security.Cryptography;
using System.Text;

namespace api_gardienbit.Controllers_
{
    [ApiController]
    [Route("api/vaults")]
    public class VaultsController(VaultRepository vaultRepository, VaultUserLinkRepository vaultUserLinkRepository, UserIdentifierService userIdentifierService,
        UserCacheService userCacheService, TotpService totpService, LoggingService loggingService) : ControllerBase
    {
        private readonly VaultRepository _vaultRepository = vaultRepository;
        private readonly VaultUserLinkRepository _vaultUserLinkRepository = vaultUserLinkRepository;
        private readonly UserIdentifierService _userIdentifierService = userIdentifierService;
        private readonly UserCacheService _userCacheService = userCacheService;
        private readonly TotpService _totpService = totpService;
        private readonly LoggingService _loggingService = loggingService;

        [HttpGet]
        [Authorize(Roles = Role.Admin)]
        public ActionResult<IEnumerable<GetVaultDTO>> GetVaultsAsync()
        {
            var vaults = _vaultRepository.GetObjects();
            List<GetVaultDTO> vaultsDtos = [];

            vaults.ToList().ForEach(async v =>
            {
                vaultsDtos.Add(new GetVaultDTO
                {
                    vauId = v.VauId,
                    vauFavorite = v.VauFavorite,
                    vauName = v.VauName,
                    isShared = v.IsShared,
                    isTotp = v.VauTOTP != null,
                });
                await _loggingService.LogAsync(LogActionName.Visualize, v);
            });

            return Ok(vaultsDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetVaultDTO>> GetVault(Guid id)
        {
            var vault = await _vaultRepository.GetObjectById(id);

            if (vault == null)
                throw new EntityNotFoundException<Vault>();

            var vaultDTO = new GetVaultDTO
            {
                vauId = vault.VauId,
                vauFavorite = vault.VauFavorite,
                vauName = vault.VauName,
                isTotp = vault.VauTOTP != null,
            };
            _ =  _loggingService.LogAsync(LogActionName.Visualize, vault);
            return Ok(vaultDTO);
        }

        [HttpPost]
        public async Task<ActionResult<CreateVaultDTO>> CreateVault(CreateVaultDTO vaultDto)
        {
            Client? client = _userIdentifierService.CurrentInternalUser;
            if (client == null)
                throw new EntityNotFoundException<Client>();

            byte[]? totpKey = null;
            if (vaultDto.vauTotp)
                totpKey = KeyGeneration.GenerateRandomKey(20);

            var vault = new Vault
            {
                VauId = new Guid(),
                VauFavorite = vaultDto.vauFavorite,
                VauHash = Convert.FromBase64String(vaultDto.vauHash),
                VauName = vaultDto.vauName,
                VauSalt = Convert.FromBase64String(vaultDto.vauSalt),
                VauClient = client,
                VauTOTP = totpKey,
                VauLastUpdateDate = DateTime.UtcNow,
                VauEntryDate = DateTime.UtcNow
            };
            var createdObj = await _vaultRepository.CreateObject(vault);

            string qrCode = "";
            if (vaultDto.vauTotp)
                qrCode = _totpService.GetQrCode(vault);

            await _loggingService.LogAsync(LogActionName.Create, vault);
            return Created("api/[controller]", new CreateSuccessVaultDTO()
            {
                cliId = client.CliId,
                vauId = createdObj.VauId,
                vauFavorite = createdObj.VauFavorite,
                vauName = createdObj.VauName,
                isShared = createdObj.IsShared,
                qrCodeTotp = qrCode,
            });
        }

        [HttpPut("{idString}")]
        public async Task<IActionResult> UpdateVault(string idString, UpdateVaultDTO vaultDto)
        {
            if (String.IsNullOrEmpty(idString))
                throw new ParameterMissingOrIncorrectException(["vaultId"]);

            Guid id = Guid.Parse(idString);
            var vault = await _vaultRepository.GetObjectById(id) ?? throw new ParameterMissingOrIncorrectException(["vaultId"]);
            var vaultSession = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault) ?? throw new UserVaultSessionExpiredException();
            try
            {
                RSA rsa = RSA.Create(2048);
                rsa.ImportRSAPrivateKey(vaultSession.VasPrivateKey, out _);

                byte[] decryptedFavorite = rsa.Decrypt(Convert.FromBase64String(vaultDto.vauFavorite ?? throw new ArgumentNullException()), RSAEncryptionPadding.OaepSHA256);
                string decryptedStringFavorite = Encoding.UTF8.GetString(decryptedFavorite);
                if (Boolean.TryParse(decryptedStringFavorite, out bool favoriteParseResult) == false)
                    throw new ParsingEncryptedParameterException(["vauFavorite"]);

                byte[] decryptedName = rsa.Decrypt(Convert.FromBase64String(vaultDto.vauName), RSAEncryptionPadding.OaepSHA256);
                string decryptedStringName = Encoding.UTF8.GetString(decryptedName);

                if (String.IsNullOrEmpty(decryptedStringName))
                    throw new ParsingEncryptedParameterException(["vauFavorite"]); ;

                if (String.IsNullOrEmpty(decryptedStringFavorite))
                    throw new ParsingEncryptedParameterException(["vauName"]);

                vault.VauName = decryptedStringName;
                vault.VauFavorite = favoriteParseResult;
            }
            catch (Exception)
            {
                throw new ParsingEncryptedParameterException(["unknwown"]);
            }

            _ = await _vaultRepository.UpdateObject(id, vault) ?? throw new EntityNotFoundException<Vault>();
            await _loggingService.LogAsync(LogActionName.Update, vault);

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult> LoginVault(LoginVaultDTO vaultDto)
        {
            var vault = await _vaultRepository.GetObjectById(vaultDto.vauId) ?? throw new EntityNotFoundException<Vault>();

            var vaults = await _vaultRepository.GetVaultsAccessibleByUser(_userIdentifierService.CurrentInternalUser?.CliId ?? throw new EntityNotFoundException<Client>());

            if (vaults.Any(v => v.VauId.Equals(vault.VauId)) == false)
                return Unauthorized();

            string hash64 = Convert.ToBase64String(vault.VauHash);

            if (String.Equals(vaultDto.vauHash, hash64) == false)
                throw new WrongVaultPasswordException();

            var vs = await _userCacheService.CreateUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault);
            var vaultSuccessDto = new LoginSuccessVaultDTO
            {
                vauId = vault.VauId,
                vauPKey = vs.VasPublicKey,
                vauSalt = vault.VauSalt
            };
            return Ok(vaultSuccessDto);
        }

        [HttpPost("totp")]
        public async Task<IActionResult> VerifyTotp(CheckTotpVaultDTO totpVaultDTO)
        {
            var vault = await _vaultRepository.GetObjectById(totpVaultDTO.vauId) ?? throw new EntityNotFoundException<Vault>();
            var vs = await _userCacheService.GetUserVaultSessionTotp(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault.VauId) ?? throw new UserVaultSessionExpiredException();
            var totp = new Totp(vault.VauTOTP ?? throw new EntityNotFoundException<Totp>());

            if (totp.VerifyTotp(totpVaultDTO.token, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay) == false)
                return Unauthorized("Code TOTP invalide");

            await _userCacheService.UpdateTotpVaultSession(vs, true);
            // Continuer avec la logique d’accès au coffre
            return Ok("Accès au coffre autorisé");
        }

        [HttpPost("getKey")]
        public async Task<ActionResult> KeyVault(IdVaultDto vaultDto)
        {
            var vault = await _vaultRepository.GetObjectById(vaultDto.vauId) ?? throw new EntityNotFoundException<Vault>();

            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault) ?? throw new UserVaultSessionExpiredException();

            _userCacheService.AuthenticateVaultSession(vs, vaultDto.vauEncryptedData, vault.VauId);


            var vaultKeyDto = new KeyVaultDTO
            {
                vauPrivateKey = vs.VasEncryptionKeyPrivate,
                vauPublicKey = vs.VasEncryptionKeyPublic
            };
            return Ok(vaultKeyDto);
        }

        [HttpGet("resetSession/{vaultId}")]
        public async Task<ActionResult> ResetSession(Guid vaultId)
        {
            var vault = await _vaultRepository.GetObjectById(vaultId) ?? throw new EntityNotFoundException<Vault>();

            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault) ?? throw new UserVaultSessionExpiredException();
            var resetDTO = new ResetSessionVaultDTO()
            {
                keyVaultDTO = new KeyVaultDTO
                {
                    vauPrivateKey = vs.VasPrivateKey,
                    vauPublicKey = vs.VasPublicKey
                },
                successDTO = new LoginSuccessVaultDTO
                {
                    vauId = vault.VauId,
                    vauPKey = vs.VasPublicKey,
                    vauSalt = vault.VauSalt
                }
            };

            return Ok(resetDTO);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<GetVaultDTO>>> GetUserVaults()
        {
            var currentUserId = _userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>();

            var vaults = await _vaultRepository.GetVaultsAccessibleByUser(currentUserId.CliId);

            List<GetVaultDTO> vaultsDtos = [];
            vaults.ToList().ForEach(async v =>
            {
                vaultsDtos.Add(new GetVaultDTO
                {
                    vauId = v.VauId,
                    vauFavorite = v.VauFavorite,
                    vauName = v.VauName,
                    isShared = v.IsShared,
                    isTotp = v.VauTOTP != null,
                });
                await _loggingService.LogAsync(LogActionName.Visualize, v);
            });

            return Ok(vaultsDtos);
        }

        [HttpPost("{id}/share")]
        public async Task<IActionResult> GenerateShareLink(Guid id, [FromBody] int nbUsed)
        {
            var currentUser = _userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>();
            string token = TokenGenerator.GenerateSecureToken();

            var vaultAccess = await _vaultRepository.SaveVaultShareTokenAsync(id, token.ToString(), currentUser.CliId, nbUsed);
            string? qrCode = null;
            if (vaultAccess?.Vault?.VauTOTP != null)
                qrCode = _totpService.GetQrCode(vaultAccess.Vault);

            var vaultAccessDto = new GetVaultAccessDTO
            {
                token = token,
                vaultId = vaultAccess?.Vault?.VauId,
                userId = vaultAccess!.UserId,
                nbUsed = vaultAccess.NbUsed,
                qrCodeTotp = qrCode

            };
            await _loggingService.LogAsync(LogActionName.Share, vaultAccess.Vault);
            return Ok(vaultAccessDto);
        }

        [HttpGet("{id}/share")]
        public async Task<IActionResult> GetShareLink(Guid id)
        {
            var vaultAccess = await _vaultRepository.GetVaultShareTokenAsync(id);
            string? qrCode = null;
            if (vaultAccess?.Vault?.VauTOTP != null)
                qrCode = _totpService.GetQrCode(vaultAccess.Vault);

            var vaultsAccessDtos = new GetVaultAccessDTO
            {
                token = vaultAccess?.Token,
                vaultId = vaultAccess?.VaultId,
                userId = vaultAccess!.UserId,
                isUsed = vaultAccess?.IsUsed,
                nbUsed = vaultAccess?.NbUsed,
                qrCodeTotp = qrCode
            };

            return Ok(vaultsAccessDtos);
        }
        [HttpPost("share/consume")]
        public async Task<IActionResult> ConsumeShareToken([FromBody] string token)
        {
            var currentUser = _userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>();

            var result = await _vaultRepository.AddUserToVaultFromToken(token, currentUser.CliId);
            var vaultsAccessDtos = new GetVaultAccessDTO
            {
                vaultId = result.VaultId,


            };

            return Ok(vaultsAccessDtos);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVault(Guid id)
        {
            var vault = await _vaultRepository.GetObjectById(id) ?? throw new EntityNotFoundException<Vault>();
            if (vault?.VauClient?.CliId != _userIdentifierService.CurrentInternalUser?.CliId)
                return Unauthorized();
            await _vaultRepository.DeleteObject(id);
            await _loggingService.LogAsync(LogActionName.Delete, vault);
            return Ok();
        }

        [HttpGet("{id}/revoke")]
        public async Task<IActionResult> GetRevokeSharedUsers(Guid id)
        {
            var vaultAccess = await _vaultRepository.GetVaultAccessibleByUsers(id) ?? throw new EntityNotFoundException<VaultUserLink>(); ;

            var vaultsAccessDtos = vaultAccess.Select(c => new GetRevokeSharedDTO
            {
                id = c.Id,
                vaultId = c.VaultId,
                userId = c.UserId,
                userName = c.User!.CliName,

            }).ToList();
            return Ok(vaultsAccessDtos);
        }

        [HttpDelete("{userId}/{vaultId}/revoke")]
        public async Task<IActionResult> DeleteRevokeSharedUsers(Guid userId, Guid vaultId)
        {

            await _vaultUserLinkRepository.DeleteVaultUserLink(vaultId, userId);

            return Ok();
        }
    }
}
