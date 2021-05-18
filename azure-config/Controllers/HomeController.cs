using System.Threading.Tasks;
using azure_config.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace azure_config.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptionsSnapshot<SiteDetailsOptions> _siteDetails;
        private readonly IFeatureManager _features;

        public HomeController(ILogger<HomeController> logger, IOptionsSnapshot<SiteDetailsOptions> siteDetails, IFeatureManager features)
        {
            _logger = logger;
            _siteDetails = siteDetails;
            _features = features;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return new JsonResult(new {
                Name = _siteDetails.Value.Name,
                Maintenance = _siteDetails.Value.IsUnderMaintenance,
                NormalFeature = await _features.IsEnabledAsync(FeatureFlags.NORMAL_FEATURE),
                PercentageFeature = await _features.IsEnabledAsync(FeatureFlags.PERCENTAGE_FEATURE)
            });
        }
    }
}
