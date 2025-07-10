using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Models;
using IIABlazorWebAssembly.Pages.AuthenticationVault;
using IIABlazorWebAssembly.Pages.Vaults.Share;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;

namespace IIABlazorWebAssembly.Pages.Vaults
{
    partial class Vaults
    {
        [CascadingParameter]
        private Task<AuthenticationState> authenticationState { get; set; } = default!;

        private GetVaultDTO[]? vaultsData;
        private MudDataGrid<GetVaultDTO> dataGrid { get; set; } = new MudDataGrid<GetVaultDTO>();
        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] VaultService VaultContext { get; set; } = default!;
        [Inject] IDialogService DialogService { get; set; } = default!;
        private Func<Guid, Task> callbackAfterTotp = default!;
        private async Task OnVaultSelected(GetVaultDTO vault)
        {
            VaultContext.isShared = vault.isShared;

            var (session, keyVault) = await vaultService.LoginVaultFromLocalStorage(vault.vauId);

            if (session != null)
            {
                await NavigateVaultSelectedFromGrid(vault.vauId);
            }
            else
            {
                Func<Guid, Task> callback = NavigateVaultSelectedFromGrid;

                if (vault.isTotp)
                {
                    callback = OpenTotpVaultDialog;
                    this.callbackAfterTotp = NavigateVaultSelectedFromGrid;
                }

                await PromptForVaultPassword(vault.vauId, vault.isTotp, callback);
            }

            StateHasChanged();
        }

        public Task NavigateVaultSelectedFromGrid(Guid vaultId)
        {
            Navigation.NavigateTo($"/vault/{vaultId}");
            return Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await authenticationState;
                await dataGrid.ReloadServerData();
            }
        }

        private async Task<GridData<GetVaultDTO>> LoadVaults(GridState<GetVaultDTO> state)
        {
            var vaultsDataRequest = await apiService.GetDataAsync<GetVaultDTO[]>("/vaults/my");

            if (vaultsDataRequest.StatusCode == HttpStatusCode.OK && vaultsDataRequest.Data != null)
                vaultsData = vaultsDataRequest.Data;
            else
                vaultsData = Array.Empty<GetVaultDTO>();

            return new GridData<GetVaultDTO> { Items = vaultsData, TotalItems = vaultsData.Length };
        }

        private async Task CreateVault()
        {
            try
            {
                var options = new DialogOptions { CloseOnEscapeKey = true };
                var dialogReference = await DialogService.ShowAsync<AddVaultDialog>("Ajout d'un coffre", options);
                var dialogResult = await dialogReference.Result;
                if (dialogResult?.Canceled == false)
                {
                    CreateVaultDTO cv = dialogResult.Data as CreateVaultDTO ?? throw new InvalidCastException();

                    (HttpStatusCode, CreateSuccessVaultDTO?) vaultTuple1 = await apiService.PostDataAsync<CreateSuccessVaultDTO>("/vaults", cv);

                    if (vaultTuple1.Item1 == HttpStatusCode.Created)
                    {
                        if (cv.vauTotp == false)
                        {
                            mudAlertService.ShowAlert("Coffre créé", Severity.Success);
                            await dataGrid.ReloadServerData();
                        }
                        else
                        {
                            var parameters = new DialogParameters
                            {
                                ["Vault"] = vaultTuple1.Item2
                            };
                            var dialogTotpReference = await DialogService.ShowAsync<TotpVaultDialog>("Sauvegarde code auth", parameters, options);
                            var dialogTotpResult = await dialogTotpReference.Result;
                            if (dialogTotpResult?.Canceled == false)
                            {
                                mudAlertService.ShowAlert("Coffre créé", Severity.Success);
                                await dataGrid.ReloadServerData();
                            }
                        }
                    }
                }
            }
            catch (InvalidCastException)
            {

            }
            catch (Exception)
            {

            }


        }

        async Task UpdateVault(GetVaultDTO vault)
        {
            (UserVaultSession?, KeyVaultDTO?) alreadyLogIn = await vaultService.LoginVaultFromLocalStorage(vault.vauId);
            var vaultSession = alreadyLogIn.Item1;
            var vaultKey = alreadyLogIn.Item2;
            var parameters = new DialogParameters
            {
                ["Vault"] = vault,
                ["Session"] = vaultSession
            };

            var options = new DialogOptions { CloseOnEscapeKey = true };
            var dialogReference = await DialogService.ShowAsync<UpdateVaultDialog>("Édition d'un coffre", parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult != null && !dialogResult.Canceled)
            {
                try
                {
                    // Login to the vault and get the session
                    UpdateDialogResultModel dialog = dialogResult.Data as UpdateDialogResultModel ?? throw new Exception("Dialog data cannot be retrieved");
                    if (dialog.vauId == Guid.Empty || dialog.vauPassword == null)
                    {
                        throw new InvalidDataException("Vault ID or password is invalid.");
                    }
                    (UserVaultSession?, KeyVaultDTO?) loginResult = vaultSession == null ?
                        await vaultService.LoginVault(dialog.vauId, dialog.vauPassword, vault.isTotp)
                        : (vaultSession, vaultKey);

                    if (loginResult.Item1 != null && loginResult.Item1.successDTO != null && loginResult.Item2 != null && dialog.vauName != null)
                    {
                        // Update the Vault with the new name and favorite status
                        var updateVaultDTO = await encryptionService.EncryptSessionStringObject<UpdateVaultDTO>
                         (loginResult.Item1.successDTO.vauPKey, new UpdateVaultDTO { vauFavorite = dialog.vauFavorite.ToString(), vauName = dialog.vauName });

                        var resultRequest3 = await apiService.PutDataAsync("/vaults/" + dialog?.vauId, updateVaultDTO);

                        if ((int)resultRequest3 == 403)
                        {
                            throw new MemberAccessException();
                        }

                    }
                    else
                    {
                        throw new InvalidDataException();

                    }

                }
                catch (MemberAccessException)
                {
                    Navigation.NavigateToLogin("/");
                    mudAlertService.ShowAlert("La session est expirée", Severity.Warning);
                }
                catch (InvalidDataException)
                {
                    mudAlertService.ShowAlert("Mauvais mot de passe", Severity.Error);
                    return;
                }
                catch (Exception ex)
                {
                    mudAlertService.ShowAlert("Error: " + ex.Message, Severity.Error);
                    return;
                }

                mudAlertService.ShowAlert("Vault Updated", Severity.Success);
                await dataGrid.ReloadServerData();
            }

        }
        private string GroupClassFunc(GroupDefinition<GetVaultDTO> item)
        {
            return item.Grouping.Key?.ToString() == "Nonmetal" || item.Grouping.Key?.ToString() == "Other"
                    ? "mud-theme-warning"
                    : string.Empty;
        }

        private async Task ShareVault(GetVaultDTO vault)
        {
            var (session, keyVault) = await vaultService.LoginVaultFromLocalStorage(vault.vauId);

            if (session != null)
            {
                await OpenShareVaultDialog(vault.vauId);
            }
            else
            {
                var callback = new Func<Guid, Task>(OpenShareVaultDialog);
                if (vault.isTotp)
                {
                    callback = new Func<Guid, Task>(OpenTotpVaultDialog);
                    this.callbackAfterTotp = new Func<Guid, Task>(OpenShareVaultDialog);
                }

                await PromptForVaultPassword(vault.vauId, vault.isTotp, callback);
            }
        }

        private async Task OpenTotpVaultDialog(Guid vaultId)
        {
            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small
            };

            var dialog = await DialogService.ShowAsync<TotpAuthentificationVaultDialog>("Authentification MFA", options);
            var dialogResult = await dialog.Result;
            if (!dialogResult?.Canceled ?? true)
            {
                var apiResult = await apiService.PostDataAsync($"/vaults/totp",
                    new CheckTotpVaultDTO() { vauId = vaultId, token = (string)dialogResult!.Data! });
                if (apiResult == HttpStatusCode.OK)
                {
                    await encryptionService.GetEncryptionKeyVaultAfterTotp(vaultId);
                    await this.callbackAfterTotp(vaultId);
                    this.callbackAfterTotp = null!;
                }
            }
        }

        private async Task OpenShareVaultDialog(Guid vaultId)
        {
            var parameters = new DialogParameters
            {
                ["VaultId"] = vaultId
            };

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small
            };

            await DialogService.ShowAsync<ShareVaultDialog>("Partager ce coffre", parameters, options);
        }

        private async Task RevokeSharedVault(GetVaultDTO vault)
        {
            var (session, keyVault) = await vaultService.LoginVaultFromLocalStorage(vault.vauId);

            if (session != null)
            {
                await OpenRevokeVaultDialog(vault.vauId);
            }
            else
            {
                var callback = new Func<Guid, Task>(OpenRevokeVaultDialog);
                if (vault.isTotp)
                {
                    callback = new Func<Guid, Task>(OpenTotpVaultDialog);
                    this.callbackAfterTotp = new Func<Guid, Task>(OpenRevokeVaultDialog);
                }
                await PromptForVaultPassword(vault.vauId, vault.isTotp, callback);
            }
        }

        public async Task PromptForVaultPassword(Guid vaultId, bool isTotp, Func<Guid, Task> onSuccess)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true };
            var dialogReference = await DialogService.ShowAsync<AuthenticationVaultDialog>("Veuillez saisir le mot de passe", options);
            var dialogResult = await dialogReference.Result;

            if (dialogResult != null && dialogResult.Data != null && !dialogResult.Canceled)
            {
                try
                {
                    await vaultService.AttemptVaultLogin(vaultId, (string)dialogResult.Data, isTotp, onSuccess);
                }
                catch (InvalidDataException)
                {
                    mudAlertService.ShowAlert("Mauvais mot de passe", Severity.Error);
                    return;
                }
            }
        }

        private async Task OpenRevokeVaultDialog(Guid vaultId)
        {
            var parameters = new DialogParameters
            {
                ["VaultId"] = vaultId
            };

            var options = new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small
            };

            await DialogService.ShowAsync<RevokeSharedVault>("Supprimer les accès au coffre", parameters, options);
        }

        private async Task DeleteVault(Guid selectedVaultId)
        {
            var parameters = new DialogParameters
            {
                ["VaultId"] = selectedVaultId
            };
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small };
            var dialogReference = await DialogService.ShowAsync<DeleteVaultDialog>("Supprimer ce coffre", parameters, options);
            var dialogResult = await dialogReference.Result;
            if (dialogResult != null && !dialogResult.Canceled)
            {
                var vaultTuple1 = await apiService.DeleteDataAsync($"/vaults/{selectedVaultId}");

                if ((int)vaultTuple1 == 200)
                {
                    mudAlertService.ShowAlert("Vault Deleted", Severity.Success);
                    await dataGrid.ReloadServerData();
                }
                else
                {
                    mudAlertService.ShowAlert("Vault Delete error", Severity.Error);

                }
            }
        }

    }
}