#region

using System.Globalization;
using System.Text;
using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using rag_2_backend.Config;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Mapper;
using rag_2_backend.Models;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;
using rag_2_backend.Utils;

#endregion

namespace rag_2_backend.Services;

public class GameRecordService(DatabaseContext context, IConfiguration configuration, UserUtil userUtil)
{
    public List<RecordedGameResponse> GetRecordsByGameAndUser(int gameId, string email)
    {
        return context.RecordedGames
            .Include(r => r.Game)
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId && r.User.Email == email)
            .ToList()
            .Select(RecordedGameMapper.Map)
            .ToList();
    }

    public byte[] DownloadRecordData(int recordedGameId, string email)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);
        var recordedGame = GetRecordedGameById(recordedGameId);

        if (user.Id != recordedGame.User.Id && user.Role != Role.Admin && user.Role != Role.Teacher)
            throw new BadRequestException("Permission denied");

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(recordedGame));
    }

    public void AddGameRecord(RecordedGameRequest request, string email)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);

        if (GetSizeByUser(user.Id, request.Values.Count) > configuration.GetValue<int>("UserDataLimitMb"))
            throw new BadRequestException("Space limit exceeded");

        var game = context.Games.SingleOrDefault(g => Equals(g.Name.ToLower(), request.GameName.ToLower()))
                   ?? throw new NotFoundException("Game not found");

        var recordedGame = new RecordedGame
        {
            Game = game,
            Values = request.Values,
            User = user,
            Players = request.Values[0].Players,
            OutputSpec = request.Values[0].OutputSpec,
            EndState = request.Values[^1].State?.ToString()
        };

        UpdateTimestamps(request, recordedGame);

        var executionStrategy = context.Database.CreateExecutionStrategy();

        executionStrategy.Execute(() => { PerformGameRecordTransaction(game, recordedGame, user); });
    }

    public void RemoveGameRecord(int gameRecordId, string email)
    {
        var user = userUtil.GetUserByEmailOrThrow(email);
        var recordedGame = GetRecordedGameById(gameRecordId);

        if (user.Id != recordedGame.User.Id)
            throw new BadRequestException("Permission denied");

        context.RecordedGames.Remove(recordedGame);
        context.SaveChanges();
    }

    //

    private RecordedGame GetRecordedGameById(int recordedGameId)
    {
        return context.RecordedGames.Include(recordedGame => recordedGame.User)
                   .SingleOrDefault(g => g.Id == recordedGameId)
               ?? throw new NotFoundException("Game record not found");
    }

    private double GetSizeByUser(int userId, double initialSizeBytes)
    {
        var results = context.RecordedGames
            .Where(e => e.User.Id == userId)
            .Select(e => new
            {
                StringFieldLength = e.Values.Count
            })
            .ToList();

        var totalBytes = results.Sum(r => r.StringFieldLength) + initialSizeBytes;
        return totalBytes / 1024.0;
    }

    private static void UpdateTimestamps(RecordedGameRequest request, RecordedGame recordedGame)
    {
        var startTimestamp = request.Values[0].Timestamp;
        var endTimestamp = request.Values[^1].Timestamp;
        if (startTimestamp is not null)
            recordedGame.Started = DateTime.Parse(startTimestamp, null, DateTimeStyles.RoundtripKind);
        if (endTimestamp is not null)
            recordedGame.Ended = DateTime.Parse(endTimestamp, null, DateTimeStyles.RoundtripKind);
    }

    private void PerformGameRecordTransaction(Game game, RecordedGame recordedGame, User user)
    {
        using var transaction = context.Database.BeginTransaction();
        try
        {
            context.Database.ExecuteSqlRaw(
                "SELECT InsertRecordedGame(@GameId, @Values, @UserId, @Players, @OutputSpec, @EndState, @Started, @Ended)",
                new NpgsqlParameter("@GameId", game.Id),
                new NpgsqlParameter("@Values", JsonSerializer.Serialize(recordedGame.Values)),
                new NpgsqlParameter("@UserId", user.Id),
                new NpgsqlParameter("@Players", JsonSerializer.Serialize(recordedGame.Players)),
                new NpgsqlParameter("@OutputSpec", recordedGame.OutputSpec),
                new NpgsqlParameter("@EndState", recordedGame.EndState),
                new NpgsqlParameter("@Started", recordedGame.Started),
                new NpgsqlParameter("@Ended", recordedGame.Ended)
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