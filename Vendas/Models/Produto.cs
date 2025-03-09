using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendas.Models
{
    public class Produto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProduto { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string DscProduto { get; set; }

        [Required(ErrorMessage = "O valor unitário é obrigatório.")]
        public float VlrUnitario { get; set; }
    }
}
