using Moq;
using MockQueryable.Moq;
using rag_2_backend.data;
using rag_2_backend.DTO;
using rag_2_backend.models.entity;
using rag_2_backend.Models;
using rag_2_backend.Models.Entity;
using rag_2_backend.Services;
using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace rag_2_backend.Test;

public class GameRecordServiceTest
{
    [Fact]
    public void GetRecordsByGameTest()
    {
        var expectedRecords = new List<RecordedGameResponse> {
            new() {
                Id = 1,
                Value = "10",
                GameResponse = new GameResponse { Id = 1 , Name = "Game1", GameType = GameType.EventGame },
                UserResponse = new UserResponse { Id = 1,  Email = "email", Role = Role.Student },
            },
        };

        var contextMock = new Mock<DatabaseContext>(
            new DbContextOptionsBuilder<DatabaseContext>().Options
        );
        contextMock.Setup(c => c.RecordedGames).Returns(
            GetMockRecordedGames().AsQueryable().BuildMockDbSet().Object
        );

        var gameRecordService = new GameRecordService(contextMock.Object);

        // Act
        var actualRecords = gameRecordService.GetRecordsByGame(1);

        // Assert
        Assert.Equal(expectedRecords.Count, actualRecords.Count());
        Assert.Equal(JsonSerializer.Serialize(expectedRecords), JsonSerializer.Serialize(actualRecords));
        // Assert.Equal(expectedRecords, actualRecords);
    }

    private static List<RecordedGame> GetMockRecordedGames()
    {
        return [
            new RecordedGame {
                Id = 1,
                Game = new Game { Id = 1, Name = "Game1", GameType = GameType.EventGame },
                Value = "10",
                User = new User {
                    Id = 1,
                    Email = "email",
                    Role = Role.Student,
                    Password = "password",
                }
            },
        ];
    }
}