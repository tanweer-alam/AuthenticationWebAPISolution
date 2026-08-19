using API.Data;
using API.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly UserDbContext _userDbContext;
        public ProjectController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<ProjectDto>> Get()
        {
            var projects = await _userDbContext.Projects.AsNoTracking().Select(p => new ProjectDto
            {
                ProjectName = p.ProjectName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ClientCode = p.Client.ClientName
            }).ToListAsync();
            return projects;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProjectDto project)
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            var client = await _userDbContext.Clients.FirstOrDefaultAsync(x => x.ClientCode == project.ClientCode);
            if (client == null)
            {
                return BadRequest("Client does not exist");
            }
            var projectExists = await _userDbContext.Projects.AnyAsync(x => x.ProjectName == project.ProjectName);
            if(projectExists)
            {
                return BadRequest("Project already exists");
            }

            _userDbContext.Projects.Add(new Models.Project
            {
                ProjectName = project.ProjectName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ClientId = client.ClientId
            });
            await _userDbContext.SaveChangesAsync();
            return Ok(project);
        }
    }
}
