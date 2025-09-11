using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.Enums;

namespace Helpi.Domain.Entities
{

    public class StudentContract
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }


        public string ContractNumber { get; set; } = null!;

        [MaxLength(512)]
        public string CloudPath { get; set; } = null!;
        public DateOnly EffectiveDate { get; set; }
        public DateOnly ExpirationDate { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Student Student { get; set; } = null!;

        public ICollection<JobInstance> JobInstances { get; set; } = new List<JobInstance>();

        public ContractStatus Status
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                if (today < EffectiveDate)
                    return ContractStatus.Pending;

                if (today > ExpirationDate)
                    return ContractStatus.Expired;

                return ContractStatus.Active;
            }
        }

    }
}
