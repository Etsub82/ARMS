using Application.Contracts.IRepository;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Application.DTO.Admin;
using Application.Response;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator")] // Secure AdminController
    public class AdminController : ControllerBase
    {
        private readonly IGenericRepository<ApplicationGroup> _groupRepository;
        private readonly IGenericRepository<RoleModel> _roleRepository;
        private readonly IGenericRepository<ApplicationModel> _applicationRepository;
        private readonly IGenericRepository<GroupRole> _groupRoleRepository;
        private readonly IMapper _mapper;

        public AdminController(IGenericRepository<ApplicationGroup> groupRepository, IGenericRepository<RoleModel> roleRepository, IGenericRepository<ApplicationModel> applicationRepository, IGenericRepository<GroupRole> groupRoleRepository, IMapper mapper)
        {
            _groupRepository = groupRepository;
            _roleRepository = roleRepository;
            _applicationRepository = applicationRepository;
            _groupRoleRepository = groupRoleRepository;
            _mapper = mapper;
        }

        [HttpPost("createGroup")]
        public async Task<ActionResult<BaseCommandResponse>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            if (createGroupDto == null || string.IsNullOrWhiteSpace(createGroupDto.Name))
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Group name cannot be empty." });
            }

            var group = new ApplicationGroup 
            {
                Name = createGroupDto.Name,
                DateCreated = DateTime.UtcNow,
                CreatedBy = "Admin", // Set default for now
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = "Admin" // Set default for now
            };
            await _groupRepository.Add(group);

            return Ok(new BaseCommandResponse { Success = true, Message = "Group created successfully.", Id = group.Id });
        }

        [HttpPost("createRole")]
        public async Task<ActionResult<BaseCommandResponse>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (createRoleDto == null || string.IsNullOrWhiteSpace(createRoleDto.Name))
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Role name cannot be empty." });
            }

            var role = new RoleModel
            {
                Name = createRoleDto.Name,
                DateCreated = DateTime.UtcNow,
                CreatedBy = "Admin", // Set default for now
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = "Admin" // Set default for now
            };
            await _roleRepository.Add(role);

            return Ok(new BaseCommandResponse { Success = true, Message = "Role created successfully.", Id = role.Id });
        }

        [HttpPost("createApplication")]
        public async Task<ActionResult<BaseCommandResponse>> CreateApplication([FromBody] CreateApplicationDto createApplicationDto)
        {
            if (createApplicationDto == null || string.IsNullOrWhiteSpace(createApplicationDto.Name))
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Application name cannot be empty." });
            }

            var application = new ApplicationModel
            {
                Name = createApplicationDto.Name,
                AppId = (string.IsNullOrWhiteSpace(createApplicationDto.AppId) || createApplicationDto.AppId.Equals("null", StringComparison.OrdinalIgnoreCase)) ? Guid.NewGuid().ToString() : createApplicationDto.AppId, // Generate if not provided or empty or literal "null"
                AppKey = (string.IsNullOrWhiteSpace(createApplicationDto.AppKey) || createApplicationDto.AppKey.Equals("null", StringComparison.OrdinalIgnoreCase)) ? Guid.NewGuid().ToString() : createApplicationDto.AppKey, // Generate if not provided or empty or literal "null"
                Status = "Pending", // Initial status
                DateCreated = DateTime.UtcNow,
                CreatedBy = "Admin",
                LastModifiedDate = DateTime.UtcNow,
                LastModifiedBy = "Admin"
            };

            Console.WriteLine($"Generated AppId: {application.AppId}");
            Console.WriteLine($"Generated AppKey: {application.AppKey}");

            await _applicationRepository.Add(application);

            return Ok(new BaseCommandResponse { Success = true, Message = "Application created successfully.", Id = application.Id });
        }

        [HttpGet("pendingApplications")]
        public async Task<ActionResult<List<ApplicationListDto>>> GetPendingApplications()
        {
            var pendingApplications = await _applicationRepository.Find(app => app.Status == "Pending");
            return Ok(_mapper.Map<List<ApplicationListDto>>(pendingApplications));
        }

        [HttpPut("approveApplication")]
        public async Task<ActionResult<BaseCommandResponse>> ApproveApplication([FromBody] ApproveApplicationDto approveApplicationDto)
        {
            var application = await _applicationRepository.GetById(approveApplicationDto.Id);
            if (application == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application not found." });
            }

            if (application.Status == "Approved")
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Application is already approved." });
            }

            application.Status = "Approved";
            application.LastModifiedDate = DateTime.UtcNow;
            application.LastModifiedBy = "Admin";
            await _applicationRepository.Update(application);

            return Ok(new BaseCommandResponse { Success = true, Message = "Application approved successfully.", Id = application.Id });
        }

        [HttpPut("rejectApplication")]
        public async Task<ActionResult<BaseCommandResponse>> RejectApplication([FromBody] RejectApplicationDto rejectApplicationDto)
        {
            var application = await _applicationRepository.GetById(rejectApplicationDto.Id);
            if (application == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application not found." });
            }

            if (application.Status == "Rejected")
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Application is already rejected." });
            }

            application.Status = "Rejected";
            application.LastModifiedDate = DateTime.UtcNow;
            application.LastModifiedBy = "Admin";
            await _applicationRepository.Update(application);

            return Ok(new BaseCommandResponse { Success = true, Message = "Application rejected successfully.", Id = application.Id });
        }

        [HttpPut("assignApplicationToGroup")]
        public async Task<ActionResult<BaseCommandResponse>> AssignApplicationToGroup([FromBody] AssignApplicationToGroupDto assignApplicationToGroupDto)
        {
            var application = await _applicationRepository.GetById(assignApplicationToGroupDto.ApplicationId);
            if (application == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application not found." });
            }

            var group = await _groupRepository.GetById(assignApplicationToGroupDto.ApplicationGroupId);
            if (group == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application Group not found." });
            }

            if (application.Status != "Approved")
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Application must be approved before assigning to a group." });
            }

            application.ApplicationGroupId = assignApplicationToGroupDto.ApplicationGroupId;
            application.LastModifiedDate = DateTime.UtcNow;
            application.LastModifiedBy = "Admin";
            await _applicationRepository.Update(application);

            return Ok(new BaseCommandResponse { Success = true, Message = "Application assigned to group successfully.", Id = application.Id });
        }

        [HttpPost("assignRolesToGroup")]
        public async Task<ActionResult<BaseCommandResponse>> AssignRolesToGroup([FromBody] AssignRolesToGroupDto assignRolesToGroupDto)
        {
            var group = await _groupRepository.GetById(assignRolesToGroupDto.ApplicationGroupId);
            if (group == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application Group not found." });
            }

            var roles = new List<RoleModel>();
            foreach (var roleId in assignRolesToGroupDto.RoleIds)
            {
                var role = await _roleRepository.GetById(roleId);
                if (role == null)
                {
                    return NotFound(new BaseCommandResponse { Success = false, Message = $"Role with ID {roleId} not found." });
                }
                roles.Add(role);
            }

            foreach (var role in roles)
            {
                var groupRole = new GroupRole
                {
                    ApplicationGroupId = assignRolesToGroupDto.ApplicationGroupId,
                    RoleModelId = role.Id,
                    DateCreated = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    LastModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "Admin"
                };
                await _groupRoleRepository.Add(groupRole);
            }

            return Ok(new BaseCommandResponse { Success = true, Message = "Roles assigned to group successfully." });
        }

        [HttpDelete("deleteGroup/{id}")]
        public async Task<ActionResult<BaseCommandResponse>> DeleteGroup(int id)
        {
            var group = await _groupRepository.GetById(id);
            if (group == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application Group not found." });
            }

            // Check for dependent applications
            var dependentApplications = await _applicationRepository.Find(app => app.ApplicationGroupId == id);
            if (dependentApplications != null && dependentApplications.Any())
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Cannot delete group: Applications are still assigned to it." });
            }

            // Check for dependent group-roles
            var dependentGroupRoles = await _groupRoleRepository.Find(gr => gr.ApplicationGroupId == id);
            if (dependentGroupRoles != null && dependentGroupRoles.Any())
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Cannot delete group: Roles are still assigned to it." });
            }
            
            await _groupRepository.Delete(group);

            return Ok(new BaseCommandResponse { Success = true, Message = "Group deleted successfully." });
        }

        [HttpDelete("deleteApplication/{id}")]
        public async Task<ActionResult<BaseCommandResponse>> DeleteApplication(int id)
        {
            var application = await _applicationRepository.GetById(id);
            if (application == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Application not found." });
            }
            
            // For now, we will allow deletion even if assigned to a group.
            // The ApplicationGroupId is nullable, so it will be set to NULL on deletion of the application.
            // In a real scenario, you might add a check here to prevent deletion or prompt for unassignment.

            await _applicationRepository.Delete(application);

            return Ok(new BaseCommandResponse { Success = true, Message = "Application deleted successfully." });
        }

        [HttpDelete("deleteRole/{id}")]
        public async Task<ActionResult<BaseCommandResponse>> DeleteRole(int id)
        {
            var role = await _roleRepository.GetById(id);
            if (role == null)
            {
                return NotFound(new BaseCommandResponse { Success = false, Message = "Role not found." });
            }

            // Check for dependent GroupRoles
            var dependentGroupRoles = await _groupRoleRepository.Find(gr => gr.RoleModelId == id);
            if (dependentGroupRoles != null && dependentGroupRoles.Any())
            {
                return BadRequest(new BaseCommandResponse { Success = false, Message = "Cannot delete role: It is currently assigned to one or more groups." });
            }
            
            await _roleRepository.Delete(role);

            return Ok(new BaseCommandResponse { Success = true, Message = "Role deleted successfully." });
        }
    }
}
