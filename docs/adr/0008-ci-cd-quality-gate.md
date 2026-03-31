8. GitHub Actions CI/CD Quality Gate
Status: Accepted

Context: Need for an automated, rigorous Quality Gate on the main branch to prevent bad code, untested logic, or architectural violations from being merged, utilizing native and free Student Pack tools instead of relying on local GPU/LLM infrastructure.

Decision: GitHub Actions workflow integrating xUnit, Codecov (strict 80% coverage threshold), CodeQL (SAST), and GitHub Copilot Code Review.

Consequences:
- Positive: We gain a highly automated, senior-level CI/CD pipeline, absolute deployment confidence, and semantic AI reviews without managing local LLM infrastructure.
- Negative: We increase the Pull Request feedback loop time and strictly block rapid, untested hotfixes.
