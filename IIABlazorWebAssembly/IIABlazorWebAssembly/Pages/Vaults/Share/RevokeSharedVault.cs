using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net;

namespace IIABlazorWebAssembly.Pages.Vaults.Share
{
    partial class RevokeSharedVault
    {
        [Parameter] public Guid vaultId { get; set; }
        [CascadingParameter] private IMudDialogInstance? MudDialog { get; set; }
        [Inject] APIService apiService { get; set; } = default!;
        private GetRevokeSharedDTO[]? revokeShareds { get; set; }
        [Inject] MudAlertService mudAlertService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await GetUsersShared();
        }
        private async Task GetUsersShared()
        {
            (HttpStatusCode, GetRevokeSharedDTO[]?) response = await apiService.GetDataAsync<GetRevokeSharedDTO[]>($"/vaults/{vaultId}/revoke");
            if (response.Item1 == HttpStatusCode.OK)
            {
                revokeShareds = response.Item2;
            }
            else
            {
                revokeShareds = Array.Empty<GetRevokeSharedDTO>();
            }
        }
        private async Task RevokeSharedUser(GetRevokeSharedDTO selectRevoke)
        {
            var response = await apiService.DeleteDataAsync($"/vaults/{selectRevoke.userId}/{selectRevoke.vaultId}/revoke");
            if (response == HttpStatusCode.OK)
            {
                mudAlertService.ShowAlert("L'utilisateur a bien été révoqué", Severity.Success);
                MudDialog?.Close();
            }
            else
            {
                mudAlertService.ShowAlert("Une erreur est survenue", Severity.Error);
            }

        }

    }
}
