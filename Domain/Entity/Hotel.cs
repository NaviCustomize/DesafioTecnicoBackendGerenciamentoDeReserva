using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGerenciamentoDeReserva.Domain.Entity
{
    public class Hotel
    {

        public long Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Localizacao { get; set; } = string.Empty;
        public string? Descricao { get; set; }

        public Hotel() { }

        public Hotel(string nome, string localizacao, string descricao)
        {
            Nome = nome;
            Localizacao = localizacao;
            Descricao = descricao;
        }
    }
}
