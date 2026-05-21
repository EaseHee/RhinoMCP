---
name: Add editor byline to edited docs pages
description: When editing a Hugo docs page authored by someone else, add editor: SteveF to frontmatter under author:
type: feedback
---

When editing any docs page under `docs/content/` that has an existing `author:` frontmatter field (e.g. `author: Callum`), add an `editor: SteveF` line directly beneath it. If `editor:` is already present, leave it alone — don't append a list or duplicate.

**Why:** The user (Steve Fuchs) wants attribution that distinguishes original authorship from his editorial passes. Established on the docs-edits branch where he was revising Callum's pages across the Hugo site.

**How to apply:**
- Triggers when editing any `.md` file under `docs/content/`.
- Skip if the page lists him as `author:` already (he's the author, not the editor of his own page).
- Hugo doesn't currently render `editor:` — it's metadata-only, exposed as `.Params.editor` if a template ever wants it. Don't add template wiring unless asked.
- He goes by `SteveF` (not `Steve`) to disambiguate from Steve Baer; see [user_identity.md](user_identity.md).
