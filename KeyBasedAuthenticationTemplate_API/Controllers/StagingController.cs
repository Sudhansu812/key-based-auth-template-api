using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Staging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Writes to SQL Server, so on top of ReportAccess it also requires WriteAccess (ReadWrite key).
    [ApiController]
    [Route("api/staging")]
    [Authorize(Policy = ReportAccessRequirement.PolicyName)]
    public class StagingController : ControllerBase
    {
        private readonly IStagingService _stagingService;

        public StagingController(IStagingService stagingService)
        {
            _stagingService = stagingService;
        }

        // Full refresh: truncates stg.ProductRiskSummary and reloads it from PostgreSQL.
        [HttpPost("product-risk-summary")]
        [Authorize(Policy = "WriteAccess")]
        public async Task<ActionResult<StagingLoadResponse>> LoadProductRiskSummary(CancellationToken cancellationToken)
        {
            var result = await _stagingService.LoadProductRiskSummaryAsync(cancellationToken);
            return Ok(result);
        }
    }
}
