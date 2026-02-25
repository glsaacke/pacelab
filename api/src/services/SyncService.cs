using Microsoft.EntityFrameworkCore;
using api.src.data;
using api.src.models.responses;

namespace api.src.services;

public class SyncService : ISyncService
{
    private readonly ApplicationDbContext _context;
    private readonly IStravaService _stravaService;

    public SyncService(ApplicationDbContext context, IStravaService stravaService)
    {
        _context = context;
        _stravaService = stravaService;
    }

    /// <inheritdoc />
    public async Task<SyncStatusResponse> GetSyncStatusAsync(int userId)
    {
        var connection = await _context.UserStravaConnections
            .FirstOrDefaultAsync(c => c.UserId == userId);

        var totalActivities = await _context.Activities
            .CountAsync(a => a.UserId == userId);

        var latestLog = await _context.SyncLogs
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        return new SyncStatusResponse
        {
            StravaConnected = connection != null,
            LastSyncAt = connection?.LastSync,
            LastSyncStatus = latestLog?.Status,
            LastSyncActivitiesSynced = latestLog?.ActivitiesSynced,
            LastSyncError = latestLog?.ErrorMessage,
            TotalActivities = totalActivities
        };
    }

    /// <inheritdoc />
    public async Task<PagedResponse<SyncHistoryResponse>> GetSyncHistoryAsync(int userId, int page, int limit)
    {
        var query = _context.SyncLogs
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt);

        var total = await query.CountAsync();

        var logs = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        var data = logs.Select(s => new SyncHistoryResponse
        {
            SyncLogId = s.SyncLogId,
            SyncType = s.SyncType,
            Status = s.Status,
            ActivitiesSynced = s.ActivitiesSynced,
            ErrorMessage = s.ErrorMessage,
            StartedAt = s.StartedAt,
            CompletedAt = s.CompletedAt,
            DurationSeconds = s.StartedAt.HasValue && s.CompletedAt.HasValue
                ? (s.CompletedAt.Value - s.StartedAt.Value).TotalSeconds
                : null
        }).ToList();

        return new PagedResponse<SyncHistoryResponse>
        {
            Data = data,
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };
    }

    /// <inheritdoc />
    public async Task<SyncTestResult> TestSyncConnectionAsync(int userId)
    {
        return await _stravaService.TestConnectionAsync(userId);
    }
}
