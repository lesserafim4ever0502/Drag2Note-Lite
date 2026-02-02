---
name: git-ops
description: Comprehensive version control agent. Combines local Git operations (commit, log, status) with GitHub remote operations (create repo, pull request, issue).
---

# Git & GitHub Operations Skill

This skill allows the agent to orchestrate workflows that span across local disk (Git) and the cloud (GitHub).

## Available Tools
The agent should utilize the following MCP tools:
- **Local**: `git_status`, `git_add`, `git_commit`, `git_push` (via terminal command)
- **Remote**: `create_repository` (GitHub), `create_pull_request` (GitHub)

## Workflows

### 1. 📦 Smart Commit (Local Only)
*Use when user asks to "save", "commit", or "checkpoint".*

1.  **Check**: Call `git_status` to identify changes.
2.  **Diff**: Call `git_diff_unstaged` to understand the context.
3.  **Commit**: 
    - Generate a Conventional Commit message (e.g., `feat: ...`, `fix: ...`).
    - Call `git_add` for the modified files.
    - Call `git_commit` with the generated message.

### 2. 🚀 Zero to Cloud (Local + GitHub)
*Use when user asks to "publish", "upload to github", or "initialize new repo".*

1.  **Local Init**:
    - If `.git` is missing, run `git init` (terminal).
    - Call `git_add` and `git_commit` (message: "chore: initial commit").
2.  **Remote Create**:
    - **CRITICAL**: Call `github.create_repository` using the current folder name as the repo name. Set `private: true` by default.
3.  **Sync**:
    - Run terminal command: `git remote add origin <html_url_from_previous_step>`
    - Run terminal command: `git push -u origin main`

### 3. 🔀 Quick PR (Local + GitHub Combined)
*Use when user asks to "create PR", "push and PR", or "submit changes".*

1.  **Local Commit**: Perform the "Smart Commit" steps (Add & Commit).
2.  **Push**: Execute `git push` (terminal).
3.  **Create PR**: 
    - Call `github.create_pull_request`.
    - Use the commit message as the PR title.
    - Set the `base` to "main" (or "master") and `head` to the current branch.
    - **Draft**: Create the description based on `git_diff`.

## Constraints
- When using `create_repository`, always check if the remote 'origin' already exists to avoid errors.
- If `git_commit` fails (e.g., nothing to commit), notify the user but proceed to push/PR if that was the intent.