using System;

using CreditWorks.Domain.Entities;

namespace CreditWorks.Application.Interfaces;

public interface IManufacturerService
{
    Task<List<Manufacturer>> GetAllAsync();
}