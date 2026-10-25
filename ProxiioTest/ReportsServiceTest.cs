using Entities;
using Entities.Enums;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Reports;
using Services;

namespace ProxiioTest
{
    /// <summary>
    ///  All the unit tests for our ReportsService
    /// </summary>
    public class ReportsServiceTest
    {
        private readonly Mock<IReportsRepository> _repository = new();
        private readonly IReportsService _service;

        public ReportsServiceTest() => _service = new ReportsService(_repository.Object);

        private static Report MakeReport(Guid? id = null, Guid? postId = null) => new()
        {
            Id = id ?? Guid.NewGuid(), PostId = postId ?? Guid.NewGuid(), TypeOfReport = ReportType.SPAM,
            MessageOfReport = "Contenu indésirable", StatusOfReport = ReportStatus.Received
        };

        #region AddReport
        [Fact]
        public async Task AddReport_ValidRequest_AddsAndMapsReport()
        {
            var postId = Guid.NewGuid();
            var request = new AddReportRequest { PostId = postId, TypeOfReport = ReportType.HARASSMENT, MessageOfReport = "Propos signalés", StatusOfReport = ReportStatus.Received };
            _repository.Setup(r => r.GetReportByReportId(postId)).ReturnsAsync(MakeReport(postId));
            _repository.Setup(r => r.AddReport(It.IsAny<Report>())).ReturnsAsync((Report report) => report);
            var result = await _service.AddReport(request);
            result.PostId.Should().Be(postId);
            result.TypeOfReport.Should().Be(request.TypeOfReport);
            result.MessageOfReport.Should().Be(request.MessageOfReport);
            _repository.Verify(r => r.AddReport(It.Is<Report>(report => report.PostId == postId && report.MessageOfReport == request.MessageOfReport)), Times.Once);
        }

        [Fact]
        public async Task AddReport_NullOrInvalidOrUnknownPost_Throws()
        {
            await FluentActions.Invoking(() => _service.AddReport(null)).Should().ThrowAsync<ArgumentNullException>();
            await FluentActions.Invoking(() => _service.AddReport(new AddReportRequest { PostId = Guid.NewGuid(), MessageOfReport = "" })).Should().ThrowAsync<ArgumentException>();
            var request = new AddReportRequest { PostId = Guid.NewGuid(), MessageOfReport = "Signalement" };
            _repository.Setup(r => r.GetReportByReportId(request.PostId)).ReturnsAsync((Report?)null);
            await FluentActions.Invoking(() => _service.AddReport(request)).Should().ThrowAsync<ArgumentException>();
            _repository.Verify(r => r.AddReport(It.IsAny<Report>()), Times.Never);
        }

        [Fact]
        public async Task AddReport_RepositoryReturnsNull_Throws()
        {
            var request = new AddReportRequest { PostId = Guid.NewGuid(), MessageOfReport = "Signalement" };
            _repository.Setup(r => r.GetReportByReportId(request.PostId)).ReturnsAsync(MakeReport(request.PostId));
            _repository.Setup(r => r.AddReport(It.IsAny<Report>())).ReturnsAsync((Report?)null);
            await FluentActions.Invoking(() => _service.AddReport(request)).Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion

        #region GetAllReports
        [Fact]
        public async Task GetAllReports_MapsRepositoryResults()
        {
            var reports = new List<Report> { MakeReport(), MakeReport() };
            _repository.Setup(r => r.GetAllReports()).ReturnsAsync(reports);
            var result = await _service.GetAllReports();
            result.Select(r => r.Id).Should().Equal(reports.Select(r => r.Id));
            result[0].MessageOfReport.Should().Be(reports[0].MessageOfReport);
            _repository.Verify(r => r.GetAllReports(), Times.Once);
        }

        #endregion

        #region GetReportById
        [Fact]
        public async Task GetReportById_Found_ReturnsResponse()
        {
            var report = MakeReport();
            _repository.Setup(r => r.GetReportByReportId(report.Id)).ReturnsAsync(report);
            var result = await _service.GetReportByReportId(report.Id);
            result.Id.Should().Be(report.Id);
            result.PostId.Should().Be(report.PostId);
        }

        [Fact]
        public async Task GetReportById_Missing_Throws()
        {
            _repository.Setup(r => r.GetReportByReportId(It.IsAny<Guid>())).ReturnsAsync((Report?)null);
            await FluentActions.Invoking(() => _service.GetReportByReportId(Guid.NewGuid())).Should().ThrowAsync<KeyNotFoundException>();
        }

        #endregion

        #region UpdateReport
        [Fact]
        public async Task UpdateReport_ValidRequest_UpdatesAndReturnsResponse()
        {
            var report = MakeReport();
            var request = new UpdateReportRequest { Id = report.Id, PostId = report.PostId, TypeOfReport = ReportType.HATE_SPEECH, MessageOfReport = "Mis à jour", StatusOfReport = ReportStatus.Terminated };
            _repository.Setup(r => r.GetReportByReportId(report.Id)).ReturnsAsync(report);
            _repository.Setup(r => r.UpdateReport(It.IsAny<Report>())).ReturnsAsync((Report updated) => updated);
            var result = await _service.UpdateReport(request);
            result.Id.Should().Be(report.Id);
            result.TypeOfReport.Should().Be(request.TypeOfReport);
            result.MessageOfReport.Should().Be(request.MessageOfReport);
            result.StatusOfReport.Should().Be(request.StatusOfReport);
            _repository.Verify(r => r.UpdateReport(It.Is<Report>(r => r.Id == request.Id && r.MessageOfReport == request.MessageOfReport && r.StatusOfReport == request.StatusOfReport)), Times.Once);
        }

        [Fact]
        public async Task UpdateReport_NullOrInvalidOrMissingOrFailed_Throws()
        {
            await FluentActions.Invoking(() => _service.UpdateReport(null)).Should().ThrowAsync<ArgumentNullException>();
            var request = new UpdateReportRequest { Id = Guid.NewGuid(), PostId = Guid.NewGuid(), MessageOfReport = "Valid" };
            _repository.Setup(r => r.GetReportByReportId(request.Id)).ReturnsAsync((Report?)null);
            await FluentActions.Invoking(() => _service.UpdateReport(request)).Should().ThrowAsync<KeyNotFoundException>();
            request.MessageOfReport = "";
            await FluentActions.Invoking(() => _service.UpdateReport(request)).Should().ThrowAsync<ArgumentException>();
            request.MessageOfReport = "Valid";
            _repository.Setup(r => r.GetReportByReportId(request.Id)).ReturnsAsync(MakeReport(request.Id));
            _repository.Setup(r => r.UpdateReport(It.IsAny<Report>())).ReturnsAsync((Report?)null);
            await FluentActions.Invoking(() => _service.UpdateReport(request)).Should().ThrowAsync<InvalidOperationException>();
        }

        #endregion

        #region DeleteReport
        [Fact]
        public async Task DeleteReport_Missing_ReturnsFalseWithoutDeleting()
        {
            _repository.Setup(r => r.GetReportByReportId(It.IsAny<Guid>())).ReturnsAsync((Report?)null);
            (await _service.DeleteReportByReportId(Guid.NewGuid())).Should().BeFalse();
            _repository.Verify(r => r.DeleteReportByReportId(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteReport_Found_DeletesReport()
        {
            var report = MakeReport();
            _repository.Setup(r => r.GetReportByReportId(report.Id)).ReturnsAsync(report);
            _repository.Setup(r => r.DeleteReportByReportId(report.Id)).ReturnsAsync(true);
            (await _service.DeleteReportByReportId(report.Id)).Should().BeTrue();
            _repository.Verify(r => r.DeleteReportByReportId(report.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteReport_RepositoryFailure_Throws()
        {
            var report = MakeReport();
            _repository.Setup(r => r.GetReportByReportId(report.Id)).ReturnsAsync(report);
            _repository.Setup(r => r.DeleteReportByReportId(report.Id)).ReturnsAsync(false);
            await FluentActions.Invoking(() => _service.DeleteReportByReportId(report.Id)).Should().ThrowAsync<InvalidOperationException>();
        }
        #endregion

        #region ExportsData
        [Fact]
        public async Task GetReportsCSV_ReturnsCsvContent()
        {
            _repository.Setup(r => r.GetAllReports()).ReturnsAsync(new List<Report> { MakeReport() });
            using var stream = await _service.GetReportsCSV();
            stream.Position.Should().Be(0);
            using var reader = new StreamReader(stream, leaveOpen: true);
            (await reader.ReadToEndAsync()).Should().Contain("Contenu indésirable");
        }

        [Fact]
        public async Task GetReportsEXCEL_ReturnsNonEmptyWorkbook()
        {
            _repository.Setup(r => r.GetAllReports()).ReturnsAsync(new List<Report> { MakeReport() });
            using var stream = await _service.GetReportsEXCEL();
            stream.Position.Should().Be(0);
            stream.Length.Should().BeGreaterThan(0);
        }

        #endregion
    }
}
