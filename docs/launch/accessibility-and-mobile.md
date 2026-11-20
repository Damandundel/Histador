# Accessibility and mobile rules for every page

These come from the launch-polish pass (#32). Most are already handled by the shared layout and `site.css`. Follow the rest when you build a new page.

## Already handled for you

- **Skip link:** the first Tab on any page shows "Skip to main content" and jumps to `<main id="main-content">`.
- **Focus:** links, buttons and anything with `tabindex` get a clear blue outline when focused with the keyboard (`:focus-visible`).
- **Tap targets:** `.btn`, `.nav-link`, form fields, the navbar toggler and footer links are at least **44px** tall.
- **Text size:** the base font is 16px on all screens. Avoid going below `0.875rem` (14px) for anything people need to read.
- **Reduced motion:** with "reduce motion" turned on in the OS, CSS animations and transitions are effectively switched off.
- **Nav:** collapses into the hamburger menu below 992px, so it never wraps or overflows on phones and tablets.

## Your checklist for a new page

1. **One `<h1>`** per page, and set `ViewData["Title"]`. If the design has no visible heading, use `<h1 class="visually-hidden">`.
2. **Images:** every `<img>` needs `alt`. Describe the image, or use `alt=""` if it's purely decorative.
3. **Icon-only buttons** (`+`, `−`, `⟲`, `✕`, emoji) need `aria-label="Zoom in"` etc.
4. **Form fields** need a `<label asp-for>` or an `aria-label`.
5. **Progress bars** (`role="progressbar"`) need an `aria-label`.
6. **Colour contrast:** text needs at least 4.5:1 against its background. Bootstrap's `text-secondary` and `text-muted` on coloured backgrounds (cards, parchment) often fail. Use a darker colour there. Check with the browser's DevTools colour picker.
7. **JavaScript animations** (D3 transitions, fog reveal, counters): check `window.matchMedia("(prefers-reduced-motion: reduce)").matches` and skip or shorten them. See `Views/Map/Index.cshtml` for the pattern.
8. **Keyboard:** everything you can click must also work with Tab plus Enter/Space. That includes lesson Next/Back, quiz answers and duel answers. Use real `<button>`s, not clickable `<div>`s.
9. **Phones:** check your page at **375px** and **768px** wide in DevTools' device mode. There should be no sideways scrolling.

## Automated checks

`Histador.Tests/Polish/AccessibilityTests.cs` checks every page listed in `AllPages`:

- `lang` is set, there's a title, and there's exactly one `<h1>`
- there's a skip link
- images have `alt`
- buttons and links have accessible names
- form fields have labels
- progress bars have labels
- `site.css` keeps the reduced-motion, 44px and focus rules

**When you add a page, add its URL to `AllPages`** (and to `PublicPages` if it doesn't need login).
