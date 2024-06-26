﻿using CurrieTechnologies.Razor.SweetAlert2;
using InteraCoop.Frontend.Repositories;
using InteraCoop.Shared.Entities;
using InteraCoop.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace InteraCoop.Frontend.Pages.Opportunities
{
    [Authorize(Roles = "Employee")]
    public partial class OpportunitiesIndex
    {
        private int currentPage = 1;
        private int totalPages;
        private User? user;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 10;
        public List<Opportunity>? Opportunities { get; set; }
        [Parameter, SupplyParameterFromQuery] public string Page { get; set; } = string.Empty;
        [Parameter, SupplyParameterFromQuery] public string Filter { get; set; } = string.Empty;
        [SupplyParameterFromQuery] public string Document { get; set; } = string.Empty;
        public bool FormPostedSuccessfully { get; set; } = false;

        List<string> OpportunityStatus = new List<string>();

        private async Task FilterCallBack(string filter)
        {
            Filter = filter;
            await ApplyFilterAsync();
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            await LoadUserAsyc();
            await LoadAsync();
        }

        private async Task LoadUserAsyc()
        {
            var responseHttp = await Repository.GetAsync<User>($"/api/accounts");
            if (responseHttp.Error)
            {
                var messageError = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", messageError, SweetAlertIcon.Error);
                return;
            }
            user = responseHttp.Response;
            Document = user.Document;
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadAsync(page);
        }

        private async Task SelectedRecordsNumberAsync(int recordsnumber)
        {
            RecordsNumber = recordsnumber;
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task LoadAsync(int page = 1)
        {
            if (!string.IsNullOrWhiteSpace(Page))
            {
                page = Convert.ToInt32(Page);
            }

            var ok = await LoadListAsync(page);
            if (ok)
            {
                await LoadPagesAsync();
            }
        }

        private void ValidateRecordsNumber()
        {
            if (RecordsNumber == 0)
            {
                RecordsNumber = 10;
            }
        }

        private async Task<bool> LoadListAsync(int page)
        {
            ValidateRecordsNumber();
            var url = $"api/opportunities?page={page}&recordsnumber={RecordsNumber}";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }
            if (!string.IsNullOrEmpty(Document) && user.UserType == UserType.Employee)
            {
                url += $"&userDocument={Document}";
            }

            var responseHttp = await Repository.GetAsync<List<Opportunity>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Opportunities = responseHttp.Response;
            GetUniqueStatusList(Opportunities);
            return true;
        }

        private List<string> GetUniqueStatusList(List<Opportunity>? opportunities)
        { 

            if (opportunities != null)
            {
                foreach (var opportunity in opportunities)
                {
                    if (!OpportunityStatus.Contains(opportunity.Status))
                    {
                        OpportunityStatus.Add(opportunity.Status);
                    }
                }
            }
            return OpportunityStatus;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/opportunities/totalPages?recordsnumber={RecordsNumber}";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }
            if (!string.IsNullOrEmpty(Document) && user.UserType == UserType.Employee)
            {
                url += $"&userDocument={Document}";
            }

            var responseHttp = await Repository.GetAsync<int>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            totalPages = responseHttp.Response;
        }

        private async Task CleanFilterAsync()
        {
            Filter = string.Empty;
            await ApplyFilterAsync();
        }

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task Delete(int opportunityId)
        {
            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Confirmación",
                Text = "¿Esta seguro que quieres borrar el registro?",
                Icon = SweetAlertIcon.Question,
                ShowCancelButton = true
            });
            var confirm = string.IsNullOrEmpty(result.Value);

            if (confirm)
            {
                return;
            }

            var responseHttp = await Repository.DeleteAsync<Opportunity>($"api/opportunities/{opportunityId}");

            if (responseHttp.Error)
            {
                if (responseHttp.HttpResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NavigationManager.NavigateTo("/opportunities");

                }
                else
                {
                    var mensajeError = await responseHttp.GetErrorMessageAsync();
                    await SweetAlertService.FireAsync("Error", mensajeError, SweetAlertIcon.Error);
                }
                return;
            }

            await LoadAsync();
            var toast = SweetAlertService.Mixin(new SweetAlertOptions
            {
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = true,
                Timer = 3000
            });
            await toast.FireAsync(icon: SweetAlertIcon.Success, message: "Registro borrado con éxito.");
        }

        private string GetBadgeStyle(string propertyToColor)
        {
            return propertyToColor switch
            {
                "Formalizada" => "background-color:olivedrab",
                "Desestimada" => "background-color: #E6443E",
                "En trámite" => "background-color:cornflowerblue",
                "Visita a cliente" => "background-color:olivedrab",
                "Visita en oficina" => "background-color:goldenrod",
                "Llamada entrante" => "background-color:cornflowerblue",
                "Llamada saliente" => "background-color: #7E6FFF",
                "Sin asignar" => "background-color:olivedrab",
                "Asignada" => "background-color:olivedrab",
                "Vencida" => "background-color: #E6443E",
                _ => "background-color:cornflowerblue"
            };
        }
    }
}
