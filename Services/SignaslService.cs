using CsvHelper;
using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO.Reports;
using ServiceContracts.DTO.Signals;
using System.Globalization;

namespace Services;

public class SignaslService : ISignalsService
{
    private readonly ISignalsRepository _repository;
    public SignaslService(ISignalsRepository repository) => _repository = repository;

    public async Task<SignalResponse> AddSignal(AddSignalRequest request)
    {

        ArgumentNullException.ThrowIfNull(request);

        Helpers.HelpersValidation.ModelValidation(request);

        Signal signalToAdd = request.ToSignal();

        Signal? addedSignal = await _repository.AddSignal(signalToAdd);

        if (addedSignal == null)
            throw new InvalidOperationException("The signal could not be added.");

        return SignalResponse.From(addedSignal);
    }

    public async Task<List<SignalResponse>> GetAllSignals() => (await _repository.GetAllSignals()).Select(s => s.ToSignalResponse()).ToList();

    public async Task<SignalResponse> GetSignalById(Guid id)
    {
        // Find the corresponding signal and convert it in the DTO response
        Signal signal = await _repository.GetSignalById(id) ?? throw new KeyNotFoundException($"Signal {id} was not found.");
        return SignalResponse.From(signal);
    }

    public async Task<SignalResponse> UpdateSignal(UpdateSignalRequest request)
    {

        ArgumentNullException.ThrowIfNull(request);


        Helpers.HelpersValidation.ModelValidation(request);

        Signal matchingSignal = await _repository.GetSignalById(request.Id) ?? throw new KeyNotFoundException($"Signal {request.Id} was not found.");
        Signal signalToUpdate = request.ToSignal();
        Signal? updatedSignal = await _repository.UpdateSignal(signalToUpdate);

        if (updatedSignal == null)
            throw new InvalidOperationException("The signal could not be updated.");


        return updatedSignal.ToSignalResponse();
    }

    public async Task<bool> DeleteSignal(Guid id)
    {

        Signal? signalToDelete = await _repository.GetSignalById(id);


        if (signalToDelete is null)
            return false;

        bool isDeleted = await _repository.DeleteSignal(signalToDelete.Id);

        if (!isDeleted)
            throw new InvalidOperationException("This signal could not be deleted.");

        return true;
    }

    public async Task<MemoryStream> GetSignalsCSV()
    {

        List<SignalResponse> signals = (await _repository.GetAllSignals()).Select(report => report.ToSignalResponse()).ToList();


        MemoryStream memoryStream = new MemoryStream();


        StreamWriter streamWriter = new StreamWriter(memoryStream);


        CsvWriter csvWriter = new CsvWriter(streamWriter, culture: CultureInfo.InvariantCulture, leaveOpen: true);


        csvWriter.WriteHeader<SignalResponse>();

        csvWriter.NextRecord();


        await csvWriter.WriteRecordsAsync(signals);


        await streamWriter.FlushAsync();

        memoryStream.Position = 0;

        return memoryStream;

    }

    public async Task<MemoryStream> GetSignalsEXCEL()
    {
        List<SignalResponse> allSignals = (await _repository.GetAllSignals()).Select(report => report.ToSignalResponse()).ToList();

        MemoryStream memoryStream = new MemoryStream();

        using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
        {

            ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("ReportsSheet");

            workSheet.Cells["A1"].Value = "Signal Id";
            workSheet.Cells["B1"].Value = "Problem Name";
            workSheet.Cells["C1"].Value = "Problem Description";
            workSheet.Cells["D1"].Value = "Level of signal";
            workSheet.Cells["E1"].Value = "Status of signal";
            workSheet.Cells["F1"].Value = "Problem is confirmed";

            int row = 2;

            foreach (SignalResponse signalResponse in allSignals)
            {
                workSheet.Cells[$"A{row}"].Value = signalResponse.Id.ToString();
                workSheet.Cells[$"B{row}"].Value = signalResponse.ProblemName;
                workSheet.Cells[$"C{row}"].Value = signalResponse.ProblemDescription;
                workSheet.Cells[$"D{row}"].Value = signalResponse.Level.ToString();
                workSheet.Cells[$"E{row}"].Value = signalResponse.Status.ToString();
                workSheet.Cells[$"F{row}"].Value = signalResponse.IsConfirmed.ToString();

                row++;
            }

            workSheet.Cells[$"A1:F{row}"].AutoFitColumns();


            await excelPackage.SaveAsync();

        }

        memoryStream.Position = 0;
        return memoryStream;
    }


}
