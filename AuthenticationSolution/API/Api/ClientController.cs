using API.Data;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly UserDbContext _userDbContext;
        public ClientController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
        }

        [HttpGet]
        public async Task<IEnumerable<ClientDto>> Get()
        {
            return await _userDbContext.Clients.AsNoTracking()
                .Select(c => new ClientDto { ClientCode = c.ClientCode, ClientName = c.ClientName})
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClientDto client)
        {
            if(ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }
            if(await _userDbContext.Clients.AnyAsync(x => x.ClientName == client.ClientName || x.ClientCode == client.ClientCode))
            {
                return BadRequest("Client already exists");
            }
            _userDbContext.Clients.Add(new Client
            {
                ClientName = client.ClientName,
                ClientCode = client.ClientCode
            });
            await _userDbContext.SaveChangesAsync();
            return Ok(client);
        }
    }
}
