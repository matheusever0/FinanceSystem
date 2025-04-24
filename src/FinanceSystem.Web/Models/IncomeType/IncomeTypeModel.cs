using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.IncomeType
{
    public class IncomeTypeModel
    {
        public required string Id { get; set; }

        [Display(Name = "Nome")]
        public required string Name { get; set; }

        [Display(Name = "Descrição")]
        public required string Description { get; set; }

        [Display(Name = "Tipo do Sistema")]
        public bool IsSystem { get; set; }

        public required string UserId { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }
    }
}