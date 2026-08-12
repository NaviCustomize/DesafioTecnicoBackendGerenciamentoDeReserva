using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGerenciamentoDeReserva.Domain.Entity
{
    public class Hotel
    {

        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public decimal PrecoPorNoite { get; set; }
        public int QtdQuartos { get; set; }
        public string? Descricao { get; set; }

        public Hotel() { }

        public Hotel(string nome, string localizacao, decimal precoPorNoite, int qtdQuartos, string descricao)
        {
            Nome = nome;
            Localizacao = localizacao;
            PrecoPorNoite = precoPorNoite;
            QtdQuartos = qtdQuartos;
            Descricao = descricao;
        }
    }
}
