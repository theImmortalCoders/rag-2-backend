using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.models.entity;
using rag_2_backend.Models.Entity;

namespace rag_2_backend.Services;

public class GameRecordService(DatabaseContext context)
{
    public IEnumerable<RecordedGameResponse> GetRecordsByGame(int gameId)
    {
        var games = context.RecordedGames.ToArray();
        var records = context.RecordedGames
            .Include(r => r.Game) //include nullable reference
            .Include(r => r.User)
            .Where(r => r.Game != null && r.Game.Id == gameId)
            .ToList();
        return records.Select(RecordedGameMapper.Map).ToList();
    }

    public void AddGameRecord([FromBody] RecordedGameRequest request, User user)
    {
        var game = context.Games.Find(request.GameId) ?? throw new KeyNotFoundException("Game not found");
        var recordedGame = new RecordedGame
        {
            Game = game,
            Value = request.Value,
            User = user
        };
        context.RecordedGames.Add(recordedGame);
        context.SaveChanges();
    }
}