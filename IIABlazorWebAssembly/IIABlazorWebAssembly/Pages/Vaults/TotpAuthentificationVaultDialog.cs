using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class TotpAuthentificationVaultDialog
    {
        public string Token { get; set; } = string.Empty;

        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }

        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(Token))
                return;

            MudDialog?.Close(DialogResult.Ok(Token));
        }

        private void Cancel() => MudDialog?.Cancel();
    }
}
