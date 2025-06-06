using System.Threading.Tasks;
using Cw12.DTOs;

namespace Cw12.Services
{
    public interface ITripService
    {
        Task<PagedResult<TripDto>> GetTripsAsync(int page, int pageSize);
        Task<(bool Success, string Message)> AddClientToTripAsync(int idTrip, CreateClientInTripDto dto);
    }
}