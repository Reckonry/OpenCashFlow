using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs.Identity;

namespace OpenCashFlow.API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Role_List_DTO>> GetVisibleRolesAsync(CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetVisibleRolesAsync(cancellationToken);
            return _mapper.Map<IEnumerable<Role_List_DTO>>(roles);
        }
    }
}

