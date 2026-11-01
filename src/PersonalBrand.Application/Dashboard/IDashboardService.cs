using PersonalBrand.Application.Dashboard.Contracts;

namespace PersonalBrand.Application.Dashboard;

/// <summary>
/// Owner dashboard use cases.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
