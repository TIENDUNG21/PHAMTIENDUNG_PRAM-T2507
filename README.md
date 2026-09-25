# BattleGame – Developing Microsoft Azure Solutions (SET01)

Student: Pham Tien Dung – PRAM-T2507

| Requirement | Location |
|-------------|----------|
| Create database (Database First) | [Database/BattleGame.sql](Database/BattleGame.sql) – database, tables, stored procedures, sample data |
| Q1 API `registerplayer` | [BattleGame.Functions/Functions/RegisterPlayerFunction.cs](BattleGame.Functions/Functions/RegisterPlayerFunction.cs) |
| Q2 API `createasset` | [BattleGame.Functions/Functions/CreateAssetFunction.cs](BattleGame.Functions/Functions/CreateAssetFunction.cs) |
| Q3 API `getassetsbyplayer` | [BattleGame.Functions/Functions/GetAssetsByPlayerFunction.cs](BattleGame.Functions/Functions/GetAssetsByPlayerFunction.cs) |
| Q4 Website (ReactJS) | [battlegame-web/](battlegame-web/) |
| Q5 Deployment document | [DEPLOY.md](DEPLOY.md) |
| Extra API `addplayerasset` | [BattleGame.Functions/Functions/AddPlayerAssetFunction.cs](BattleGame.Functions/Functions/AddPlayerAssetFunction.cs) – fills the `PlayerAsset` table |

## Tech stack

- Azure Functions v4, C# .NET 8 isolated worker, HTTP triggers
- SQL Server (LocalDB locally / Azure SQL in cloud), ADO.NET + stored procedures
- ReactJS 18 + Vite

## Project structure

```
Database/BattleGame.sql          Database First script
BattleGame.Functions/
  Functions/                     HTTP-triggered Azure Functions (one class per API)
  Data/                          Repository (data access via stored procedures)
  Models/                        Entities, request DTOs and report model
  Helpers/                       JSON request helper
  Program.cs                     Host + dependency injection
battlegame-web/                  React website showing the report table
DEPLOY.md                        Deployment tutorial
```

## Run locally

### 1. Database

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Database/BattleGame.sql
```

### 2. Azure Functions (http://localhost:7071)

Connection string is in `BattleGame.Functions/local.settings.json` (`SqlConnectionString`).

```bash
cd BattleGame.Functions
func start
```

### 3. Website (http://localhost:5173)

```bash
cd battlegame-web
npm install
npm run dev
```

## API reference

| Method | URL | Body / Query | Response |
|--------|-----|--------------|----------|
| POST | `/api/registerplayer` | `{ "playerName", "fullName", "age", "level", "email" }` | `201` created player, `400` invalid, `409` duplicate name |
| POST | `/api/createasset` | `{ "assetName", "levelRequire" }` | `201` created asset, `400` invalid |
| GET | `/api/getassetsbyplayer` | optional `?playerName=Player 1` | `200` list of `{ no, playerName, level, age, assetName }` |
| POST | `/api/addplayerasset` | `{ "playerId", "assetId" }` | `201`, `404` unknown ids, `409` already owned |

Example report response:

```json
[
  { "no": 1, "playerName": "Player 1", "level": 10, "age": "20", "assetName": "Hero 1" },
  { "no": 2, "playerName": "Player 2", "level": 3,  "age": "19", "assetName": "Hero 2" },
  { "no": 3, "playerName": "Player 3", "level": 10, "age": "23", "assetName": "Hero 1" }
]
```
