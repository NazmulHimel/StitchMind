using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// WHS-01: 3D Warehouse Coordinate Location Mapping Engine.
/// Tracks physical warehouse storage down to Warehouse, Aisle, Rack, Tier, and Bin.
/// Format: {WarehouseCode}-{Aisle}-{Rack}-{Tier}-{Bin} (e.g. WH1-A03-R02-T01-B04).
/// </summary>
public class WarehouseLocation : BaseEntity
{
    public string LocationCode { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = "WH1";
    public string Aisle { get; set; } = "A01";
    public string Rack { get; set; } = "R01";
    public string Tier { get; set; } = "T01";
    public string Bin { get; set; } = "B01";
    public WarehouseZone Zone { get; set; } = WarehouseZone.RawMaterialFabric;
    public int MaxCapacityRolls { get; set; } = 12;
    public int CurrentRollCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public Guid CompanyId { get; set; }

    public decimal OccupancyPercentage => MaxCapacityRolls > 0
        ? Math.Round(((decimal)CurrentRollCount / MaxCapacityRolls) * 100m, 1)
        : 0m;

    public bool IsFull => CurrentRollCount >= MaxCapacityRolls;

    public static string FormatLocationCode(string warehouse, string aisle, string rack, string tier, string bin)
    {
        return $"{warehouse.Trim().ToUpper()}-{aisle.Trim().ToUpper()}-{rack.Trim().ToUpper()}-{tier.Trim().ToUpper()}-{bin.Trim().ToUpper()}";
    }

    public static WarehouseLocation Create(
        string warehouse, string aisle, string rack, string tier, string bin,
        WarehouseZone zone, int maxCapacityRolls, Guid companyId)
    {
        var code = FormatLocationCode(warehouse, aisle, rack, tier, bin);
        return new WarehouseLocation
        {
            LocationCode = code,
            WarehouseCode = warehouse.Trim().ToUpper(),
            Aisle = aisle.Trim().ToUpper(),
            Rack = rack.Trim().ToUpper(),
            Tier = tier.Trim().ToUpper(),
            Bin = bin.Trim().ToUpper(),
            Zone = zone,
            MaxCapacityRolls = maxCapacityRolls,
            CurrentRollCount = 0,
            IsActive = true,
            CompanyId = companyId
        };
    }

    public bool CanAccommodate(int rolls)
    {
        return IsActive && (CurrentRollCount + rolls <= MaxCapacityRolls);
    }

    public void AssignRoll(int count = 1)
    {
        if (!CanAccommodate(count))
        {
            throw new InvalidOperationException($"Location '{LocationCode}' capacity exceeded. Max: {MaxCapacityRolls}, Current: {CurrentRollCount}, Requested: {count}.");
        }
        CurrentRollCount += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRoll(int count = 1)
    {
        if (CurrentRollCount - count < 0)
        {
            throw new InvalidOperationException($"Location '{LocationCode}' roll count cannot be negative.");
        }
        CurrentRollCount -= count;
        UpdatedAt = DateTime.UtcNow;
    }
}
