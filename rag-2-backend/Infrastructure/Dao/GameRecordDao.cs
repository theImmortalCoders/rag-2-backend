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
    public List<GameRecordResponse> GetRecordsByGameAndUser(int gameId, string email)
    {
        return dbContext.RecordedGames
            .Include(r => r.Game)
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId && r.User.Email == email)
            .ToList()
            .Select(GameRecordMapper.Map)
            .ToList();
    }

    public List<GameRecord> GetGameRecordsByUserWithGame(int userId)
    {
        return dbContext.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.User.Id == userId)
            .Include(recordedGame => recordedGame.Game)
            .ToList();
    }

    public List<GameRecord> GetGameRecordsByGameWithUser(int gameId)
    {
        return dbContext.RecordedGames
            .OrderBy(r => r.Started)
            .Where(r => r.Game.Id == gameId)
            .Include(recordedGame => recordedGame.User)
            .ToList();
    }

    public int CountGameRecordsSizeByUser(int userId)
    {
        return dbContext.RecordedGames
            .Where(e => e.User.Id == userId)
            .Select(e => e.Values.Count)
            .ToList()
            .Sum();
    }

    public int CountGameRecordsSizeByGame(int gameId)
    {
        return dbContext.RecordedGames
            .Where(e => e.Game.Id == gameId)
            .Select(e => e.Values.Count)
            .ToList()
            .Sum();
    }

    public GameRecord GetRecordedGameById(int recordedGameId)
    {
        return dbContext.RecordedGames.Include(recordedGame => recordedGame.User)
                   .Include(r => r.Game)
                   .SingleOrDefault(g => g.Id == recordedGameId)
               ?? throw new NotFoundException("Game record not found");
    }

    public void PerformGameRecordTransaction(Game game, GameRecord gameRecord,
        User user)
    {
        using var transaction = dbContext.Database.BeginTransaction();
        try
        {
            dbContext.Database.ExecuteSqlRaw(
                "SELECT InsertRecordedGame(@GameId, @Values, @UserId, @Players, @OutputSpec, @EndState, @Started, @Ended)",
                new NpgsqlParameter("@GameId", game.Id),
                new NpgsqlParameter("@Values", JsonSerializer.Serialize(gameRecord.Values)),
                new NpgsqlParameter("@UserId", user.Id),
                new NpgsqlParameter("@Players", JsonSerializer.Serialize(gameRecord.Players)),
                new NpgsqlParameter("@OutputSpec", gameRecord.OutputSpec),
                new NpgsqlParameter("@EndState", gameRecord.EndState),
                new NpgsqlParameter("@Started", gameRecord.Started),
                new NpgsqlParameter("@Ended", gameRecord.Ended)
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