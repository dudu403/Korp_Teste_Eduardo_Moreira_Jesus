# 🧾 Korp NF-e System

Sistema de emissão de notas fiscais integrado a um controle de estoque, desenvolvido com arquitetura de microserviços.

---

## 📌 Descrição do Problema

O projeto tem como objetivo simular um sistema onde dois serviços precisam trabalhar de forma integrada:

* Um serviço responsável pelo **controle de estoque de produtos**
* Um serviço responsável pela **emissão de notas fiscais**

Durante o funcionamento do sistema:

* Produtos são cadastrados com quantidade em estoque
* Notas fiscais são criadas contendo produtos e quantidades
* Ao fechar uma nota fiscal, o sistema deve reduzir o estoque dos produtos utilizados
* O sistema precisa garantir consistência mesmo com múltiplas operações simultâneas
* Também deve lidar com falhas de comunicação entre os serviços

---

## 🏗️ Arquitetura

O sistema é composto por:

### 📦 EstoqueService

* Gerencia produtos
* Controla saldo em estoque
* Realiza baixa de estoque

### 🧾 FaturamentoService

* Gerencia notas fiscais
* Controla status das notas
* Se comunica com o serviço de estoque

### 🌐 Frontend (Angular)

* Interface do usuário
* Consome as APIs
* Exibe dados e feedbacks

---

## ⚙️ Tecnologias Utilizadas

### Backend

* .NET / C#
* Entity Framework Core
* SQL Server
* FluentValidation
* HttpClient

### Frontend

* Angular
* RxJS
* Signals
* TailwindCSS
* Angular Material
* ngx-toastr

---

# 🚀 Como rodar o projeto

## 🔧 Pré-requisitos

Antes de iniciar, você precisa ter instalado:

* .NET SDK (versão compatível com o projeto)
* Node.js
* Angular CLI
* SQL Server

---

## 📦 Instalar Angular CLI

```bash
npm install -g @angular/cli
```

---

## 🗄️ Configuração do Banco de Dados

### 1. Criar os bancos

Abra o SQL Server e execute:

```sql
CREATE DATABASE EstoqueDb;
CREATE DATABASE FaturamentoDb;
```

---

### 2. Configurar Connection String

Abra os arquivos:

* `EstoqueService/appsettings.json`
* `FaturamentoService/appsettings.json`

Altere a connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;"
}
```

### Exemplo:

```json
Server=SEU_SERVIDOR;Database=EstoqueDb;Trusted_Connection=True;TrustServerCertificate=True
```

⚠️ Faça o mesmo para o banco `FaturamentoDb`

---

### 3. Aplicar migrations

No terminal, dentro de cada projeto:

#### EstoqueService

```bash
dotnet ef database update
```

#### FaturamentoService

```bash
dotnet ef database update
```

---

## ▶️ Rodar o Backend

Abra dois terminais:

### 1. EstoqueService

```bash
cd EstoqueService
dotnet run
```

---

### 2. FaturamentoService

```bash
cd FaturamentoService
dotnet run
```

---

⚠️ Observe as portas que cada serviço está rodando (ex: 7093, 7250)

---

## 🌐 Rodar o Frontend

```bash
cd korp-frontend
npm install
ng serve
```

Acesse:

```
http://localhost:4200
```

---

## 🔗 Configurar URLs da API no Frontend

Verifique os arquivos de service:

### produtos.service.ts

```ts
private apiUrl = 'https://localhost:7093/api/produtos';
```

### notas-fiscais.service.ts

```ts
private apiUrl = 'https://localhost:7250/api/notasfiscais';
```

Ajuste conforme as portas do seu backend.

---

## 🧪 Execução

Após subir tudo:

1. Acesse o sistema no navegador
2. Cadastre produtos
3. Crie uma nota fiscal
4. Adicione produtos
5. Finalize a nota

---

## ⚠️ Observações

* Ambos os serviços precisam estar rodando ao mesmo tempo
* O banco deve estar configurado corretamente
* As portas do frontend devem bater com as do backend
* Caso algum serviço esteja fora, o sistema apresentará erro de conexão

---
