using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TecNM.Api.Core.Http;

namespace TecNM.Api.Modules.Companies;

[ApiController]
[Route("api/companies")]
public class CompaniesController(CompaniesService companiesService) : ControllerBase
{
    private const string StaffAndDeptRoles = "admin,social_service_officer,internship_coordinator,department_head";
    private const string StaffRoles = "admin,social_service_officer,internship_coordinator";

    [Authorize(Roles = StaffAndDeptRoles)]
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CompanyResponse>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? sector = null,
        [FromQuery] string? companySize = null,
        [FromQuery] bool? isTecnmInternal = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        var result = await companiesService.GetPagedAsync(
            page, pageSize, search, sector, companySize, isTecnmInternal, isActive, sort, cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpGet("options")]
    public async Task<ActionResult<IReadOnlyList<OptionItem>>> GetOptions(
        [FromQuery] bool? isTecnmInternal = null,
        CancellationToken cancellationToken = default)
    {
        var options = await companiesService.GetOptionsAsync(isTecnmInternal, cancellationToken);
        return Ok(options);
    }

    [Authorize(Roles = StaffAndDeptRoles)]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<CompanyResponse>> GetById(long id, CancellationToken cancellationToken = default)
    {
        var company = await companiesService.GetByIdAsync(id, cancellationToken);
        return Ok(company);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create(
        [FromBody] CompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var created = await companiesService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<CompanyResponse>> Update(
        long id,
        [FromBody] CompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        var updated = await companiesService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpPatch("{id:long}/status")]
    public async Task<ActionResult<CompanyResponse>> UpdateStatus(
        long id,
        [FromBody] UpdateStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var updated = await companiesService.UpdateStatusAsync(id, request.IsActive, cancellationToken);
        return Ok(updated);
    }

    [Authorize(Roles = StaffRoles)]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken = default)
    {
        await companiesService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
