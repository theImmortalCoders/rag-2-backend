#region

using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using rag_2_backend.Infrastructure.Common.Mapper;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Dao;

public class GameRecordDao(DatabaseContext dbContext)
{
    public virtual List<GameRecordResponse> GetRecordsByGameAndUser(int gameId, int userId)
    {
        return dbContext.GameRecords
            .Include(r => r.Game)
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId && r.User.Id == userId)
            .ToList()
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
                "SELECT InsertRecordedGame(@GameId, @Values, @UserId, @Players, @OutputSpec, @EndState, @Started, @Ended, @SizeMb)",
                new NpgsqlParameter("@GameId", game.Id),
                new NpgsqlParameter("@Values", JsonSerializer.Serialize(gameRecord.Values)),
                new NpgsqlParameter("@UserId", user.Id),
                new NpgsqlParameter("@Players", JsonSerializer.Serialize(gameRecord.Players)),
                new NpgsqlParameter("@OutputSpec", gameRecord.OutputSpec),
                new NpgsqlParameter("@EndState", gameRecord.EndState),
                new NpgsqlParameter("@Started", gameRecord.Started),
                new NpgsqlParameter("@Ended", gameRecord.Ended),
                new NpgsqlParameter("@SizeMb", gameRecord.SizeMb)
            );
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}