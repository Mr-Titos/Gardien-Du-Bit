using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class DeleteVaultDialog
    {
        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }
        [Parameter] public Guid VaultId { get; set; }
        private void Cancel()
        {
            MudDialog?.Cancel();
        }
        private void Confirm()
        {

            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}
