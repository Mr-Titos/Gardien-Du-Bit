using api_gardienbit.Models;
using api_gardienbit.Repositories;
using common_gardienbit.DTO.Category;
using common_gardienbit.DTO.Exception;
using Microsoft.AspNetCore.Mvc;

namespace api_gardienbit.Controllers_
{
    //[Authorize]
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly VaultRepository _vaultRepository;

        public CategoryController(CategoryRepository categoryRepository, VaultRepository vaultRepository)
        {
            _categoryRepository = categoryRepository;
            _vaultRepository = vaultRepository;
        }

        [HttpGet("{vauId}")]
        public ActionResult<IEnumerable<GetCategoryDTO>> GetCategories(Guid vauId)
        {
            var category = _categoryRepository.GetCategoriesByVault(vauId);
            var categoriesDtos = category.Select(c => new GetCategoryDTO
            {
                catId = c.CatId,
                catName = c.CatName,
                catVauId = c!.CatVault!.VauId

            }).ToList();

            return Ok(categoriesDtos);
        }

        [HttpPost]
        public async Task<ActionResult<CreateCategoryDTO>> CreateCategory(CreateCategoryDTO catDTO)
        {
            Vault? vault = await _vaultRepository.GetObjectById(catDTO.vaultId);
            if (vault == null)
                throw new EntityNotFoundException<Vault>();

            var category = new Category
            {
                CatId = new Guid(),
                CatEntryDate = DateTime.Now,
                CatName = catDTO.catName,
                CatVault = vault

            };
            var createdObj = await _categoryRepository.CreateObject(category);

            return Created("api/[controller]", new GetCategoryDTO()
            {
                catId = createdObj.CatId,
                catName = createdObj.CatName,
                catVauId = createdObj!.CatVault!.VauId
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, UpdateCategoryDTO catDTO)
        {
            var category = await _categoryRepository.GetObjectById(id);

            if (category == null)
                throw new EntityNotFoundException<Category>();

            category.CatName = catDTO.catName;

            var objToSave = await _categoryRepository.UpdateObject(id, category);
            if (objToSave == null)
                throw new Exception("Error in the category update");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            bool isDeleted = await _categoryRepository.DeleteObject(id);

            if (!isDeleted)
                throw new EntityNotFoundException<Category>();

            return NoContent();
        }
    }
}
