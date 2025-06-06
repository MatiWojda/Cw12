using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cw12.DTOs;
using Cw12.Services;

namespace Cw12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _tripService.GetTripsAsync(page, pageSize);
            return Ok(result);
        }
        
        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AddClientToTrip(int idTrip, [FromBody] CreateClientInTripDto dto)
        {
            var (success, message) = await _tripService.AddClientToTripAsync(idTrip, dto);

            if (!success)
            {
                if (message.StartsWith("TripNotFound"))
                    return NotFound(new { Message = $"Wycieczka o ID={idTrip} nie istnieje." });

                if (message == "TripInPast")
                    return BadRequest(new { Message = "Nie można zapisać na wycieczkę, która już się odbyła lub jest w toku." });

                if (message == "ClientAlreadyRegistered")
                    return Conflict(new { Message = "Klient o tym numerze PESEL jest już zapisany na tę wycieczkę." });
            }

            if (message == "ExistingClientAssigned")
                return CreatedAtAction(nameof(AddClientToTrip), new { idTrip },
                    new { Message = "Istniejący klient został przypisany do wycieczki." });

            if (message == "NewClientCreatedAndAssigned")
                return CreatedAtAction(nameof(AddClientToTrip), new { idTrip },
                    new { Message = "Nowy klient został utworzony i przypisany do wycieczki." });
            
            return StatusCode(500, new { Message = "Nieoczekiwany błąd." });
        }
    }
}