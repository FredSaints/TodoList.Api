# To-Do List API

API RESTful para gestão de tarefas desenvolvida em .NET 8.0 com SQL Server e ADO.NET.

## 📋 Descrição

API de backend para uma ferramenta interna de produtividade que permite aos colaboradores registrar, consultar, atualizar e remover tarefas do dia a dia.

## 🔧 Tecnologias Utilizadas

- .NET 8.0
- SQL Server
- ADO.NET (sem Entity Framework)
- FluentValidation
- Serilog
- Swagger/OpenAPI
- xUnit (testes unitários)

## 📦 Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express ou superior)
- Editor de código (Visual Studio, VS Code, Rider)

## 🚀 Setup do Projeto

### 1. Clonar o Repositório

```bash
git clone <url-do-repositorio>
cd TodoList.Api
```

### 2. Configurar Base de Dados

#### 2.1. Criar a Base de Dados e Tabela

Execute o script SQL localizado em `SqlScripts/db_And_Table_Creation.sql`:

```bash
sqlcmd -S localhost -i SqlScripts/db_And_Table_Creation.sql
```

Ou execute manualmente no SQL Server Management Studio.

#### 2.2. Criar as Stored Procedures

Execute os seguintes scripts na ordem:

```bash
sqlcmd -S localhost -d TodoListDB -i SqlScripts/sp_GetAllTasks.sql
sqlcmd -S localhost -d TodoListDB -i SqlScripts/sp_GetTaskById.sql
sqlcmd -S localhost -d TodoListDB -i SqlScripts/sp_CreateTask.sql
sqlcmd -S localhost -d TodoListDB -i SqlScripts/sp_UpdateTask.sql
sqlcmd -S localhost -d TodoListDB -i SqlScripts/sp_DeleteTask.sql
```

### 3. Configurar Connection String

Edite o arquivo `appsettings.json` e ajuste a connection string conforme seu ambiente:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TodoListDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Restaurar Dependências

```bash
dotnet restore
```

### 5. Executar o Projeto

```bash
dotnet run
```

A API estará disponível em:
- HTTPS: `https://localhost:7193`
- HTTP: `http://localhost:5219`
- Swagger UI: `https://localhost:7193/swagger`

## 📚 Endpoints da API

### 1. Listar Todas as Tarefas

**GET** `/tasks`

Lista todas as tarefas com paginação e filtro opcional.

**Parâmetros de Query:**
- `pageNumber` (opcional): Número da página (padrão: 1)
- `pageSize` (opcional): Itens por página (padrão: 20, máximo: 100)
- `completed` (opcional): Filtrar por status de conclusão (true/false)

**Exemplo de Requisição:**

```bash
curl -X GET "https://localhost:7193/tasks?pageNumber=1&pageSize=20" -k
```

**Exemplo com Filtro:**

```bash
curl -X GET "https://localhost:7193/tasks?completed=true" -k
```

**Resposta (200 OK):**

```json
{
  "tasks": [
    {
      "id": 1,
      "title": "Estudar .NET",
      "description": "Completar curso de ASP.NET Core",
      "createDate": "2025-11-06T20:00:00",
      "isCompleted": false
    },
    {
      "id": 2,
      "title": "Revisar código",
      "description": "Code review do PR #123",
      "createDate": "2025-11-06T21:30:00",
      "isCompleted": true
    }
  ],
  "totalCount": 2,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### 2. Obter Tarefa por ID

**GET** `/tasks/{id}`

Retorna os detalhes de uma tarefa específica.

**Exemplo de Requisição:**

```bash
curl -X GET "https://localhost:7193/tasks/1" -k
```

**Resposta (200 OK):**

```json
{
  "id": 1,
  "title": "Estudar .NET",
  "description": "Completar curso de ASP.NET Core",
  "createDate": "2025-11-06T20:00:00",
  "isCompleted": false
}
```

**Resposta (404 Not Found):**

```json
{
  "message": "Task with ID 999 not found"
}
```

### 3. Criar Nova Tarefa

**POST** `/tasks`

Cria uma nova tarefa.

**Corpo da Requisição:**

```json
{
  "title": "Nova tarefa",
  "description": "Descrição da tarefa"
}
```

**Exemplo de Requisição:**

```bash
curl -X POST "https://localhost:7193/tasks" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Estudar .NET",
    "description": "Completar curso de ASP.NET Core"
  }' -k
```

**Resposta (201 Created):**

```json
{
  "message": "Task created successfully",
  "task": {
    "id": 1,
    "title": "Estudar .NET",
    "description": "Completar curso de ASP.NET Core",
    "createDate": "2025-11-06T20:00:00",
    "isCompleted": false
  }
}
```

**Resposta (400 Bad Request) - Validação:**

```json
{
  "Title": [
    "Title must be between 3 and 100 characters"
  ]
}
```

### 4. Atualizar Tarefa

**PUT** `/tasks/{id}`

Atualiza uma tarefa existente.

**Corpo da Requisição:**

```json
{
  "title": "Título atualizado",
  "description": "Descrição atualizada",
  "isCompleted": true
}
```

**Exemplo de Requisição:**

```bash
curl -X PUT "https://localhost:7193/tasks/1" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Estudar .NET - Completo",
    "description": "Curso finalizado",
    "isCompleted": true
  }' -k
```

**Resposta (200 OK):**

```json
{
  "message": "Task updated successfully"
}
```

**Resposta (404 Not Found):**

```json
{
  "message": "Task with ID 999 not found"
}
```

### 5. Remover Tarefa

**DELETE** `/tasks/{id}`

Remove uma tarefa permanentemente.

**Exemplo de Requisição:**

```bash
curl -X DELETE "https://localhost:7193/tasks/1" -k
```

**Resposta (200 OK):**

```json
{
  "message": "Task removed successfully"
}
```

**Resposta (404 Not Found):**

```json
{
  "message": "Task with ID 999 not found"
}
```

## 🧪 Executar Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar com Cobertura

```bash
dotnet test /p:CollectCoverage=true
```

## 📝 Regras de Negócio

1. **Título obrigatório**: Deve ter entre 3 e 100 caracteres
2. **Descrição opcional**: Pode ser nula ou vazia
3. **Nova tarefa sempre pendente**: `isCompleted` inicia como `false`
4. **Exclusão definitiva**: Tarefas removidas são permanentemente apagadas
5. **Data de criação automática**: Definida pelo sistema no momento da criação

## 🔒 Validações

- **Título vazio**: Retorna 400 Bad Request
- **Título menor que 3 caracteres**: Retorna 400 Bad Request
- **Título maior que 100 caracteres**: Retorna 400 Bad Request
- **ID inexistente**: Retorna 404 Not Found
- **Parâmetros de paginação inválidos**: Retorna 400 Bad Request

## 📂 Estrutura do Projeto

```
TodoList.Api/
├── Controllers/
│   └── TasksController.cs          # Endpoints REST
├── Data/
│   ├── Interfaces/
│   │   ├── IDatabase.cs            # Interface para conexão DB
│   │   └── ITaskRepository.cs      # Interface para operações CRUD
│   └── Implementations/
│       ├── Database.cs              # Gestão de conexões ADO.NET
│       └── TaskRepository.cs        # Operações na base de dados
├── Dtos/
│   ├── TaskItemCreateDto.cs        # DTO para criação
│   └── TaskItemUpdateDto.cs        # DTO para atualização
├── Models/
│   ├── TaskItem.cs                 # Modelo de domínio
│   └── ResponseModels.cs           # Modelos de resposta
├── Validators/
│   ├── CreateTaskRequestValidator.cs
│   └── UpdateTaskRequestValidator.cs
├── SqlScripts/
│   ├── db_And_Table_Creation.sql   # Script de criação
│   ├── sp_CreateTask.sql
│   ├── sp_DeleteTask.sql
│   ├── sp_GetAllTasks.sql
│   ├── sp_GetTaskById.sql
│   └── sp_UpdateTask.sql
├── appsettings.json                # Configurações
└── Program.cs                      # Configuração da aplicação
```

## 📊 Features Implementadas

### Requisitos Obrigatórios
✅ Web API REST em .NET 8.0  
✅ Testes Unitários  
✅ Scripts SQL Server  
✅ Comunicação via ADO.NET (sem EF)  
✅ Documentação Swagger  

### Features Opcionais
✅ Filtro por status (completed=true/false)  
✅ Paginação completa  
✅ Stored Procedures  
✅ Log de erros em ficheiro (Serilog)  
✅ Projeto no GitHub  

## 🔍 Logs

Os logs da aplicação são guardados em:
```
Logs/log-YYYYMMDD.txt
```

Formato do log:
```
2025-11-06 20:30:45.123 +00:00 [INF] Task 1 created successfully
2025-11-06 20:31:12.456 +00:00 [WRN] Task 999 not found
2025-11-06 20:31:45.789 +00:00 [ERR] Error creating task
```

## 🛠️ Troubleshooting

### Erro de Conexão à Base de Dados

```
Unable to connect to the database
```

**Solução:**
1. Verifique se o SQL Server está em execução
2. Confirme a connection string em `appsettings.json`
3. Teste a conexão com: `sqlcmd -S localhost -Q "SELECT 1"`

### Stored Procedures Não Encontradas

```
Could not find stored procedure 'sp_GetAllTasks'
```

**Solução:**
Execute todos os scripts em `SqlScripts/` na base de dados `TodoListDB`.

### Porta Já Em Uso

```
Failed to bind to address https://localhost:7193
```

**Solução:**
Altere as portas em `Properties/launchSettings.json` ou encerre o processo que está a usar a porta.

## 📖 Documentação API

Aceda à documentação interativa Swagger em:

```
https://localhost:7193/swagger
```

A documentação inclui:
- Descrição de todos os endpoints
- Schemas de request/response
- Possibilidade de testar diretamente no browser

## 👥 Autor

Projeto desenvolvido como desafio técnico para demonstração de competências em .NET 8.0, SQL Server e ADO.NET.

## 📄 Licença

Este projeto é de uso educacional e demonstrativo.
