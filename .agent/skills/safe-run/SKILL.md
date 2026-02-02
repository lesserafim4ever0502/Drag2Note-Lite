---
name: safe-run
description: Safely executes Python scripts in Windows CMD by handling encoding (UTF-8) and Conda environment activation. Use this whenever the user asks to "run code", "execute script", or "start python".
---

# Safe Python Execution Skill

This skill ensures Python runs without crashing on Windows CMD by enforcing UTF-8 encoding and activating the correct Conda environment.

## Dependencies
- Requires the user to have defined `{{TargetCondaEnv}}` in the global context.

## Workflow
When the user wants to run a Python command (e.g., `python script.py`):

1.  **Construct the Command Chain**:
    Always combine the following commands using `&&`:
    - `chcp 65001` (Fix terminal encoding)
    - `set PYTHONIOENCODING=utf-8` (Fix output encoding)
    - `conda activate {{TargetCondaEnv}}` (Activate environment)
    - `python [script_name] [arguments]` (The actual task)

2.  **Example Output**:
    If the user asks to run `main.py`, generate:
    ```cmd
    chcp 65001 && set PYTHONIOENCODING=utf-8 && conda activate {{TargetCondaEnv}} && python main.py
    ```

3.  **Safety Check**:
    - Ensure `{{TargetCondaEnv}}` is not explicitly "base" unless authorized.
    - Wait for user approval before execution.