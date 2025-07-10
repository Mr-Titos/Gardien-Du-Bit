using common_gardienbit.DTO.Vault;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class TotpVaultDialog
    {
        [Parameter]
        public CreateSuccessVaultDTO? Vault { get; set; }


        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }

        protected override void OnInitialized()
        { }

        private void Submit()
        {
            MudDialog?.Close(DialogResult.Ok(""));
        }
        private void Cancel() => MudDialog?.Cancel();

    }
}
