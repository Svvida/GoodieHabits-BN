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
    public class GetByIdAsyncTests
    {
        private readonly Mock<IOneTimeQuestRepository> _repositoryMock;
        private readonly IMapper _mapper;
        private readonly IOneTimeQuestService _service;

        public GetByIdAsyncTests()
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
        public async Task GetByIdAsync_ShouldReturnQuest_WhenQuestExists()
        {
            // Arrange: Setup expected values 
            var expectedQuest = new OneTimeQuest { Id = 1, Title = "Mock Quest" };
            _repositoryMock
                .Setup(repo => repo.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedQuest);

            // Act: call the method being tested
            var result = await _service.GetByIdAsync(1, CancellationToken.None);

            // Assert: verify the output is as expected
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Mock Quest", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenQuestDoesNotExists()
        {
            // Act
            var result = await _service.GetByIdAsync(2, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
