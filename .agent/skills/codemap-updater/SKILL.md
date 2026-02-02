---
name: codemap-updater
description: Passive documentation agent. Automatically maintains `CODEMAP.md` whenever file structure changes. Analyzes architecture for documentation purposes.
---

# CodeMap Updater Skill

This skill acts as a background service to keep the project's architectural map (`CODEMAP.md`) synchronized with the actual codebase.

## Available Tools
The agent should utilize the following MCP tools:
- **Filesystem**: `list_directory`, `read_file`, `write_file`
- **System**: `run_terminal_command` (optional, for `tree` command if available)

## Workflows

### 1. ⚡ Auto-Sync (Implicit Background Hook)
*Trigger: Automatically runs AFTER the agent creates, moves, or deletes files. Also runs when user says "code changed" or "sync structure".*

1.  **Check**: Verify if `CODEMAP.md` exists. (If not, switch to "Genesis Map" workflow).
2.  **Silent Diff**: 
    - Compare current file list with `CODEMAP.md` entries.
    - **CRITICAL**: If no structural changes (files added/removed/moved), STOP. Do not waste tokens.
3.  **Update**: 
    - Read only the *new* files to generate summaries.
    - Remove entries for deleted files.
    - Re-generate the ASCII Tree.
4.  **Quiet Save**: 
    - Overwrite `CODEMAP.md`.
    - **Notification**: Do NOT chat about this. Just append a small checkmark to the final output (e.g., "✅ CODEMAP synced").

### 2. 🗺️ Genesis / Full Refresh (Explicit)
*Use when user asks to "create codemap", "remap project", or "generate documentation".*

1.  **Scan**: 
    - Call `list_directory` recursively (depth limit: 3 initially).
    - **Ignore**: `node_modules`, `.git`, `__pycache__`, `dist`, `.DS_Store`.
2.  **Analyze**: 
    - Identify key entry points (e.g., `main.py`, `src/index.js`).
    - Call `read_file` to generate a 1-sentence "Responsibility Summary" for key files.
3.  **Draft**: 
    - Generate ASCII Directory Tree.
    - Create "Module Details" table (`File` -> `Responsibility` -> `Key Exports`).
4.  **Persist**: 
    - Call `write_file` to save to `CODEMAP.md`.

### 3. 🔍 Deep Dive (Specific Scope)
*Use when user asks to "explain the utils folder" or "map the auth module".*

1.  **Target**: Identify the subdirectory (e.g., `./src/utils`).
2.  **Detailed Scan**: 
    - Call `list_directory` with deeper recursion (up to level 5).
    - Read all non-trivial files in that scope.
3.  **Report**: 
    - Append a "## Sub-module: [Name]" section to `CODEMAP.md`.
    - Also output the explanation directly to the chat.

## Constraints
- **Language Adaptability**: 
    - Logic must follow these English instructions.
    - **Output must match User's Language**: If user speaks Chinese, the summaries in `CODEMAP.md` and the chat response MUST be in Chinese.
- **Token Efficiency**: Do not read large assets or minified code.
- **Formatting**: Use standard ASCII characters (`├──`, `└──`).
- **Safety**: Never overwrite source code; only write to `CODEMAP.md`.