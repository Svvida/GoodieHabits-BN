using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces.Quests;
using Application.MappingProfiles;
using Application.Services.Quests;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services.OneTimeQuestServiceTests
{
    public class UpdateAsyncTests
    {
        private readonly Mock<IOneTimeQuestRepository> _repositoryMock;
        private readonly IMapper _mapper;
        private readonly IOneTimeQuestService _service;

        public UpdateAsyncTests()
        {
            _repositoryMock = new Mock<IOneTimeQuestRepository>();

            var configuation = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OneTimeQuestProfile>();
            });
            _mapper = configuation.CreateMapper();

            var logger = new Mock<ILogger<OneTimeQuestService>>();

            _service = new OneTimeQuestService(_repositoryMock.Object, _mapper, logger.Object);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateQuest()
        {
            // Arrange
            var questId = 1;
            var questBeforeUpdate = new OneTimeQuest { Id = questId, Title = "Before update", Description = "Should be null after update" };
            var updateValues = new UpdateOneTimeQuestDto { Title = "After update" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(questBeforeUpdate);

            _repositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<OneTimeQuest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _service.UpdateAsync(questId, updateValues, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(repo => repo.UpdateAsync(It.Is<OneTimeQuest>(
                q => q.Id == questId &&
                     q.Title == "After update" &&
                     q.Description == null
                ), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
