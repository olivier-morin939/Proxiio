using Entities;
using Entities.Enums;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Signals;
using Services;

namespace ProxiioTest
{
    /// <summary>
    ///  All the unit tests for our SignalsService
    /// </summary>
    public class SignalsServiceTest
    {
        private readonly Mock<ISignalsRepository> _repository = new();
        private readonly ISignalsService _service;

        public SignalsServiceTest() => _service = new SignaslService(_repository.Object);

        private static Signal MakeSignal(Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(), ProblemName = "Connexion", ProblemDescription = "Échec de connexion",
            Level = BugSignalLevel.high, Status = BugSignalStatus.Received, IsConfirmed = true
        };

        #region AddSignal

        [Fact]
        public async Task AddSignal_ValidRequest_AddsAndMapsSignal()
        {
            var request = new AddSignalRequest { ProblemName = "Connexion", ProblemDescription = "Échec", Level = BugSignalLevel.critical, Status = BugSignalStatus.Correcting, IsConfirmed = true };
            _repository.Setup(r => r.AddSignal(It.IsAny<Signal>())).ReturnsAsync((Signal s) => s);
            var response = await _service.AddSignal(request);
            response.ProblemName.Should().Be(request.ProblemName);
            response.Level.Should().Be(request.Level);
            response.Status.Should().Be(request.Status);
            response.IsConfirmed.Should().BeTrue();
            _repository.Verify(r => r.AddSignal(It.Is<Signal>(s => s.ProblemName == request.ProblemName && s.Level == request.Level)), Times.Once);
        }

        [Fact]
        public async Task AddSignal_NullOrInvalidRequest_Throws()
        {
            await FluentActions.Invoking(() => _service.AddSignal(null!)).Should().ThrowAsync<ArgumentNullException>();
            await FluentActions.Invoking(() => _service.AddSignal(new AddSignalRequest { ProblemName = "" })).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddSignal_RepositoryReturnsNull_Throws()
        {
            _repository.Setup(r => r.AddSignal(It.IsAny<Signal>())).ReturnsAsync((Signal?)null);
            await FluentActions.Invoking(() => _service.AddSignal(new AddSignalRequest { ProblemName = "Incident" })).Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion

        #region GetAllSignals
        [Fact]
        public async Task GetAllSignals_MapsRepositoryResults()
        {
            var signals = new List<Signal> { MakeSignal(), MakeSignal() };
            _repository.Setup(r => r.GetAllSignals()).ReturnsAsync(signals);
            var result = await _service.GetAllSignals();
            result.Select(s => s.Id).Should().Equal(signals.Select(s => s.Id));
            result[0].ProblemDescription.Should().Be(signals[0].ProblemDescription);
            _repository.Verify(r => r.GetAllSignals(), Times.Once);
        }

        #endregion

        #region GetSignalById
        [Fact]
        public async Task GetSignalById_Found_ReturnsResponse()
        {
            var signal = MakeSignal();
            _repository.Setup(r => r.GetSignalById(signal.Id)).ReturnsAsync(signal);
            var result = await _service.GetSignalById(signal.Id);
            result.Id.Should().Be(signal.Id);
            result.ProblemName.Should().Be(signal.ProblemName);
        }

        [Fact]
        public async Task GetSignalById_Missing_Throws()
        {
            _repository.Setup(r => r.GetSignalById(It.IsAny<Guid>())).ReturnsAsync((Signal?)null);
            await FluentActions.Invoking(() => _service.GetSignalById(Guid.NewGuid())).Should().ThrowAsync<KeyNotFoundException>();
        }
        #endregion

        #region UpdateSignal
        [Fact]
        public async Task UpdateSignal_ValidRequest_UpdatesAndReturnsResponse()
        {
            var signal = MakeSignal();
            var request = new UpdateSignalRequest { Id = signal.Id, ProblemName = "Nouveau problème", ProblemDescription = "Description mise à jour", Level = BugSignalLevel.critical, Status = BugSignalStatus.Patched, IsConfirmed = false };
            _repository.Setup(r => r.GetSignalById(signal.Id)).ReturnsAsync(signal);
            _repository.Setup(r => r.UpdateSignal(It.IsAny<Signal>())).ReturnsAsync((Signal s) => s);
            var result = await _service.UpdateSignal(request);
            result.ProblemName.Should().Be(request.ProblemName);
            result.Status.Should().Be(request.Status);
            result.IsConfirmed.Should().BeFalse();
            _repository.Verify(r => r.UpdateSignal(It.Is<Signal>(s => s.Id == signal.Id && s.ProblemName == request.ProblemName)), Times.Once);
        }

        [Fact]
        public async Task UpdateSignal_MissingOrInvalidOrUnsuccessfulUpdate_Throws()
        {
            var request = new UpdateSignalRequest { Id = Guid.NewGuid(), ProblemName = "Incident" };
            _repository.Setup(r => r.GetSignalById(request.Id)).ReturnsAsync((Signal?)null);
            await FluentActions.Invoking(() => _service.UpdateSignal(request)).Should().ThrowAsync<KeyNotFoundException>();
            request.ProblemName = "";
            await FluentActions.Invoking(() => _service.UpdateSignal(request)).Should().ThrowAsync<ArgumentException>();
            request.ProblemName = "Incident";
            _repository.Setup(r => r.GetSignalById(request.Id)).ReturnsAsync(MakeSignal(request.Id));
            _repository.Setup(r => r.UpdateSignal(It.IsAny<Signal>())).ReturnsAsync((Signal?)null);
            await FluentActions.Invoking(() => _service.UpdateSignal(request)).Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region DeleteSignal
        [Fact]
        public async Task DeleteSignal_MissingSignal_ReturnsFalseWithoutDeleting()
        {
            _repository.Setup(r => r.GetSignalById(It.IsAny<Guid>())).ReturnsAsync((Signal?)null);
            (await _service.DeleteSignal(Guid.NewGuid())).Should().BeFalse();
            _repository.Verify(r => r.DeleteSignal(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteSignal_FoundSignal_DeletesIt()
        {
            var signal = MakeSignal();
            _repository.Setup(r => r.GetSignalById(signal.Id)).ReturnsAsync(signal);
            _repository.Setup(r => r.DeleteSignal(signal.Id)).ReturnsAsync(true);
            (await _service.DeleteSignal(signal.Id)).Should().BeTrue();
            _repository.Verify(r => r.DeleteSignal(signal.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSignal_RepositoryFailure_Throws()
        {
            var signal = MakeSignal();
            _repository.Setup(r => r.GetSignalById(signal.Id)).ReturnsAsync(signal);
            _repository.Setup(r => r.DeleteSignal(signal.Id)).ReturnsAsync(false);
            await FluentActions.Invoking(() => _service.DeleteSignal(signal.Id)).Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region ExportsData
        [Fact]
        public async Task GetSignalsCSV_ReturnsCsvContent()
        {
            _repository.Setup(r => r.GetAllSignals()).ReturnsAsync(new List<Signal> { MakeSignal() });
            using var stream = await _service.GetSignalsCSV();
            stream.Position.Should().Be(0);
            using var reader = new StreamReader(stream, leaveOpen: true);
            (await reader.ReadToEndAsync()).Should().Contain("Connexion");
        }

        [Fact]
        public async Task GetSignalsEXCEL_ReturnsNonEmptyWorkbook()
        {
            _repository.Setup(r => r.GetAllSignals()).ReturnsAsync(new List<Signal> { MakeSignal() });
            using var stream = await _service.GetSignalsEXCEL();
            stream.Position.Should().Be(0);
            stream.Length.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
