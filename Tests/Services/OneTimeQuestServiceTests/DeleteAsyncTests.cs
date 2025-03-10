using Application.Interfaces.Quests;
using Application.MappingProfiles;
using Application.Services.Quests;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Services.OneTimeQuestServiceTests
{
    public class DeleteAsyncTests
    {
        private readonly Mock<IOneTimeQuestRepository> _repositoryMock;
        private readonly IMapper _mapper;
        private readonly IOneTimeQuestService _service;

        public DeleteAsyncTests()
        {
            _repositoryMock = new Mock<IOneTimeQuestRepository>();

            var configuraion = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<OneTimeQuestProfile>();
            });
            _mapper = configuraion.CreateMapper();

            var logger = new Mock<ILogger<OneTimeQuestService>>();

            _service = new OneTimeQuestService(_repositoryMock.Object, _mapper, logger.Object);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRepository_WhenQuestExists()
        {
            // Arrange
            int questId = 1;
            int accountId = 2;
            var quest = new OneTimeQuest { Id = questId, Title = "Test Quest" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(quest);

            _repositoryMock
                .Setup(repo => repo.DeleteAsync(It.Is<OneTimeQuest>(q => q.Id == questId), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _service.DeleteUserQuestAsync(questId, accountId, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.Is<OneTimeQuest>(q => q.Id == questId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenQuestDoesNotExist()
        {
            // Arrange
            int questId = 2;
            int accountId = 1;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OneTimeQuest?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteUserQuestAsync(questId, accountId, CancellationToken.None));

            // Ensure DeleteAsync is never called
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<OneTimeQuest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
