using common_gardienbit.DTO.Vault;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class AddVaultDialog
    {
        public string? Name { get; set; }
        public bool Favorite { get; set; }
        public bool isTotp { get; set; } = false;
        private string? _password;
        private int _length = 12;
        private bool _includeUpper { get; set; } = true;
        private bool _includeLower { get; set; } = true;
        private bool _includeSpecial { get; set; } = true;
        private int _minDigits = 2;

        private bool generatePasswordDisplay { get; set; } = false;

        public string? Password
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
        private IMudDialogInstance? MudDialog { get; set; }

        void EvaluatePasswordStrength()
        {
            int score = 0;
            PasswordStrengthText = "Très faible";
            PasswordStrengthColor = Color.Error;

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordStrength = 0;
                return;
            }

            var length = Password.Length;

            if (length >= 8) score += 1;
            if (length >= 12) score += 1;
            if (length >= 16) score += 1;



            if (Regex.IsMatch(Password, @"\d")) score += 1;
            if (Regex.IsMatch(Password, @"[a-z]")) score += 1;
            if (Regex.IsMatch(Password, @"[A-Z]")) score += 1;
            if (Regex.IsMatch(Password, @"[!@#$%^&*(),.?""{}[\]|<>/~`_+=\\-]")) score += 1;


            if (!Regex.IsMatch(Password, @"(.)\1{2,}")) score += 1;

            // Mot de passe non basé sur un mot commun ?
            var lowerPwd = Password.ToLower();

            // Note max sur 10
            PasswordStrength = score * 10;

            // Affichage personnalisé
            switch (score)
            {
                case <= 2:
                    PasswordStrengthColor = Color.Error;
                    PasswordStrengthText = "Très faible";
                    break;
                case <= 4:
                    PasswordStrengthColor = Color.Warning;
                    PasswordStrengthText = "Faible";
                    break;
                case <= 6:
                    PasswordStrengthColor = Color.Info;
                    PasswordStrengthText = "Moyen";
                    break;
                case <= 8:
                    PasswordStrengthColor = Color.Success;
                    PasswordStrengthText = "Fort";
                    break;
                default:
                    PasswordStrengthColor = Color.Success;
                    PasswordStrengthText = "Très fort";
                    break;
            }
        }

        void ButtonShowPassword()
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
            if (string.IsNullOrWhiteSpace(Name))
            {
                // Afficher un message d'erreur ou gérer la validation ici
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                // Afficher un message d'erreur ou gérer la validation ici
                return;
            }

            var hash = encryptionService.HashPasswordString(Password);
            var salt = encryptionService.GenerateSalt();
            var addVault = new CreateVaultDTO
            {
                vauName = Name,
                vauFavorite = Favorite,
                vauHash = Convert.ToBase64String(hash),
                vauSalt = Convert.ToBase64String(salt),
                vauTotp = isTotp,
            };
            MudDialog?.Close(DialogResult.Ok(addVault));
        }
        private void GeneratePassword()
        {
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*()_-+=<>?";

            var availableChars = new List<char>();

            if (_includeLower) availableChars.AddRange(lower);
            if (_includeUpper) availableChars.AddRange(upper);
            if (_includeSpecial) availableChars.AddRange(specials);
            if (_minDigits > 0) availableChars.AddRange(digits);

            if (availableChars.Count == 0)
            {
                Password = "⚠️ Sélection invalide";
                return;
            }

            var password = new List<char>();

            if (_minDigits > 0)
            {
                for (int i = 0; i < _minDigits; i++)
                {
                    password.Add(GetRandomChar(digits));
                }
            }

            while (password.Count < _length)
            {
                char nextChar = GetRandomChar(availableChars);

                if (!_includeLower && char.IsLower(nextChar)) continue;
                if (!_includeUpper && char.IsUpper(nextChar)) continue;
                if (!_includeSpecial && specials.Contains(nextChar)) continue;
                if (_minDigits == 0 && digits.Contains(nextChar)) continue;

                password.Add(nextChar);
            }

            Password = new string(password.OrderBy(_ => GetRandomInt()).ToArray());
        }


        private char GetRandomChar(string charset)
        {
            return charset[GetRandomInt(0, charset.Length)];
        }


        private char GetRandomChar(List<char> charset)
        {
            return charset[GetRandomInt(0, charset.Count)];
        }


        private int GetRandomInt(int min = 0, int max = int.MaxValue)
        {
            if (min >= max)
                throw new ArgumentException("min doit être inférieur à max");

            var diff = (long)max - min;
            var uint32Buffer = new byte[4];

            using (var rng = RandomNumberGenerator.Create())
            {
                while (true)
                {
                    rng.GetBytes(uint32Buffer);
                    uint rand = BitConverter.ToUInt32(uint32Buffer, 0);

                    const long maxExclusive = 1L + uint.MaxValue;
                    long remainder = maxExclusive % diff;

                    if (rand < maxExclusive - remainder)
                    {
                        return (int)(min + (rand % diff));
                    }
                }
            }
        }

        private void Cancel()
        {
            generatePasswordDisplay = false;
            MudDialog?.Cancel();
        }
    }
}
