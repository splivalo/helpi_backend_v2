using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class SuspensionService
{
    private readonly IUserRepository _userRepository;
    private readonly ISuspensionLogRepository _suspensionLogRepository;
    private readonly IMapper _mapper;

    public SuspensionService(
        IUserRepository userRepository,
        ISuspensionLogRepository suspensionLogRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _suspensionLogRepository = suspensionLogRepository;
        _mapper = mapper;
    }

    public async Task<UserSuspensionStatusDto> GetSuspensionStatusAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var logs = await _suspensionLogRepository.GetByUserIdAsync(userId);

        return new UserSuspensionStatusDto
        {
            IsSuspended = user.IsSuspended,
            SuspensionReason = user.SuspensionReason,
            SuspendedAt = user.SuspendedAt,
            SuspendedByAdminId = user.SuspendedByAdminId,
            SuspensionHistory = _mapper.Map<List<SuspensionLogDto>>(logs)
        };
    }

    public async Task<SuspensionLogDto> SuspendUserAsync(int userId, string reason, int adminId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user.IsSuspended)
            throw new InvalidOperationException("User is already suspended.");

        if (user.UserType == UserType.Admin)
            throw new InvalidOperationException("Cannot suspend an admin user.");

        user.IsSuspended = true;
        user.SuspensionReason = reason;
        user.SuspendedAt = DateTime.UtcNow;
        user.SuspendedByAdminId = adminId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var log = new SuspensionLog
        {
            UserId = userId,
            Action = SuspensionAction.Suspended,
            Reason = reason,
            AdminId = adminId
        };

        await _suspensionLogRepository.AddAsync(log);

        return _mapper.Map<SuspensionLogDto>(log);
    }

    public async Task<SuspensionLogDto> ActivateUserAsync(int userId, int adminId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (!user.IsSuspended)
            throw new InvalidOperationException("User is not suspended.");

        user.IsSuspended = false;
        user.SuspensionReason = null;
        user.SuspendedAt = null;
        user.SuspendedByAdminId = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var log = new SuspensionLog
        {
            UserId = userId,
            Action = SuspensionAction.Activated,
            AdminId = adminId
        };

        await _suspensionLogRepository.AddAsync(log);

        return _mapper.Map<SuspensionLogDto>(log);
    }
}
