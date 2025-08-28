using Application.DTO.Api;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Application.Contracts.IRepository;
using Domain;
using System.Linq;
using AutoMapper;
using System.Collections.Generic;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IMapper _mapper;

        public ApiController(IApplicationRepository applicationRepository, IMapper mapper)
        {
            _applicationRepository = applicationRepository;
            _mapper = mapper;
        }

        [HttpPost("getaccess")]
        public async Task<ActionResult<AppAccessDto>> GetAppAccess([FromBody] AppCredentialDto credentials)
        {
            if (string.IsNullOrEmpty(credentials.AppId) || string.IsNullOrEmpty(credentials.AppKey))
            {
                return BadRequest(new { message = "AppId and AppKey cannot be null or empty." });
            }

            // 1. Validate Credentials
            var application = await _applicationRepository.GetApplicationByCredentials(credentials.AppId, credentials.AppKey);

            if (application == null)
            {
                return StatusCode(401, new { message = "Invalid AppId or AppKey." });
            }

            // 2. Checks Approval Status
            if (application.Status != "Approved")
            {
                return StatusCode(403, new { message = "Application is not approved." });
            }

            // 3. Retrieves Group & Roles
            var groupDto = application.ApplicationGroup != null
                ? _mapper.Map<GroupDto>(application.ApplicationGroup)
                : null;

            // Ensure roles are mapped correctly if the group exists and has roles
            if (groupDto != null && application.ApplicationGroup != null && application.ApplicationGroup.GroupRoles != null)
            {
                groupDto.Roles = _mapper.Map<List<RoleDto>>(application.ApplicationGroup.GroupRoles.Select(gr => gr.RoleModel));
            }

            // 4. Returns Access Info
            var appAccessDto = new AppAccessDto
            {
                AppName = application.Name,
                IsApproved = application.Status == "Approved",
                Group = groupDto
            };

            return Ok(appAccessDto);
        }
    }
}
