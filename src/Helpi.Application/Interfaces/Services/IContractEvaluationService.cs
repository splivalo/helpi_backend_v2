using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Interfaces.Services;


public interface IContractEvaluationService
{
    /// <summary>
    /// Evaluates a student's contracts relative to "today" (UTC).
    /// Pure logic — no side effects. Suitable for unit testing.
    /// </summary>
    ContractEvaluationResult Evaluate(Student student);
}

