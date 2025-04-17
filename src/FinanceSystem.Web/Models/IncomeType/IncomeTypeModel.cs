using System.ComponentModel.DataAnnotations;

namespace FinanceSystem.Web.Models.IncomeType
{
    // Modelo principal para exibição de tipos de receita
    public class IncomeTypeModel
    {
        public string Id { get; set; }

        [Display(Name = "Nome")]
        public string Name { get; set; }

        [Display(Name = "Descrição")]
        public string Description { get; set; }

        [Display(Name = "Tipo do Sistema")]
        public bool IsSystem { get; set; }

        public string UserId { get; set; }

        [Display(Name = "Data de Criação")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; }
    }

    // Modelo para criação de novos tipos de receita
    public class CreateIncomeTypeModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }

    // Modelo para atualização de tipos de receita existentes
    public class UpdateIncomeTypeModel
    {
        [StringLength(50, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 50 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres")]
        [Display(Name = "Descrição")]
        public string Description { get; set; }
    }
}