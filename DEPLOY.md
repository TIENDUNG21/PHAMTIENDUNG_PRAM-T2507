# Tutorial: Deploy the BattleGame Azure Function to Azure Cloud

This guide explains how to deploy the `BattleGame.Functions` project (.NET 8 isolated worker) and the
`BATTLEGAME` database to Microsoft Azure, then connect the React website to it.

---

## 1. Prerequisites

| Tool | Purpose |
|------|---------|
| Azure subscription | Hosts the resources (Azure for Students works) |
| .NET 8 SDK | Builds the Function project |
| Azure Functions Core Tools v4 | `npm i -g azure-functions-core-tools@4` |
| Azure CLI | `winget install Microsoft.AzureCLI` |
| Visual Studio 2022 / VS Code + Azure Functions extension | Optional, for GUI deployment |

Log in to Azure:

```bash
az login
```

---

## 2. Create the Azure resources

Set some variables (change the names so they are globally unique):

```bash
RG=rg-battlegame
LOCATION=southeastasia
SQL_SERVER=battlegame-sql-<yourname>
SQL_ADMIN=sqladmin
SQL_PASSWORD='<StrongPassword123!>'
STORAGE=battlegamestore<yourname>
FUNC_APP=battlegame-func-<yourname>
```

### 2.1 Resource group

```bash
az group create --name $RG --location $LOCATION
```

### 2.2 Azure SQL Database

```bash
az sql server create --name $SQL_SERVER --resource-group $RG --location $LOCATION \
  --admin-user $SQL_ADMIN --admin-password $SQL_PASSWORD

az sql db create --resource-group $RG --server $SQL_SERVER --name BATTLEGAME \
  --service-objective Basic

# Allow Azure services (the Function App) to access the server
az sql server firewall-rule create --resource-group $RG --server $SQL_SERVER \
  --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# Allow your own machine to run the script
az sql server firewall-rule create --resource-group $RG --server $SQL_SERVER \
  --name AllowMyIp --start-ip-address <your-public-ip> --end-ip-address <your-public-ip>
```

### 2.3 Create tables and stored procedures

Azure SQL does not support `USE <database>` or `CREATE DATABASE` inside the script,
so remove the first block (`CREATE DATABASE` ... `USE BATTLEGAME; GO`) from `Database/BattleGame.sql`
and run the rest against the `BATTLEGAME` database:

```bash
sqlcmd -S $SQL_SERVER.database.windows.net -d BATTLEGAME -U $SQL_ADMIN -P $SQL_PASSWORD \
  -i Database/BattleGame.sql
```

(Alternative: open the database in the Azure Portal → **Query editor** and paste the script.)

### 2.4 Storage account and Function App

```bash
az storage account create --name $STORAGE --resource-group $RG --location $LOCATION --sku Standard_LRS

az functionapp create --name $FUNC_APP --resource-group $RG \
  --storage-account $STORAGE --consumption-plan-location $LOCATION \
  --runtime dotnet-isolated --runtime-version 8 --functions-version 4 --os-type Windows
```

> Portal alternative: **Create a resource → Function App** → Runtime stack `.NET`,
> Version `8 (LTS) isolated`, Hosting `Consumption`, then **Review + create**.

---

## 3. Configure application settings

The code reads the connection string from the app setting `SqlConnectionString`
(locally it comes from `local.settings.json`, which is **not** published).

```bash
az functionapp config appsettings set --name $FUNC_APP --resource-group $RG --settings \
  "SqlConnectionString=Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=BATTLEGAME;User ID=$SQL_ADMIN;Password=$SQL_PASSWORD;Encrypt=True;TrustServerCertificate=False;"
```

Enable CORS so the website can call the API:

```bash
az functionapp cors add --name $FUNC_APP --resource-group $RG --allowed-origins "*"
```

(Use the real website URL instead of `*` in production.)

---

## 4. Deploy the Function code

Choose **one** of the following ways.

### Option A – Azure Functions Core Tools (command line)

```bash
cd BattleGame.Functions
func azure functionapp publish $FUNC_APP
```

At the end the tool prints the invoke URLs, for example:

```
Functions in battlegame-func-<yourname>:
    addplayerasset    - [httpTrigger] https://battlegame-func-<yourname>.azurewebsites.net/api/addplayerasset
    createasset       - [httpTrigger] https://battlegame-func-<yourname>.azurewebsites.net/api/createasset
    getassetsbyplayer - [httpTrigger] https://battlegame-func-<yourname>.azurewebsites.net/api/getassetsbyplayer
    registerplayer    - [httpTrigger] https://battlegame-func-<yourname>.azurewebsites.net/api/registerplayer
```

### Option B – Visual Studio 2022

1. Open `BattleGame.Functions.csproj`.
2. Right-click the project → **Publish** → Target **Azure** → **Azure Function App (Windows)**.
3. Sign in, select the Function App created in step 2.4 (or create a new one here).
4. Click **Finish** → **Publish**.
5. In the publish profile, open **Hosting → Manage Azure App Service settings** and add `SqlConnectionString`.

### Option C – VS Code

1. Install the **Azure Functions** extension and sign in to Azure.
2. Open the `BattleGame.Functions` folder.
3. Press `F1` → **Azure Functions: Deploy to Function App...** → choose the Function App.
4. Add `SqlConnectionString` via **Azure: Functions → Application Settings → Add New Setting**.

---

## 5. Test the deployed APIs

```bash
BASE=https://$FUNC_APP.azurewebsites.net/api

# 1. Register a player
curl -X POST $BASE/registerplayer -H "Content-Type: application/json" \
  -d '{"playerName":"Player 4","fullName":"Pham Van D","age":"21","level":5,"email":"p4@battlegame.com"}'

# 2. Create an asset
curl -X POST $BASE/createasset -H "Content-Type: application/json" \
  -d '{"assetName":"Hero 3","levelRequire":2}'

# 3. Give the asset to the player (ids come from the two responses above)
curl -X POST $BASE/addplayerasset -H "Content-Type: application/json" \
  -d '{"playerId":"<playerId>","assetId":"<assetId>"}'

# 4. Report
curl $BASE/getassetsbyplayer
curl "$BASE/getassetsbyplayer?playerName=Player%201"
```

Logs can be viewed in the Azure Portal → Function App → **Functions → <function> → Monitor**
or with `func azure functionapp logstream $FUNC_APP`.

---

## 6. Deploy the React website (optional)

1. Point the website at the cloud API:

   ```bash
   cd battlegame-web
   echo VITE_API_BASE_URL=https://$FUNC_APP.azurewebsites.net/api > .env.production
   npm install
   npm run build
   ```

2. Host the `dist` folder, e.g. with **Azure Static Web Apps**:

   ```bash
   npm i -g @azure/static-web-apps-cli
   swa deploy ./dist --env production
   ```

   or with a Storage Account static website:

   ```bash
   az storage blob service-properties update --account-name $STORAGE --static-website --index-document index.html
   az storage blob upload-batch --account-name $STORAGE -s ./dist -d '$web'
   ```

3. Add the website URL to the Function App CORS list (step 3).

---

## 7. Clean up

```bash
az group delete --name $RG --yes --no-wait
```
