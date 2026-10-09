using Microsoft.EntityFrameworkCore;
using N3.Backend.Application.DTOs;
using N3.Backend.Application.Interfaces;
using N3.Backend.Application.Mappings;
using N3.Backend.Application.Parameters;
using N3.Backend.Domain.Exceptions;
using N3.Backend.Infrastructure.Database;
using N3.Backend.Infrastructure.Database.Models;

namespace N3.Backend.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _context;

    public CompanyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(CompanyParameters parameters, CancellationToken cancellationToken)
    {
        // TODO: Apply filters from parameters
        var companies = await _context.Companies
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return companies.Select(company => company.ToDto()).ToList();
    }

    public async Task<CompanyDto> GetCompanyByIdAsync(int id, CancellationToken cancellationToken)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(company => company.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), id);

        return company.ToDto();
    }
}
