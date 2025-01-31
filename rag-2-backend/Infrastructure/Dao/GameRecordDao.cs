#region

using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class GameRecordDao(DatabaseContext dbContext)
{
    public virtual async Task<List<GameRecordResponse>> GetRecordsByGameAndUser(
        int gameId,
        int userId,
        bool? includeEmptyRecords,
        DateTime? endDateFrom,
        DateTime? endDateTo,
        SortDirection sortDirection,
        GameRecordSortByFields sortBy
    )
    {
        var query = dbContext.GameRecords
            .Include(r => r.Game)
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId && r.User.Id == userId)
            .AsQueryable();

        query = FilterGameRecords(includeEmptyRecords, endDateFrom, endDateTo, query);
        query = SortGameRecords(sortDirection, sortBy, query);

        return await Task.Run(() => query.AsEnumerable()
            .Select(GameRecordMapper.Map)
            .ToList());
    }

    public virtual async Task<List<GameRecord>> GetGameRecordsByUserWithGame(int userId)
    {
        return await dbContext.GameRecords
            .OrderBy(r => r.Started)
            .Where(r => r.User.Id == userId)
            .Include(recordedGame => recordedGame.Game)
            .ToListAsync();
    }

    public virtual async Task<List<GameRecord>> GetGameRecordsByGameWithUser(int gameId)
    {
        return await dbContext.GameRecords
            .OrderBy(r => r.Started)
            .Where(r => r.Game.Id == gameId)
            .Include(recordedGame => recordedGame.User)
            .ToListAsync();
    }

    public virtual async Task<GameRecord> GetRecordedGameById(int recordedGameId)
    {
        return await dbContext.GameRecords.Include(recordedGame => recordedGame.User)
                   .Include(r => r.Game)
                   .SingleOrDefaultAsync(g => g.Id == recordedGameId)
               ?? throw new NotFoundException("Game record not found");
    }

    public virtual async Task<double> CountTotalStorageMb()
    {
        return (await dbContext.GameRecords
                .Select(r => r.SizeMb)
                .ToListAsync())
            .Sum();
    }

    public virtual async Task<int> CountAllGameRecords()
    {
        return await dbContext.GameRecords.CountAsync();
    }

    public virtual void PerformGameRecordTransaction(Game game, GameRecord gameRecord,
        User user)
    {
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            dbContext.Database.ExecuteSqlRaw(
                "SELECT InsertRecordedGame(@GameId, @Values, @UserId, @Players, @OutputSpec, @EndState, @Started, @Ended, @SizeMb, @IsEmptyRecord)",
                new NpgsqlParameter("@GameId", game.Id),
                new NpgsqlParameter("@Values", JsonSerializer.Serialize(gameRecord.Values)),
                new NpgsqlParameter("@UserId", user.Id),
                new NpgsqlParameter("@Players", JsonSerializer.Serialize(gameRecord.Players)),
                new NpgsqlParameter("@OutputSpec", gameRecord.OutputSpec),
                new NpgsqlParameter("@EndState", gameRecord.EndState),
                new NpgsqlParameter("@Started", gameRecord.Started),
                new NpgsqlParameter("@Ended", gameRecord.Ended),
                new NpgsqlParameter("@SizeMb", gameRecord.SizeMb),
                new NpgsqlParameter("@IsEmptyRecord", gameRecord.IsEmptyRecord)
            );
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    //

    private static IQueryable<GameRecord> FilterGameRecords(
        bool? includeEmptyRecords,
        DateTime? endDateFrom,
        DateTime? endDateTo,
        IQueryable<GameRecord> query
    )
    {
        if (includeEmptyRecords is null or false)
            query = query.Where(u => u.IsEmptyRecord == false);
        if (endDateFrom.HasValue)
            query = query.Where(u => u.Ended >= endDateFrom);
        if (endDateTo.HasValue)
            query = query.Where(u => u.Ended <= endDateTo);

        return query;
    }

    private static IQueryable<GameRecord> SortGameRecords(
        SortDirection sortDirection,
        GameRecordSortByFields sortBy,
        IQueryable<GameRecord> query
    )
    {
        return sortBy switch
        {
            GameRecordSortByFields.Id => DataSortingUtil.ApplySorting(query, x => x.Id, sortDirection),
            GameRecordSortByFields.Ended => DataSortingUtil.ApplySorting(query, x => x.Ended, sortDirection),
            GameRecordSortByFields.SizeMb => DataSortingUtil.ApplySorting(query, x => x.SizeMb, sortDirection),
            _ => query
        };
    }
}