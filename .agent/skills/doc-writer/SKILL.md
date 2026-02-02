---
name: doc-writer
description: Generates professional documentation, docstrings, and README files. Use when user asks to "document this", "add comments", or "write readme".
---

# Documentation Writer Skill

Standardizes code documentation and project explanations.

## Standards
- **Python**: Use **Google Style** docstrings (Triple quotes `"""`).
- **Language**: Use English for code comments, but use Chinese for high-level summaries/README if the user speaks Chinese.

## Workflows

### 1. Function/Class Documentation
When asking to document a specific file:
1.  Read the code to understand inputs (Args), outputs (Returns), and exceptions (Raises).
2.  Insert docstrings immediately after the function/class definition.
3.  **Do not** change the code logic, only add comments/docstrings.

### 2. README Generation
When asking to "generate readme":
1.  Scan the project structure (`dir` or `ls`).
2.  Create/Update `README.md` with:
    - **Project Title**: Folder name or inferred from content.
    - **Description**: What does this project do?
    - **Setup**: How to install (referencing `requirements.txt` if present).
    - **Usage**: Example commands to run the script.