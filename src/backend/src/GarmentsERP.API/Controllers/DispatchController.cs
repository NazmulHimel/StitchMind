using Microsoft.AspNetCore.Mvc;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispatchController : ControllerBase
{
    private readonly IProductionRepository _repo;

    public DispatchController(IProductionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("cartons")]
    public async Task<IActionResult> GetCartons([FromQuery] CartonStatus? status)
    {
        var cartons = await _repo.GetCartonPacksAsync(status);
        var dtos = cartons.Select(c => new CartonPackDto(
            c.Id,
            c.CartonBarcode,
            c.BuyerOrderId,
            c.BuyerOrder?.OrderNumber ?? "ORD-2026-001",
            c.BuyerOrder?.Buyer?.Name ?? "General Export",
            c.StyleNumber,
            c.Color,
            c.SizeBreakdownJson,
            c.TotalQuantityPcs,
            c.GrossWeightKg,
            c.NetWeightKg,
            c.Status.ToString(),
            c.AssignedPallet
        )).ToList();

        return Ok(Result<List<CartonPackDto>>.Success(dtos));
    }

    [HttpPost("cartons")]
    public async Task<IActionResult> CreateCarton([FromBody] CreateCartonPackRequest req)
    {
        var barcode = $"CTN-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var carton = new CartonPack
        {
            CartonBarcode = barcode,
            BuyerOrderId = req.BuyerOrderId,
            StyleNumber = req.StyleNumber,
            Color = req.Color,
            SizeBreakdownJson = req.SizeBreakdownJson,
            TotalQuantityPcs = req.TotalQuantityPcs,
            GrossWeightKg = req.GrossWeightKg,
            NetWeightKg = req.NetWeightKg,
            Status = CartonStatus.Packed,
            AssignedPallet = req.AssignedPallet
        };

        await _repo.AddCartonPackAsync(carton);
        await _repo.SaveChangesAsync();

        var dto = new CartonPackDto(
            carton.Id,
            carton.CartonBarcode,
            carton.BuyerOrderId,
            "ORD-EX-2026",
            "Buyer Direct",
            carton.StyleNumber,
            carton.Color,
            carton.SizeBreakdownJson,
            carton.TotalQuantityPcs,
            carton.GrossWeightKg,
            carton.NetWeightKg,
            carton.Status.ToString(),
            carton.AssignedPallet
        );

        return Ok(Result<CartonPackDto>.Success(dto));
    }

    [HttpGet("challans")]
    public async Task<IActionResult> GetChallans([FromQuery] ChallanStatus? status)
    {
        var challans = await _repo.GetDeliveryChallansAsync(status);
        var dtos = challans.Select(d => new DeliveryChallanDto(
            d.Id,
            d.ChallanNumber,
            d.BuyerId,
            d.Buyer?.Name ?? "Export Consignee",
            d.VehicleNumber,
            d.DriverName,
            d.DriverPhone,
            d.DestinationAddress,
            d.TotalCartons,
            d.TotalGarmentPcs,
            d.TotalWeightKg,
            d.DispatchDate,
            d.Status.ToString(),
            d.GatePassNumber,
            d.Remarks,
            d.Items.Select(i => new DeliveryChallanItemDto(i.Id, i.CartonBarcode, i.StyleNumber, i.Color, i.QuantityPcs)).ToList()
        )).ToList();

        return Ok(Result<List<DeliveryChallanDto>>.Success(dtos));
    }

    [HttpPost("challans")]
    public async Task<IActionResult> CreateChallan([FromBody] CreateDeliveryChallanRequest req)
    {
        var challanNo = $"CH-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100, 999)}";
        var challan = new DeliveryChallan
        {
            ChallanNumber = challanNo,
            BuyerId = req.BuyerId,
            VehicleNumber = req.VehicleNumber,
            DriverName = req.DriverName,
            DriverPhone = req.DriverPhone,
            DestinationAddress = req.DestinationAddress,
            TotalCartons = req.CartonBarcodes.Count,
            TotalGarmentPcs = req.TotalGarmentPcs,
            TotalWeightKg = req.TotalWeightKg,
            DispatchDate = req.DispatchDate,
            Status = ChallanStatus.Prepared,
            Remarks = req.Remarks
        };

        foreach (var code in req.CartonBarcodes)
        {
            challan.Items.Add(new DeliveryChallanItem
            {
                CartonBarcode = code,
                StyleNumber = "ST-882",
                Color = "Navy Blue",
                QuantityPcs = 50
            });
        }

        await _repo.AddDeliveryChallanAsync(challan);
        await _repo.SaveChangesAsync();

        var dto = new DeliveryChallanDto(
            challan.Id,
            challan.ChallanNumber,
            challan.BuyerId,
            "Selected Buyer",
            challan.VehicleNumber,
            challan.DriverName,
            challan.DriverPhone,
            challan.DestinationAddress,
            challan.TotalCartons,
            challan.TotalGarmentPcs,
            challan.TotalWeightKg,
            challan.DispatchDate,
            challan.Status.ToString(),
            challan.GatePassNumber,
            challan.Remarks,
            challan.Items.Select(i => new DeliveryChallanItemDto(i.Id, i.CartonBarcode, i.StyleNumber, i.Color, i.QuantityPcs)).ToList()
        );

        return Ok(Result<DeliveryChallanDto>.Success(dto));
    }

    [HttpPost("challans/{id}/dispatch")]
    public async Task<IActionResult> DispatchChallan(Guid id, [FromBody] DispatchChallanRequest req)
    {
        var challan = await _repo.GetDeliveryChallanByIdAsync(id);
        if (challan == null)
            return NotFound(Result<bool>.Failure("Challan not found."));

        challan.Status = ChallanStatus.Dispatched;
        challan.GatePassNumber = req.GatePassNumber ?? $"GP-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";

        await _repo.UpdateDeliveryChallanAsync(challan);
        await _repo.SaveChangesAsync();

        return Ok(Result<bool>.Success(true));
    }
}

public record CreateCartonPackRequest(
    Guid BuyerOrderId,
    string StyleNumber,
    string Color,
    string SizeBreakdownJson,
    int TotalQuantityPcs,
    decimal GrossWeightKg,
    decimal NetWeightKg,
    string? AssignedPallet
);

public record CreateDeliveryChallanRequest(
    Guid BuyerId,
    string VehicleNumber,
    string DriverName,
    string DriverPhone,
    string DestinationAddress,
    DateTime DispatchDate,
    string? Remarks,
    int TotalGarmentPcs,
    decimal TotalWeightKg,
    List<string> CartonBarcodes
);

public record DispatchChallanRequest(string? GatePassNumber);
