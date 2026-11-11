using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.ApiKeys;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Report data for the Automation project. "ef" endpoints read through EF Core, "dapper"
    // endpoints return the same data through raw SQL. Products live in PostgreSQL, API keys in SQL Server.
    // (Products via EF is the existing GET /api/products, which Reporter keys read unscoped.)
    [ApiController]
    [Route("api/reports")]
    [Authorize(Policy = Policies.ReportAccess)]
    public class ReportsController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IReportService _reportService;

        public ReportsController(IApiKeyService apiKeyService, IReportService reportService)
        {
            _apiKeyService = apiKeyService;
            _reportService = reportService;
        }

        [HttpGet("ef/apikeys")]
        public async Task<ActionResult<List<ApiKeySummaryResponse>>> GetApiKeysEf(CancellationToken cancellationToken)
        {
            var result = await _apiKeyService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("dapper/products")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsDapper(CancellationToken cancellationToken)
        {
            var result = await _reportService.GetProductsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("dapper/apikeys")]
        public async Task<ActionResult<List<ApiKeySummaryResponse>>> GetApiKeysDapper(CancellationToken cancellationToken)
        {
            var result = await _reportService.GetApiKeysAsync(cancellationToken);
            return Ok(result);
        }
    }
}
