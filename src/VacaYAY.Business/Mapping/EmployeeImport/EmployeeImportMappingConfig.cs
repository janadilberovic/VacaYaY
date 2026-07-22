using Mapster;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Domain.Entities;

namespace VacaYAY.Business.Mapping.EmployeeImport;

public class EmployeeImportMappingConfig : IRegister
{
    /// <summary>Imported employees start on the standard allowance rather than inheriting the old system's balance.</summary>
    private const int StartingDaysOff = 20;

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LegacyEmployee, LegacyEmployeeDto>();

        config.NewConfig<LegacyEmployeeDto, LegacyEmployeeRosterItemDto>();

        // The old system names things differently, and its leave balance is deliberately dropped.
        config.NewConfig<LegacyEmployeeDto, CreateEmployeeRequest>()
            .Map(dest => dest.JobTitle, src => src.Title)
            .Map(dest => dest.HireDate, src => src.HiredOn)
            .Map(dest => dest.EmploymentStartDate, src => src.HiredOn)
            .Map(dest => dest.EmploymentEndDate, src => src.ContractEnd)
            .Map(dest => dest.Role, src => UserRole.Employee)
            .Map(dest => dest.DaysOff, src => StartingDaysOff);
    }
}
