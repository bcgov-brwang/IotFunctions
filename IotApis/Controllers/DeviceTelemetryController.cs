using IotApis.HttpClients;
using IotApis.Model;
using IotApis.Service;
using IotCommon;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace IotApis.Controllers
{
    [ApiController]
    [Route("api/weather/devices")]
    public class DeviceTelemetryController : ControllerBase
    {
        private readonly IDeviceTelemetryService _deviceTelemetryService;
        private readonly IIotCentralApi _iotCentralApi;
        private readonly ILogger<DeviceTelemetryController> _logger;

        public DeviceTelemetryController(IDeviceTelemetryService deviceTelemetryService, IIotCentralApi iotCentralApi, ILogger<DeviceTelemetryController> logger)
        {
            _deviceTelemetryService = deviceTelemetryService;
            _iotCentralApi = iotCentralApi;
            _logger = logger;
        }

        [HttpGet("{deviceId}/telemetries", Name = "GetDeviceTelemetries")]
        public async Task<ActionResult<IEnumerable<DeviceTelemetry>>> GetDeviceTelemetries(string deviceId, string dateFrom, string dateTo)
        {
            if (!ValidateDate(dateFrom) || !ValidateDate(dateTo))
                return BadRequest();

            var dateFromTs = DateUtils.ConvertPacificToUtcTotalMilliseconds(DateTime.Parse(dateFrom));
            var dateToTs = DateUtils.ConvertPacificToUtcTotalMilliseconds(DateTime.Parse(dateTo).AddDays(1).AddSeconds(-1));

            return await _deviceTelemetryService.GetDeviceTelemetries(deviceId, dateFromTs, dateToTs);
        }

        [HttpGet("iotcentral/{deviceId}/telemetries", Name = "GetIotCentralDeviceTelemetries")]
        public async Task<ActionResult> GetIotCentralDeviceTelemetries(string deviceId, string dateFrom, string dateTo)
        {
            if (!ValidateDate(dateFrom) || !ValidateDate(dateTo))
                return BadRequest();

            var dateFromTs = DateUtils.ConvertPacificToUtcTime(DateTime.Parse(dateFrom)).ToString("o", CultureInfo.InvariantCulture);
            var dateToTs = DateUtils.ConvertPacificToUtcTime(DateTime.Parse(dateTo).AddDays(1).AddSeconds(-1)).ToString("o", CultureInfo.InvariantCulture);

            var content = await _iotCentralApi.GetWeatherTelemetry(deviceId, dateFromTs, dateToTs);

            return Ok(await content.ReadAsStreamAsync());
        }

        private bool ValidateDate(string dateStr)
        {
            var regex = new Regex(@"\d\d\d\d-\d\d-\d\d");

            return regex.Match(dateStr).Success;
        }
    }
}