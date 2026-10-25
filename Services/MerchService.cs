using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Entities;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class MerchService : IMerchService
{
    private readonly AppDbContext _db;

    public MerchService(AppDbContext db) => _db = db;

    public async Task<List<MerchDto>> GetActiveMerchAsync()
    {
        var list = await _db.Merch
            .Where(m => m.IsActive)
            .OrderByDescending(m => m.IsFeatured)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<List<MerchDto>> GetAllMerchAsync()
    {
        var list = await _db.Merch
            .OrderByDescending(m => m.IsFeatured)
            .ThenByDescending(m => m.CreatedAt)
            .ToListAsync();
        return list.Select(MapToDto).ToList();
    }

    public async Task<MerchDto?> GetMerchAsync(Guid id)
    {
        var m = await _db.Merch.FindAsync(id);
        return m == null ? null : MapToDto(m);
    }

    public async Task<MerchDto> CreateMerchAsync(CreateMerchRequest request)
    {
        var merch = new Merch
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            Sku = request.Sku,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl ?? string.Empty,
            Images = request.Images ?? new List<string>(),
            IsFeatured = request.IsFeatured,
            IsActive = true
        };
        _db.Merch.Add(merch);
        await _db.SaveChangesAsync();
        return MapToDto(merch);
    }

    public async Task<MerchDto> UpdateMerchAsync(Guid id, UpdateMerchRequest request)
    {
        var merch = await _db.Merch.FindAsync(id)
            ?? throw new KeyNotFoundException("Merch not found");

        if (request.Name is not null) merch.Name = request.Name;
        if (request.Description is not null) merch.Description = request.Description;
        if (request.Category is not null) merch.Category = request.Category;
        if (request.Price.HasValue) merch.Price = request.Price.Value;
        if (request.Sku is not null) merch.Sku = request.Sku;
        if (request.Stock.HasValue) merch.Stock = request.Stock.Value;
        if (request.ImageUrl is not null) merch.ImageUrl = request.ImageUrl;
        if (request.Images is not null) merch.Images = request.Images;
        if (request.IsActive.HasValue) merch.IsActive = request.IsActive.Value;
        if (request.IsFeatured.HasValue) merch.IsFeatured = request.IsFeatured.Value;
        merch.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(merch);
    }

    public async Task DeleteMerchAsync(Guid id)
    {
        var merch = await _db.Merch.FindAsync(id)
            ?? throw new KeyNotFoundException("Merch not found");
        _db.Merch.Remove(merch);
        await _db.SaveChangesAsync();
    }

    private static MerchDto MapToDto(Merch m) => new(
        m.Id.ToString(),
        m.Name,
        m.Description,
        m.Category,
        m.Price,
        m.Sku,
        m.Stock,
        m.ImageUrl,
        m.Images,
        m.IsActive,
        m.IsFeatured,
        m.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        m.UpdatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"));
}