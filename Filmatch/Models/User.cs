using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Filmatch.Models;

public class User : IdentityUser
{
    public ICollection<Match> MatchesAsUser1 { get; set; } = new List<Match>();
    public ICollection<Match> MatchesAsUser2 { get; set; } = new List<Match>();
    
    [NotMapped]
    public IEnumerable<Match> Matches => MatchesAsUser1.Concat(MatchesAsUser2);
    
    public List<Swipe> Swipes { get; set; }
}