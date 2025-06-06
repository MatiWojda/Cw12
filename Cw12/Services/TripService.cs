using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Cw12.Data;
using Cw12.Models;
using Cw12.DTOs;
using Cw12.Services;

namespace Cw12.Services
{
    public class TripService : ITripService
    {
        private readonly Apbd2Context _context;

        public TripService(Apbd2Context context)
        {
            _context = context;
        }
        
        public async Task<PagedResult<TripDto>> GetTripsAsync(int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            
            var totalCount = await _context.Trips.CountAsync();
            var allPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            
            var tripsList = await _context.Trips
                .Include(t => t.IdCountries) 
                .Include(t => t.ClientTrips)
                    .ThenInclude(ct => ct.IdClientNavigation)
                .OrderByDescending(t => t.DateFrom)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            var tripsDto = tripsList.Select(t => new TripDto
            {
                Name        = t.Name,
                Description = t.Description,
                DateFrom    = t.DateFrom,
                DateTo      = t.DateTo,
                MaxPeople   = t.MaxPeople,
                
                Countries = t.IdCountries
                    .Select(c => new CountryDto { Name = c.Name })
                    .ToList(),
                
                Clients = t.ClientTrips
                    .Select(ct => new ClientDto
                    {
                        FirstName = ct.IdClientNavigation.FirstName,
                        LastName  = ct.IdClientNavigation.LastName
                    })
                    .ToList()
            }).ToList();

            return new PagedResult<TripDto>
            {
                PageNum  = page,
                PageSize = pageSize,
                AllPages = allPages,
                Items    = tripsDto
            };
        }
        
        public async Task<(bool Success, string Message)> AddClientToTripAsync(int idTrip, CreateClientInTripDto dto)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.IdTrip == idTrip);
            if (trip == null)
                return (false, $"TripNotFound:{idTrip}");
            
            var now = DateTime.Now;
            if (trip.DateFrom <= now)
                return (false, "TripInPast");
            
            var existingClient = await _context.Clients
                .FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);

            if (existingClient != null)
            {
                var alreadyRegistered = await _context.ClientTrips
                    .AnyAsync(ct => ct.IdTrip == idTrip && ct.IdClient == existingClient.IdClient);
                if (alreadyRegistered)
                    return (false, "ClientAlreadyRegistered");
                
                var newLink = new ClientTrip
                {
                    IdClient     = existingClient.IdClient,
                    IdTrip       = idTrip,
                    RegisteredAt = now,
                    PaymentDate  = dto.PaymentDate
                };
                _context.ClientTrips.Add(newLink);
                await _context.SaveChangesAsync();
                return (true, "ExistingClientAssigned");
            }
            
            var newClient = new Client
            {
                FirstName = dto.FirstName,
                LastName  = dto.LastName,
                Email     = dto.Email,
                Telephone = dto.Telephone,
                Pesel     = dto.Pesel
            };
            await _context.Clients.AddAsync(newClient);
            await _context.SaveChangesAsync();

            var clientTrip = new ClientTrip
            {
                IdClient     = newClient.IdClient,
                IdTrip       = idTrip,
                RegisteredAt = now,
                PaymentDate  = dto.PaymentDate
            };
            _context.ClientTrips.Add(clientTrip);
            await _context.SaveChangesAsync();

            return (true, "NewClientCreatedAndAssigned");
        }
    }
}