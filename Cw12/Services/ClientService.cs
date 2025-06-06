using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cw12.Data;
using Cw12.Models;

namespace Cw12.Services
{
    public class ClientService : IClientService
    {
        private readonly Apbd2Context _context;

        public ClientService(Apbd2Context context)
        {
            _context = context;
        }
        
        public async Task<(bool Found, bool HasTrips, string Message)> DeleteClientAsync(int idClient)
        {
            var client = await _context.Clients
                .Include(c => c.ClientTrips)
                .FirstOrDefaultAsync(c => c.IdClient == idClient);

            if (client == null)
                return (false, false, $"ClientNotFound:{idClient}");
            
            if (client.ClientTrips != null && client.ClientTrips.Count > 0)
                return (true, true, "ClientHasTrips");
            
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return (true, false, "ClientDeleted");
        }
    }
}