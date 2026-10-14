using JobFlow.Application.Common;
using JobFlow.Application.DTOs.Companies;

namespace JobFlow.Application.Interfaces;

public interface ICompanyService
{
    Task<PagedResult<CompanyDto>> GetCompaniesAsync(int page, int pageSize, string? search);
    Task<CompanyDto> GetCompanyByIdAsync(Guid id);
    Task<List<CompanyDto>> GetMyCompaniesAsync(Guid ownerId);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequest request, Guid ownerId);
    Task<CompanyDto> UpdateCompanyAsync(Guid id, UpdateCompanyRequest request, Guid ownerId);
    Task UpdateLogoAsync(Guid id, string logoFileName, string logoUrl, Guid ownerId);
    Task DeleteCompanyAsync(Guid id, Guid ownerId);
}
