using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendas.Models
{
    public class Venda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idVenda { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public int idCliente { get; set; }

        [Required(ErrorMessage = "O produto é obrigatório.")]
        public int idProduto { get; set; }

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        public int qtdVenda { get; set; }

        [Required(ErrorMessage = "O valor unitário é obrigatório.")]
        public int vlrUnitarioVenda { get; set; }

        [Required(ErrorMessage = "A data da venda é obrigatória.")]
        public DateTime dthVenda { get; set; }

        public float vlrTotalVenda { get; set; }

        [ForeignKey("idCliente")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("idProduto")]
        public virtual Produto Produto { get; set; }
    }
}
