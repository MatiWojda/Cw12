using System.Threading.Tasks;

namespace Cw12.Services
{
    public interface IClientService
    {
        Task<(bool Found, bool HasTrips, string Message)> DeleteClientAsync(int idClient);
    }
}