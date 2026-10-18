using System;
using CreditWorks.Application.Interfaces;
using CreditWorks.Domain.Entities;
using CreditWorks.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Infrastructure.Services;

public class ManufacturerService : IManufacturerService
{
    private readonly ApplicationDbContext _context;

    public ManufacturerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Manufacturer>> GetAllAsync()
    {
        return await _context.Manufacturers
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}