# Histador — Issue list (created on GitHub as #7–#32)

Each issue is separated by a line `=== ISSUE ===`. `{{NN}}` placeholders were replaced with real GitHub issue numbers (NN + 6) when the issues were created.
Design doc: `docs/superpowers/specs/2026-11-13-histador-design.md`.

Milestones (target dates, 3-month plan):
- **M1 Foundation & Lessons**: 2026-12-18
- **M2 Map & Social**: 2027-01-22
- **M3 Duels & Launch**: 2027-02-12

=== ISSUE ===
TITLE: Project setup: unify repo, database, CI and README
LABELS: area:platform,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev A (Platform)
**Depends on:** nothing. This blocks almost everything else, so do it first, ideally in the first week.

## Context
There are two local copies that both point at `github.com/Damandundel/Histador` but have unrelated git histories:
- The outer folder holds the ASP.NET Core MVC (.NET 8) scaffold.
- The nested `Histador/` folder holds only the README plan.

They need to become one history on GitHub before anyone else starts pushing code. Read the design doc (`docs/superpowers/specs/2026-11-13-histador-design.md`) first. It explains the whole product.

## What to do
1. Merge the two histories into a single `master` on GitHub. Keep the README plan and the scaffold. The nested `Histador/` folder must not end up as a folder inside the repo.
2. Add EF Core with the SQL Server provider. Create an `ApplicationDbContext` and the first migration. Use LocalDB for development, with the connection string in `appsettings.Development.json`. Real secrets go in user-secrets, never in git.
3. Create the folder layout: `Data/`, `Models/` (entities), `ViewModels/`, `Services/`, `Hubs/`. Add a test project `Histador.Tests` (xUnit) to the solution.
4. Add a GitHub Actions workflow that runs `dotnet build` and `dotnet test` on every push and pull request.
5. Rewrite the README with: what Histador is (short), how to run it locally (prerequisites, `dotnet ef database update`, `dotnet run`), how to run the tests, and the branching rule (feature branch → PR → 1 review → merge).
6. Commit the design doc and this issue list into `docs/`.
7. Add a shared base layout with a top nav bar (Map, Lessons, Random, Duels, Profile). Links can point to placeholder pages for now.

## Acceptance criteria
- [ ] A fresh clone builds and runs with just the README steps.
- [ ] `dotnet test` runs (one placeholder test is fine) and CI is green on `master`.
- [ ] The database is created from migrations. There's no manual SQL.
- [ ] No secrets are committed.
- [ ] All 4 team members confirm they can clone, run, and open a PR.

## Tips for working with AI
Paste the design doc into your assistant's context. Ask it for the GitHub Actions YAML for .NET 8. Ask it for the exact git commands to merge unrelated histories (`--allow-unrelated-histories`), and review them before running.

=== ISSUE ===
TITLE: Accounts: registration, login and roles (Player / Admin)
LABELS: area:platform,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev A (Platform)
**Depends on:** #{{01}}

## Context
Every feature needs a logged-in user: progress, the map, duels, friends. Admins need a separate role so they can author content and use admin-only icons.

## What to build
- ASP.NET Core Identity with a custom `ApplicationUser : IdentityUser` that adds:
  - `DisplayName`, unique and 3–20 characters. This is the name friends search for.
  - `PreferredLanguage` (`bg` or `en`).
  - `HintPoints` (int, default 0).
  - `Mmr` (int, default 1000).
  - `SelectedIconId` (nullable).
  - `CreatedAt`.
- Pages for register, login, logout, forgot/reset password, and change password. Style them to match the site. The default Identity UI is fine if it's re-styled.
- Roles `Player` (given to everyone at sign-up) and `Admin`.
- Seed one admin account from configuration (user-secrets). It must not be hard-coded.
- Require login for everything except the landing page, login and register.
- The landing page for logged-out visitors explains the app in one screen and has a "Start learning" button.

## Acceptance criteria
- [ ] A new user can register, log in, log out, and reset a password. In dev, the email can just be logged to the console.
- [ ] Duplicate display names are rejected with a clear message.
- [ ] Admin-only routes return 403 for normal players.
- [ ] The seeded admin can log in on a fresh database.
- [ ] Unit test: a new user starts with MMR 1000 and 0 hint points.

## Out of scope
Social login (Google and so on) and email verification. These can come after v1.

=== ISSUE ===
TITLE: Bulgarian / English localization (interface + content pattern)
LABELS: area:platform,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev A (Platform)
**Depends on:** #{{01}}

## Context
The app is bilingual, Bulgarian and English, for both the interface and the lesson content (see the design doc, "Builder's calls"). This has to be in place early. Otherwise every page and every database table has to be redone later.

## What to build
1. **Interface text:** use `IStringLocalizer` / `IViewLocalizer` with `.resx` files (`*.bg.resx`, `*.en.resx`).
   - Add a language switcher in the nav. It saves the choice in a culture cookie and in `ApplicationUser.PreferredLanguage`.
   - Default to the browser language, falling back to English.
2. **Content text pattern:** decide and document how translatable database content is stored. The recommendation is a translation table per entity, e.g. `Lesson` + `LessonTranslation (LessonId, Language, Title, Summary)`.
   - Write a small helper or service that picks the current culture's translation and falls back to English.
   - Document the pattern in `docs/localization.md` with a code example, so Devs B, C and D follow it.
3. Translate the nav, the landing page, and the account pages as the first examples.

## Acceptance criteria
- [ ] Switching language changes all existing interface text immediately and is remembered after logout and login.
- [ ] `docs/localization.md` explains how to add interface text and how to add a translatable entity.
- [ ] Unit test: the content helper returns the Bulgarian text when present and English when the Bulgarian is missing.

## Tips for working with AI
Ask your assistant to set up `RequestLocalizationOptions` with a cookie provider. Have it produce the translation-table pattern with a generic helper. Keep it simple and avoid third-party localization packages.

=== ISSUE ===
TITLE: Content data model: lessons, cards, questions, countries
LABELS: area:lessons,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev B (Lessons)
**Depends on:** #{{01}}, #{{03}} (follow its translation pattern)

## Context
Lessons, quizzes, duels, and the map all read from these tables. Get them right early and share the entity diagram with the team.

## What to build
EF Core entities and a migration for:
- **Country:** `Id`, `IsoNumeric` (the id used by the `world-atlas` map files, e.g. "100" = Bulgaria), `IsoAlpha2`, plus translated `Name`.
  - Seed all countries in `world-atlas` `countries-50m.json`. Generate the seed from that file. Don't type it by hand.
- **Lesson:** `Id`, `Slug`, `WorldWeight` (int 1–100; WWII ≈ 100, a small country's lesson ≈ 5–15), `EstimatedMinutes`, `Era` (enum: Ancient, Medieval, EarlyModern, Modern, Contemporary), `IsPublished`, `CreatedAt`. Translated `Title` and `Summary`.
- **LessonCountry:** many-to-many between Lesson and Country, so one lesson can involve many countries.
- **LessonCard:** `LessonId`, `Order`, `Type` (Text, FunFact, Image), optional `ImageUrl`. Translated `Body` (markdown allowed).
- **Question:** `LessonId` (nullable, for "general pool" questions), `Type` (MultipleChoice, Year), `UseInQuiz` (bool), `UseInDuel` (bool), `CorrectYear` (for Year type). Translated `Text`.
- **AnswerOption:** `QuestionId`, `IsCorrect`. Translated `Text`. Exactly 4 options for multiple choice.
- **UserLessonProgress:** `UserId`, `LessonId`, `LastCardIndex`, `CompletedAt` (nullable), `BestGrade` (0–100, nullable), `PerfectAwarded` (bool), `LessonCompletedSinceLastQuiz` (bool, enforces the retake rule).
- **QuizAttempt:** `UserId`, `LessonId`, `Grade`, `Mistakes`, `TakenAt`.

Add one sample lesson (with cards and 5 questions) as dev seed data so other devs can work before real content exists.

## Acceptance criteria
- [ ] The migration applies cleanly on an empty database.
- [ ] All `world-atlas` 50m countries are seeded with BG and EN names.
- [ ] The sample lesson appears in the database after seeding.
- [ ] An entity diagram (Mermaid in `docs/data-model.md` is fine) is shared with the team.

=== ISSUE ===
TITLE: Lesson content style guide and AI drafting template
LABELS: area:content,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev B (Lessons). Everyone reviews it.
**Depends on:** nothing. Can start on day 1.

## Context
All 4 developers will write lessons with AI help. Without a shared guide, lessons will vary wildly in tone, length, and accuracy. Two anti-goals in the design doc apply here: lessons must never feel bloated or boring, and players must not be able to skip the learning.

## What to produce
`docs/content-guide.md` containing:
1. **Tone and length:**
   - Cards are 40–80 words each.
   - Lessons run 5–15 minutes, roughly 8–25 cards depending on the topic.
   - Every lesson has at least 2 "fun fact" cards.
   - Only important material: who, what, when, why it mattered.
2. **Weight guide:** how to choose `WorldWeight`, with anchor examples (WWII 100, WWI 85, Roman Empire 80, French Revolution 60, history of Jamaica 10). Also how to choose the involved countries. Always use present-day countries; for the Roman Empire, list Italy, France, Spain, Greece, Turkey, Egypt, and so on.
3. **Quiz rules:**
   - 5–10 questions per lesson, every one answerable from the lesson's cards.
   - Mix multiple-choice and year questions.
   - Wrong options must be plausible.
   - Mark which questions are also usable in duels.
4. **Fact-checking rule:** every lesson lists at least 2 real sources (encyclopedia, university or museum site) in an admin-only "Sources" note. AI text is never published unchecked.
5. **Translation rule:** write in English first, translate to Bulgarian with AI, and have a second team member proofread the Bulgarian.
6. **The AI prompt template** the team pastes into their assistant to draft a lesson in the right shape (cards + questions + weight + countries), with output formatted for easy entry into the admin editor.
7. **The launch lesson list:** about 28 lessons split between 4 people (see the two content issues).

## Acceptance criteria
- [ ] The guide is merged, and each team member has drafted one test lesson with the template and given feedback.
- [ ] The launch lesson list is final and each lesson has an owner.

=== ISSUE ===
TITLE: Admin area: lesson and question editor
LABELS: area:platform,area:content,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev A (Platform)
**Depends on:** #{{02}}, #{{04}}

## Context
All lesson content is written inside the app, not in code. The whole team needs this editor early, by the end of week 4, so content writing can start in parallel.

## What to build (Admin role only, under `/Admin`)
- **Lesson list:** search, filter by published/draft, buttons to create, edit, and delete.
- **Lesson edit:**
  - Title and summary in BG and EN.
  - WorldWeight, EstimatedMinutes, Era.
  - Involved countries: a searchable multi-select of countries.
  - Published toggle.
  - Admin-only "Sources" notes.
- **Cards:** add, edit, delete, and reorder (up/down buttons are fine). Choose a type (Text / FunFact / Image), with body text in BG and EN and an image URL.
- **Questions:** add, edit, and delete per lesson, plus a separate "General pool" list for questions with no lesson.
  - Type MultipleChoice: 4 options, exactly 1 marked correct.
  - Type Year: a correct year.
  - Checkboxes for UseInQuiz and UseInDuel.
  - Text in BG and EN.
- **Preview:** the admin can open the lesson in the normal lesson player without saving any progress.
- **Validation:** a lesson can't be published unless it has ≥ 3 cards, ≥ 5 quiz questions, ≥ 1 country, and both languages are filled in. Missing items are shown clearly.

## Acceptance criteria
- [ ] An admin can create a full lesson from scratch and publish it without touching the database.
- [ ] Non-admins get 403 on every `/Admin` route.
- [ ] Publishing an incomplete lesson is blocked and the reasons are shown.
- [ ] Works on a laptop screen. Mobile isn't required for admin pages.

=== ISSUE ===
TITLE: Lesson catalog and the Random button
LABELS: area:lessons
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev B (Lessons)
**Depends on:** #{{04}}

## What to build
1. **Lessons page (`/Lessons`):**
   - Shows published lessons as cards: title, estimated minutes, era, a flag/list of countries, and the player's status (Not started / In progress / Passed with grade / Failed).
   - Filter by era and country, plus a text search.
   - "Continue" appears for lessons in progress.
2. **Random (`/Random` and the nav button):** starts a lesson picked uniformly at random from all published lessons on the platform, including ones the player has already done (the owner wants it to be "any lesson on the platform").
   - It goes straight into the lesson player.
   - Show a short "🎲 Random lesson: <title>" banner at the start.

## Acceptance criteria
- [ ] Unpublished lessons never appear and are never picked by Random.
- [ ] Filters and search combine correctly.
- [ ] Status labels match the player's real progress.
- [ ] Unit test: Random only picks published lessons and can return any of them.
- [ ] The page is usable on a phone (375px wide).

=== ISSUE ===
TITLE: Lesson player: cards, progress, quit and resume
LABELS: area:lessons,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev B (Lessons)
**Depends on:** #{{04}}, #{{07}}

## Context
This is the heart of the "Duolingo" feeling. It has to be fast, clean, and pleasant on a phone.

## What to build
- One card at a time with Next and Back buttons, a progress bar at the top, and a Quit (X) button.
- Fun-fact cards get a distinct style.
- Card bodies support basic markdown and an optional image.
- Progress is saved (`LastCardIndex`) every time the player moves to the next card.
- Quitting asks "Leave lesson? Your progress is saved." On returning to the lesson, the player gets **Resume** or **Start over**.
- After the last card: mark the lesson completed (`CompletedAt`, `LessonCompletedSinceLastQuiz = true`) and show a "Take the quiz" button.
- Shown in the user's language, using the localization helper from #{{03}}.

## Acceptance criteria
- [ ] Closing the browser mid-lesson and coming back resumes at the same card.
- [ ] "Start over" resets to card 1 without losing the player's best grade.
- [ ] Works smoothly on a 375px-wide phone screen. Swipe gestures are a nice-to-have.
- [ ] Keyboard: the left and right arrows move between cards.

=== ISSUE ===
TITLE: Quiz after each lesson: grading, pass mark, hint points, retake rule
LABELS: area:lessons,priority:high
MILESTONE: M1 Foundation & Lessons
BODY:
**Owner:** Dev B (Lessons)
**Depends on:** #{{08}}

## Rules (decided with the owner; see the design doc)
- The quiz uses the lesson's questions where `UseInQuiz = true`, in random order with options shuffled.
- **Grade** = % of questions answered correctly. Year questions are correct within ±2 years.
- **Pass mark: 60%.** Below it, nothing unlocks on the map.
- The **best grade** counts for the map. A worse retake never lowers it.
- **0 mistakes → +1 hint point**, but only the *first* time the player gets a perfect score on that lesson (`PerfectAwarded`).
- **Retake rule:** a player can only retake the quiz after completing the lesson again (`LessonCompletedSinceLastQuiz` must be true). Taking the quiz sets it back to false.

## What to build
- A quiz page, one question at a time. After each answer, show right or wrong immediately with the correct answer and a one-line reason (from the question text or the lesson card).
- A results screen: grade, pass or fail, "+1 hint point" if earned, and the map % gained (calculated by #{{13}}; until that's done, show a placeholder). Buttons: "See it on the map" and "Back to lessons". On a fail, show "Review lesson".
- Put all the rule logic in a `QuizService` that the controllers call, not in the controllers themselves.

## Acceptance criteria
- [ ] Unit tests cover:
  - Grade calculation, including the year-tolerance edge cases (exactly ±2, ±3).
  - Pass and fail at exactly 59/60%.
  - The best grade is kept.
  - The hint point is awarded only on the first perfect score.
  - A retake is blocked until the lesson is completed again.
- [ ] Refreshing the page mid-quiz can't be used to cheat. Answers are checked on the server, and the correct answer is never sent to the browser before the player answers.

=== ISSUE ===
TITLE: Write launch lessons, batch 1: major world events (~12 lessons)
LABELS: area:content
MILESTONE: M2 Map & Social
BODY:
**Owner:** All 4 devs, about 3 lessons each (assignments are in the lesson list in the content guide)
**Depends on:** #{{05}} (style guide), #{{06}} (admin editor)

## Context
Big world events carry the most `WorldWeight` and clear fog across many countries at once. They make the map feel alive quickly, and they power duels.

## Suggested list (adjust in the content guide)
WWII, WWI, the Roman Empire, Ancient Greece, the Mongol Empire, the Ottoman Empire, the Age of Exploration, the French Revolution, the Industrial Revolution, the Cold War, Decolonization of Africa, the Fall of the USSR.

## For each lesson
1. Draft it with the AI template from the content guide.
2. **Fact-check every card and question** against at least 2 real sources, and add them to the Sources note.
3. Enter it in the admin editor in English, with countries and weight.
4. Have another team member review it (accuracy, length, "is it interesting?") before publishing.

## Acceptance criteria
- [ ] About 12 lessons published in English, each 5–15 minutes, with 5–10 quiz questions and at least 3 duel-usable questions.
- [ ] Each lesson was reviewed by someone other than its author. Note who in the lesson's Sources note.
- [ ] Together the lessons cover every inhabited continent.

=== ISSUE ===
TITLE: Write launch lessons, batch 2: country histories (~16 lessons)
LABELS: area:content
MILESTONE: M2 Map & Social
BODY:
**Owner:** All 4 devs, about 4 lessons each
**Depends on:** #{{05}}, #{{06}}

## Context
Country lessons have smaller weights but let players clear fog on specific places, and they give the Random button variety.

## Suggested list (adjust in the content guide)
Bulgaria (2 lessons: First Bulgarian Empire, Liberation and modern Bulgaria), Japan, China, India, Egypt, the USA, Brazil, Mexico, the UK, Russia, Australia, South Africa, Jamaica, Iran (Persia), Poland.

## For each lesson
Same process as batch 1: AI draft → fact-check with 2+ sources → enter in the editor → review by another team member → publish.

## Acceptance criteria
- [ ] About 16 lessons published in English, following the content guide.
- [ ] At least 4 lessons are on countries outside Europe and North America.
- [ ] Each lesson was reviewed by someone other than its author.

=== ISSUE ===
TITLE: Translate and proofread all launch content into Bulgarian
LABELS: area:content,area:platform
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** All 4 devs. Each translates the lessons they wrote and proofreads someone else's.
**Depends on:** #{{03}}, #{{10}}, #{{11}}

## What to do
- Translate each lesson's title, summary, cards, questions, and answer options into Bulgarian with AI help, then enter them in the editor.
- A **different** team member proofreads the Bulgarian for natural wording and correct historical names (e.g. "Освобождението", not a literal translation).
- Translate the rest of the interface text (all `.resx` files) and the icon names.
- Add an admin-only "Missing translations" report page listing anything without Bulgarian text. Dev A can help with this page.

## Acceptance criteria
- [ ] The Missing translations report is empty at launch.
- [ ] A native Bulgarian speaker on the team has clicked through the whole app in Bulgarian and fixed awkward text.

=== ISSUE ===
TITLE: Map progress calculation: world % and per-country %
LABELS: area:map,priority:high
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev C (Map)
**Depends on:** #{{04}}, #{{09}}

## Rules (from the design doc)
- Only passed lessons (best grade ≥ 60) count.
- **World %** = Σ(lesson.WorldWeight × bestGrade/100) over the player's passed lessons ÷ Σ(WorldWeight) over **all published** lessons × 100.
- **Country X %** = the same formula, using only lessons that include country X (both the top and the bottom sum).
- A country with no published lessons stays fully fogged and is shown as "No lessons yet". It's excluded from nothing; the world % simply ignores it.
- Publishing new lessons lowers everyone's % a little. That's intended.
- **Account level** = world % (shown as e.g. "Level 23 · 23.4% of the world discovered"). Rounding rule: level = floor(world %).

## What to build
- `ProgressService` with `GetWorldPercent(userId)`, `GetCountryPercents(userId)` (a dictionary keyed by `IsoNumeric`), and `GetGainFromQuiz(userId, lessonId, oldGrade, newGrade)` for the quiz results screen.
- Make it fast enough for the map page. A single query per call is fine, plus caching per user that's cleared when a quiz is passed.
- Hook it into the quiz results screen from #{{09}} (replace the placeholder).

## Acceptance criteria
- [ ] Unit tests cover:
  - No lessons passed → 0%.
  - Everything at 100% → 100%.
  - Weighting (a WWII-style lesson moves the number more than a small country lesson).
  - Per-country isolation.
  - Failed lessons count as 0.
  - Adding a new published lesson lowers the %.
- [ ] The results screen shows the correct "+X.X% of the world" after a quiz.

=== ISSUE ===
TITLE: World map page with fog of war
LABELS: area:map,priority:high
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev C (Map)
**Depends on:** #{{13}}

## Context
The map is the main progress screen and the home page after login. It must look great and run well on phones.

## What to build
- `/Map`: render the world as SVG with **D3** + **TopoJSON** using `world-atlas` `countries-50m.json`, served locally from `wwwroot/lib`. Don't load it from a CDN at runtime.
- **Fog:** each country is drawn under a fog layer whose opacity depends on the country's %:
  - 0% = fully fogged (e.g. grey or parchment texture).
  - 100% = full colour.
  - Countries with no lessons get a distinct "unexplored, no lessons yet" style.
- Pan and zoom (mouse wheel, pinch on mobile) with sensible limits.
- A header bar showing "X% of the world discovered" and the level.
- The page gets its data from a JSON endpoint `/api/progress/countries` that returns `{ isoNumeric: percent }`.
- Theme: a parchment / old-map look is suggested. Agree on it with the team.

## Acceptance criteria
- [ ] A new player sees an all-fogged world. After passing a lesson, the involved countries visibly clear by the right amount.
- [ ] Small countries (e.g. Jamaica, Malta) are visible and tappable when zoomed in.
- [ ] Loads in under 2 seconds on a normal connection. Pan and zoom stay smooth on a mid-range phone.
- [ ] Works at 375px width.

=== ISSUE ===
TITLE: Country detail panel on the map
LABELS: area:map
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev C (Map)
**Depends on:** #{{14}}

## What to build
Clicking or tapping a country opens a side panel (a bottom sheet on mobile) showing:
- The country name (localized) and its discovered %.
- All published lessons involving this country, each with status, grade, and estimated minutes, and a button to start or continue it.
- Lessons that would clear the most fog for this country come first (highest weight, not yet passed).
- If there are no lessons: "No lessons for this country yet."

## Acceptance criteria
- [ ] Every country is clickable, including small ones when zoomed in.
- [ ] Starting a lesson from the panel opens the lesson player. Coming back afterwards returns to the map.
- [ ] The panel closes with X, Escape, or a tap outside.

=== ISSUE ===
TITLE: Fog-reveal moment after passing a quiz
LABELS: area:map,area:lessons
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev C (Map), working with Dev B
**Depends on:** #{{09}}, #{{14}}

## Context
This is the reward moment of the whole app. Passing a quiz should *feel* like discovering part of the world.

## What to build
- After a passed quiz, "See it on the map" opens the map zoomed to fit the lesson's countries.
- The fog lifts from the old % to the new % over about 1.5–2 seconds, country by country.
- A counter animates "+X.X% of the world".
- If a hint point was earned, show a small "+1 hint point" toast.
- Respect `prefers-reduced-motion`: show the final state without the animation.

## Acceptance criteria
- [ ] Works for a lesson with 1 country and for one with 30+ countries (WWII).
- [ ] A retake that improves the grade animates only the extra gain.
- [ ] No animation if the quiz was failed. Instead show an encouraging "Review and try again" message.

=== ISSUE ===
TITLE: Player profile page: level, rank, icon, stats
LABELS: area:map,area:platform
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev C (Map)
**Depends on:** #{{02}}, #{{13}}

## What to build
- `/Profile` (your own) and `/Profile/{displayName}` (anyone's, visible to logged-in users), showing:
  - The selected player icon (a placeholder until #{{24}}).
  - Display name.
  - **Level and "X% of the world discovered"** with a small mini-map thumbnail of their fog state.
  - **Rank and MMR** (placeholder until #{{21}}).
  - Hint points (own profile only).
  - Lessons passed, perfect quizzes, duels won/lost (placeholder until the duel issues are done).
- On another player's profile, show "Add friend" and "Challenge to duel" buttons. Wire them up when #{{18}} and #{{22}} are done.
- Admin accounts show an "Admin" badge.

## Acceptance criteria
- [ ] Both your own and other players' profiles render correctly in BG and EN.
- [ ] Other players can't see your hint points.
- [ ] The mini-map matches the real map state.

=== ISSUE ===
TITLE: Friends: search, requests, friend list
LABELS: area:duels
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{02}}

## What to build
- `Friendship` entity: `RequesterId`, `AddresseeId`, `Status` (Pending, Accepted, Declined), `CreatedAt`.
- `/Friends` page:
  - Search players by display name (partial match, max 20 results).
  - Send a request.
  - See incoming and outgoing requests, with accept, decline, or cancel.
  - The friend list shows each friend's icon, level, rank, online status (from SignalR presence once #{{19}} is done; show "—" until then), and a **Challenge** button.
- Remove a friend.
- A notification badge in the nav for pending incoming requests.

## Acceptance criteria
- [ ] Two accounts can find each other, send and accept a request, and see each other in their lists.
- [ ] Duplicate and self-requests are impossible.
- [ ] Unit tests for the friendship rules (can't friend yourself, no duplicates, a declined request can be re-sent).

=== ISSUE ===
TITLE: Duel engine: SignalR hub, rounds, scoring, question selection
LABELS: area:duels,priority:high
MILESTONE: M2 Map & Social
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{04}}, #{{09}}

## Context
Duels are live 1v1 quiz battles in the style of Triviador. The server is the referee: it decides the questions, the timing, and the scores, and it never trusts the browser with the correct answer.

## Rules (design doc "Builder's calls")
- **5 rounds.** Each round both players get the same question at the same time and have **15 seconds** to answer.
- Questions are mostly multiple choice, with some year questions. A year answer is correct within ±2 years.
- Correct = 1 point. If both are correct, the faster player gets +0.5 speed bonus.
- **Tie after 5 rounds:** a numeric tiebreaker ("In what year…?"). The closest guess wins; if the guesses are equally close, the faster player wins.
- **Disconnect:** a player who's gone for more than 20 seconds forfeits.
- **Question selection:** pick from questions (`UseInDuel = true`) belonging to lessons **both players have passed**. If there aren't enough, fill from the general pool and then from any published lesson. Never repeat a question within a duel.

## What to build
- `DuelHub` (SignalR) plus a `DuelService` that holds an in-memory state machine per duel: Waiting → Round 1..5 → Tiebreaker? → Finished.
- Server timers (not the browser's) decide when a round ends.
- `Duel` and `DuelRound` entities to store results: players, questions, answers, times, winner, MMR change.
- An online-presence tracker (who's connected), used by Friends and matchmaking.
- Hooks for hints (#{{23}}) and MMR (#{{21}}). For now, just call an interface that does nothing.

## Acceptance criteria
- [ ] Two browsers can complete a full duel through the hub, using a bare test page.
- [ ] Unit tests for: scoring, speed bonus, tiebreaker logic, question selection (shared passed lessons first, then the fallbacks, no repeats), and forfeit on disconnect.
- [ ] Correct answers are only sent to clients *after* a round ends.

=== ISSUE ===
TITLE: Duel screen UI
LABELS: area:duels
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{19}}

## What to build
- `/Duel/{id}` page connected to `DuelHub`:
  - **Intro:** both players' icons, names, ranks, and MMR. "VS" with a 3-second countdown.
  - **Round:** question text, 4 answer buttons (or a year input for year questions), a 15-second timer bar, a round counter (e.g. "2 / 5"), and the score. Show "Opponent answered" without revealing what they picked.
  - **Round result:** the correct answer highlighted, what each player picked, and the time taken.
  - **Tiebreaker:** a year input, then both guesses shown against the correct year.
  - **Finish:** win or lose, the final score, and the MMR change (+/−) with the new rank. If the player ranked up, celebrate. Buttons: "Rematch" (friends only) and "Back".
- A hint button area (wired in #{{23}}).
- Handles reconnecting: if the player refreshes mid-duel, they're put back into the current round.

## Acceptance criteria
- [ ] A full duel is playable on two phones (375px) and on desktop.
- [ ] The timer never drifts more than half a second from the server.
- [ ] Works in BG and EN.

=== ISSUE ===
TITLE: MMR and ranks (Elo)
LABELS: area:duels,priority:high
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{19}}

## Rules (design doc "Builder's calls"; numbers can be tuned)
- Elo, starting MMR 1000, **K = 32**. Ranks come only from MMR:

| Rank | MMR |
|---|---|
| Peasant | < 1100 |
| Soldier | 1100–1249 |
| General | 1250–1449 |
| King / Queen | 1450–1699 (the title follows the player's selected icon gender) |
| God / Goddess | 1700+ |

- Duels between friends can be marked **unranked** (no MMR change) when the challenge is created. Random matches are always ranked.
- MMR can't go below 0.

## What to build
- `RankingService`: `CalculateMmrChange(winnerMmr, loserMmr)` and `GetRank(mmr)`.
- Implement the hook from #{{19}} so a finished ranked duel updates both players' MMR and stores the change on the `Duel` record.
- Rank names localized in BG and EN.
- Rank badges (simple icons or styled labels) used on the profile, the friend list, and the duel screen.
- Keep the rank thresholds in configuration so they can be tuned without code changes.

## Acceptance criteria
- [ ] Unit tests: equal MMR → ±16; a strong favourite winning gains little; an underdog winning gains a lot; each rank boundary; unranked duels change nothing.
- [ ] The profile shows the correct rank after duels.

=== ISSUE ===
TITLE: Matchmaking (random opponents) and friend challenges
LABELS: area:duels,priority:high
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{18}}, #{{19}}, #{{21}}

## What to build
1. **Random match ("Find opponent" on `/Duels`):**
   - Join an in-memory queue. The server pairs players within **±100 MMR**, widening by 50 every 10 seconds.
   - Show "Searching… (0:23)" with a Cancel button.
   - When matched, both players go to the duel page.
   - Losing the queue on a server restart is acceptable for v1.
2. **Friend challenge:**
   - From the friend list or a profile, send a challenge, choosing ranked or unranked.
   - The friend gets a live popup (if online) with Accept or Decline, which expires after 60 seconds.
   - Accepting starts the duel for both.
3. The `/Duels` page also lists the player's recent duels: opponent, result, MMR change, and date.

## Acceptance criteria
- [ ] Two players with similar MMR searching at the same time are matched within a few seconds.
- [ ] A player can't be in the queue and in a duel at the same time. Cancelling really removes them from the queue.
- [ ] Challenges to offline friends show "Friend is offline".
- [ ] Unit tests for the queue pairing and range-widening logic.

=== ISSUE ===
TITLE: Duel hints: 50/50 and Range
LABELS: area:duels
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev D (Duels)
**Depends on:** #{{09}}, #{{19}}, #{{20}}

## Rules
- Hint points are earned only through perfect quizzes (the first perfect score per lesson, see #{{09}}). They can never be bought.
- Each hint costs **1 hint point**. Max **1 hint per round** per player.
- **50/50:** removes two wrong options. Multiple-choice questions only.
- **Range:** shows a ±10-year window that contains the correct year. Year questions and the tiebreaker only.
- The opponent sees that a hint was used ("💡 used a hint") but not which one.

## What to build
- Server-side validation in `DuelHub`: the player has points, hasn't used a hint this round, and the hint type fits the question. The point is deducted on the server.
- Hint buttons in the duel UI showing the player's remaining points, disabled when they're not allowed.

## Acceptance criteria
- [ ] A player with 0 points can't use hints, even by calling the hub directly.
- [ ] The Range window is randomized so the correct year isn't always in the middle.
- [ ] Unit tests for every validation rule.

=== ISSUE ===
TITLE: Player icons: historical figures unlocked by rank
LABELS: area:platform,area:content
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev A (Platform)
**Depends on:** #{{17}}, #{{21}}

## Context
Player icons are historical figures of different fame levels, with male and female options. Higher ranks unlock more famous figures. Some icons are reserved for administrator accounts.

## What to build
- `Icon` entity: `Id`, `ImageUrl`, `Gender` (Male / Female), `RequiredRank` (nullable), `AdminOnly` (bool), plus translated `Name` and a one-line `Description` ("Spartan king who held Thermopylae").
- An icon picker on the profile: all icons shown, locked ones greyed out with "Unlocks at General". Selecting an icon sets `SelectedIconId`.
- New players choose a starter icon (male or female) during sign-up.
- At least **2 male + 2 female icons per rank** (about 20 in total) plus 2 admin-only icons.
- **Art:** use illustrated portraits only, never photos or real likenesses of recent people.
  - Sources: public-domain art (Wikimedia Commons, with the licence noted in `docs/icon-credits.md`) or AI-generated illustrations, used consistently in one style.
  - Prefer figures who lived long ago. Leave out mythological characters (Aphrodite, Helen of Troy).
- **Note for the team:** the October plan's admin rank nickname "Чичо Ади" reads as a Hitler reference. Choose a different name for the admin rank/badge before the app goes public.

## Acceptance criteria
- [ ] Players can only select icons their rank allows. The server enforces this.
- [ ] Admin-only icons can't be selected by non-admins.
- [ ] Every icon has a credit/licence entry.
- [ ] Icons look consistent with each other (same style and frame).

=== ISSUE ===
TITLE: Deploy to Azure (App Service + Azure SQL) with automatic deployment
LABELS: area:platform,priority:high
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** Dev A (Platform)
**Depends on:** #{{01}}. Start in M2 so problems surface early. Don't leave this to the last week.

## What to build
- An Azure App Service (Linux, .NET 8) and an Azure SQL Database. Use **Azure for Students** credits; check every member's eligibility.
- **WebSockets enabled** on the App Service (SignalR needs them). Use a single instance; scale-out isn't needed for v1.
- Secrets (connection string, admin seed password) in App Service configuration, never in git.
- A GitHub Actions workflow: on merge to `master`, build, test, run `dotnet ef` migrations against Azure SQL, and deploy.
- A custom domain is optional. HTTPS is required.
- Basic health check and error logging (Application Insights free tier).
- A backup plan: Azure SQL point-in-time restore is enabled. Document in the README how to restore.

## Acceptance criteria
- [ ] The public URL works. Registration, lessons, the map, and a duel between two real devices all work in production.
- [ ] A merge to `master` deploys automatically within ~10 minutes.
- [ ] Monthly cost stays within the student credits. Note the estimate in the README.

=== ISSUE ===
TITLE: Launch polish: mobile pass, playtest with real users, bug bash
LABELS: area:platform,area:lessons,area:map,area:duels
MILESTONE: M3 Duels & Launch
BODY:
**Owner:** All 4 devs (Dev C coordinates)
**Depends on:** all other issues

## What to do
1. **Mobile pass:** every page at 375px and 768px. Fix overflow, tiny tap targets (minimum 44px), and unreadable text.
2. **Accessibility basics:** colour contrast, keyboard navigation for lessons and quizzes, alt text on images, `prefers-reduced-motion` respected.
3. **Playtest:** at least **10 real users** outside the team (classmates, family) play for a week. Collect feedback with a short form: Were lessons interesting? Too long? Were duels fun? Was anything confusing?
4. **Bug bash:** one shared session where the team tries to break everything (double clicks, refresh mid-duel, two tabs, bad input). File each bug as a new issue.
5. **Tune:** lesson lengths, the pass mark, MMR thresholds, and duel timer, based on playtest feedback.
6. **Final README:** features, screenshots, architecture overview, how to run, team credits. This also prepares the school presentation.

## Acceptance criteria
- [ ] All bugs found in the playtest and bug bash are fixed or deliberately postponed (labelled `after-v1`).
- [ ] 10+ outside players completed at least one lesson and one duel.
- [ ] Final README and demo are ready one week before the school deadline.
