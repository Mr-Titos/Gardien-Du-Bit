using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.AuthenticationVault
{
    partial class AuthenticationVaultDialog
    {
        public string Password { get; set; } = string.Empty;

        private bool isShow = false;
        private InputType PasswordInput = InputType.Password;
        private string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;

        [CascadingParameter]
        private IMudDialogInstance? MudDialog { get; set; }

        [Inject]
        private EncryptionService? EncryptionService { get; set; }

        private void TogglePasswordVisibility()
        {
            isShow = !isShow;
            PasswordInputIcon = isShow ? Icons.Material.Filled.Visibility : Icons.Material.Filled.VisibilityOff;
            PasswordInput = isShow ? InputType.Text : InputType.Password;
        }
        private void Submit()
        {
            if (string.IsNullOrWhiteSpace(Password))
                return;

            MudDialog?.Close(DialogResult.Ok(Password));
        }

        private void Cancel() => MudDialog?.Cancel();
    }
}
