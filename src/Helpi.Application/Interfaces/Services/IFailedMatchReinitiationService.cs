namespace Helpi.Application.Services;

public interface IFailedMatchReinitiationService
{

    Task ReinitiateAllFailedMatches();
    Task ReinitiateMatchingForOrderSchedule(int orderScheduleId);
    Task ReinitiateMatchingForReassignmentRecord(int reassignmentRecordId);
}