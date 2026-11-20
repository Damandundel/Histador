# Bug bash (issue #32)

One shared session, about 2 hours, with **all 4 devs in the same room or call**. The goal is to break the app before players do. Run it on the **deployed** site (not localhost) once all M3 features are merged.

## Setup (15 min)

- Everyone has at least two devices or browsers: a phone and a laptop, plus one private/incognito window for a second account.
- Each person makes two test accounts (e.g. `bb-anna-1`, `bb-anna-2`).
- One admin account is shared for content checks.
- Open the bug-report template below in a shared doc so filing is fast.
- Pick a timekeeper. Rotate areas every 25 minutes so everyone tests everything.

## What to try

### Accounts
- [ ] Register with a display name that's 2 chars, 21 chars, only spaces, emoji, or Cyrillic.
- [ ] Register the same name with different capitals.
- [ ] Submit register or login twice quickly (double click).
- [ ] Log in on two devices at once. Then change the password on one. What happens on the other?
- [ ] Use the password reset link twice. Use an old link after resetting.
- [ ] Press Back after logging out. Can you see private pages?

### Lessons and quizzes
- [ ] Quit halfway, close the browser, come back. Do you resume on the right card?
- [ ] Open the same lesson in two tabs and click through both.
- [ ] Double-click "Next" and the quiz answer buttons.
- [ ] Refresh in the middle of a quiz. Can you get a second attempt or see the answers?
- [ ] Try to retake a quiz without redoing the lesson. Try typing the quiz URL directly.
- [ ] Get a perfect score twice on the same lesson. You should earn only 1 hint point.
- [ ] Type a year question answer as `-500`, `abc`, `99999`, or leave it empty.
- [ ] Press 🎲 Random 20 times. Do you ever get an unpublished lesson or an error?

### Map
- [ ] Pass a lesson, then open the map. Did the right countries clear?
- [ ] Pinch and zoom on a phone. Can you still tap small countries (Malta, Jamaica)?
- [ ] Rotate the phone during the fog-reveal animation.
- [ ] Turn on "reduce motion" in the OS. Are animations skipped?

### Duels
- [ ] Refresh in the middle of a duel round.
- [ ] Close the tab mid-duel. Does the other player win after 20 s?
- [ ] Join the random queue in two tabs with the same account.
- [ ] Cancel matchmaking right as a match is found.
- [ ] Challenge a friend who's offline. Accept a challenge after it expired.
- [ ] Use a hint with 0 hint points (also try calling it from the browser console).
- [ ] Use two hints in one round.
- [ ] Answer after the timer runs out (slow network: DevTools → Network → "Slow 3G").
- [ ] Tie the duel on purpose. Does the tiebreaker work?
- [ ] Check that both players' MMR changes correctly afterwards, and that the rank updates.

### Everywhere
- [ ] Every page at phone width (375px): no sideways scrolling, buttons easy to tap.
- [ ] Keyboard only: Tab through lessons, quiz, and duel. Can you do everything? Is the focus ring visible?
- [ ] Switch BG ↔ EN on every page. Is any text untranslated?
- [ ] Type URLs that don't exist, like `/Lessons/99999` or `/Profile/nobody`. Do you get a friendly 404?
- [ ] Paste very long text and `<script>alert(1)</script>` into any text field.

## Filing bugs

File each bug as a **new GitHub issue** right away, one bug per issue:

```markdown
**What happened:**
**What should happen:**
**Steps to reproduce:**
1.
2.
**Device / browser:**
**Account(s) used:**
**Screenshot / video:**
```

Labels: an `area:*` label, plus `bug`. Add `priority:high` if it blocks playing or loses progress.

## After the session

- Triage together (20 min): for each bug, decide **fix before launch** or **`after-v1`**, and assign an owner.
- Fix-before-launch bugs go into the M3 milestone.
- #32 acceptance: every bug-bash bug is closed or labelled `after-v1`.
