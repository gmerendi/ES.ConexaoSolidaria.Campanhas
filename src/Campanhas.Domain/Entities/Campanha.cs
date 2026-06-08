using Campanhas.Domain.Enums;
using Campanhas.Domain.Events;
using Campanhas.Domain.Exceptions;
using Campanhas.Domain.ValueObjects;

namespace Campanhas.Domain.Entities;

public sealed class Campanha : EntityBase
{
    public TituloCampanha Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = null!;
    public MetaFinanceira MetaFinanceira { get; private set; } = null!;
    public decimal ValorArrecadado { get; private set; }
    public DateTime DataInicio { get; private set; }
    public DateTime DataFim { get; private set; }
    public CampanhaStatus StatusCampanha { get; private set; }

    private Campanha() { }

    private Campanha(
        TituloCampanha titulo,
        string descricao,
        MetaFinanceira metaFinanceira,
        DateTime dataInicio,
        DateTime dataFim)
    {
        Titulo = titulo;
        Descricao = descricao;
        MetaFinanceira = metaFinanceira;
        ValorArrecadado = 0;
        DataInicio = dataInicio;
        DataFim = dataFim;
        StatusCampanha = CampanhaStatus.Ativa;

        AddDomainEvent(new CampanhaCriadaEvent(Id, titulo.Valor, metaFinanceira.Valor));
    }

    public static Campanha Criar(
        TituloCampanha titulo,
        string descricao,
        MetaFinanceira metaFinanceira,
        DateTime dataInicio,
        DateTime dataFim)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória.");

        if (dataFim <= dataInicio)
            throw new DomainException("Data de fim deve ser posterior à data de início.");

        return new Campanha(titulo, descricao.Trim(), metaFinanceira, dataInicio, dataFim);
    }

    public void Editar(
        TituloCampanha titulo,
        string descricao,
        MetaFinanceira metaFinanceira,
        DateTime dataInicio,
        DateTime dataFim)
    {
        if (StatusCampanha != CampanhaStatus.Ativa)
            throw new DomainException("Apenas campanhas ativas podem ser editadas.");

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException("Descrição é obrigatória.");

        if (dataFim <= dataInicio)
            throw new DomainException("Data de fim deve ser posterior à data de início.");

        Titulo = titulo;
        Descricao = descricao.Trim();
        MetaFinanceira = metaFinanceira;
        DataInicio = dataInicio;
        DataFim = dataFim;
        SetDataModificacao();
    }

    public void Cancelar()
    {
        if (StatusCampanha == CampanhaStatus.Cancelada)
            throw new DomainException("Campanha já está cancelada.");

        if (StatusCampanha == CampanhaStatus.Concluida)
            throw new DomainException("Campanha concluída não pode ser cancelada.");

        StatusCampanha = CampanhaStatus.Cancelada;
        SetDataModificacao();

        AddDomainEvent(new CampanhaCanceladaEvent(Id, Titulo.Valor));
    }

    public void Concluir()
    {
        if (StatusCampanha != CampanhaStatus.Ativa)
            throw new DomainException("Apenas campanhas ativas podem ser concluídas.");

        StatusCampanha = CampanhaStatus.Concluida;
        SetDataModificacao();

        AddDomainEvent(new CampanhaConcluidaEvent(Id, Titulo.Valor, ValorArrecadado));
    }

    public void AdicionarArrecadacao(decimal valor)
    {
        if (StatusCampanha != CampanhaStatus.Ativa)
            throw new DomainException("Apenas campanhas ativas podem receber arrecadações.");

        if (valor <= 0)
            throw new DomainException("Valor de arrecadação deve ser positivo.");

        ValorArrecadado += valor;
        SetDataModificacao();
    }
}
