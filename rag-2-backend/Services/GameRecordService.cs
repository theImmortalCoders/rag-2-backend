using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using rag_2_backend.Config;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Mapper;
using rag_2_backend.Models;
using rag_2_backend.models.entity;

namespace rag_2_backend.Services;

public class GameRecordService(DatabaseContext context)
{
    public readonly double UserLimitMb = 10;

    public List<RecordedGameResponse> GetRecordsByGame(int gameId)
    {
        var records = context.RecordedGames
            .Include(r => r.Game) //include nullable reference
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId)
            .ToList();
        return records.Select(RecordedGameMapper.Map).ToList();
    }

    public byte[] DownloadRecordData(int recordedGameId, string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email)
                   ?? throw new KeyNotFoundException("User not found");

        var recordedGame = context.RecordedGames.Include(recordedGame => recordedGame.User)
                               .SingleOrDefault(g => g.Id == recordedGameId)
                           ?? throw new KeyNotFoundException("Game record not found");

        if (user.Id != recordedGame.User.Id && user.Role != Role.Admin)
            throw new BadHttpRequestException("Permission denied");

        return System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(recordedGame));
    }

    public void AddGameRecord(RecordedGameRequest request, string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email)
                   ?? throw new KeyNotFoundException("User not found");

        if (GetSizeByUser(user.Id, request.Values.Count) > UserLimitMb)
            throw new BadHttpRequestException("Space limit exceeded");

        var game = context.Games.SingleOrDefault(g => Equals(g.Name.ToLower(), request.GameName.ToLower()))
                   ?? throw new KeyNotFoundException("Game not found");

        var recordedGame = new RecordedGame
        {
            Game = game,
            Values = request.Values,
            User = user,
            Players = request.Values[0].Players,
            OutputSpec = request.Values[0].OutputSpec,
            EndState = request.Values[^1].State?.ToString()
        };

        var startTimestamp = request.Values[0].Timestamp;
        var endTimestamp = request.Values[^1].Timestamp;
        if (startTimestamp is not null)
            recordedGame.Started = DateTime.Parse(startTimestamp, null, DateTimeStyles.RoundtripKind);
        if (endTimestamp is not null)
            recordedGame.Ended = DateTime.Parse(endTimestamp, null, DateTimeStyles.RoundtripKind);

        context.RecordedGames.Add(recordedGame);
        context.SaveChanges();
    }

    public void RemoveGameRecord(int gameRecordId, string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email)
                   ?? throw new KeyNotFoundException("User not found");

        var recordedGame = context.RecordedGames.Include(recordedGame => recordedGame.User)
                               .SingleOrDefault(g => g.Id == gameRecordId)
                           ?? throw new KeyNotFoundException("Game record not found");

        if (user.Id != recordedGame.User.Id && user.Role != Role.Admin)
            throw new BadHttpRequestException("Permission denied");

        context.RecordedGames.Remove(recordedGame);
        context.SaveChanges();
    }

    //

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
}