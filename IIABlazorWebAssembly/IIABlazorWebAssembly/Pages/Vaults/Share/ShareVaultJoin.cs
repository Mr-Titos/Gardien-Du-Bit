using common_gardienbit.DTO.Vault;
using IIABlazorWebAssembly.Services;
using Microsoft.AspNetCore.Components;
using System.Net;

namespace IIABlazorWebAssembly.Pages.Vaults.Share
{
    partial class ShareVaultJoin
    {
        [Parameter] public required string token { get; set; }
        [Inject] APIService apiService { get; set; } = default!;
        [Inject] NavigationManager NavigationManager { get; set; } = default!;
        private bool processing { get; set; } = true;
        private bool success { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var (statusCode, result) = await apiService.PostDataAsync<GetVaultAccessDTO>("/vaults/share/consume", token);

                if (statusCode == HttpStatusCode.OK)
                {
                    success = true;
                    StateHasChanged();
                }
                else
                {
                    success = false;
                    StateHasChanged();
                }

            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                processing = false;
                StateHasChanged();
            }
        }

    }
}
