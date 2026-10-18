using System;
using CreditWorks.Domain.Entities;
namespace CreditWorks.Application.Interfaces;

public interface IVehicleService
{
    Task<List<Vehicle>> GetAllAsync(
        string? sortBy = null,
        bool descending = false);

    Task<Vehicle?> GetByIdAsync(int id);

    Task<Vehicle> CreateAsync(Vehicle vehicle);

    Task<bool> UpdateAsync(int id, Vehicle vehicle);

    Task<bool> DeleteAsync(int id);
}
