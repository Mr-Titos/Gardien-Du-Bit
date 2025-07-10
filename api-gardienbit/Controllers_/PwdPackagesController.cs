using api_gardienbit.DAL;
using api_gardienbit.Models;
using api_gardienbit.Repositories;
using api_gardienbit.Services;
using api_gardienbit.Utils;
using common_gardienbit.DTO.Category;
using common_gardienbit.DTO.Exception;
using common_gardienbit.DTO.PwdPackage;
using common_gardienbit.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace api_gardienbit.Controllers_
{
    //[Authorize]
    [ApiController]
    [Route("api/pwdpackage")]
    public class PwdPackagesController(EFContext context, PwdPackageRepository pwdPackageRepository, VaultRepository vaultRepository,
        CategoryRepository categoryRepository, UserCacheService userCacheService, UserIdentifierService userIdentifierService, LoggingService loggingService)
        : ControllerBase
    {

        private readonly EFContext _context = context;
        private readonly VaultRepository _vaultRepository = vaultRepository;
        private readonly PwdPackageRepository _pwdPackageRepository = pwdPackageRepository;
        private readonly CategoryRepository _categoryRepository = categoryRepository;
        private readonly UserCacheService _userCacheService = userCacheService;
        private readonly UserIdentifierService _userIdentifierService = userIdentifierService;
        private readonly LoggingService _loggingService = loggingService;

        [HttpGet("fromVault/{id}")]
        public async Task<ActionResult<IEnumerable<GetPwdPackageDTO>>> GetPwdPackages(Guid id)
        {
            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), id) ?? throw new UserVaultSessionExpiredException();
            var pwdPackages = await _pwdPackageRepository.GetObjectsFromVault(id);

            List<GetPwdPackageDTO> pwdPackagesDtos = [];
            pwdPackages.ForEach(async c =>
            {
                pwdPackagesDtos.Add(new GetPwdPackageDTO()
                {
                    pwpCom = DTOMapperUtils.MapToFieldPwdPackageDTO(c.PwpCom),
                    pwpName = DTOMapperUtils.MapToFieldPwdPackageDTO(c.PwpName),
                    pwpContent = DTOMapperUtils.MapToFieldPwdPackageDTO(c.PwpContent),
                    pwpUrl = DTOMapperUtils.MapToFieldPwdPackageDTO(c.PwpUrl),
                    pwpEntryDate = c.PwpEntryDate,
                    pwpId = c.PwpId,
                    pwpLastUpdateDate = c.PwpLastUpdateDate,
                    pwpCategory = new GetCategoryDTO
                    {
                        catId = c.PwpCategory?.CatId ?? Guid.Empty,
                        catName = c.PwpCategory?.CatName ?? "",
                        catVauId = c.PwpCategory?.CatVault?.VauId ?? Guid.Empty,
                    }
                });
                await _loggingService.LogAsync(LogActionName.Visualize, c.PwPVault, c.PwpId);
            });

            return Ok(pwdPackagesDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetPwdPackageDTO>> GetPwdPackage(Guid id)
        {
            var pwdPackage = await _pwdPackageRepository.GetObjectById(id) ?? throw new EntityNotFoundException<PwdPackage>();
            _ = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), pwdPackage.PwPVault.VauId) ?? throw new UserVaultSessionExpiredException();

            var pwdPackageDTO = new GetPwdPackageDTO
            {
                pwpId = pwdPackage.PwpId,
                pwpEntryDate = pwdPackage.PwpEntryDate,
                pwpLastUpdateDate = pwdPackage.PwpLastUpdateDate,
                pwpCom = DTOMapperUtils.MapToFieldPwdPackageDTO(pwdPackage.PwpCom),
                pwpName = DTOMapperUtils.MapToFieldPwdPackageDTO(pwdPackage.PwpName),
                pwpContent = DTOMapperUtils.MapToFieldPwdPackageDTO(pwdPackage.PwpContent),
                pwpUrl = DTOMapperUtils.MapToFieldPwdPackageDTO(pwdPackage.PwpUrl),
                pwpCategory = new GetCategoryDTO
                {
                    catId = pwdPackage.PwpCategory?.CatId ?? Guid.Empty,
                    catName = pwdPackage.PwpCategory?.CatName ?? "",
                    catVauId = pwdPackage.PwpCategory?.CatVault?.VauId ?? Guid.Empty,
                }
            };
            await _loggingService.LogAsync(LogActionName.Visualize, pwdPackage.PwPVault, pwdPackage.PwpId);

            return Ok(pwdPackageDTO);
        }

        [HttpPost]
        public async Task<ActionResult<CreatePwdPackageDTO>> CreatePwdPackage(CreatePwdPackageDTO pwdPackagesDto)
        {
            Vault vault = await _vaultRepository.GetObjectById(pwdPackagesDto.vauId) ?? throw new EntityNotFoundException<Vault>();
            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault) ?? throw new UserVaultSessionExpiredException();

            _userCacheService.AuthenticateVaultSession(vs, pwdPackagesDto.encryptedAuth, pwdPackagesDto.vauId); // throws if the auth is not valid

            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpContent, out FieldPwdPackageDTO decryptedContent);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpCom, out FieldPwdPackageDTO decryptedCom);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpName, out FieldPwdPackageDTO decryptedName);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpUrl, out FieldPwdPackageDTO decryptedUrl);

            Category? category = await _categoryRepository.GetObjectById(pwdPackagesDto.catId);

            var pwdPackage = new PwdPackage
            {
                PwpId = new Guid(),
                PwpEntryDate = DateTime.UtcNow,
                PwpLastUpdateDate = DateTime.UtcNow,
                PwpCom = DTOMapperUtils.MapToEncryptedField(decryptedCom),
                PwpName = DTOMapperUtils.MapToEncryptedField(decryptedName),
                PwpContent = DTOMapperUtils.MapToEncryptedField(decryptedContent),
                PwpUrl = DTOMapperUtils.MapToEncryptedField(decryptedUrl),
                PwPVault = vault,
                PwpCategory = category,
            };

            _ = await _pwdPackageRepository.CreateObject(pwdPackage) ?? throw new DbUpdateException("Error in the pwdPackage creation");
            await _loggingService.LogAsync(LogActionName.Create, pwdPackage.PwPVault, pwdPackage.PwpId);

            var dtoToSend = new GetPwdPackageDTO()
            {
                pwpCom = pwdPackagesDto.pwpCom,
                pwpContent = pwdPackagesDto.pwpContent,
                pwpName = pwdPackagesDto.pwpName,
                pwpUrl = pwdPackagesDto.pwpUrl,
                pwpEntryDate = pwdPackage.PwpEntryDate,
                pwpLastUpdateDate = pwdPackage.PwpLastUpdateDate,
                pwpId = pwdPackage.PwpId,
                pwpCategory = new GetCategoryDTO
                {
                    catId = pwdPackage.PwpCategory?.CatId ?? Guid.Empty,
                    catName = pwdPackage.PwpCategory?.CatName ?? "",
                    catVauId = pwdPackage.PwpCategory?.CatVault?.VauId ?? Guid.Empty,
                }
            };
            return CreatedAtAction(nameof(GetPwdPackage), new { id = pwdPackage.PwpId }, dtoToSend);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePwdPackage(string id, UpdatePwdPackageDTO pwdPackagesDto)
        {
            Vault vault = await _vaultRepository.GetObjectById(pwdPackagesDto.vauId) ?? throw new EntityNotFoundException<Vault>();
            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), vault) ?? throw new UserVaultSessionExpiredException();
            var pwdPackage = await _pwdPackageRepository.GetObjectById(Guid.Parse(id)) ?? throw new EntityNotFoundException<PwdPackage>();
            Category? category = await _categoryRepository.GetObjectById(pwdPackagesDto.catId ?? Guid.Empty);

            _userCacheService.AuthenticateVaultSession(vs, pwdPackagesDto.encryptedAuth, pwdPackagesDto.vauId); // throws if the auth is not valid

            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpContent, out FieldPwdPackageDTO decryptedContent);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpCom, out FieldPwdPackageDTO decryptedCom);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpName, out FieldPwdPackageDTO decryptedName);
            _userCacheService.DecryptFieldPwdPackageDTOVaultSession(vs, pwdPackagesDto.pwpUrl, out FieldPwdPackageDTO decryptedUrl);

            pwdPackage.PwpLastUpdateDate = DateTime.Now;
            pwdPackage.PwpCom = DTOMapperUtils.MapToEncryptedField(decryptedCom);
            pwdPackage.PwpName = DTOMapperUtils.MapToEncryptedField(decryptedName);
            pwdPackage.PwpContent = DTOMapperUtils.MapToEncryptedField(decryptedContent);
            pwdPackage.PwpUrl = DTOMapperUtils.MapToEncryptedField(decryptedUrl);
            pwdPackage.PwpCategory = category ?? pwdPackage.PwpCategory;

            _ = await _pwdPackageRepository.UpdateObject(Guid.Parse(id), pwdPackage) ?? throw new DbUpdateException("Error in the pwdPackage update");
            await _loggingService.LogAsync(LogActionName.Update, pwdPackage.PwPVault, pwdPackage.PwpId);

            return NoContent();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeletePwdPackages(DeletePwdPackageDTO pwdPackageDTO)
        {
            var pwdPackage = await _pwdPackageRepository.GetObjectById(pwdPackageDTO.pwpId) ?? throw new EntityNotFoundException<PwdPackage>();
            var vs = await _userCacheService.GetUserVaultSession(_userIdentifierService.CurrentInternalUser ?? throw new EntityNotFoundException<Client>(), pwdPackage.PwPVault) ?? throw new UserVaultSessionExpiredException();

            _userCacheService.AuthenticateVaultSession(vs, pwdPackageDTO.enryptedAuth, pwdPackageDTO.pwpId); // throws if the auth is not valid

            await _pwdPackageRepository.DeleteObject(pwdPackageDTO.pwpId);
            await _loggingService.LogAsync(LogActionName.Delete, pwdPackage.PwPVault, pwdPackage.PwpId);

            return NoContent();
        }
    }

}

