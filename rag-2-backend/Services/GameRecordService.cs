using System.Globalization;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Config;
using rag_2_backend.DTO.RecordedGame;
using rag_2_backend.Mapper;
using rag_2_backend.models.entity;

namespace rag_2_backend.Services;

public class GameRecordService(DatabaseContext context)
{
    public List<RecordedGameResponse> GetRecordsByGame(int gameId)
    {
        var records = context.RecordedGames
            .Include(r => r.Game) //include nullable reference
            .Include(r => r.User)
            .Where(r => r.Game.Id == gameId)
            .ToList();
        return records.Select(RecordedGameMapper.Map).ToList();
    }

    public void AddGameRecord(RecordedGameRequest request, string email)
    {
        var user = context.Users.SingleOrDefault(u => u.Email == email)
                   ?? throw new KeyNotFoundException("User not found");
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
        if(startTimestamp is not null)
            recordedGame.Started = DateTime.Parse(startTimestamp, null, DateTimeStyles.RoundtripKind);
        if(endTimestamp is not null)
            recordedGame.Ended = DateTime.Parse(endTimestamp, null, DateTimeStyles.RoundtripKind);

        context.RecordedGames.Add(recordedGame);
        context.SaveChanges();
    }
}