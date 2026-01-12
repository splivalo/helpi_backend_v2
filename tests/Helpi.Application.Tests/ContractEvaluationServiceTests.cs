using System;
using System.Collections.Generic;
using Helpi.Application.Services;
using Helpi.Domain.Entities;
using Xunit;

namespace Helpi.Application.Tests
{
    public class ContractEvaluationServiceTests
    {
        private readonly ContractEvaluationService _svc = new();

        private static StudentContract Make(DateOnly eff, DateOnly exp, int id = 1)
        {
            return new StudentContract
            {
                Id = id,
                EffectiveDate = eff,
                ExpirationDate = exp,
                ContractNumber = $"C{id}",
                CloudPath = $"p{id}"
            };
        }

        [Fact]
        public void ActiveContractDetected()
        {
            // today = UTC now (we compute relative to DateOnly.FromDateTime(DateTime.UtcNow))
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var c = Make(today.AddDays(-10), today.AddDays(10), id: 1);

            var s = new Student { UserId = 1, Contracts = new List<StudentContract> { c } };

            var result = _svc.Evaluate(s);

            Assert.NotNull(result.ActiveContract);
            Assert.Null(result.NextContract);
            Assert.False(result.HasGap);
            Assert.Equal(c.Id, result.ActiveContract!.Id);
        }

        [Fact]
        public void ImmediateNextContract_NoGap_ShouldNotTreatAsExpired()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var old = Make(today.AddDays(-30), today.AddDays(-1), id: 1); // expired yesterday
            var next = Make(today, today.AddMonths(12), id: 2); // starts today => immediate transition

            var s = new Student { UserId = 2, Contracts = new List<StudentContract> { old, next } };

            var res = _svc.Evaluate(s);

            // No active contract by virtue of code (next starts today -> active should be detected as active),
            // but our evaluator picks active if EffectiveDate <= today and ExpirationDate >= today.
            // Because next.EffectiveDate == today, next should be active.
            Assert.NotNull(res.ActiveContract);
            Assert.Equal(2, res.ActiveContract!.Id);
            Assert.NotNull(res.NextContract); // might be null depending on ordering, but ensure no gap
            Assert.False(res.HasGap);
        }

        [Fact]
        public void GapBetweenContracts_ShouldReportHasGap()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var old = Make(today.AddDays(-60), today.AddDays(-30), id: 1); // expired 30 days ago
            var next = Make(today.AddDays(10), today.AddDays(400), id: 2); // starts in 10 days -> gap exists

            var s = new Student { UserId = 3, Contracts = new List<StudentContract> { old, next } };

            var res = _svc.Evaluate(s);

            // No active contract today
            Assert.Null(res.ActiveContract);
            Assert.NotNull(res.NextContract);
            Assert.True(res.HasGap);
            Assert.NotNull(res.DaysSinceExpiry);
            Assert.True(res.DaysSinceExpiry >= 30);
        }
    }
}
