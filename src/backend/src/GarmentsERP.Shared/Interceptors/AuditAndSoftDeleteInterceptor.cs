using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GarmentsERP.Shared.Interceptors;

/// <summary>
/// EF Core interceptor that automatically stamps audit properties (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy)
/// and converts hard-deletes on BaseEntity to soft-deletes.
/// </summary>
public class AuditAndSoftDeleteInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditTrailStore? _auditTrailStore;

    public AuditAndSoftDeleteInterceptor(
        ICurrentUserService currentUserService,
        IAuditTrailStore? auditTrailStore = null)
    {
        _currentUserService = currentUserService;
        _auditTrailStore = auditTrailStore;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditAndSoftDelete(DbContext? context)
    {
        if (context == null) return;

        var now = DateTime.UtcNow;
        var hasUser = _currentUserService.IsAuthenticated && _currentUserService.UserId != Guid.Empty;
        var userId = hasUser ? _currentUserService.UserId : (Guid?)null;
        var userName = _currentUserService.UserName;
        var ipAddress = _currentUserService.IpAddress;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var entityName = entry.Entity.GetType().Name;
            var entityId = entry.Entity.Id.ToString();
            string actionType;

            switch (entry.State)
            {
                case EntityState.Added:
                    actionType = "INSERT";
                    entry.Entity.CreatedAt = now;
                    if (userId.HasValue && !entry.Entity.CreatedBy.HasValue)
                    {
                        entry.Entity.CreatedBy = userId.Value;
                    }
                    break;

                case EntityState.Modified:
                    actionType = "UPDATE";
                    entry.Entity.UpdatedAt = now;
                    if (userId.HasValue)
                    {
                        entry.Entity.UpdatedBy = userId.Value;
                    }
                    break;

                case EntityState.Deleted:
                    actionType = "DELETE";
                    // Convert hard delete to soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(userId ?? Guid.Empty);
                    break;

                default:
                    continue;
            }

            if (_auditTrailStore != null)
            {
                var auditLog = AuditTrailLog.Create(
                    entityName: entityName,
                    entityId: entityId,
                    actionType: actionType,
                    userId: userId,
                    userName: userName,
                    userIpAddress: ipAddress,
                    oldValuesJson: null,
                    newValuesJson: null,
                    companyId: _currentUserService.CompanyId
                );
                _auditTrailStore.RecordAuditLogAsync(auditLog);
            }
        }
    }
}
