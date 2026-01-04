---
applyTo: "plans/**"
---

# Milestone Planning Instructions

## Planning File Structure

When creating or editing milestone planning files, use this template:

```markdown
# Milestone X: [Title]

## Overview
Brief description of milestone goals.

## Prerequisites
- [ ] What must be complete before starting
- [ ] Required assets/scenes/dependencies

## Implementation Steps

### Phase 1: [Name]
- [ ] Step 1.1: Specific task with file paths
- [ ] Step 1.2: Another specific task
  - Substep detail if needed
  - Expected outcome

### Phase 2: [Name]
- [ ] Step 2.1: ...

## Testing Checklist
- [ ] Test case 1
- [ ] Test case 2

## Files to Create/Modify
| File | Action | Purpose |
|------|--------|---------|
| `Scripts/X/Y.cs` | Create | Description |

## Blockers & Decisions
- Decision needed: [description]
- Blocked by: [description]

## Session Log
### [Date] Session N
- Completed: steps X, Y
- In progress: step Z
- Blocked: [if any]
```

## Session Workflow

### Start of Milestone
1. Create `plans/milestone-X.plan.md` with full breakdown
2. Reference `design_doc.md` for milestone requirements

### Each Session
1. Read `plans/milestone-X.plan.md` first
2. Work through phases, check boxes as you go
3. Update `specs/*.spec.md` implementation tables if needed
4. Add session log entry to plan file at end

### End of Milestone
1. Mark plan file as ✅ COMPLETE
2. Add version entry to `CHANGELOG.md`
3. Update `specs/*.spec.md` with final status
4. Mark milestone ✅ in `design_doc.md` §15

## Document Update Responsibilities

| Document | Updates When |
|----------|-------------|
| `plans/milestone-X.plan.md` | Every session (checkboxes, session log) |
| `specs/*.spec.md` | When implementation status changes |
| `CHANGELOG.md` | Milestone complete (release notes) |
| `design_doc.md` | Milestone complete (✅ marker only) |
