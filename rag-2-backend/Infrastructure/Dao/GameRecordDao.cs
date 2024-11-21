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
    public virtual List<GameRecordResponse> GetRecordsByGameAndUser(
        int gameId,
        int userId,
        bool? isEmptyRecord,
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

        query = FilterGameRecords(isEmptyRecord, endDateFrom, endDateTo, query);
        query = SortGameRecords(sortDirection, sortBy, query);

        return query.AsEnumerable()
            .Select(GameRecordMapper.Map)
            .ToList();
    }

    public virtual List<GameRecord> GetGameRecordsByUserWithGame(int userId)
    {
        return dbContext.GameRecords
            .OrderBy(r => r.Started)
            .Where(r => r.User.Id == userId)
            .Include(recordedGame => recordedGame.Game)
            .ToList();
    }

    public virtual List<GameRecord> GetGameRecordsByGameWithUser(int gameId)
    {
        return dbContext.GameRecords
            .OrderBy(r => r.Started)
            .Where(r => r.Game.Id == gameId)
            .Include(recordedGame => recordedGame.User)
            .ToList();
    }

    public virtual GameRecord GetRecordedGameById(int recordedGameId)
    {
        return dbContext.GameRecords.Include(recordedGame => recordedGame.User)
                   .Include(r => r.Game)
                   .SingleOrDefault(g => g.Id == recordedGameId)
               ?? throw new NotFoundException("Game record not found");
    }

    public virtual double CountTotalStorageMb()
    {
        return dbContext.GameRecords
            .Select(r => r.SizeMb)
            .ToList()
            .Sum();
    }

    public virtual int CountAllGameRecords()
    {
        return dbContext.GameRecords.Count();
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
        bool? isEmptyRecord,
        DateTime? endDateFrom,
        DateTime? endDateTo,
        IQueryable<GameRecord> query
    )
    {
        if (isEmptyRecord.HasValue)
            query = query.Where(u => u.IsEmptyRecord == isEmptyRecord);
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