using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net;

namespace IIABlazorWebAssembly.Pages.Vaults.Share
{
    partial class ShareVaultDialog
    {
        [Parameter] public Guid VaultId { get; set; }

        [Inject] IJSRuntime JS { get; set; } = default!;
        [Inject] APIService apiService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        [Inject] MudAlertService mudAlertService { get; set; } = default!;
        [Inject] ISnackbar Snackbar { get; set; } = default!;

        [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }

        private string? qrCode { get; set; }
        public int NumberOfUses { get; set; } = 1;
        private string? shareLink;
        private bool hasActiveShareLink = false;
        private GetVaultAccessDTO? currentVaultAccess;
        private int? numberLeft;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await InitializeShareLink();

            await base.OnAfterRenderAsync(firstRender);
        }

        private async Task InitializeShareLink()
        {
            try
            {
                (HttpStatusCode, GetVaultAccessDTO?) response = await apiService.GetDataAsync<GetVaultAccessDTO>($"/vaults/{VaultId}/share")!;
                currentVaultAccess = response.Item2;

                if (!string.IsNullOrEmpty(currentVaultAccess?.token) && currentVaultAccess.nbUsed != currentVaultAccess.isUsed)
                {
                    shareLink = $"{NavigationManager.BaseUri}vaults/share/{currentVaultAccess.token}";
                    numberLeft = currentVaultAccess.nbUsed - currentVaultAccess.isUsed;
                    qrCode = currentVaultAccess.qrCodeTotp;
                    hasActiveShareLink = true;
                    mudAlertService.ShowAlert("Génération effectué avec succès", Severity.Success);

                }
                else
                {
                    hasActiveShareLink = false;
                }
            }
            catch (Exception ex)
            {
                hasActiveShareLink = false;
                mudAlertService.ShowAlert("Une erreur est survenue : " + ex.Message, Severity.Error);
            }

            StateHasChanged();
        }

        protected async Task GenerateShareLink()
        {
            try
            {
                if (!string.IsNullOrEmpty(currentVaultAccess?.token))
                {
                    shareLink = $"{NavigationManager.BaseUri}vaults/share/{currentVaultAccess.token}";
                    hasActiveShareLink = true;
                    qrCode = currentVaultAccess.qrCodeTotp;
                    return;
                }

                var response = await apiService.PostDataAsync<GetVaultAccessDTO>($"/vaults/{VaultId}/share", NumberOfUses);

                if ((int)response.StatusCode < 300 && !string.IsNullOrEmpty(response.Data?.token))
                {
                    currentVaultAccess = response.Data;
                    shareLink = $"{NavigationManager.BaseUri}vaults/share/{currentVaultAccess.token}";
                    numberLeft = currentVaultAccess.nbUsed;
                    qrCode = currentVaultAccess.qrCodeTotp;
                    mudAlertService.ShowAlert("Génération effectué avec succès", Severity.Success);

                    hasActiveShareLink = true;
                }
                else
                {
                    hasActiveShareLink = false;
                    mudAlertService.ShowAlert("Erreur lors de la génération du lien de partage", Severity.Error);
                    MudDialog?.Cancel();
                }
            }
            catch (Exception ex)
            {
                hasActiveShareLink = false;
                mudAlertService.ShowAlert("Une erreur est survenue : " + ex.Message, Severity.Error);
            }

            StateHasChanged();
        }

        protected async Task NextStep()
        {
            hasActiveShareLink = true;
            await GenerateShareLink();
        }

        private async Task CopyToClipboard()
        {
            if (!string.IsNullOrEmpty(shareLink))
                await JS.InvokeVoidAsync("navigator.clipboard.writeText", shareLink);
            mudAlertService.ShowAlert("Lien copié dans le presse-papiers ✅", Severity.Success);

        }
    }
}
