using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cw12.Services;

namespace Cw12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }
        
        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var (found, hasTrips, message) = await _clientService.DeleteClientAsync(idClient);

            if (!found)
                return NotFound(new { Message = $"Klient o ID={idClient} nie istnieje." });

            if (hasTrips)
                return BadRequest(new { Message = "Nie można usunąć klienta, ponieważ jest przypisany do wycieczki." });

            return NoContent();
        }
    }
}