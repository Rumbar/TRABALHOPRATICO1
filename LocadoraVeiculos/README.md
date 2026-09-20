# Locadora de Veículos — Trabalho Prático 1

**Etapa 1 — Modelagem do Banco de Dados**

Aluno: Diogo — Código de Pessoa: 1285293
PUC Minas — Análise e Desenvolvimento de Sistemas

Sistema de aluguel de veículos desenvolvido em C# com ASP.NET Core, Entity Framework Core e SQL Server Express. Esta etapa entrega o modelo conceitual, as entidades da camada Model, as chaves primárias e estrangeiras e a configuração da classe `ApplicationContext`.

---

## 1. Modelo conceitual

![Diagrama Entidade-Relacionamento](docs/DER.png)

O banco possui **6 entidades**, uma a mais que o mínimo exigido (5):

| Entidade | Descrição |
|---|---|
| `Fabricante` | Marca do veículo (Volkswagen, Fiat, Toyota...) |
| `Categoria` | Classificação de locação (Econômico, SUV, Executivo...) |
| `Veiculo` | Veículo da frota, com modelo, ano de fabricação e quilometragem |
| `Cliente` | Cliente da locadora, com nome, CPF e e-mail |
| `Funcionario` | Funcionário responsável por registrar o contrato |
| `Aluguel` | Contrato que vincula cliente, veículo e funcionário em um período |

### Regras do enunciado atendidas

| Regra | Como foi atendida |
|---|---|
| Todo veículo pertence a um fabricante | FK `Veiculo.FabricanteId` → `Fabricante` (obrigatória) |
| Veículo registra modelo, ano de fabricação e quilometragem | Colunas `Modelo`, `AnoFabricacao` e `Quilometragem`, todas obrigatórias |
| Cliente tem pelo menos nome, CPF e e-mail | Colunas `Nome`, `Cpf` e `Email`, obrigatórias, com CPF e e-mail únicos |
| Aluguel atrelado a um cliente e a um veículo em um período | FKs `ClienteId` e `VeiculoId` + `DataRetirada` e `DataPrevistaDevolucao` |
| Registro da devolução | `DataDevolucao` (nula enquanto o aluguel está em andamento) e `Status` |
| Quilometragem inicial e final do aluguel | `QuilometragemInicial` e `QuilometragemFinal` |
| Valor da diária e valor total | `ValorDiaria` e `ValorTotal`, ambos `decimal(10,2)` |
| Mínimo de 5 entidades | 6 entidades: as 4 citadas no enunciado + `Categoria` e `Funcionario` |

---

## 2. Chaves e restrições

### Chaves primárias

| Tabela | Chave primária |
|---|---|
| `Fabricante` | `FabricanteId` (identity) |
| `Categoria` | `CategoriaId` (identity) |
| `Veiculo` | `VeiculoId` (identity) |
| `Cliente` | `ClienteId` (identity) |
| `Funcionario` | `FuncionarioId` (identity) |
| `Aluguel` | `AluguelId` (identity) |

### Chaves estrangeiras

| Constraint | Tabela origem | Coluna | Tabela destino | Cardinalidade |
|---|---|---|---|---|
| `FK_Veiculo_Fabricante` | `Veiculo` | `FabricanteId` | `Fabricante` | N:1 |
| `FK_Veiculo_Categoria` | `Veiculo` | `CategoriaId` | `Categoria` | N:1 |
| `FK_Aluguel_Cliente` | `Aluguel` | `ClienteId` | `Cliente` | N:1 |
| `FK_Aluguel_Veiculo` | `Aluguel` | `VeiculoId` | `Veiculo` | N:1 |
| `FK_Aluguel_Funcionario` | `Aluguel` | `FuncionarioId` | `Funcionario` | N:1 |

Todas as FKs usam `DeleteBehavior.Restrict`, impedindo a exclusão de um registro que ainda possua dependentes (não é possível apagar um cliente que tenha alugueis registrados).

### Restrições de unicidade

`UQ_Fabricante_Nome`, `UQ_Categoria_Nome`, `UQ_Veiculo_Placa`, `UQ_Cliente_Cpf`, `UQ_Cliente_Email`, `UQ_Funcionario_Cpf`, `UQ_Funcionario_Matricula`.

### Restrições de verificação (CHECK)

| Constraint | Regra |
|---|---|
| `CK_Veiculo_AnoFabricacao` | `AnoFabricacao >= 1950` |
| `CK_Veiculo_Quilometragem` | `Quilometragem >= 0` |
| `CK_Veiculo_ValorDiaria` | `ValorDiaria > 0` |
| `CK_Aluguel_Periodo` | `DataPrevistaDevolucao >= DataRetirada` |
| `CK_Aluguel_Quilometragem` | `QuilometragemFinal >= QuilometragemInicial` (quando preenchida) |
| `CK_Aluguel_ValorDiaria` | `ValorDiaria > 0` |

---

## 3. Estrutura do projeto

```
LocadoraVeiculos/
├── Models/                      # Camada Model — entidades do domínio
│   ├── Fabricante.cs
│   ├── Categoria.cs
│   ├── Veiculo.cs
│   ├── Cliente.cs
│   ├── Funcionario.cs
│   ├── Aluguel.cs
│   └── Enums.cs                 # StatusVeiculo e StatusAluguel
├── Data/
│   └── ApplicationContext.cs    # Mapeamento objeto-relacional (Fluent API)
├── docs/
│   ├── DER.png                  # Diagrama entidade-relacionamento
│   └── DER.svg
├── Program.cs                   # Injeção do contexto + Swagger
├── appsettings.json             # Connection strings
├── docker-compose.yml           # SQL Server em container
└── LocadoraVeiculos.csproj
```

---

## 4. Como executar

### 4.1. Subir o SQL Server

**Opção A — container Docker (usado neste projeto):**

```bash
docker compose up -d
```

**Opção B — SQL Server Express instalado localmente:** troque a connection string
`LocadoraConnection` em `appsettings.json` pelo valor de `LocadoraConnectionSqlExpress`.

### 4.2. Criar o banco a partir do modelo

```bash
dotnet restore
dotnet tool install --global dotnet-ef        # apenas na primeira vez
dotnet ef migrations add CriacaoInicial
dotnet ef database update
```

O Entity Framework traduz as classes da pasta `Models` em tabelas, criando as chaves primárias, as chaves estrangeiras, os índices únicos e as constraints de verificação descritas acima.

### 4.3. Executar a aplicação

```bash
dotnet run --urls http://0.0.0.0:5000
```

Swagger: `http://localhost:5000/swagger`

### Rotas de verificação da Etapa 1

| Rota | Retorno |
|---|---|
| `GET /api/status` | Confirma a conexão com o banco configurado |
| `GET /api/modelo` | Lista as tabelas mapeadas com suas chaves primárias e estrangeiras |

---

## 5. Carga inicial

O `ApplicationContext` já popula, via `HasData`, os dados fixos de apoio:

- **Fabricantes:** Volkswagen, Fiat, Toyota e Chevrolet
- **Categorias:** Econômico, Intermediário, SUV e Executivo

Os registros de veículos, clientes, funcionários e aluguéis serão inseridos na Etapa 2.

---

## 6. Próximas etapas

| Etapa | Escopo |
|---|---|
| 2 | Criação de registros, consultas com joins e endpoints CRUD |
| 3 | Testes e documentação das APIs no Swagger |
| 4 | Vídeo de apresentação (pitch) |
