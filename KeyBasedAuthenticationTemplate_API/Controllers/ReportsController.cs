using KeyBasedAuthenticationTemplate_API.Authorization;
using KeyBasedAuthenticationTemplate_API.Services.Interfaces;
using KeyBasedAuthenticationTemplate_Contracts.Products;
using KeyBasedAuthenticationTemplate_Contracts.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KeyBasedAuthenticationTemplate_API.Controllers
{
    // Report data for the Automation project. "ef" endpoints read through EF Core, "dapper"
    // endpoints return the same data through raw SQL. Products live in PostgreSQL, users in SQL Server.
    // (Products via EF is the existing GET /api/products, which already accepts API keys.)
    [ApiController]
    [Route("api/reports")]
    [Authorize(Policy = ReportAccessRequirement.PolicyName)]
    public class ReportsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IReportService _reportService;

        public ReportsController(IUserService userService, IReportService reportService)
        {
            _userService = userService;
            _reportService = reportService;
        }

        [HttpGet("ef/users")]
        public async Task<ActionResult<List<UserSummaryResponse>>> GetUsersEf(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("dapper/products")]
        public async Task<ActionResult<List<ProductResponse>>> GetProductsDapper(CancellationToken cancellationToken)
        {
            var result = await _reportService.GetProductsAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("dapper/users")]
        public async Task<ActionResult<List<UserSummaryResponse>>> GetUsersDapper(CancellationToken cancellationToken)
        {
            var result = await _reportService.GetUsersAsync(cancellationToken);
            return Ok(result);
        }
    }
}
