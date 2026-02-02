---
name: pkg-manager
description: Handles package installation and dependency management safely. Use when user asks to "install library", "add package", or "setup requirements".
---

# Package Manager Skill

Safely manages Python/Node dependencies by enforcing environment activation and using fast tools.

## Dependencies
- Context Variable: `{{TargetCondaEnv}}` (Required)

## Strategy
1.  **Environment Check**: NEVER install packages in the `(base)` environment unless explicitly told.
2.  **Tool Priority**: 
    - For Python: Use `uv pip install` (fastest) > `pip install` (fallback).
    - For Node: Use `npm install`.

## Workflows

### Install a Python Package
When user asks to install `[package_name]`:
1.  Construct the CMD command chain:
    ```cmd
    conda activate {{TargetCondaEnv}} && uv pip install [package_name]
    ```
2.  If `uv` is not available or fails, fall back to:
    ```cmd
    conda activate {{TargetCondaEnv}} && pip install [package_name]
    ```

### Install from requirements.txt
When user asks to "install dependencies":
1.  Check if `requirements.txt` exists.
2.  Run:
    ```cmd
    conda activate {{TargetCondaEnv}} && uv pip install -r requirements.txt
    ```