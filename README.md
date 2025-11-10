# Configuração do Backend ASP.NET Core com EF Core

## 📦 Pacotes NuGet Necessários

Execute os seguintes comandos no terminal dentro da pasta `EstoqueBackEnd`:

```bash
# Entity Framework Core para PostgreSQL
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.0

# Entity Framework Core Tools (para migrations)
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 9.0.0

# Entity Framework Core Design (para migrations)
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.0

# Swagger/OpenAPI
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
```

## ⚙️ Configuração

### 1. Atualizar Connection String

Edite o arquivo `appsettings.json` e atualize a connection string com suas credenciais do PostgreSQL:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=mespin_stock;Username=seu_usuario;Password=sua_senha"
  }
}
```

### 2. Verificar o Banco de Dados

Certifique-se de que você já executou o script SQL para criar o banco:

```bash
psql -U postgres -d mespin_stock -f ../EstoqueFrontEnd/database/schema_postgresql.sql
```

### 3. Sincronizar o EF Core com o Banco Existente

Como o banco de dados já existe, você tem duas opções:

#### Opção A: Gerar migration a partir do banco existente (Scaffold)

```bash
dotnet ef dbcontext scaffold "Host=localhost;Port=5432;Database=mespin_stock;Username=postgres;Password=senha" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c AppDbContext --context-dir Data --force
```

#### Opção B: Criar migration inicial (Recomendado)

```bash
# Criar a migration inicial
dotnet ef migrations add InitialCreate

# Não aplicar a migration! O banco já existe
# Se quiser marcar a migration como aplicada sem executar:
dotnet ef database update --no-build
```

## 🚀 Executar a Aplicação

```bash
dotnet run
```

A API estará disponível em:
- HTTPS: https://localhost:7xxx
- HTTP: http://localhost:5xxx
- Swagger UI: https://localhost:7xxx/swagger

## 📝 Estrutura Criada

```
EstoqueBackEnd/
├── Models/                       # Entidades do banco de dados
│   ├── Setting.cs
│   ├── Customer.cs
│   ├── Product.cs
│   ├── Material.cs
│   ├── ProductionMaterial.cs
│   ├── PriceHistory.cs
│   ├── Sale.cs
│   ├── SaleItem.cs
│   ├── Order.cs
│   ├── CategoryPrice.cs
│   ├── Expense.cs
│   ├── InstallmentPayment.cs
│   └── InstallmentPaymentStatus.cs
├── Data/
│   └── AppDbContext.cs           # Contexto do EF Core
├── Controllers/                  # Controllers da API (criar)
├── DTOs/                        # Data Transfer Objects (criar)
├── Services/                    # Serviços de negócio (criar)
├── Program.cs                   # Configuração da aplicação
└── appsettings.json            # Configurações e connection string
```

## 🔧 Próximos Passos

1. **Instalar os pacotes NuGet** (comandos acima)
2. **Configurar a connection string**
3. **Testar a conexão com o banco**
4. **Criar os Controllers** (próxima etapa)

## 🐛 Troubleshooting

### Erro de conexão com PostgreSQL

Verifique se:
- O PostgreSQL está rodando: `sudo service postgresql status` (Linux) ou Services (Windows)
- As credenciais estão corretas
- O banco `mespin_stock` existe: `psql -U postgres -l`

### Erro de pacotes não encontrados

Execute na pasta do projeto:
```bash
dotnet restore
dotnet build
```

### Porta já em uso

Mude as portas em `Properties/launchSettings.json` ou use:
```bash
dotnet run --urls "https://localhost:7000;http://localhost:5000"
```

## 📚 Documentação

- [EF Core com PostgreSQL](https://www.npgsql.org/efcore/)
- [ASP.NET Core Web API](https://learn.microsoft.com/aspnet/core/web-api/)
- [Swagger/OpenAPI](https://learn.microsoft.com/aspnet/core/tutorials/web-api-help-pages-using-swagger)
