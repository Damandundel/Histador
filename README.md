# Histador

Histador is a history learning game for the web. It's like Duolingo, but for history.

- **Learn:** short lessons (5–15 minutes) with only the important material and the best facts. Each lesson ends with a quiz.
- **Discover:** the world map starts covered in fog of war. Passing a lesson's quiz clears fog over every country involved. Big events like WWII clear more than small ones, and better grades clear more. Your account level is the % of the world you've discovered.
- **Duel:** live quiz duels (Triviador-style) against friends or random opponents. Duels earn MMR, MMR sets your rank (Peasant → Soldier → General → King/Queen → God/Goddess), and ranks unlock historical-figure player icons.
- **Random:** start any lesson on the platform at random.

The app is in Bulgarian and English, for ages 14+. It's built with ASP.NET Core MVC (.NET 8), EF Core + SQL Server, and SignalR.

The full design is in [docs/superpowers/specs/2026-11-13-histador-design.md](docs/superpowers/specs/2026-11-13-histador-design.md), and the work plan is in [docs/issues.md](docs/issues.md) and the GitHub issues.

## Run it locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server **LocalDB**. It's included with Visual Studio's "ASP.NET and web development" workload, or you can get it from the [SQL Server Express installer](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb).
- The EF Core command-line tool, installed once:

  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.11
  ```

### Steps

```bash
git clone https://github.com/Damandundel/Histador.git
cd Histador
dotnet ef database update
dotnet run --launch-profile https
```

Then open https://localhost:7170. In Visual Studio, you can open `Histador.sln` and press F5 instead.

`dotnet ef database update` creates the `Histador` database on LocalDB from the migrations in `Data/Migrations`. Run it again whenever you pull new migrations. Never change the database by hand.

### Configuration and secrets

- The development connection string is in `appsettings.Development.json`. It points at LocalDB, so it holds no secrets.
- Anything secret, like passwords or API keys, goes in **user-secrets** and never in git:

  ```bash
  dotnet user-secrets set "Some:Key" "value"
  ```

### Accounts and the admin account

- Anyone can register at `/Account/Register`. New accounts get the **Player** role.
- Every page requires login, except the landing page, login, register and password reset. To make a page public, put `[AllowAnonymous]` on its controller or action.
- To get an **Admin** account locally, set these user-secrets once, then start the app. The admin is created on startup if it doesn't exist yet.

  ```bash
  dotnet user-secrets set "AdminSeed:Email" "admin@histador.local"
  dotnet user-secrets set "AdminSeed:Password" "<a strong password>"
  dotnet user-secrets set "AdminSeed:DisplayName" "Herodotus"
  ```

- To protect admin-only pages, use `[Authorize(Roles = Roles.Admin)]`. Players who open them get a 403.
- In development, emails such as password-reset links aren't sent. They're written to the console log, so look for `EMAIL to …` in the `dotnet run` output.
- The app applies pending migrations on startup, so `dotnet ef database update` is optional once accounts exist.

## Tests

```bash
dotnet test
```

The tests live in `Histador.Tests` (xUnit). They use in-memory SQLite or the in-memory web host, so you don't need a database to run them. GitHub Actions runs build and test on every push and pull request (`.github/workflows/ci.yml`).

## Project layout

| Folder | What goes there |
|---|---|
| `Controllers/` | MVC controllers |
| `Data/` | `ApplicationDbContext` and `Migrations/` |
| `Models/` | Database entities |
| `ViewModels/` | Classes passed to views and forms |
| `Services/` | Business logic (quiz grading, map %, MMR, …). Keep it out of controllers. |
| `Hubs/` | SignalR hubs (live duels) |
| `Views/` | Razor views |
| `Histador.Tests/` | Unit and integration tests |

To add database tables, add the entity and its `DbSet` to `ApplicationDbContext`, then run:

```bash
dotnet ef migrations add <DescriptiveName> --output-dir Data/Migrations
```

## How we work

1. Pick a GitHub issue and assign yourself.
2. Create a branch from `master` named after the issue, e.g. `14-lesson-player-cards-progress-quit-and-resume`.
3. Commit, push, and open a **pull request** that says `Closes #<issue>`.
4. CI must be green, and **one other team member reviews and approves** the PR.
5. Merge into `master`. Nobody pushes directly to `master`. Every merge deploys to production automatically (see below).

## Deploy to Azure

Production runs on **Azure App Service** (Linux, .NET 8, WebSockets on for SignalR, one instance) with an **Azure SQL** database and **Application Insights** for errors and logs. It's paid for with **Azure for Students** credits. Everything is described in [`infra/main.bicep`](infra/main.bicep).

On every merge to `master`, [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml) builds, runs the tests, applies the EF Core migrations to Azure SQL, deploys to App Service, and checks that `https://<app>.azurewebsites.net/health` returns `Healthy`. It takes about 5–8 minutes. You can also start it by hand from the **Actions** tab ("Deploy to Azure" → "Run workflow"). Until the setup below is done, the workflow is skipped and doesn't fail.

### One-time setup

You need the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) and an Azure for Students subscription. Check that each team member is eligible (school email at [azure.microsoft.com/free/students](https://azure.microsoft.com/free/students)); if one person's credits run out, the resources can be recreated in another member's subscription with the same steps. The commands below use bash (Git Bash works on Windows).

1. **Create the Azure resources.** Pick a globally unique `APP` name, which becomes `https://<APP>.azurewebsites.net`, and strong passwords. Type the passwords in your terminal only, never in a file in the repo.

   ```bash
   az login
   APP=histador-<something-unique>
   RG=histador-rg
   az group create --name $RG --location westeurope
   az deployment group create --resource-group $RG --template-file infra/main.bicep \
     --parameters appName=$APP sqlAdminLogin=histadoradmin sqlAdminPassword='<sql-password>' \
                  adminSeedEmail='<admin-email>' adminSeedPassword='<admin-password>'
   ```

   If Azure for Students says the region isn't allowed, use another one, such as `northeurope` or `swedencentral`.

2. **Let GitHub Actions sign in to Azure** without a stored password (OpenID Connect):

   ```bash
   SUBSCRIPTION_ID=$(az account show --query id -o tsv)
   TENANT_ID=$(az account show --query tenantId -o tsv)
   CLIENT_ID=$(az ad app create --display-name histador-github-deploy --query appId -o tsv)
   az ad sp create --id $CLIENT_ID
   az role assignment create --assignee $CLIENT_ID --role Contributor \
     --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RG
   az ad app federated-credential create --id $CLIENT_ID --parameters '{
     "name": "github-production",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:Damandundel/Histador:environment:production",
     "audiences": ["api://AzureADTokenExchange"]
   }'
   ```

3. **Add the settings in GitHub.** In the repo, go to **Settings → Environments**, create an environment named `production`, and add these **environment secrets**:

   | Secret | Value |
   |---|---|
   | `AZURE_CLIENT_ID` | `$CLIENT_ID` from step 2 |
   | `AZURE_TENANT_ID` | `$TENANT_ID` from step 2 |
   | `AZURE_SUBSCRIPTION_ID` | `$SUBSCRIPTION_ID` from step 2 |
   | `AZURE_SQL_CONNECTION_STRING` | `Server=tcp:<APP>-sql.database.windows.net,1433;Initial Catalog=Histador;User ID=histadoradmin;Password=<sql-password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;` |

   Then go to **Settings → Secrets and variables → Actions → Variables** and add these **repository variables**. They must be repository-level, because the workflow checks `AZURE_WEBAPP_NAME` before it enters the environment.

   | Variable | Value |
   |---|---|
   | `AZURE_WEBAPP_NAME` | `<APP>` |
   | `AZURE_RESOURCE_GROUP` | `histador-rg` |
   | `AZURE_SQL_SERVER` | `<APP>-sql` |

4. Run **Actions → Deploy to Azure → Run workflow**, then open the URL shown on the run.

**Secrets** (the SQL connection string and the admin seed password) live only in App Service configuration and in GitHub secrets, never in git. To change one later, use the Azure portal (App Service → **Settings → Environment variables**) or `az webapp config appsettings set`.

**HTTPS** is required and on by default: `*.azurewebsites.net` has a certificate, and HTTP redirects to HTTPS. A custom domain is optional. It needs the B1 plan or higher: add it under App Service → **Custom domains** and pick a free App Service managed certificate.

### Health and error logs

- `GET /health` returns `Healthy` (200) when the app can reach the database, and `Unhealthy` (503) otherwise. App Service pings it every minute.
- Exceptions, failed requests and `ILogger` warnings go to Application Insights (`<APP>-insights` in the portal → **Failures** or **Logs**). Logging is capped at 1 GB/day to protect the credits.
- To stream live logs: `az webapp log tail --resource-group histador-rg --name <APP>`.

### Backups and restoring the database

Azure SQL takes automatic backups, and **point-in-time restore** is on with 7 days of history (the Basic tier maximum). To roll the database back to a moment before something went wrong (the time is in UTC):

```bash
az sql db restore --resource-group histador-rg --server <APP>-sql --name Histador \
  --dest-name Histador-restored --time "2026-12-01T13:00:00Z" --service-objective Basic
```

That creates a new database next to the old one. To make the app use it, swap the names and restart:

```bash
az sql db rename --resource-group histador-rg --server <APP>-sql --name Histador --new-name Histador-old
az sql db rename --resource-group histador-rg --server <APP>-sql --name Histador-restored --new-name Histador
az webapp restart --resource-group histador-rg --name <APP>
```

Once you've checked the restored data, delete `Histador-old` so you don't pay for both: `az sql db delete --resource-group histador-rg --server <APP>-sql --name Histador-old`.

### Monthly cost (estimate)

| Resource | Tier | ≈ per month |
|---|---|---|
| App Service plan | Linux B1, 1 instance | $13 |
| Azure SQL Database | Basic, 5 DTU, 2 GB | $5 |
| Application Insights + Log Analytics | first 5 GB/month free, capped at 1 GB/day | $0 |
| **Total** | | **≈ $18** |

Azure for Students gives $100 of credit per year, so one account covers about 5 months, which is enough to reach the launch target. The prices are list prices for West Europe; check the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) for your region. To stretch the credits during quiet periods, drop the plan to the free tier with `az appservice plan update --resource-group histador-rg --name <APP>-plan --sku F1`. On F1 the site sleeps when idle and allows only 5 live duel connections, so go back to `B1` before playtests and launch. Watch spending at [microsoftazuresponsorships.com](https://www.microsoftazuresponsorships.com/Balance).

## License

See [LICENSE.txt](LICENSE.txt).
