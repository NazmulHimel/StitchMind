using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Queries;

public record GetProcurementMetricsQuery(Guid CompanyId) : IRequest<Result<ProcurementMetricsDto>>;

public class GetProcurementMetricsQueryHandler(IProcurementRepository repository)
    : IRequestHandler<GetProcurementMetricsQuery, Result<ProcurementMetricsDto>>
{
    public async Task<Result<ProcurementMetricsDto>> Handle(GetProcurementMetricsQuery request, CancellationToken cancellationToken)
    {
        var (totalPOs, totalSpend, pendingDeliveries, completedPOs) =
            await repository.GetProcurementMetricsAsync(request.CompanyId, cancellationToken);

        var suppliers = await repository.GetSuppliersAsync(request.CompanyId, ct: cancellationToken);

        var dto = new ProcurementMetricsDto(
            totalPOs,
            totalSpend,
            pendingDeliveries,
            completedPOs,
            suppliers.Count,
            suppliers.Count(s => s.IsActive)
        );

        return Result<ProcurementMetricsDto>.Success(dto);
    }
}
