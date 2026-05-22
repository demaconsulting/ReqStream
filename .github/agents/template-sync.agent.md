---
name: template-sync
description: Audits or synchronizes repository files against the canonical template.
  Supports four modes - Audit, Sync, Scaffold, and Recreate.
user-invocable: true
---

# Template Sync Agent

This agent is an orchestrator supporting four modes:

- **Audit** - report structural deviations; no changes
- **Sync** - patch missing sections into existing files
- **Scaffold** - create files that do not yet exist; skip existing files
- **Recreate** - rebuild existing files from the template, migrating old content

Read the template URL and `repository-map.md` from the `# Reference Template`
section in `AGENTS.md`, then map the requested scope onto the work groups below.
Delegate each group to a sub-agent.

# Work Groups

- **Root config files** - all non-collection files at the repository root
- **One group per flat `docs/` folder** - e.g. `docs/build_notes/`, `docs/user_guide/`
- **One group per system subtree** in `docs/design/`, `docs/verification/`, `docs/reqstream/` -
  each subtree and all its descendants is one group

# Orchestration

For each group intersecting the requested scope, call a sub-agent with:

- **context**:
  - Group scope and template URL from the `# Reference Template` section in `AGENTS.md`
  - Project-specific names substitute for placeholders at matching path depth
    (e.g. `SystemName` → `{SystemName}`, `system-name` → `{system-name}`)
  - If a template counterpart cannot be fetched, skip the file and report it
- **goal**:
  - Based on the given mode:
    - **Audit** - fetch each template counterpart; compare headings; report missing
      sections and depth mismatches; no changes
    - **Sync** - as Audit, then insert each missing section; run `pwsh ./fix.ps1`
    - **Scaffold** - fetch `repository-map.md` from the template URL in `AGENTS.md`
      to identify files that should exist but don't; for each, fetch the template,
      populate all sections, write the file; run `pwsh ./fix.ps1`
    - **Recreate** - read the existing file in full, then fetch the template; use
      full semantic understanding to map old content onto template sections,
      splitting or consolidating as needed; create extra sections for any content
      that has no template home; write the rebuilt file; run `pwsh ./fix.ps1`
  - When writing any section: HTML comments and TODO placeholders in the template
    are instructions - always resolve them to real content; infer from README,
    related files, sibling docs, and path; if confident write directly; if
    ambiguous offer 2–3 concrete options and ask the user; keep asking until they
    answer - never leave a TODO unless the user explicitly requests it

Collect sub-agent results and assemble the final report.

# Report Template

```markdown
# Template Sync Report

**Result**: (SUCCEEDED|FAILED)
**Mode**: (Audit|Sync|Scaffold|Recreate)

## Files

### {file-path}

- **Template**: {template path}
- **Missing sections**: {list or "none"}
- **Heading depth issues**: {list or "none"}
- **Action**: (Reported | Sections added | Created | Rebuilt | No template found)

## Summary

- **Conformant**: {count} | **Deviations**: {count} | **Updated**: {count}
```
