# Playtest kit (issue #32)

**Goal:** at least **10 people outside the team** play Histador for **one week**. Each completes at least **one lesson and one duel** and fills in the feedback form.

**When:** start as soon as lessons, quizzes, the map, and duels are deployed (#31). Aim for the week after M3 features are done. That leaves time to fix things and tune before the school deadline.

**Coordinator:** Dev C. Everyone recruits 2–3 testers.

---

## 1. Before the week starts

- [ ] The app is deployed and the public URL works on phones (#31).
- [ ] At least ~15 lessons are published, so testers have choices and the map visibly changes.
- [ ] The duel question pool has enough questions (at least ~60 duel-usable ones) that the same question doesn't repeat every game.
- [ ] Create the Google Form from section 3 and test it once yourself.
- [ ] Make a shared sheet to track testers: *name, who recruited them, device, joined?, lesson done?, duel done?, form done?*
- [ ] Decide on a "duel hour" or two (e.g. Tuesday and Thursday 19:00) when the team is online. Random matchmaking then always finds someone. With few players, testers will otherwise sit in an empty queue.

## 2. Recruiting (10+ people)

Mix it up: classmates who like history *and* ones who don't, a parent or two, someone on an older phone.

Message you can send:

> Hi! For our school project we built **Histador**: short history lessons that uncover a world map, plus quiz duels against other players. Could you try it for a week? It's ~10 minutes a day. Do at least one lesson and one duel, then answer a 3-minute form. Link: <URL>. Thank you!

## 3. Feedback form (copy into Google Forms)

Use **linear scales 1–5** unless noted. Keep it short, because people stop answering long forms.

**About you**
1. How old are you? *(Under 14 / 14–17 / 18–25 / 26+)*
2. Which device did you mostly use? *(Phone / Tablet / Computer)*
3. How much do you like history? *(1 = not at all … 5 = love it)*

**Lessons**
4. Were the lessons interesting? *(1 = boring … 5 = very interesting)*
5. How did the lesson length feel? *(Too short / About right / Too long)*
6. Did the quiz feel fair, meaning the questions were covered in the lesson? *(1 = not fair … 5 = very fair)*
7. Which lesson did you like most, and why? *(short text)*

**Map**
8. Was it satisfying to see the fog lift from the map? *(1 = didn't care … 5 = loved it)*
9. Did you understand how to uncover more of the map? *(Yes / Partly / No)*

**Duels**
10. Were the duels fun? *(1 = not fun … 5 = very fun)*
11. How did the 15-second timer feel? *(Too short / About right / Too long)*
12. Did you get matched with an opponent quickly? *(Yes / Sometimes / No, I waited too long)*
13. Did you use a hint? If yes, did it help? *(Didn't use / Used, it helped / Used, it didn't help)*

**Overall**
14. Was anything confusing? Tell us what and where. *(long text)*
15. Did anything break or look wrong? What were you doing at the time? *(long text)*
16. Would you keep playing after this week? *(1 = no … 5 = definitely)*
17. Anything else? *(long text)*

## 4. During the week

- Day 1: send the link, and make sure everyone can register and log in.
- Day 3: send a reminder with the duel-hour times. Check the tracking sheet and nudge anyone who hasn't done a duel.
- Day 6: send the form link to everyone.
- Every day, a team member checks the error logs (Application Insights, #31) and files new bugs.

## 5. After the week: turning feedback into changes

1. Export the form responses. For each 1–5 question, note the **average** and the **lowest answers**.
2. Read every text answer. Group them into themes, e.g. "the timer is too short" or "couldn't find the Random button".
3. Tuning (part 5 of #32):

   | Question(s) | What to tune | Where |
   |---|---|---|
   | 5 (lesson length) | If ≥ 40% say "too long", trim cards in the longest lessons | Admin editor |
   | 6 (quiz fairness) and quiz pass rates | Pass mark (60%) and unclear questions | Quiz settings / questions |
   | 11 (timer) | Duel timer (15 s) | Duel settings |
   | 12 + MMR spread | Matchmaking range, rank thresholds | Rank/matchmaking config |
   | 10, 13 | Hint rules, question difficulty | Duel question pool |

4. File each problem as a GitHub issue (see the bug template in `bug-bash.md`). Label anything you decide not to fix before launch as `after-v1`.
5. Write a short **playtest summary** for the README and the school presentation: number of testers, the key numbers, what you changed because of them, and one or two quotes.

## Acceptance check (from #32)

- [ ] 10+ outside players completed at least one lesson **and** one duel. Check this in the tracking sheet and the database.
- [ ] Every bug they reported is fixed or labelled `after-v1`.
