using System.ComponentModel.DataAnnotations;

namespace Equilibrium.Web.Models.Financing
{
    public class UpdateFinancingModel
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 100 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string Notes { get; set; }
    }
}
