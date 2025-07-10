using common_gardienbit.DTO.Category;
using common_gardienbit.DTO.Exception;
using common_gardienbit.DTO.PwdPackage;
using IIABlazorWebAssembly.Pages.Category;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class VaultPage
    {
        [Inject] IDialogService DialogService { get; set; } = default!;
        private List<GetPwdPackageDTO> pwdPackages = [];
        private List<GetCategoryDTO> categories = [];

        private GetPwdPackageDTO? selectedPackage;
        private bool isAdding = false;
        private bool isEditing = false;
        private bool isShared { get; set; }

        [Parameter] public string vaultId { get; set; } = "";

        [Inject] VaultService VaultContext { get; set; } = default!;
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] MudAlertService mudAlertService { get; set; } = default!;


        // Champs pour l'ajout
        private string newName = "";
        private string newUrl = "";
        private string newComment = "";
        private GetCategoryDTO? newCategory { get; set; }
        private Guid? selectedCategoryId = null;



        private string? newPassword;
        private int _length = 12;
        private bool _includeUpper { get; set; } = true;
        private bool _includeLower { get; set; } = true;
        private bool _includeSpecial { get; set; } = true;
        private int _minDigits = 2;

        private bool generatePasswordDisplay { get; set; } = false;

        public string? Password
        {
            get => newPassword;
            set
            {
                newPassword = value;
                EvaluatePasswordStrength();
            }
        }

        bool isShow;
        InputType PasswordInput = InputType.Password;
        string PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
        int PasswordStrength { get; set; } = 0;
        Color PasswordStrengthColor { get; set; } = Color.Default;
        string PasswordStrengthText { get; set; } = "Faible";
        protected override async void OnInitialized()
        {
            await this.LoadPwdPackages();
            await this.LoadCategories();
        }

        private async Task LoadPwdPackages()
        {
            var vaultsDataRequest = await apiService.GetDataAsync<GetPwdPackageDTO[]>("/pwdpackage/fromVault/" + vaultId);
            var key = await encryptionService.GetEncryptionKeyDecryptedFromStorage(Guid.Parse(vaultId)) ?? throw new Exception("Error in the key");
            if (vaultsDataRequest.Data != null)
            {

                if (vaultsDataRequest.StatusCode == HttpStatusCode.OK)
                {
                    var entries = vaultsDataRequest.Data.ToList();
                    isShared = VaultContext.isShared;

                    foreach (var pp in entries)
                    {
                        pp.pwpName = await encryptionService.DecryptFieldPwdPackageFromDerivedKey(key, pp.pwpName) ?? throw new Exception("Error encryption");
                        pp.pwpContent = await encryptionService.DecryptFieldPwdPackageFromDerivedKey(key, pp.pwpContent) ?? throw new Exception("Error encryption");
                        pp.pwpUrl = await encryptionService.DecryptFieldPwdPackageFromDerivedKey(key, pp.pwpUrl) ?? throw new Exception("Error encryption");
                        pp.pwpCom = await encryptionService.DecryptFieldPwdPackageFromDerivedKey(key, pp.pwpCom) ?? throw new Exception("Error encryption");
                    }
                    this.pwdPackages = entries;
                    newName = String.Empty;
                    newPassword = String.Empty;
                    newUrl = String.Empty;
                    newComment = String.Empty;
                }
                else
                    this.pwdPackages = [];
            }

            StateHasChanged();
        }

        // Fonction pour sélectionner un coffre
        private void SelectItem(GetPwdPackageDTO item)
        {
            selectedPackage = item;
            isAdding = false; // Ferme le formulaire d'ajout si un coffre est sélectionné
        }

        // Changer l'état pour afficher/masquer le formulaire d'ajout
        private void ToggleAddForm()
        {
            isAdding = !isAdding;
            if (isAdding)
            {
                selectedPackage = null;
            }
        }
        private async void ToggleEdit(GetPwdPackageDTO item)
        {
            if (isEditing)
            {
                // Sauvegarder les modifications
                await this.UpdateItem(item);
                newComment = String.Empty;
                newName = String.Empty;
                newPassword = String.Empty;
                newUrl = String.Empty;
                selectedPackage = null;
            }

            isEditing = !isEditing;
        }

        private async Task UpdateItem(GetPwdPackageDTO item)
        {
            try
            {
                var key = await encryptionService.GetEncryptionKeyDecryptedFromStorage(Guid.Parse(vaultId)) ?? throw new Exception("Error in the key");

                FieldPwdPackageDTO? pwpContentEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, item.pwpContent?.EnfDecryptedText ?? throw new Exception());
                FieldPwdPackageDTO? pwpNameEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, item.pwpName?.EnfDecryptedText ?? throw new Exception());
                FieldPwdPackageDTO? pwpComEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, item.pwpCom?.EnfDecryptedText ?? throw new Exception());
                FieldPwdPackageDTO? pwpUrlEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, item.pwpUrl?.EnfDecryptedText ?? throw new Exception());

                var sessionKey = await encryptionService.LoginFromLocalStorage(Guid.Parse(vaultId));
                if (sessionKey.Item2?.vauPublicKey == null)
                    throw new UserVaultSessionExpiredException();

                var newPackage = new UpdatePwdPackageDTO
                {
                    pwpCom = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpComEncrypted!) ?? throw new Exception("Echec chiffrement commentaire"),
                    pwpContent = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpContentEncrypted!) ?? throw new Exception("Echec chiffrement mot de passe"),
                    pwpName = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpNameEncrypted!) ?? throw new Exception("Echec chiffrement nom"),
                    pwpUrl = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpUrlEncrypted!) ?? throw new Exception("Echec chiffrement url"),
                    vauId = Guid.Parse(vaultId),
                    catId = item.pwpCategory.catId,
                    encryptedAuth = await encryptionService.EncryptSessionStringAsync(sessionKey.Item2.vauPublicKey, vaultId) ?? throw new Exception("Echec chiffrement auth session")
                };

                var result = await apiService.PutDataAsync("/pwdpackage/" + item.pwpId.ToString(), newPackage);

                if (result != HttpStatusCode.NoContent)
                {
                    throw new Exception("Code requête : " + result);
                }
                else
                {
                    isEditing = false;
                    await this.LoadPwdPackages();
                    mudAlertService.ShowAlert("Entrée bien modifiée", Severity.Success);
                }
            }
            catch (UserVaultSessionExpiredException)
            {
                alertService.ShowAlert("Authentification échouée", Severity.Error);
                return;
            }
            catch (Exception ex)
            {
                alertService.ShowAlert("Erreur lors de l'ajout de l'entrée : " + ex.Message);
                return;
            }
        }

        private async Task DeleteItem(GetPwdPackageDTO item)
        {
            try
            {
                var sessionKey = await encryptionService.LoginFromLocalStorage(Guid.Parse(vaultId ?? ""));
                if (sessionKey.Item2?.vauPublicKey == null)
                    throw new UserVaultSessionExpiredException();

                var encryptedAuth = await encryptionService.EncryptSessionStringAsync(sessionKey.Item2.vauPublicKey, item.pwpId.ToString()) ?? throw new Exception("Echec chiffrement auth");
                var result = await apiService.PostDataAsync("/pwdpackage/delete", new DeletePwdPackageDTO() { pwpId = item.pwpId, enryptedAuth = encryptedAuth });
                if (result != HttpStatusCode.NoContent)
                    throw new Exception("Code requête " + result);
                else
                {
                    await this.LoadPwdPackages();
                    mudAlertService.ShowAlert("Entrée bien supprimée", Severity.Success);
                }
                pwdPackages.Remove(item);
                selectedPackage = null; // Désélectionner l'élément après la suppression
            }
            catch (UserVaultSessionExpiredException)
            {
                alertService.ShowAlert("Authentification échouée", Severity.Error);
                return;
            }
            catch (Exception ex)
            {
                alertService.ShowAlert("Erreur : " + ex.Message);
                return;
            }
        }

        private async Task CopyToClipboard(string? value)
        {
            if (!string.IsNullOrEmpty(value))
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", value);
            mudAlertService.ShowAlert("Lien copié dans le presse-papiers ✅", Severity.Success);
        }
        private async Task SharePassword()
        {
            if (selectedPackage is not null)
                await CopyToClipboard(Convert.ToBase64String(new byte[8]));
        }

        // Sauvegarder un nouveau coffre
        private async Task SaveNewPackage()
        {
            try
            {
                var key = await encryptionService.GetEncryptionKeyDecryptedFromStorage(Guid.Parse(vaultId)) ?? throw new Exception("Error in the key");

                FieldPwdPackageDTO? pwpContentEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, newPassword);
                FieldPwdPackageDTO? pwpNameEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, newName);
                FieldPwdPackageDTO? pwpComEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, newComment);
                FieldPwdPackageDTO? pwpUrlEncrypted = await encryptionService.EncryptFieldPwdPackageFromDerivedKey(key, newUrl);

                var sessionKey = await encryptionService.LoginFromLocalStorage(Guid.Parse(vaultId));
                if (sessionKey.Item2?.vauPublicKey == null)
                    throw new UserVaultSessionExpiredException();

                var newPackage = new CreatePwdPackageDTO
                {
                    pwpCom = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpComEncrypted!) ?? throw new Exception("Echec chiffrement commentaire"),
                    pwpContent = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpContentEncrypted!) ?? throw new Exception("Echec chiffrement mot de passe"),
                    pwpName = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpNameEncrypted!) ?? throw new Exception("Echec chiffrement nom"),
                    pwpUrl = await encryptionService.EncryptSessionStringObject<FieldPwdPackageDTO>(sessionKey.Item2.vauPublicKey, pwpUrlEncrypted!) ?? throw new Exception("Echec chiffrement url"),
                    vauId = Guid.Parse(vaultId),
                    catId = newCategory!.catId,
                    encryptedAuth = await encryptionService.EncryptSessionStringAsync(sessionKey.Item2.vauPublicKey, vaultId) ?? throw new Exception("Echec chiffrement auth session")
                };

                var result = await apiService.PostDataAsync<CreatePwdPackageDTO>("/pwdpackage", newPackage);

                if (result.StatusCode != HttpStatusCode.Created)
                {
                    throw new Exception("Code requête : " + result.StatusCode);
                }
                else
                {
                    await this.LoadPwdPackages();
                    isAdding = false; // Ferme le formulaire après l'ajout
                    PasswordInput = InputType.Password;
                    PasswordInputIcon = Icons.Material.Filled.VisibilityOff;
                    PasswordStrength = 0;
                    PasswordStrengthColor = Color.Default;
                    PasswordStrengthText = "Faible";
                    newCategory = new GetCategoryDTO() { catName = newCategory.catName };
                    mudAlertService.ShowAlert("Entrée bien ajouté", Severity.Success);
                }
            }
            catch (UserVaultSessionExpiredException)
            {
                alertService.ShowAlert("Authentification échouée", Severity.Error);
                return;
            }
            catch (Exception ex)
            {
                alertService.ShowAlert("Erreur lors de l'ajout de l'entrée : " + ex.Message);
                return;
            }
        }

        private string GetItemClass(GetPwdPackageDTO item)
            => selectedPackage == item ? "mud-theme-primary text-white" : "";
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

        #region Category
        private async Task AddCategory()
        {
            var parameters = new DialogParameters
            {
                ["VaultId"] = Guid.Parse(vaultId)
            };

            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialogReference = await DialogService.ShowAsync<AddCategoryDialog>("Ajouter une catégorie", parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult != null && dialogResult.Data != null)
            {
                (HttpStatusCode, CreateCategoryDTO?) response = await apiService.PostDataAsync<CreateCategoryDTO>("/categories", dialogResult.Data);
                if (response.Item1 == HttpStatusCode.Created)
                {
                    await this.LoadCategories();

                }
            }
        }
        private async Task LoadCategories()
        {
            var categoriesDataRequest = await apiService.GetDataAsync<GetCategoryDTO[]>($"/categories/{vaultId}");
            if (categoriesDataRequest.StatusCode == HttpStatusCode.OK && categoriesDataRequest.Item2 != null)
            {
                this.categories = categoriesDataRequest.Item2.ToList();

            }
            else
                this.categories = [];

            StateHasChanged();

        }
        private void SelectCategory(GetCategoryDTO category)
        {
            if (selectedCategoryId == category.catId)
                selectedCategoryId = null; // Retirer le filtre
            else
                selectedCategoryId = category.catId;
        }
        private IEnumerable<GetPwdPackageDTO> GetFilteredPwdPackages()
        {
            if (selectedCategoryId is null)
                return pwdPackages;

            return pwdPackages.Where(p => p.pwpCategory.catId == selectedCategoryId);
        }

        private string GetClassCategory(GetCategoryDTO category)
        {
            return selectedCategoryId == category.catId ? "mud-chip-selected" : "";
        }


        #endregion
    }
}
