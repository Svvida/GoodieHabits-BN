using Application.Interfaces;
using Application.MappingProfiles;
using Application.Services;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
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
            _repositoryMock
                .Setup(repo => repo.DeleteAsync(questId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _service.DeleteAsync(questId, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(repo => repo.DeleteAsync(questId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenQuestDoesNotExist()
        {
            // Arrange
            int questId = 2;
            _repositoryMock
                .Setup(repo => repo.DeleteAsync(questId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException($"Quest with ID: {questId} not found"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(questId, CancellationToken.None));
        }
    }
}
