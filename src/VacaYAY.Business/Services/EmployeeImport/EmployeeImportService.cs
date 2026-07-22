using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Business.Interfaces.Employee;
using VacaYAY.Business.Interfaces.EmployeeImport;
using VacaYAY.Data;

namespace VacaYAY.Business.Services.EmployeeImport;

/// <summary>
/// Migrates employees from the old system into VacaYAY. Idempotency is delegated to
/// <see cref="IEmployeeService.CreateAsync"/>, which already rejects an email held by an active or
/// archived account — so re-importing the same people can only ever produce skips.
/// </summary>
public class EmployeeImportService : IEmployeeImportService
{
    private readonly ILegacyEmployeeService _legacyEmployeeService;
    private readonly IEmployeeService _employeeService;
    private readonly IValidator<CreateEmployeeRequest> _createValidator;
    private readonly VacaYAYDbContext _db;

    public EmployeeImportService(
        ILegacyEmployeeService legacyEmployeeService,
        IEmployeeService employeeService,
        IValidator<CreateEmployeeRequest> createValidator,
        VacaYAYDbContext db)
    {
        _legacyEmployeeService = legacyEmployeeService;
        _employeeService = employeeService;
        _createValidator = createValidator;
        _db = db;
    }

    public async Task<IReadOnlyList<LegacyEmployeeRosterItemDto>> GetRosterAsync(CancellationToken cancellationToken = default)
    {
        var legacyEmployees = await _legacyEmployeeService.GetAllAsync(cancellationToken);

        return await BuildRosterAsync(legacyEmployees, cancellationToken);
    }

    public async Task<ImportLegacyEmployeesResult> ImportAsync(ImportLegacyEmployeesRequest request, CancellationToken cancellationToken = default)
    {
        var legacyEmployees = await _legacyEmployeeService.GetAllAsync(cancellationToken);

        var byLegacyId = legacyEmployees.ToDictionary(e => e.LegacyId);
        var result = new ImportLegacyEmployeesResult();

        foreach (var legacyId in request.LegacyIds.Distinct())
        {
            if (!byLegacyId.TryGetValue(legacyId, out var legacyEmployee))
            {
                result.NotFound++;
                continue;
            }

            var createRequest = legacyEmployee.Adapt<CreateEmployeeRequest>();

            // The old system is not a trusted source — a row with, say, a malformed email must not
            // become a User just because it arrived over the import path instead of the form.
            var validation = await _createValidator.ValidateAsync(createRequest, cancellationToken);
            if (!validation.IsValid)
            {
                result.Invalid++;
                continue;
            }

            var created = await _employeeService.CreateAsync(createRequest, cancellationToken);

            if (created.Status == CreateEmployeeStatus.Created)
            {
                result.Imported++;
                result.ImportedEmployees.Add(created.Dto!);
            }
            else
            {
                result.Skipped++;
            }
        }

        result.Roster = await BuildRosterAsync(legacyEmployees, cancellationToken);

        return result;
    }

    private async Task<List<LegacyEmployeeRosterItemDto>> BuildRosterAsync(
        IReadOnlyList<LegacyEmployeeDto> legacyEmployees,
        CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters is required: the unique Email index spans soft-deleted rows, so an
        // archived account still owns its email and its legacy twin must show as already imported.
        var takenEmails = await _db.Users
            .IgnoreQueryFilters()
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        var taken = new HashSet<string>(takenEmails, StringComparer.OrdinalIgnoreCase);

        return legacyEmployees
            .Select(e =>
            {
                var item = e.Adapt<LegacyEmployeeRosterItemDto>();
                item.AlreadyImported = taken.Contains(e.Email);
                return item;
            })
            .OrderBy(i => i.LastName)
            .ThenBy(i => i.FirstName)
            .ToList();
    }
}
