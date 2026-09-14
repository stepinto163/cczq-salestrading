# Performance testing

The user had to hard-reboot after an earlier grid benchmark hung the laptop.
Treat performance experiments conservatively:
- Never launch a performance test without explicit user authorization.
- Start with short, minimal-load runs and increase pressure only after reviewing responsiveness and results with the user.
- Keep grid virtualization enabled. Do not offer disabled-virtualization stress cases by default.
- Distinguish cached quote count from rendered rows; 10,000 quotes does not mean 10,000 rendered rows.
- Run tests under a separate process watchdog that can terminate a hung UI.
- Avoid concurrent benchmark instances and never automatically escalate load.
- Discuss in English; write documentation in Chinese.

# Naming and readability

- Prefer clear, descriptive class and method names over brevity.
- Do not use cryptic abbreviations such as `J` for shared utilities.
- Use one statement per line, conventional indentation, and separate field declarations. Do not compress methods, branches, or several operations onto a single line.
- When changing existing code, format the affected methods clearly and use descriptive local variable names.
- Keep utility responsibilities explicit; separate JSON handling, API replies and validation.
