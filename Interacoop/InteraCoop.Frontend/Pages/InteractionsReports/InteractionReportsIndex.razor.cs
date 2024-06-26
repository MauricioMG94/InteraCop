﻿using CurrieTechnologies.Razor.SweetAlert2;
using InteraCoop.Frontend.Repositories;
using InteraCoop.Shared.Dtos;
using Microsoft.AspNetCore.Components;

namespace InteraCoop.Frontend.Pages.InteractionsReports
{
    public partial class InteractionReportsIndex
    {
        private int currentPage = 1;
        private int totalPages;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Parameter, SupplyParameterFromQuery] public int RecordsNumber { get; set; } = 10;
        public List<ReportDto>? Reports { get; set; }
        public List<string> InteractionTypes = ["Visita a cliente", "Visita en oficina", "Llamada entrante", "Llamada saliente"];

        private async Task FilterCallBack(string filter)
        {
            Filter = filter;
            await ApplyFilterAsync();
            StateHasChanged();
        }

        [Parameter]
        [SupplyParameterFromQuery]
        public string Page { get; set; } = string.Empty;

        [Parameter]
        [SupplyParameterFromQuery]
        public string Filter { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        private async Task SelectedRecordsNumberAsync(int recordsnumber)
        {
            RecordsNumber = recordsnumber;
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task SelectedPageAsync(int page)
        {
            currentPage = page;
            await LoadAsync(page);
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

        private async Task ApplyFilterAsync()
        {
            int page = 1;
            await LoadAsync(page);
            await SelectedPageAsync(page);
        }

        private async Task<bool> LoadListAsync(int page)
        {
            ValidateRecordsNumber();
            var url = $"api/interactionsReport/interactionsReports?page={page}&recordsnumber={RecordsNumber}";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }

            var response = await Repository.GetAsync<List<ReportDto>>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Reports = response.Response;
            return true;
        }

        private async Task LoadPagesAsync()
        {
            var url = $"api/interactionsReport/totalPages?recordsnumber={RecordsNumber}";
            if (!string.IsNullOrEmpty(Filter))
            {
                url += $"&filter={Filter}";
            }

            var response = await Repository.GetAsync<int>(url);
            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return;
            }
            totalPages = response.Response;
        }

        private string GetBadgeStyle(string propertyToColor)
        {
            return propertyToColor switch
            {
                "Visita a cliente" => "background-color:olivedrab",
                "Visita en oficina" => "background-color:goldenrod",
                "Llamada entrante" => "background-color:cornflowerblue",
                "Llamada saliente" => "background-color: #7E6FFF",
                _ => "background-color:cornflowerblue"
            };
        }

    }
}
