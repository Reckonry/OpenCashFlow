# GitHub CLI Integration

**Status**: ⚠️ Not Yet Configured
**Updated**: 2025-10-13

---

## Setup Required

GitHub CLI (gh) is not currently installed on this system. Follow these steps to enable GitHub Issues integration with JITD.

### Installation Steps

**macOS**:
```bash
brew install gh
```

**Linux (Debian/Ubuntu)**:
```bash
sudo apt install gh
```

**Linux (Fedora/RHEL)**:
```bash
sudo dnf install gh
```

**Windows**:
```bash
# Using winget
winget install --id GitHub.cli

# Or download from https://cli.github.com
```

### Authentication

After installation, authenticate with GitHub:

```bash
gh auth login
```

Follow the prompts:
1. Choose "GitHub.com"
2. Choose "HTTPS" protocol
3. Authenticate via web browser (recommended) or paste token
4. Complete authentication

### Verify Installation

```bash
# Check version
gh --version

# Test authentication
gh auth status

# List issues from current repo
gh issue list
```

---

## Common Commands

### List Your Issues

```bash
# All issues assigned to you
gh issue list --assignee @me

# Filter by state
gh issue list --assignee @me --state open
gh issue list --assignee @me --state closed

# Filter by label
gh issue list --label bug
gh issue list --label enhancement
```

### View Issue Details

```bash
# View issue in terminal
gh issue view 123

# Open issue in browser
gh issue view 123 --web

# View with comments
gh issue view 123 --comments
```

### Create Issue

```bash
# Interactive creation
gh issue create

# With flags
gh issue create --title "Add Stripe integration" --body "Implement payment processing with Stripe API" --label enhancement

# From template
gh issue create --template bug_report.md
```

### Update Issue

```bash
# Add labels
gh issue edit 123 --add-label "in-progress"
gh issue edit 123 --add-label "backend,priority-high"

# Remove labels
gh issue edit 123 --remove-label "todo"

# Change title
gh issue edit 123 --title "New title"

# Add to milestone
gh issue edit 123 --milestone "v2.0"

# Assign to user
gh issue edit 123 --add-assignee username
```

### Close Issue

```bash
# Close issue
gh issue close 123

# Close with comment
gh issue close 123 --comment "Fixed in PR #456"

# Reopen issue
gh issue reopen 123
```

### Comment on Issue

```bash
# Add comment
gh issue comment 123 --body "Working on this now"

# View comments
gh issue view 123 --comments
```

---

## JITD Workflow Integration

### Starting Work on Issue

Once GitHub CLI is installed and authenticated:

1. **Start JITD session**:
   ```bash
   /jitd:start
   ```
   This will list your assigned GitHub issues.

2. **Select issue to work on** (e.g., issue #42)

3. **Claude loads issue details** automatically

4. **Creates task documentation**:
   `.agent/tasks/TASK-42-feature-name.md`

5. **Begin implementation** with context loaded

### Completing Issue

1. **Finish implementation**

2. **Archive task documentation**:
   ```bash
   /jitd:update-doc feature TASK-42
   ```
   - Archives task doc with completion details
   - Updates system docs if needed

3. **Close GitHub issue**:
   ```bash
   gh issue close 42 --comment "Implemented in commit abc123"
   ```

4. **Update labels** (optional):
   ```bash
   gh issue edit 42 --add-label "completed"
   ```

### Best Practices

#### Commit Messages

Reference issue numbers in commits:

```bash
git commit -m "feat: Add Stripe payment integration

Implements subscription payment flow with Stripe API.

Fixes #42"
```

GitHub automatically links commits to issues when you use:
- `Fixes #42`
- `Closes #42`
- `Resolves #42`

#### Task Documentation

In `.agent/tasks/TASK-42-stripe-integration.md`:

```markdown
# TASK-42: Add Stripe Integration

## Ticket
- GitHub Issue: https://github.com/owner/repo/issues/42
- Status: Completed
- Labels: enhancement, backend

## Context
[Why this feature is needed]

## Implementation
[What was done]
```

#### Labels for Progress Tracking

Use labels to track progress:

```bash
# Starting work
gh issue edit 42 --add-label "in-progress" --remove-label "todo"

# Blocked
gh issue edit 42 --add-label "blocked"

# Ready for review
gh issue edit 42 --add-label "ready-for-review"

# Completed
gh issue close 42 --add-label "completed"
```

---

## Integration with Gestione Incassi Workflow

### Current Repository

This project is located at:
```
/Users/lorenzosalami/Documents/GitHub/Scunio/GestioneIncassi
```

After installing `gh`, verify it detects the correct repository:

```bash
cd /Users/lorenzosalami/Documents/GitHub/Scunio/GestioneIncassi
gh repo view
```

Expected output: Shows your repository details (Scunio/GestioneIncassi)

### Project-Specific Labels

Consider creating these labels for better organization:

```bash
# Create labels
gh label create "api" --description "Backend API changes" --color "0366d6"
gh label create "app" --description "Frontend app changes" --color "5319e7"
gh label create "admin" --description "Admin portal changes" --color "d73a4a"
gh label create "database" --description "Database migrations" --color "fbca04"
gh label create "stripe" --description "Stripe integration" --color "0e8a16"
gh label create "priority-high" --description "High priority" --color "d93f0b"
gh label create "priority-low" --description "Low priority" --color "cccccc"
```

### Issue Templates

Create issue templates in `.github/ISSUE_TEMPLATE/`:

**Feature Request** (`.github/ISSUE_TEMPLATE/feature_request.md`):
```markdown
---
name: Feature Request
about: Suggest a new feature
labels: enhancement
---

## Description
[What feature do you want?]

## Use Case
[Why is this needed?]

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2

## Technical Notes
[Any technical considerations]
```

**Bug Report** (`.github/ISSUE_TEMPLATE/bug_report.md`):
```markdown
---
name: Bug Report
about: Report a bug
labels: bug
---

## Description
[What's broken?]

## Steps to Reproduce
1. Step 1
2. Step 2

## Expected Behavior
[What should happen?]

## Actual Behavior
[What actually happens?]

## Environment
- Project: API / App / Admin
- Environment: Development / Staging / Production
```

---

## Troubleshooting

### GH Command Not Found

**Problem**: `gh: command not found`

**Solution**:
1. Install GitHub CLI (see Installation Steps above)
2. Restart your terminal
3. Verify: `gh --version`

### Authentication Failed

**Problem**: `gh auth status` shows "not logged in"

**Solution**:
```bash
gh auth login
# Follow prompts
```

### Wrong Repository

**Problem**: `gh` commands affect wrong repository

**Solution**:
1. Check current directory: `pwd`
2. Verify repo: `gh repo view`
3. Use `-R` flag to specify repo:
   ```bash
   gh issue list -R Scunio/GestioneIncassi
   ```

### Permission Denied

**Problem**: Cannot create/edit issues

**Solution**:
- Ensure you have write access to repository
- Re-authenticate: `gh auth refresh`
- Check token scopes: `gh auth status`

### Rate Limiting

**Problem**: "API rate limit exceeded"

**Solution**:
- Authenticated requests have higher limits (5000/hour vs 60/hour)
- Ensure you're authenticated: `gh auth status`
- Wait for rate limit to reset (shown in error message)

---

## Next Steps

1. **Install GitHub CLI**: Follow installation steps above
2. **Authenticate**: Run `gh auth login`
3. **Test**: Run `gh issue list --assignee @me`
4. **Try JITD**: Run `/jitd:start` to see your issues
5. **Create first task doc**: Use `/jitd:update-doc feature TASK-XX`

---

## Alternative: Manual Workflow (No gh CLI)

If you prefer not to install GitHub CLI, you can still use JITD with manual issue tracking:

1. Check GitHub Issues in browser
2. Manually create task docs:
   ```
   Write(
     file_path: ".agent/tasks/TASK-42-feature.md"
     content: [task details from GitHub issue]
   )
   ```
3. Work on implementation
4. Archive with `/jitd:update-doc feature TASK-42`
5. Close issue manually in GitHub web UI

---

**GitHub CLI + JITD = Seamless issue-to-documentation workflow** 🚀

**Status**: ⚠️ Waiting for GitHub CLI installation
**Last Updated**: 2025-10-13
