using System.ComponentModel.DataAnnotations;

namespace Helpi.Domain.Entities
{

    public class Faculty
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string FacultyName { get; set; } = null!;

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
