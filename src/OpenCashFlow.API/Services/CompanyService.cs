using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;

        public CompanyService(ICompanyRepository CompanyRepository, IAuthenticationService authenticationService, IMapper mapper)
        {
            _companyRepository = CompanyRepository;
            _authenticationService = authenticationService;
            _mapper = mapper;
        }

        public async Task<Company_Detail_DTO?> GetCompanyAsync(CancellationToken cancellationToken)
        {
            var companies = await _companyRepository.GetCompanyByIdAsync(_authenticationService.GetTenantID(), cancellationToken);
            return _mapper.Map<Company_Detail_DTO?>(companies);
        }

        public async Task<Company_Detail_DTO?> GetCompanyAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            var companies = await _companyRepository.GetCompanyByIdAsync(TenantID, cancellationToken);
            return _mapper.Map<Company_Detail_DTO?>(companies);
        }

        public async Task<IEnumerable<Company_Detail_DTO>?> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            var companies = await _companyRepository.GetAllCompaniesAsync(cancellationToken);
            return _mapper.Map<List<Company_Detail_DTO>?>(companies);
        }
    }
}
