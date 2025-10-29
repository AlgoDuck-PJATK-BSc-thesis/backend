using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Models.User.ApplicationUser;

namespace AlgoDuck.Models.Duel
{
    public class DuelParticipant
    {
        [ForeignKey("Duel")]
        public Guid DuelId { get; set; }
        public required Duel Duel { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required ApplicationUser User { get; set; }
    }
}