using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;

namespace GarmentsERP.Production.Application.Interfaces;

public interface IProductionRepository
{
    // Buyers
    Task<IEnumerable<Buyer>> GetBuyersAsync(CancellationToken cancellationToken = default);
    Task<Buyer?> GetBuyerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Buyer> AddBuyerAsync(Buyer buyer, CancellationToken cancellationToken = default);

    // Buyer Orders
    Task<IEnumerable<BuyerOrder>> GetBuyerOrdersAsync(OrderStatus? status = null, CancellationToken cancellationToken = default);
    Task<BuyerOrder?> GetBuyerOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BuyerOrder> AddBuyerOrderAsync(BuyerOrder order, CancellationToken cancellationToken = default);
    Task UpdateBuyerOrderAsync(BuyerOrder order, CancellationToken cancellationToken = default);

    // Cutting Orders & Bundles
    Task<IEnumerable<CuttingOrder>> GetCuttingOrdersAsync(CuttingStatus? status = null, CancellationToken cancellationToken = default);
    Task<CuttingOrder?> GetCuttingOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CuttingOrder> AddCuttingOrderAsync(CuttingOrder order, CancellationToken cancellationToken = default);
    Task UpdateCuttingOrderAsync(CuttingOrder order, CancellationToken cancellationToken = default);
    Task<IEnumerable<CutBundle>> GetBundlesAsync(Guid? cuttingOrderId = null, BundleSection? section = null, CancellationToken cancellationToken = default);
    Task<CutBundle?> GetBundleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddBundlesAsync(IEnumerable<CutBundle> bundles, CancellationToken cancellationToken = default);
    Task UpdateBundleAsync(CutBundle bundle, CancellationToken cancellationToken = default);

    // Sewing Lines & Hourly Logs
    Task<IEnumerable<SewingLine>> GetSewingLinesAsync(CancellationToken cancellationToken = default);
    Task<SewingLine?> GetSewingLineByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SewingLine> AddSewingLineAsync(SewingLine line, CancellationToken cancellationToken = default);
    Task<IEnumerable<HourlyProductionLog>> GetHourlyLogsAsync(DateTime? date = null, Guid? lineId = null, CancellationToken cancellationToken = default);
    Task<HourlyProductionLog> AddHourlyLogAsync(HourlyProductionLog log, CancellationToken cancellationToken = default);

    // Quality Inspections
    Task<IEnumerable<QualityInspection>> GetQualityInspectionsAsync(InspectionType? type = null, CancellationToken cancellationToken = default);
    Task<QualityInspection?> GetQualityInspectionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<QualityInspection> AddQualityInspectionAsync(QualityInspection inspection, CancellationToken cancellationToken = default);

    // Packing & Dispatch (Phase 4)
    Task<IEnumerable<CartonPack>> GetCartonPacksAsync(CartonStatus? status = null, CancellationToken cancellationToken = default);
    Task<CartonPack?> GetCartonPackByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<CartonPack> AddCartonPackAsync(CartonPack carton, CancellationToken cancellationToken = default);
    Task UpdateCartonPackAsync(CartonPack carton, CancellationToken cancellationToken = default);

    Task<IEnumerable<DeliveryChallan>> GetDeliveryChallansAsync(ChallanStatus? status = null, CancellationToken cancellationToken = default);
    Task<DeliveryChallan?> GetDeliveryChallanByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DeliveryChallan> AddDeliveryChallanAsync(DeliveryChallan challan, CancellationToken cancellationToken = default);
    Task UpdateDeliveryChallanAsync(DeliveryChallan challan, CancellationToken cancellationToken = default);

    // Metrics
    Task<ProductionMetricsDto> GetProductionMetricsAsync(CancellationToken cancellationToken = default);

    // Phase 4 Cutting Extensions
    Task<IEnumerable<CADMarker>> GetCADMarkersAsync(CancellationToken cancellationToken = default);
    Task<CADMarker?> GetCADMarkerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CADMarker> AddCADMarkerAsync(CADMarker marker, CancellationToken cancellationToken = default);

    Task<IEnumerable<RelaxationSession>> GetRelaxationSessionsAsync(RelaxationStatus? status = null, CancellationToken cancellationToken = default);
    Task<RelaxationSession?> GetRelaxationSessionByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RelaxationSession> AddRelaxationSessionAsync(RelaxationSession session, CancellationToken cancellationToken = default);
    Task UpdateRelaxationSessionAsync(RelaxationSession session, CancellationToken cancellationToken = default);

    Task<IEnumerable<FusingMachineLog>> GetFusingLogsAsync(string? cutOrderNumber = null, CancellationToken cancellationToken = default);
    Task<FusingMachineLog> AddFusingLogAsync(FusingMachineLog log, CancellationToken cancellationToken = default);

    Task<IEnumerable<SubcontractCuttingChallan>> GetSubcontractChallansAsync(SubcontractStatus? status = null, CancellationToken cancellationToken = default);
    Task<SubcontractCuttingChallan?> GetSubcontractChallanByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SubcontractCuttingChallan> AddSubcontractChallanAsync(SubcontractCuttingChallan challan, CancellationToken cancellationToken = default);
    Task UpdateSubcontractChallanAsync(SubcontractCuttingChallan challan, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
