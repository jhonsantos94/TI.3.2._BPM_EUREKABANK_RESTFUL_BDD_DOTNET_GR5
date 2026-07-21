// En: EurekaBank.Core/Services/Implementations/RestTransactionService.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace EurekaBank.Core.Services.Implementations
{
    public class RestTransactionService : ITransactionService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private ApiPlatform _currentTarget = ApiPlatform.Java;

        public RestTransactionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void SetTarget(ApiPlatform target)
        {
            _currentTarget = target;
        }

        // --- DEPOSITO ---
        public async Task<TransactionResponse<DepositResponseData>> RealizarDepositoAsync(DepositRequest request)
        {
            return await PostAsync<DepositRequest, DepositResponseData>(request, "deposito");
        }

        // --- RETIRO ---
        public async Task<TransactionResponse<WithdrawResponseData>> RealizarRetiroAsync(DepositRequest request)
        {
            return await PostAsync<DepositRequest, WithdrawResponseData>(request, "retiro");
        }

        // --- TRANSFERENCIA ---
        public async Task<TransactionResponse<TransferResponseData>> RealizarTransferenciaAsync(TransferRequest request)
        {
            return await PostAsync<TransferRequest, TransferResponseData>(request, "transferencia");
        }

        // --- MÉTODO GENÉRICO PRIVADO PARA EVITAR REPETIR CÓDIGO ---
        private async Task<TransactionResponse<TResponseData>> PostAsync<TRequest, TResponseData>(TRequest request, string endpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage? response = null;

            try
            {
                string hostKey = _currentTarget == ApiPlatform.Java ? "Hosts:Rest:Java" : "Hosts:Rest:DotNet";
                string? baseUrl = _configuration[hostKey];

                var baseIp = _configuration["ServerConfig:BaseIp"];

                if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(baseIp))
                {
                    baseUrl = baseUrl.Replace("{IP}", baseIp);
                }

                if (string.IsNullOrEmpty(baseUrl))
                {
                    return new TransactionResponse<TResponseData> { Exitoso = false, Mensaje = $"La URL base para '{hostKey}' no está configurada." };
                }

                var fullUrl = $"{baseUrl}/api/Transaccion/{endpoint}";

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                response = await httpClient.PostAsync(fullUrl, content);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return new TransactionResponse<TResponseData> { Exitoso = false, Mensaje = $"El servidor respondió con un error pero sin detalles: {(int)response.StatusCode} ({response.ReasonPhrase})" };
                }

                var transactionResponse = JsonConvert.DeserializeObject<TransactionResponse<TResponseData>>(jsonResponse);
                return transactionResponse!;
            }
            catch (JsonException jsonEx)
            {
                string responseContent = response != null ? await response.Content.ReadAsStringAsync() : "La respuesta fue nula.";
                return new TransactionResponse<TResponseData> { Exitoso = false, Mensaje = $"Error al procesar el JSON: {jsonEx.Message}. Contenido: {responseContent}" };
            }
            catch (Exception ex)
            {
                return new TransactionResponse<TResponseData> { Exitoso = false, Mensaje = $"Error de conexión o inesperado: {ex.Message}" };
            }
        }
    }
}