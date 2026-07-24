using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.LeaveType;
using VacaYAY.Business.Services.LeaveType;
using VacaYAY.Data;
using LeaveTypeEntity = VacaYAY.Domain.Entities.LeaveType;

namespace VacaYAY.UnitTests;

public class LeaveTypeServiceTests : IDisposable
{
    private readonly VacaYAYDbContext _db;
    private readonly LeaveTypeService _service;

    public LeaveTypeServiceTests()
    {
        _db = NewDb();
        _service = new LeaveTypeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAllAsync_ReturnsTypesOrderedByName()
    {
        // Given
        _db.LeaveTypes.AddRange(
            new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Sick },
            new LeaveTypeEntity { Id = 2, Name = LeaveTypeName.Annual });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetAllAsync();

        // Then
        Assert.Equal(2, result.Count);
        Assert.Equal(LeaveTypeName.Annual, result[0].Name);
        Assert.Equal(LeaveTypeName.Sick, result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedTypes()
    {
        // Given
        _db.LeaveTypes.AddRange(
            new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual },
            new LeaveTypeEntity { Id = 2, Name = LeaveTypeName.Sick, IsDeleted = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetAllAsync();

        // Then — the global query filter hides the archived type
        Assert.Equal(LeaveTypeName.Annual, Assert.Single(result).Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsType_WhenFound()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual, IsPaid = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetByIdAsync(1);

        // Then
        Assert.NotNull(result);
        Assert.Equal(LeaveTypeName.Annual, result!.Name);
        Assert.True(result.IsPaid);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.GetByIdAsync(999);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated()
    {
        // Given
        var request = new CreateLeaveTypeRequest
        {
            Name = LeaveTypeName.Annual,
            Color = LeaveColor.Green,
            IsPaid = true,
            CountsAgainstBalance = true,
        };

        // When
        var result = await _service.CreateAsync(request);

        // Then
        Assert.Equal(CreateLeaveTypeStatus.Created, result.Status);
        Assert.NotNull(result.Dto);
        Assert.Equal(LeaveTypeName.Annual, result.Dto!.Name);
        Assert.True(await _db.LeaveTypes.AnyAsync(lt => lt.Name == LeaveTypeName.Annual));
    }

    [Fact]
    public async Task CreateAsync_ReturnsNameConflict_WhenActiveNameExists()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual });
        await _db.SaveChangesAsync();

        var request = new CreateLeaveTypeRequest { Name = LeaveTypeName.Annual };

        // When
        var result = await _service.CreateAsync(request);

        // Then
        Assert.Equal(CreateLeaveTypeStatus.NameConflict, result.Status);
        Assert.Null(result.Dto);
    }

    [Fact]
    public async Task CreateAsync_ReturnsArchivedExists_WhenSoftDeletedNameExists()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 7, Name = LeaveTypeName.Annual, IsDeleted = true });
        await _db.SaveChangesAsync();

        var request = new CreateLeaveTypeRequest { Name = LeaveTypeName.Annual };

        // When
        var result = await _service.CreateAsync(request);

        // Then — the name is held by an archived type; HR can restore it instead
        Assert.Equal(CreateLeaveTypeStatus.ArchivedExists, result.Status);
        Assert.Equal(7, result.ArchivedId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEditableFields()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity
        {
            Id = 1,
            Name = LeaveTypeName.Annual,
            Color = LeaveColor.Gray,
            IsPaid = false,
            CountsAgainstBalance = false,
        });
        await _db.SaveChangesAsync();

        var request = new UpdateLeaveTypeRequest
        {
            Color = LeaveColor.Blue,
            IsPaid = true,
            CountsAgainstBalance = true,
        };

        // When
        var result = await _service.UpdateAsync(1, request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(LeaveColor.Blue, result!.Color);
        Assert.True(result.IsPaid);
        Assert.True(result.CountsAgainstBalance);
        // Name stays the type's immutable identity
        Assert.Equal(LeaveTypeName.Annual, result.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.UpdateAsync(999, new UpdateLeaveTypeRequest());

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesType_AndReturnsTrue()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.DeleteAsync(1);

        // Then
        Assert.True(result);
        var archived = await _db.LeaveTypes.IgnoreQueryFilters().SingleAsync(lt => lt.Id == 1);
        Assert.True(archived.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // When
        var result = await _service.DeleteAsync(999);

        // Then
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreAsync_RestoresArchivedType()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual, IsDeleted = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.RestoreAsync(1);

        // Then
        Assert.NotNull(result);
        Assert.Equal(LeaveTypeName.Annual, result!.Name);
        var restored = await _db.LeaveTypes.SingleAsync(lt => lt.Id == 1);
        Assert.False(restored.IsDeleted);
    }

    [Fact]
    public async Task RestoreAsync_ReturnsNull_WhenTypeIsNotArchived()
    {
        // Given
        _db.LeaveTypes.Add(new LeaveTypeEntity { Id = 1, Name = LeaveTypeName.Annual });
        await _db.SaveChangesAsync();

        // When — the type exists but isn't soft-deleted, so there's nothing to restore
        var result = await _service.RestoreAsync(1);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task RestoreAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.RestoreAsync(999);

        // Then
        Assert.Null(result);
    }

    // Unique database name per call so tests never share state.
    private static VacaYAYDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VacaYAYDbContext(options);
    }
}
