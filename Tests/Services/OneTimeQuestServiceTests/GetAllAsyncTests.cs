using Application.Interfaces.Quests;
using Application.MappingProfiles;
using Application.Services.Quests;
using AutoMapper;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services.OneTimeQuestServiceTests
{
    public class GetAllAsyncTests
    {
        private readonly Mock<IOneTimeQuestRepository> _repositoryMock;
        private readonly IMapper _mapper;
        private readonly IOneTimeQuestService _service;

        public GetAllAsyncTests()
        {
            _repositoryMock = new Mock<IOneTimeQuestRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OneTimeQuestProfile>();
            });
            _mapper = configuration.CreateMapper();

            var loggerMock = new Mock<ILogger<OneTimeQuestService>>();

            _service = new OneTimeQuestService(_repositoryMock.Object, _mapper, loggerMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnListOfQuests_WhenQuestsExist()
        {
            // Arrange
            var quests = new List<OneTimeQuest>
            {
                new() { Id = 1, Title = "Quest 1" },
                new() { Id = 2, Title = "Quest 2" }
            };

            _repositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(quests);

            // Act
            var result = await _service.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, q => q.Id == 1 && q.Title == "Quest 1");
            Assert.Contains(result, q => q.Id == 2 && q.Title == "Quest 2");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoQuestsExist()
        {
            // Arrange
            _repositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<OneTimeQuest>());

            // Act
            var result = await _service.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
