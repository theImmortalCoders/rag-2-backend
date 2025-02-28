#region

using System.Globalization;
using System.Text;
using System.Text.Json;
using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Mapper;
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
    public async Task<List<GameRecordResponse>> GetRecordsByGameAndUser(
        int gameId,
        int userId,
        bool? includeEmptyRecords,
        DateTime? endDateFrom,
        DateTime? endDateTo,
        SortDirection sortDirection,
        GameRecordSortByFields sortBy,
        string email
    )
    {
        var principal = await userDao.GetUserByEmailOrThrow(email);

        if (principal.Id != userId && principal.Role.Equals(Role.Student))
            throw new BadRequestException("Permission denied");

        return await gameRecordDao.GetRecordsByGameAndUser(
            gameId,
            userId,
            includeEmptyRecords,
            endDateFrom,
            endDateTo,
            sortDirection,
            sortBy
        );
    }

    public async Task<byte[]> DownloadRecordData(int recordedGameId, string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);
        var recordedGame = await gameRecordDao.GetRecordedGameById(recordedGameId);

        if (user.Id != recordedGame.User.Id && user.Role.Equals(Role.Student))
            throw new ForbiddenException("Permission denied");
        if (recordedGame.IsEmptyRecord)
            throw new BadRequestException("Record is empty");

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(GameRecordMapper.JsonMap(recordedGame)));
    }

    public async Task AddGameRecord(GameRecordRequest recordRequest, string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);
        if (recordRequest.Values.Count > 0)
            await CheckUserDataLimit(recordRequest, user);

        var game = await gameDao.GetGameByNameOrThrow(recordRequest.GameName);

        var recordedGame = new Database.Entity.GameRecord
        {
            Game = game,
            Values = recordRequest.Values,
            User = user,
            Players = recordRequest.Players,
            OutputSpec = recordRequest.OutputSpec,
            EndState = recordRequest.Values.Count > 0 ? recordRequest.Values[^1].State?.ToString() : "{}",
            SizeMb = recordRequest.Values.Count > 0
                ? JsonSerializer.Serialize(recordRequest.Values).Length / (1024.0 * 1024.0)
                : 0,
            IsEmptyRecord = recordRequest.Values.Count == 0
        };

        UpdateTimestamps(recordRequest, recordedGame);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        executionStrategy.Execute(() => gameRecordDao.PerformGameRecordTransaction(game, recordedGame, user));
    }

    public async Task RemoveGameRecord(int gameRecordId, string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);
        var recordedGame = await gameRecordDao.GetRecordedGameById(gameRecordId);

        if (user.Id != recordedGame.User.Id)
            throw new BadRequestException("Permission denied");

        context.GameRecords.Remove(recordedGame);
        await context.SaveChangesAsync();
    }

    //

    private async Task CheckUserDataLimit(GameRecordRequest recordRequest, Database.Entity.User user)
    {
        var initialSizeMb = JsonSerializer.Serialize(recordRequest.Values).Length / (1024.0 * 1024.0);
        var totalSizeMb = await GetSizeByUser(user.Id, initialSizeMb);

        switch (user.Role)
        {
            case Role.Student when totalSizeMb >
                                   configuration.GetValue<int>("StudentDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            case Role.Teacher when totalSizeMb >
                                   configuration.GetValue<int>("TeacherDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            case Role.Admin when totalSizeMb >
                                 configuration.GetValue<int>("AdminDataLimitMb"):
                throw new BadRequestException("Space limit exceeded");
            default:
                return;
        }
    }

    private async Task<double> GetSizeByUser(int userId, double initialSizeBytes)
    {
        var results = (await gameRecordDao.GetGameRecordsByUserWithGame(userId))
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
            var startTimestamp = recordRequest.Values.Count > 0
                ? recordRequest.Values[0].Timestamp
                : TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss");
            var endTimestamp = recordRequest.Values.Count > 0
                ? recordRequest.Values[^1].Timestamp
                : TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm:ss");
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