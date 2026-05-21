---
name: Docs site typography conventions
description: Lato/Roboto font stack the user wants enforced site-wide across the Hugo docs in docs/
type: feedback
---

The Hugo docs site under `docs/` uses these font conventions and they must apply site-wide, not just on the home page:

- **H1**: Lato Black (900)
- **H2–H6**: Lato Bold (700)
- **Body copy**: Roboto

**Why:** The user established these conventions earlier and noticed I had scoped them to home-only via `{{ if .IsHome }}` in `docs/layouts/partials/custom/head-end.html`, which left subpages on the theme defaults. They corrected this and want it consistent.

**How to apply:** When adding any new font/typography rule in this repo, put it in `docs/assets/css/custom.css` and scope it so it applies across the whole site (e.g. `main#content h1..h6`), not behind a page-type conditional. Fonts are loaded once in `docs/layouts/partials/custom/head-end.html`. If introducing new headings, weights, or font usages, follow the Lato-headings / Roboto-body split.
