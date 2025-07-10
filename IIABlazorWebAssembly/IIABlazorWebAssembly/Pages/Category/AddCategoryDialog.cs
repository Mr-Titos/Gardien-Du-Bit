using common_gardienbit.DTO.Category;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.Category
{
    partial class AddCategoryDialog
    {
        [Parameter] public Guid VaultId { get; set; }
        [Inject] IDialogService? DialogService { get; set; }
        [Inject] APIService? apiService { get; set; }
        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }
        private string categoryName { get; set; } = "";
        private void Submit()
        {
            try
            {
                var categoryAdd = new CreateCategoryDTO
                {
                    catName = categoryName,
                    vaultId = VaultId
                };
                MudDialog?.Close(DialogResult.Ok(categoryAdd));

            }
            catch (Exception)
            {
            }
        }
        private void Cancel() => MudDialog?.Cancel();
    }
}
