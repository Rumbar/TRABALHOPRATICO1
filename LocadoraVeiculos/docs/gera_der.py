"""Gera o diagrama entidade-relacionamento do banco LocadoraVeiculos."""

LINHA = 22
CAB = 34
PAD = 10

ENTIDADES = {
    "Fabricante": dict(x=40, y=60, w=300, attrs=[
        ("PK", "FabricanteId", "int"),
        ("UQ", "Nome", "varchar(80)"),
        ("", "PaisOrigem", "varchar(60)"),
        ("", "AnoFundacao", "int"),
    ]),
    "Categoria": dict(x=1160, y=60, w=310, attrs=[
        ("PK", "CategoriaId", "int"),
        ("UQ", "Nome", "varchar(50)"),
        ("", "Descricao", "varchar(200)"),
        ("", "PercentualAjuste", "decimal(5,2)"),
    ]),
    "Veiculo": dict(x=590, y=40, w=330, attrs=[
        ("PK", "VeiculoId", "int"),
        ("UQ", "Placa", "char(8)"),
        ("", "Modelo", "varchar(80)"),
        ("", "AnoFabricacao", "int"),
        ("", "Quilometragem", "int"),
        ("", "Cor", "varchar(30)"),
        ("", "ValorDiaria", "decimal(10,2)"),
        ("", "Status", "int"),
        ("FK", "FabricanteId", "int"),
        ("FK", "CategoriaId", "int"),
    ]),
    "Cliente": dict(x=40, y=520, w=300, attrs=[
        ("PK", "ClienteId", "int"),
        ("", "Nome", "varchar(120)"),
        ("UQ", "Cpf", "char(11)"),
        ("UQ", "Email", "varchar(120)"),
        ("", "Telefone", "varchar(20)"),
        ("", "DataNascimento", "date"),
        ("", "NumeroCnh", "varchar(11)"),
        ("", "CategoriaCnh", "varchar(3)"),
        ("", "DataCadastro", "date"),
    ]),
    "Funcionario": dict(x=1160, y=520, w=310, attrs=[
        ("PK", "FuncionarioId", "int"),
        ("", "Nome", "varchar(120)"),
        ("UQ", "Cpf", "char(11)"),
        ("UQ", "Matricula", "varchar(20)"),
        ("", "Cargo", "varchar(60)"),
        ("", "DataAdmissao", "date"),
    ]),
    "Aluguel": dict(x=560, y=470, w=390, attrs=[
        ("PK", "AluguelId", "int"),
        ("FK", "ClienteId", "int"),
        ("FK", "VeiculoId", "int"),
        ("FK", "FuncionarioId", "int"),
        ("", "DataRetirada", "datetime2"),
        ("", "DataPrevistaDevolucao", "datetime2"),
        ("", "DataDevolucao", "datetime2 NULL"),
        ("", "QuilometragemInicial", "int"),
        ("", "QuilometragemFinal", "int NULL"),
        ("", "ValorDiaria", "decimal(10,2)"),
        ("", "ValorTotal", "decimal(10,2)"),
        ("", "Status", "int"),
        ("", "Observacao", "varchar(300)"),
    ]),
}

AZUL = "#1F3864"
AZUL_CAB = "#2F5597"
CINZA = "#D9D9D9"
LINHA_COR = "#7F7F7F"


def altura(nome):
    return CAB + len(ENTIDADES[nome]["attrs"]) * LINHA + PAD


def caixa(nome):
    e = ENTIDADES[nome]
    x, y, w = e["x"], e["y"], e["w"]
    h = altura(nome)
    s = []
    s.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="6" '
             f'fill="#FFFFFF" stroke="{AZUL}" stroke-width="2"/>')
    s.append(f'<path d="M{x},{y+CAB} h{w} v-{CAB-6} a6,6 0 0 0 -6,-6 h-{w-12} a6,6 0 0 0 -6,6 z" '
             f'fill="{AZUL_CAB}"/>')
    s.append(f'<text x="{x + w/2}" y="{y + 23}" font-family="Arial" font-size="17" '
             f'font-weight="bold" fill="#FFFFFF" text-anchor="middle">{nome}</text>')

    ty = y + CAB + 16
    for marca, campo, tipo in e["attrs"]:
        cor = AZUL if marca in ("PK", "FK") else "#333333"
        peso = "bold" if marca == "PK" else "normal"
        if marca:
            s.append(f'<text x="{x + 10}" y="{ty}" font-family="Arial" font-size="10" '
                     f'font-weight="bold" fill="{AZUL_CAB}">{marca}</text>')
        s.append(f'<text x="{x + 38}" y="{ty}" font-family="Arial" font-size="13" '
                 f'font-weight="{peso}" fill="{cor}">{campo}</text>')
        s.append(f'<text x="{x + w - 10}" y="{ty}" font-family="Arial" font-size="11" '
                 f'fill="#808080" text-anchor="end">{tipo}</text>')
        ty += LINHA
    return "\n".join(s)


def rel(pontos, rot_a, rot_b, pa, pb):
    d = " ".join(f"{'M' if i == 0 else 'L'}{x},{y}" for i, (x, y) in enumerate(pontos))
    s = [f'<path d="{d}" fill="none" stroke="{LINHA_COR}" stroke-width="2"/>']
    s.append(f'<text x="{pa[0]}" y="{pa[1]}" font-family="Arial" font-size="14" '
             f'font-weight="bold" fill="{AZUL}">{rot_a}</text>')
    s.append(f'<text x="{pb[0]}" y="{pb[1]}" font-family="Arial" font-size="14" '
             f'font-weight="bold" fill="{AZUL}">{rot_b}</text>')
    return "\n".join(s)


partes = []
partes.append('<rect x="0" y="0" width="1510" height="900" fill="#FFFFFF"/>')

# Fabricante (1) -> Veiculo (N)
partes.append(rel([(340, 160), (465, 160), (465, 190), (590, 190)],
                  "1", "N", (348, 152), (566, 182)))
# Categoria (1) -> Veiculo (N)
partes.append(rel([(1160, 160), (1040, 160), (1040, 190), (920, 190)],
                  "1", "N", (1140, 152), (928, 182)))
# Veiculo (1) -> Aluguel (N)
partes.append(rel([(755, 40 + altura("Veiculo")), (755, 470)],
                  "1", "N", (765, 40 + altura("Veiculo") + 18), (765, 462)))
# Cliente (1) -> Aluguel (N)
partes.append(rel([(340, 640), (450, 640), (450, 610), (560, 610)],
                  "1", "N", (348, 632), (536, 602)))
# Funcionario (1) -> Aluguel (N)
partes.append(rel([(1160, 640), (1055, 640), (1055, 610), (950, 610)],
                  "1", "N", (1140, 632), (958, 602)))

for nome in ENTIDADES:
    partes.append(caixa(nome))

partes.append('<text x="40" y="862" font-family="Arial" font-size="13" fill="#595959">'
              'PK = chave primaria | FK = chave estrangeira | UQ = restricao de unicidade'
              '</text>')
partes.append('<text x="1470" y="862" font-family="Arial" font-size="13" fill="#595959" '
              'text-anchor="end">Banco: LocadoraVeiculos (SQL Server)</text>')

svg = ('<svg xmlns="http://www.w3.org/2000/svg" width="1510" height="900" '
       'viewBox="0 0 1510 900">\n' + "\n".join(partes) + "\n</svg>\n")

with open("/home/claude/LocadoraVeiculos/docs/DER.svg", "w", encoding="utf-8") as f:
    f.write(svg)

print("DER.svg gerado")
