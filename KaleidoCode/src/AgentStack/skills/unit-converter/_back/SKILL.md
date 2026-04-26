---
name: unit-converter
description: Convert between common units using a multiplication factor. Use when asked to convert miles, kilometers, pounds, or kilograms.
---

## Usage

When the user requests a unit conversion:
1. First, review `references/conversion-table.md` to find the correct factor.
2. **Environment Check & Execution:**
   - **If Python is available:** Run `scripts/convert.py` with `--value <number> --factor <factor>` (e.g. `--value 26.2 --factor 1.00000`).
   - **If Python is unavailable (Windows/PowerShell environment):** Run `powershell -ExecutionPolicy Bypass -File scripts/convert.ps1` with `-Value <number> -Factor <factor>`.
3. Present the converted value clearly with both units.