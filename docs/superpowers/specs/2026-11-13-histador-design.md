# Histador — Design

Date: 2026-11-13
Status: Owner approved direction; issues being created

## Intent

Histador is a web app for learning history the way Duolingo teaches languages: short, focused lessons with only the important material and the interesting facts. Progress shows up on a world map that starts fully covered in fog of war. Passing a lesson's quiz clears fog over every country involved in it. Alongside the learning, players fight Triviador-style quiz duels against friends or random opponents to earn MMR, ranks, and historical-figure player icons.

**Why:** it's a school project, but real users should be able to play it. It has to be finished, deployed, and solid. It doesn't need to handle huge scale or ship to app stores. The hard deadline is ~5 months away (around April 2027), and the team's target is 3 months (around mid-February 2027).

## Goals

- Players learn real history in lessons of variable length (roughly 5–15 minutes). The quiz after each lesson proves they learned it.
- The world map is the main progress screen. How much fog clears depends on how important the lesson is and how well the player did on its quiz.
- Duels are fun and competitive, and they reward studying.
- The app is deployed publicly and strangers can sign up and play.

## Non-goals (v1)

- Native mobile apps. The site should still work well in a phone browser.
- Streaks, daily challenges, achievements, global leaderboards. These appeared in the October plan. They're good candidates for after v1.
- Buying hints or anything else with real money.
- Huge scale. A few hundred concurrent users is plenty.

## Anti-goals (failure even if it "works")

- **Bloated or boring lessons.** If lessons read like a textbook, the app has failed its main promise.
- **Map % driven by land area.** A Russia lesson must not outweigh twenty European ones. Map % comes from lesson importance, not area.
- **Skimming wins.** Players must not clear fog without having learned the material.
- **Pay-to-win or farmable hints.**

## Approaches considered

1. **ASP.NET Core MVC + SignalR (chosen).** Builds on the existing .NET 8 MVC scaffold. Pages are rendered on the server, SignalR handles live duels and matchmaking, and the map is an SVG drawn in the browser. One language, one project, and most likely what the school teaches. The trade-off is less of an "app" feel, and duel screens need hand-written JavaScript.
2. **ASP.NET Core Web API + React.** Gives the slickest UI and an easier path to mobile later. Rejected: two stacks plus API plumbing is too much risk for a 3-month target.
3. **Blazor.** Everything in C#. Rejected: thinner map and animation libraries, and a learning curve with less payoff.

## Decided by the owner

**Lessons**
- Every lesson is tied to one or more countries. Lesson length varies with the material.
- Each lesson carries an importance that sets how much of the world it unlocks. For example, WWII unlocks more than Jamaica's history, and it unlocks more countries because many more countries took part.
- A lesson can be quit midway. Progress is saved so the player can resume or start over.
- Each lesson ends with a short quiz. Below the passing grade, nothing unlocks. Above it, a higher grade clears more fog.
- A perfect quiz (0 mistakes) earns **1 hint point**. Hint points are spent in duels. Only the first perfect score on a given lesson awards a point.
- To retake a quiz, the player has to go through the lesson again first. Their best grade counts.
- The **Random** section starts a lesson picked entirely at random from all lessons on the platform.

**Map and level**
- The world starts fully fogged. Each country has its own discovery %.
- Account level = % of the world discovered, shown on the profile.
- Weighting: each lesson has a **world weight** (set when the lesson is written) and a list of involved countries.
  - World % = Σ(lesson weight × best grade fraction) ÷ Σ(all lesson weights).
  - Each country's % uses the same formula, restricted to lessons that include that country.
  - Adding new lessons lowers everyone's % slightly. That's accepted: it shows there's more to discover.

**Duels**
- Players can duel friends (befriended by username) or get matched at random.
- The question style follows Triviador.
- Duel questions come from lessons both players have passed. If they share none, questions come from the general pool.
- Duels change **MMR**, and MMR determines rank.

**Ranks and icons**
- Ranks come from MMR. The October plan's ladder was Peasant → Soldier → General → King/Queen → God/Goddess.
- Player icons are historical figures of varying fame, with male and female options, unlocked by rank.
- Some icons are reserved for administrator accounts.

## Builder's calls (proposed; owner may override)

**Stack:** .NET 8, ASP.NET Core MVC, EF Core + SQL Server, ASP.NET Core Identity, SignalR. The map uses D3 with the `world-atlas` TopoJSON (Natural Earth, public domain). Tests use xUnit. Hosting is Azure App Service + Azure SQL; Azure for Students credits should cover it.

**Lesson format:** a sequence of short cards (text, optional image, "fun fact" callouts) with a progress bar. Saved progress is the index of the last card reached.

**Quiz:** 5–10 questions per lesson, multiple choice plus "pick the year" questions. The **passing grade is 60%**.

**Duel format (Triviador-like):**
- Live, 1v1, 5 rounds.
- Each round, both players see the same question at the same time, with a 15-second timer. Questions are mostly multiple choice, with some "what year" questions mixed in. A year answer counts as correct within ±2 years.
- Correct answers score a point. If both are correct, the faster one gets a speed bonus.
- A tie after 5 rounds goes to a numeric tiebreaker ("In what year…?"). The closest guess wins.
- **Hints** cost 1 hint point each, max 1 per round. There are two kinds:
  - **50/50:** removes two wrong answers. Works on multiple-choice questions only.
  - **Range:** narrows the answer to a ±10-year window. Works on year questions and the tiebreaker.

**Countries on the map:** lessons always map to present-day countries. A Roman Empire lesson, for example, lists Italy, France, Greece, Turkey, Egypt, and so on. The map uses `world-atlas` at 50m detail so small states like Jamaica appear.

**MMR:**
- Elo, starting at 1000, K = 32.
- Ranks by MMR: Peasant < 1100 ≤ Soldier < 1250 ≤ General < 1450 ≤ King/Queen < 1700 ≤ God/Goddess.
- Friend duels can be marked unranked.

**Matchmaking:**
- An in-memory queue that pairs players within ±100 MMR and widens by 50 every 10 seconds.
- The player can cancel at any time.
- If the server restarts, the queue is lost. That's acceptable for v1.

**Disconnects:** a player who disconnects mid-duel forfeits after 20 seconds.

**Admin area:**
- Admins create and edit lessons, cards, quiz questions (each marked usable in quiz, duel, or both), world weights, and country lists.
- They also manage icons and user roles.
- Content is authored in the app, not in code.

**Launch content:** about 25–30 lessons covering the major world events plus a spread of individual countries. That's enough for the map and duels to feel alive.

**Icons:** illustrated portraits, never photos. Prefer figures long dead.
- Mythological characters (e.g. Aphrodite, Helen of Troy from the October list) are better left out of a history app.
- Recent figures (e.g. Princess Diana) raise likeness and taste concerns.
- The hidden admin rank's nickname in the October plan ("Чичо Ади") reads as a Hitler reference. For a public app aimed at ages 14+, that's worth renaming before launch.

## Team and process

- Four developers. Each uses AI assistants to do their work.
- Work is tracked as GitHub issues. Each issue says who owns which area, what to build, and what "done" means (acceptance criteria). Someone with an AI assistant should be able to pick one up without asking around.
- Rough ownership by area:
  - **Dev A:** platform, accounts, profiles, admin, deployment.
  - **Dev B:** lessons, quizzes, Random.
  - **Dev C:** map and fog.
  - **Dev D:** duels, MMR, matchmaking, friends.
  - Everyone helps write lesson content.

## Builder's calls on things the owner left open

- **Language:** Bulgarian and English for both the interface and the lesson content.
  - Interface text lives in .resx resource files.
  - Lesson, quiz and icon text is stored per language.
  - Content is written in English first, then translated to Bulgarian with AI help, and a team member proofreads it.
  - English is the fallback if a translation is missing.
- **Content:** shared across the team. Each developer drafts lessons with AI help, then fact-checks them against a real source before publishing. The admin editor is scheduled first so content work can begin in month 1.
- **School requirements:** none were stated. The plan includes a README with setup instructions and unit tests for the scoring, map % and MMR logic.

## Repo note

The .NET scaffold and the README plan originally lived in two clones with unrelated histories. They have since been unified on GitHub's `master`. Any leftover nested `Histador/` clone is git-ignored and can be deleted.
