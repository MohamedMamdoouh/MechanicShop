using Asp.Versioning;
using MechanicShop.Contracts.Settings;
using MechanicShop.Infrastructure.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
namespace MechanicShop.Api.Controllers;

[Route("api/[controller]")]
[ApiVersionNeutral]
[AllowAnonymous]
public sealed class SettingsController(IOptions<AppSettings> appSettings) : ApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(ShopSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Get shop settings")]
    [EndpointDescription("Returns public shop configuration including opening hours and capacity.")]
    [EndpointName("GetShopSettings")]
    public IActionResult Get()
    {
        var s = appSettings.Value;

        var dto = new ShopSettingsDto(
            s.ShopName,
            s.OpeningTime,
            s.ClosingTime,
            s.MaxSpots,
            s.MaxAppointmentDurationInMinutes);

        return Ok(dto);
    }
}
