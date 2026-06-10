using Campanhas.Domain.Entities;
using Campanhas.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Campanhas.Infrastructure.Data.Configurations;

public sealed class CampanhaConfiguration : IEntityTypeConfiguration<Campanha>
{
    public void Configure(EntityTypeBuilder<Campanha> builder)
    {
        builder.ToTable("Campanhas");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.DataCriacao).IsRequired();
        builder.Property(c => c.DataModificacao).IsRequired();
        builder.Property(c => c.Status).IsRequired();

        builder.OwnsOne(c => c.Titulo, titulo =>
        {
            titulo.Property(t => t.Valor)
                .HasColumnName("Titulo")
                .HasMaxLength(TituloCampanha.TamanhoMaximo)
                .IsRequired();
        });

        builder.OwnsOne(c => c.MetaFinanceira, meta =>
        {
            meta.Property(m => m.Valor)
                .HasColumnName("MetaFinanceira")
                .HasColumnType("numeric(18,2)")
                .IsRequired();
        });

        builder.Property(c => c.Descricao)
            .HasMaxLength(2000);

        builder.Property(c => c.ValorArrecadado)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(c => c.DataInicio).IsRequired();
        builder.Property(c => c.DataFim).IsRequired();
        builder.Property(c => c.StatusCampanha).IsRequired();

        builder.Ignore(c => c.DomainEvents);
    }
}
