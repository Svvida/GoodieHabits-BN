﻿using Application.Interfaces;
using Application.MappingProfiles;
using Application.Services;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
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
            var quest = new OneTimeQuest { Id = questId, Title = "Test Quest" };

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(quest);

            _repositoryMock
                .Setup(repo => repo.DeleteAsync(It.Is<OneTimeQuest>(q => q.Id == questId), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            // Act
            await _service.DeleteAsync(questId, CancellationToken.None);

            // Assert
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.Is<OneTimeQuest>(q => q.Id == questId), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenQuestDoesNotExist()
        {
            // Arrange
            int questId = 2;

            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(questId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OneTimeQuest?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(questId, CancellationToken.None));

            // Ensure DeleteAsync is never called
            _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<OneTimeQuest>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
