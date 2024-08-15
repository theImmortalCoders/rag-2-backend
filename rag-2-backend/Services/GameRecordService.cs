using Microsoft.EntityFrameworkCore;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.DTO.Mapper;
using rag_2_backend.models.entity;

namespace rag_2_backend.Services;

public class GameRecordService(DatabaseContext context)
{
    public async Task<List<RecordedGameResponse>> GetRecordsByGame(int gameId)
    {
        var games = await context.RecordedGames.ToArrayAsync();
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
        var game = context.Games.SingleOrDefault(g => g.Id == request.GameId)
                   ?? throw new KeyNotFoundException("Game not found");

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