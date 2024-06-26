﻿using CurrieTechnologies.Razor.SweetAlert2;
using InteraCoop.Shared.Dtos;
using InteraCoop.Shared.Entities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Authorization;
using InteraCoop.Frontend.Repositories;
using System.Net;

namespace InteraCoop.Frontend.Pages.Interactions
{
    [Authorize(Roles = "Employee")]
    public partial class InteractionForm
    {
        private User? user;
        private EditContext editContext = null!;
        [Inject] private SweetAlertService SweetAlertService { get; set; } = null!;
        [Inject] private IRepository Repository { get; set; } = null!;
        [Parameter, EditorRequired] public InteractionDto Interaction { get; set; } = null!;
        [Parameter, EditorRequired] public EventCallback OnValidSubmit { get; set; }
        [Parameter, EditorRequired] public EventCallback ReturnAction { get; set; }
        [Parameter] public required String FormName { get; set; }
        [Parameter] public required String CardName { get; set; }
        public List<Client> Clients { get; set; } = new();
        public bool FormPostedSuccessfully { get; set; } = false;
        public int? Document { get; set; }
        public string? clientName;
        public Client? client;


        protected override void OnInitialized()
        {
            editContext = new(Interaction);

            Interaction.InteractionCreationDate = DateTime.Today;
            Interaction.AuditDate = DateTime.Today;

            if (FormName.Contains("Crear"))
            {
                Interaction.StartDate = DateTime.Today;
                Interaction.EndDate = DateTime.Today;
            }
            if (FormName.Contains("Editar")) {
                clientName = Interaction.clientName;
                Document = Interaction.Document;
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadListAsync();
            await LoadUserAsyc();
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
            Interaction.UserDocument = user.Document;
        }

        private async Task<bool> LoadListAsync()
        {
            var url = $"api/clients";

            var responseHttp = await Repository.GetAsync<List<Client>>(url);
            if (responseHttp.Error)
            {
                var message = await responseHttp.GetErrorMessageAsync();
                await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
                return false;
            }
            Clients = responseHttp.Response;
            return true;
        }

        private async Task OnDocumentChanged(int? newValue)
        {
            Document = newValue;
            if (Document.HasValue)
            {
                await GetClientByDocument(Document.Value);
            }
            else
            {
                clientName = "";
                Interaction.ClientId = 0;
            }
        }

        private async Task<bool> GetClientByDocument(int? document)
        {
            client = Clients.FirstOrDefault(x => x.Document == document.Value);

            if (client != null)
            {
                clientName = client.Name;
                Interaction.ClientId = client.Id;
                return true;
            }
            else
            {
                clientName = "Cliente no existe.";
                return false;
            }
        }


        private async Task OnDataAnnotationsValidatedAsync()
        {
            await OnValidSubmit.InvokeAsync();
        }

        private async Task OnBeforeInternalNavigation(LocationChangingContext context)
        {
            var formWasEdited = editContext.IsModified();

            if (!formWasEdited)
            {
                return;
            }

            if (FormPostedSuccessfully)
            {
                return;
            }

            var result = await SweetAlertService.FireAsync(new SweetAlertOptions
            {
                Title = "Confirmación",
                Text = "¿Deseas abandonar la página y perder los cambios?",
                Icon = SweetAlertIcon.Warning,
                ShowCancelButton = true
            });

            var confirm = !string.IsNullOrEmpty(result.Value);

            if (confirm)
            {
                return;
            }

            context.PreventNavigation();
        }

    }
}
