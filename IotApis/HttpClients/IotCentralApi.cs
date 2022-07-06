﻿using Newtonsoft.Json;
using System.Net;

namespace IotApis.HttpClients
{
    public interface IIotCentralApi
    {
        Task<HttpResponseMessage> GetWeatherTelemetry(string deviceId, string dateFrom, string dateTo);
        Task<HttpResponseMessage> GetCameraTelemetry(string deviceId, string dateFrom, string dateTo);
        Task<HttpResponseMessage> GetDeviceProperty(string deviceId);
    }
    public class IotCentralApi : IIotCentralApi
    {
        private HttpClient _client;
        private IApi _api;
        private IConfiguration _config;
        private ILogger<IotCentralApi> _logger;

        public IotCentralApi(HttpClient client, IApi api, IConfiguration config, ILogger<IotCentralApi> logger)
        {
            _client = client;
            _api = api;
            _config = config;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetWeatherTelemetry(string deviceId, string dateFrom, string dateTo)
        {
            var template = await GetTemplate(deviceId);

            if (template == "")
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var path = _config.GetValue<string>("IotCentral:TelemetryPath") ?? "";

            var query = $"SELECT $id, $ts, measurements FROM {template}" +
                $" WHERE $ts >= '{dateFrom}' AND $ts <= '{dateTo}' AND $id = '{deviceId}'";

            var body = $"{{ \"query\": \"{query}\" }}";

            return await _api.Post(_client, path, body);
        }

        public async Task<HttpResponseMessage> GetCameraTelemetry(string deviceId, string dateFrom, string dateTo)
        {
            var template = await GetTemplate(deviceId);

            if (template == "")
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            var path = _config.GetValue<string>("IotCentral:TelemetryPath") ?? "";

            var query = $"SELECT $id, $ts, CameraDatas FROM {template}" +
                $" WHERE $ts >= '{dateFrom}' AND $ts <= '{dateTo}' AND $id = '{deviceId}'";

            var body = $"{{ \"query\": \"{query}\" }}";

            return await _api.Post(_client, path, body);
        }

        public async Task<HttpResponseMessage> GetDeviceProperty(string deviceId)
        {
            var path = string.Format(_config.GetValue<string>("IotCentral:PropertyPath") ?? "", deviceId);
            
            return await _api.Get(_client, path);
        }

        public async Task<string> GetTemplate(string deviceId)
        {
            var path = string.Format(_config.GetValue<string>("IotCentral:DevicePath") ?? "", deviceId);

            var responseMessage = await _api.Get(_client, path);

            if (responseMessage.IsSuccessStatusCode)
            {
                var json = await responseMessage.Content.ReadAsStringAsync();
                var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
                return jsonObj.template;
            }

            return "";
        }
    }
}
