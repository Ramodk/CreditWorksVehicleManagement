using System;


using CreditWorks.Application.Exceptions;
using CreditWorks.Application.Interfaces;
using CreditWorks.Application.Validation;
using CreditWorks.Domain.Entities;
using CreditWorks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Vehicle>> GetAllAsync(
        string? sortBy = null,
        bool descending = false)
    {
        var query = _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Manufacturer);

        IOrderedQueryable<Vehicle> ordered =
            sortBy?.Trim().ToLowerInvariant() switch
        {
            "owner" => descending
                ? query.OrderByDescending(v => v.Owner)
                : query.OrderBy(v => v.Owner),

            "manufacturer" => descending
                ? query.OrderByDescending(v => v.Manufacturer.Name)
                : query.OrderBy(v => v.Manufacturer.Name),

            "year" => descending
                ? query.OrderByDescending(v => v.Year)
                : query.OrderBy(v => v.Year),

            "weight" => descending
                ? query.OrderByDescending(v => v.Weight)
                : query.OrderBy(v => v.Weight),

            _ => query.OrderBy(v => v.Id)
        };

        // Tie-break on Id so rows with equal sort values
        // always come back in the same order.
        ordered = ordered.ThenBy(v => v.Id);

        return await ordered.ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .AsNoTracking()
            .Include(v => v.Manufacturer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehicle> CreateAsync(Vehicle vehicle)
    {
        var errors = VehicleValidator.Validate(vehicle);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var manufacturerExists =
            await _context.Manufacturers.AnyAsync(
                m => m.Id == vehicle.ManufacturerId);

        if (!manufacturerExists)
        {
            throw new ValidationException(
                "The selected manufacturer does not exist.");
        }

        // Create a new entity to avoid trusting
        // client-supplied IDs or navigation properties.
        var newVehicle = new Vehicle
        {
            Owner = vehicle.Owner.Trim(),
            ManufacturerId = vehicle.ManufacturerId,
            Year = vehicle.Year,
            Weight = vehicle.Weight
        };

        _context.Vehicles.Add(newVehicle);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(newVehicle.Id))!;
    }

    public async Task<bool> UpdateAsync(
        int id, Vehicle vehicle)
    {
        var errors = VehicleValidator.Validate(vehicle);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var existingVehicle =
            await _context.Vehicles.FindAsync(id);

        if (existingVehicle == null)
        {
            return false;
        }

        var manufacturerExists =
            await _context.Manufacturers.AnyAsync(
                m => m.Id == vehicle.ManufacturerId);

        if (!manufacturerExists)
        {
            throw new ValidationException(
                "The selected manufacturer does not exist.");
        }

        existingVehicle.Owner = vehicle.Owner.Trim();
        existingVehicle.ManufacturerId = vehicle.ManufacturerId;
        existingVehicle.Year = vehicle.Year;
        existingVehicle.Weight = vehicle.Weight;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
        {
            return false;
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return true;
    }
}
