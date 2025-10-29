using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CohortModel = AlgoDuck.Models.Cohort.Cohort;

namespace AlgoDuck.Models.Duel
{
    public class Duel
    {
        [Key]
        public Guid DuelId { get; set; } = Guid.NewGuid();

        [Required]
        public DateTime StartingDate { get; set; }

        [ForeignKey("Cohort1")]
        public Guid Cohort1Id { get; set; }
        public required CohortModel Cohort1 { get; set; }

        [ForeignKey("Cohort2")]
        public Guid Cohort2Id { get; set; }
        public required CohortModel Cohort2 { get; set; }

        public ICollection<DuelParticipant> DuelParticipants { get; set; } = new List<DuelParticipant>();
    }
}