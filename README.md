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
5. Merge into `master`. Nobody pushes directly to `master`.

## License

See [LICENSE.txt](LICENSE.txt).
