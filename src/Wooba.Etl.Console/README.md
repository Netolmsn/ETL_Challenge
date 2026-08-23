# Wooba ETL - Ferramenta de Tratamento e Ingestão de Clientes

Ferramenta desenvolvida em **.NET 10** para leitura, sanitização, log de descartes e ingestão de dados de clientes a partir de arquivos CSV em banco de dados **SQLite em memória**.

---

## Tecnologias e Arquitetura

- **Linguagem & Framework:** .NET 10 (C#)
- **Banco de Dados:** SQLite em memória (`Data Source=:memory:`) com driver `Microsoft.Data.Sqlite`
- **Injeção de Dependência:** `Microsoft.Extensions.DependencyInjection`
- **Arquitetura & Design:**
  - **SOLID:** Separação estrita entre Leitura, Processamento/Regras e Repositório.
  - **Rich Domain Model:** Entidade `Cliente` encapsulada contra estados inválidos.
  - **SQL Puro:** Operações CRUD executadas via ADO.NET (sem Entity Framework).

---

## Requisitos do Edital Atendidos

1. **Leitura de CSV:** Streaming assíncrono do arquivo de entrada[cite: 1].
2. **Tratamento & Sanitização:**
   - Remoção de espaços extras (*Trim*)[cite: 1].
   - Padronização de datas multi-formato (`dd/MM/yyyy`, `yyyy-MM-dd`, etc.)[cite: 1].
   - Descarte de nomes vazios ou e-mails em formatos inválidos (sem `@`, `@` duplo ou espaços internos)[cite: 1].
   - Deduplicação por e-mail (*case-insensitive*)[cite: 1].
   - Registro detalhado de logs de linhas descartadas com motivo[cite: 1].
3. **Persistência SQLite Persistente:** Conexão aberta durante todo o ciclo de vida da aplicação para manter os dados em RAM[cite: 1].
4. **4 Operações SQL:** Inserção em lote, consulta com resumo, atualização e exclusão por ID[cite: 1].

---

##  Como Executar o Projeto

### Pré-requisitos
- [.NET 10 SDK](https://dotnet.microsoft.com/) instalado.

### Passo a Passo

1. **Clonar o Repositório:**
   ```bash
   git clone https://github.com/Netolmsn/ETL_Challenge
   cd Wooba.Etl

2. **Restaurar Dependências**
   dotnet restore

3. **Executar a Aplicação**
   dotnet run --project src/Wooba.Etl.Console/Wooba.Etl.Console.csproj

## Como Testar as Operações do Banco de Dados

Ao rodar o comando acima, o sistema executará o pipeline ETL carregando o arquivo clientes_lote_b.csv.
Em seguida, exibirá o Resumo da Execução e abrirá um menu interativo:

1. Consultar todos os clientes (SELECT): Exibe a lista formatada no console com status Revisado e Cidade/UF.

2. Atualizar status de revisão (UPDATE): Digite o ID do cliente para alterá-lo para Revisado = SIM.

3. Excluir cliente (DELETE): Digite o ID do cliente para removê-lo da base.

0. Sair: Encerra a aplicação e libera a conexão do SQLite em memória.