using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class UpdateVaultDialog
    {
        [Parameter]
        public required GetVaultDTO Vault { get; set; }

        [Parameter]
        public UserVaultSession? Session { get; set; }

        public string? Name { get; set; }
        public bool Favorite { get; set; }
        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                EvaluatePasswordStrength();
            }
        }
        public int ValidityPwd { get; set; } = 25;

        bool isShow;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        int PasswordStrength { get; set; } = 0;
        Color PasswordStrengthColor { get; set; } = Color.Default;
        string PasswordStrengthText { get; set; } = "Faible";

        [CascadingParameter]
        private IMudDialogInstance MudDialog { get; set; } = default!;

        protected override void OnInitialized()
        {
            if (Vault != null)
            {
                Name = Vault.vauName;
                Favorite = Vault.vauFavorite;
            }
        }

        void EvaluatePasswordStrength()
        {
            int score = 0;
            if (string.IsNullOrWhiteSpace(Password)) score = 0;
            else
            {
                if (Password.Length >= 8) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"\d")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}|<>]")) score++;
            }

            PasswordStrength = score * 20;

            switch (score)
            {
                case 0:
                case 1:
                    PasswordStrengthColor = Color.Error;
                    PasswordStrengthText = "Très faible";
                    break;
                case 2:
                    PasswordStrengthColor = Color.Warning;
                    PasswordStrengthText = "Faible";
                    break;
                case 3:
                    PasswordStrengthColor = Color.Info;
                    PasswordStrengthText = "Moyen";
                    break;
                case 4:
                    PasswordStrengthColor = Color.Success;
                    PasswordStrengthText = "Fort";
                    break;
                case 5:
                    PasswordStrengthColor = Color.Success;
                    PasswordStrengthText = "Très fort";
                    break;
            }
        }
        void ButtonVisibilityClick()
        {
            if (isShow)
            {
                isShow = false;
                PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                PasswordInput = InputType.Password;
            }
            else
            {
                isShow = true;
                PasswordInputIcon = Icons.Material.Filled.Visibility;
                PasswordInput = InputType.Text;
            }
        }

        private void Submit()
        {
            var updateVault = new UpdateDialogResultModel
            {
                vauName = Name,
                vauFavorite = Favorite,
                vauId = Vault.vauId,
                vauPassword = Password
            };
            MudDialog.Close(DialogResult.Ok(updateVault));
        }
        private void Cancel() => MudDialog.Cancel();

    }
}
