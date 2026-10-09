using N3.Backend.Application.DTOs;
using N3.Backend.Application.Parameters;

namespace N3.Backend.Application.Interfaces;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(CompanyParameters parameters, CancellationToken cancellationToken);
    Task<CompanyDto> GetCompanyByIdAsync(int id, CancellationToken cancellationToken);
}
