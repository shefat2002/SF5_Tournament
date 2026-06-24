# 2026-06-24 — Arcade Red/Yellow Theme + AI Work-Log Convention

**Agent:** Claude Code
**Scope:** (1) establish the `/docs` AI work-log convention; (2) restyle the app to a light, red+yellow fighting-game/arcade aesthetic.

## Part 1 — work-log convention
- Added guideline #5 to `AGENTS.md` + a "AI Work Documentation Log (`/docs/`)" section: every code-shaping session appends a dated `/docs/YYYY-MM-DD-*.md` entry; `/docs/README.md` is the index. Declared two knowledge layers — `/docs` (history) and `code-review-graph` (structure).
- Created `/docs/README.md` + retroactive `2026-06-24-initial-build-and-deploy.md`.
- Rebuilt the code-review graph (28 files, 93 nodes, 5 communities, 2 flows).

## Part 2 — UI theme
**Direction:** arcade fight-poster — light parchment ground, rising-sun energy burst, impact type, thick ink borders + hard offset shadows.

**Files added/changed:**
- `wwwroot/images/bg.svg` (new) — radiating red/yellow sun-ray burst + glow over cream.
- `wwwroot/css/site.css` (rewritten) — CSS vars (cream/ink/red/yellow), Anton+Oswald fonts, body burst background + halftone grain overlay, restyled navbar/cards/tables/buttons/badges/forms, hero, staggered load animation. Overrides dark-theme Bootstrap utilities (`.bg-dark`, `.text-light`, `.border-secondary`, `.table-dark`, `.btn-outline-light`, …) so existing views inherit the light theme without per-view edits.
- `Views/Shared/_Layout.cshtml` — Google Fonts (Anton/Oswald), `data-bs-theme="light"`, black navbar with red+yellow borders, staggered main.
- `Views/Shared/_Layout.cshtml.css` — emptied (single source = site.css).
- `Views/Home/Index.cshtml` — energy hero (`STREET FIGHTER`, skewed tag, CTA).

## Verify
- `dotnet build` — clean.
- Local smoke: `/` 200, `/Auth/Login` 200, `bg.svg` 200, `site.css` theme markers present.
- Prod deploy `dep-d8tr5gok1i2s73e7k2tg` (commit `cc410f7`) — **live**; prod serves `bg.svg` (image/svg+xml), themed `site.css`, and the `sf-hero` markup.

## Commits
- `1a9f3eb` docs convention
- `cc410f7` feat(ui) theme

## Follow-ups
- Fonts load from Google Fonts CDN; for offline/self-host, bundle the font files.
- `UseHttpsRedirection` warning still present (Render terminates TLS at proxy).
