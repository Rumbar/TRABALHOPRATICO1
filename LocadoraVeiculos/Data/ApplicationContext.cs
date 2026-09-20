using LocadoraVeiculos.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Data
{
    /// <summary>
    /// Contexto do Entity Framework Core responsavel por mapear as entidades
    /// do dominio para as tabelas do banco de dados SQL Server.
    /// </summary>
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public ApplicationContext()
        {
        }

        // Mapeamento das entidades para as tabelas do banco
        public DbSet<Fabricante> Fabricantes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }

        /// <summary>
        /// Configuracao usada quando o contexto e instanciado sem injecao de dependencia
        /// (por exemplo, pelas ferramentas de migracao do Entity Framework).
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    "Server=localhost,1433;Database=LocadoraVeiculos;User Id=sa;Password=SqlServer@2026;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------------------------------------------------------
            // FABRICANTE
            // ---------------------------------------------------------------
            modelBuilder.Entity<Fabricante>(entity =>
            {
                entity.ToTable("Fabricante");

                // Chave primaria
                entity.HasKey(f => f.FabricanteId);
                entity.Property(f => f.FabricanteId).ValueGeneratedOnAdd();

                entity.Property(f => f.Nome).IsRequired().HasMaxLength(80);
                entity.Property(f => f.PaisOrigem).HasMaxLength(60);

                // Restricao de unicidade: nao pode haver dois fabricantes com o mesmo nome
                entity.HasIndex(f => f.Nome).IsUnique().HasDatabaseName("UQ_Fabricante_Nome");
            });

            // ---------------------------------------------------------------
            // CATEGORIA
            // ---------------------------------------------------------------
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categoria");

                // Chave primaria
                entity.HasKey(c => c.CategoriaId);
                entity.Property(c => c.CategoriaId).ValueGeneratedOnAdd();

                entity.Property(c => c.Nome).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Descricao).HasMaxLength(200);
                entity.Property(c => c.PercentualAjuste).HasColumnType("decimal(5,2)");

                entity.HasIndex(c => c.Nome).IsUnique().HasDatabaseName("UQ_Categoria_Nome");
            });

            // ---------------------------------------------------------------
            // VEICULO
            // ---------------------------------------------------------------
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("Veiculo", t =>
                {
                    t.HasCheckConstraint("CK_Veiculo_AnoFabricacao", "[AnoFabricacao] >= 1950");
                    t.HasCheckConstraint("CK_Veiculo_Quilometragem", "[Quilometragem] >= 0");
                    t.HasCheckConstraint("CK_Veiculo_ValorDiaria", "[ValorDiaria] > 0");
                });

                // Chave primaria
                entity.HasKey(v => v.VeiculoId);
                entity.Property(v => v.VeiculoId).ValueGeneratedOnAdd();

                entity.Property(v => v.Placa).IsRequired().HasMaxLength(8);
                entity.Property(v => v.Modelo).IsRequired().HasMaxLength(80);
                entity.Property(v => v.AnoFabricacao).IsRequired();
                entity.Property(v => v.Quilometragem).IsRequired();
                entity.Property(v => v.Cor).HasMaxLength(30);
                entity.Property(v => v.ValorDiaria).HasColumnType("decimal(10,2)");
                entity.Property(v => v.Status).HasConversion<int>().HasDefaultValue(StatusVeiculo.Disponivel);

                // Restricao de unicidade: a placa identifica o veiculo
                entity.HasIndex(v => v.Placa).IsUnique().HasDatabaseName("UQ_Veiculo_Placa");

                // Chave estrangeira: Veiculo (N) -> Fabricante (1)
                entity.HasOne(v => v.Fabricante)
                      .WithMany(f => f.Veiculos)
                      .HasForeignKey(v => v.FabricanteId)
                      .HasConstraintName("FK_Veiculo_Fabricante")
                      .OnDelete(DeleteBehavior.Restrict);

                // Chave estrangeira: Veiculo (N) -> Categoria (1)
                entity.HasOne(v => v.Categoria)
                      .WithMany(c => c.Veiculos)
                      .HasForeignKey(v => v.CategoriaId)
                      .HasConstraintName("FK_Veiculo_Categoria")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------------------------------------------------------
            // CLIENTE
            // ---------------------------------------------------------------
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Cliente");

                // Chave primaria
                entity.HasKey(c => c.ClienteId);
                entity.Property(c => c.ClienteId).ValueGeneratedOnAdd();

                entity.Property(c => c.Nome).IsRequired().HasMaxLength(120);
                entity.Property(c => c.Cpf).IsRequired().HasMaxLength(11).IsFixedLength();
                entity.Property(c => c.Email).IsRequired().HasMaxLength(120);
                entity.Property(c => c.Telefone).HasMaxLength(20);
                entity.Property(c => c.NumeroCnh).HasMaxLength(11);
                entity.Property(c => c.CategoriaCnh).HasMaxLength(3);
                entity.Property(c => c.DataNascimento).HasColumnType("date");
                entity.Property(c => c.DataCadastro).HasColumnType("date");

                // Restricoes de unicidade: CPF e e-mail nao podem se repetir
                entity.HasIndex(c => c.Cpf).IsUnique().HasDatabaseName("UQ_Cliente_Cpf");
                entity.HasIndex(c => c.Email).IsUnique().HasDatabaseName("UQ_Cliente_Email");
            });

            // ---------------------------------------------------------------
            // FUNCIONARIO
            // ---------------------------------------------------------------
            modelBuilder.Entity<Funcionario>(entity =>
            {
                entity.ToTable("Funcionario");

                // Chave primaria
                entity.HasKey(f => f.FuncionarioId);
                entity.Property(f => f.FuncionarioId).ValueGeneratedOnAdd();

                entity.Property(f => f.Nome).IsRequired().HasMaxLength(120);
                entity.Property(f => f.Cpf).IsRequired().HasMaxLength(11).IsFixedLength();
                entity.Property(f => f.Matricula).IsRequired().HasMaxLength(20);
                entity.Property(f => f.Cargo).HasMaxLength(60);
                entity.Property(f => f.DataAdmissao).HasColumnType("date");

                entity.HasIndex(f => f.Cpf).IsUnique().HasDatabaseName("UQ_Funcionario_Cpf");
                entity.HasIndex(f => f.Matricula).IsUnique().HasDatabaseName("UQ_Funcionario_Matricula");
            });

            // ---------------------------------------------------------------
            // ALUGUEL
            // ---------------------------------------------------------------
            modelBuilder.Entity<Aluguel>(entity =>
            {
                entity.ToTable("Aluguel", t =>
                {
                    t.HasCheckConstraint("CK_Aluguel_Periodo", "[DataPrevistaDevolucao] >= [DataRetirada]");
                    t.HasCheckConstraint("CK_Aluguel_Quilometragem", "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");
                    t.HasCheckConstraint("CK_Aluguel_ValorDiaria", "[ValorDiaria] > 0");
                });

                // Chave primaria
                entity.HasKey(a => a.AluguelId);
                entity.Property(a => a.AluguelId).ValueGeneratedOnAdd();

                entity.Property(a => a.DataRetirada).IsRequired();
                entity.Property(a => a.DataPrevistaDevolucao).IsRequired();
                entity.Property(a => a.QuilometragemInicial).IsRequired();
                entity.Property(a => a.ValorDiaria).HasColumnType("decimal(10,2)");
                entity.Property(a => a.ValorTotal).HasColumnType("decimal(10,2)");
                entity.Property(a => a.Observacao).HasMaxLength(300);
                entity.Property(a => a.Status).HasConversion<int>().HasDefaultValue(StatusAluguel.EmAndamento);

                // Chave estrangeira: Aluguel (N) -> Cliente (1)
                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Alugueis)
                      .HasForeignKey(a => a.ClienteId)
                      .HasConstraintName("FK_Aluguel_Cliente")
                      .OnDelete(DeleteBehavior.Restrict);

                // Chave estrangeira: Aluguel (N) -> Veiculo (1)
                entity.HasOne(a => a.Veiculo)
                      .WithMany(v => v.Alugueis)
                      .HasForeignKey(a => a.VeiculoId)
                      .HasConstraintName("FK_Aluguel_Veiculo")
                      .OnDelete(DeleteBehavior.Restrict);

                // Chave estrangeira: Aluguel (N) -> Funcionario (1)
                entity.HasOne(a => a.Funcionario)
                      .WithMany(f => f.Alugueis)
                      .HasForeignKey(a => a.FuncionarioId)
                      .HasConstraintName("FK_Aluguel_Funcionario")
                      .OnDelete(DeleteBehavior.Restrict);

                // Indices de apoio as consultas por cliente, veiculo e periodo
                entity.HasIndex(a => a.ClienteId).HasDatabaseName("IX_Aluguel_ClienteId");
                entity.HasIndex(a => a.VeiculoId).HasDatabaseName("IX_Aluguel_VeiculoId");
                entity.HasIndex(a => a.DataRetirada).HasDatabaseName("IX_Aluguel_DataRetirada");
            });

            // ---------------------------------------------------------------
            // CARGA INICIAL DE DADOS FIXOS
            // ---------------------------------------------------------------
            modelBuilder.Entity<Fabricante>().HasData(
                new Fabricante { FabricanteId = 1, Nome = "Volkswagen", PaisOrigem = "Alemanha", AnoFundacao = 1937 },
                new Fabricante { FabricanteId = 2, Nome = "Fiat", PaisOrigem = "Italia", AnoFundacao = 1899 },
                new Fabricante { FabricanteId = 3, Nome = "Toyota", PaisOrigem = "Japao", AnoFundacao = 1937 },
                new Fabricante { FabricanteId = 4, Nome = "Chevrolet", PaisOrigem = "Estados Unidos", AnoFundacao = 1911 }
            );

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { CategoriaId = 1, Nome = "Economico", Descricao = "Veiculos de entrada, motor 1.0", PercentualAjuste = 0m },
                new Categoria { CategoriaId = 2, Nome = "Intermediario", Descricao = "Sedans compactos e hatches medios", PercentualAjuste = 15m },
                new Categoria { CategoriaId = 3, Nome = "SUV", Descricao = "Utilitarios esportivos", PercentualAjuste = 35m },
                new Categoria { CategoriaId = 4, Nome = "Executivo", Descricao = "Veiculos de luxo e alto padrao", PercentualAjuste = 60m }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
