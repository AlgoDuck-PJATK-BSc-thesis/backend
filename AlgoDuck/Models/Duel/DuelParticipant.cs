using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Modules.User.Models.ApplicationUser;
using UserNamespace = AlgoDuck.Modules.User.Models;

namespace AlgoDuck.Modules.Duel.Models
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