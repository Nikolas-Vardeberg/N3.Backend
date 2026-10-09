using Microsoft.AspNetCore.Mvc;
using N3.Backend.Application.DTOs;
using N3.Backend.Application.Interfaces;
using N3.Backend.Application.Parameters;

namespace N3.Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies(
        [FromQuery] CompanyParameters parameters,
        CancellationToken cancellationToken)
    {
        var companies = await _companyService.GetCompaniesAsync(parameters, cancellationToken);
        return Ok(companies);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyDto>> GetCompanyById(int id, CancellationToken cancellationToken)
    {
        var company = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
        return Ok(company);
    }
}
