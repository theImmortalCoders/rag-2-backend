#region

using System.Globalization;
using System.Text;
using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Module.GameRecord.Dto;

#endregion

namespace rag_2_backend.Infrastructure.Module.GameRecord;

public class GameRecordService(
    DatabaseContext context,
    IConfiguration configuration,
    UserDao userDao,
    GameRecordDao gameRecordDao,
    GameDao gameDao
)
{
    public List<GameRecordResponse> GetRecordsByGameAndUser(int gameId, string email)
    {
        return gameRecordDao.GetRecordsByGameAndUser(gameId, email);
    }

    public byte[] DownloadRecordData(int recordedGameId, string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);
        var recordedGame = gameRecordDao.GetRecordedGameById(recordedGameId);

        if (user.Id != recordedGame.User.Id && user.Role.Equals(Role.Student))
            throw new BadRequestException("Permission denied");

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(recordedGame));
    }

    public void AddGameRecord(GameRecordRequest recordRequest, string email)
    {
        if (recordRequest.Values.Count == 0 || recordRequest.Values[^1].State == null)
            throw new BadRequestException("Value state cannot be empty");

        var user = userDao.GetUserByEmailOrThrow(email);
        CheckUserDataLimit(recordRequest, user);

        var game = gameDao.GetGameByNameOrThrow(recordRequest);

        var recordedGame = new Database.Entity.GameRecord
        {
            Game = game,
            Values = recordRequest.Values,
            User = user,
            Players = recordRequest.Values[0].Players,
            OutputSpec = recordRequest.Values[0].OutputSpec ?? "",
            EndState = recordRequest.Values[^1].State?.ToString(),
            SizeMb = JsonSerializer.Serialize(recordRequest.Values).Length / (1024.0 * 1024.0)
        };

        UpdateTimestamps(recordRequest, recordedGame);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        executionStrategy.Execute(() => gameRecordDao.PerformGameRecordTransaction(game, recordedGame, user));
    }

    public void RemoveGameRecord(int gameRecordId, string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);
        var recordedGame = gameRecordDao.GetRecordedGameById(gameRecordId);

        if (user.Id != recordedGame.User.Id)
            throw new BadRequestException("Permission denied");

        context.GameRecords.Remove(recordedGame);
        context.SaveChanges();
    }

    //

    private void CheckUserDataLimit(GameRecordRequest recordRequest, Database.Entity.User user)
    {
        switch (user.Role)
        {
            case Role.Student when GetSizeByUser(user.Id, recordRequest.Values.Count) >
                                   configuration.GetValue<int>("StudentDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            case Role.Teacher when GetSizeByUser(user.Id, recordRequest.Values.Count) >
                                   configuration.GetValue<int>("TeacherDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            case Role.Admin when GetSizeByUser(user.Id, recordRequest.Values.Count) >
                                 configuration.GetValue<int>("AdminDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            default:
                return;
        }
    }

    private double GetSizeByUser(int userId, double initialSizeBytes)
    {
        var results = gameRecordDao.GetGameRecordsByUserWithGame(userId)
            .Select(r => r.SizeMb)
            .ToList()
            .Sum();
        var totalBytes = results + initialSizeBytes;
        return totalBytes;
    }

    private static void UpdateTimestamps(GameRecordRequest recordRequest, Database.Entity.GameRecord gameRecord)
    {
        try
        {
            var startTimestamp = recordRequest.Values[0].Timestamp;
            var endTimestamp = recordRequest.Values[^1].Timestamp;
            if (startTimestamp is not null)
                gameRecord.Started = DateTime.Parse(startTimestamp, null, DateTimeStyles.RoundtripKind);
            if (endTimestamp is not null)
                gameRecord.Ended = DateTime.Parse(endTimestamp, null, DateTimeStyles.RoundtripKind);
        }
        catch (Exception e)
        {
            throw new BadRequestException("Failed to update timestamps", e);
        }
    }
}