using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models
{
    public class Player
    {
        [Required]
        public string Nickname { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "O level deve ser maior que zero")]
        public int Level { get; set; }

        public bool Gira50x { get; set; }

        public bool Descanso { get; set; }

        public bool Prelive { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A energia máxima deve ser um valor positivo")]
        public int MaxEnergia { get; set; }
    }

    public class Team
    {
        public int TeamNumber { get; set; }
        public List<Player> Players { get; set; } = new List<Player>();
    }

    public class TeamDistributionViewModel
    {
        public List<Team> Teams { get; set; } = new List<Team>();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public int MaxPlayersPerTeam { get; set; } = 22;
    }
}