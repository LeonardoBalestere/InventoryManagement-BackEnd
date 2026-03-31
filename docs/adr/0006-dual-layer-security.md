6. Dual-layer security
Status: Accepted

Context: Mandatory mitigation of malicious script injection vectors (Cross-Site Scripting) in free-text fields, while simultaneously protecting the Azure VM's public IP from Layer 7 DDoS attacks without incurring Azure Web Application Firewall costs.

Decision: Dual-layer security: Edge WAF and DNS masking via Cloudflare, combined with Application-level Input Sanitization (HtmlSanitizer) for Anti-XSS.

Consequences:
- Positive: We gain defense-in-depth security, masking the true infrastructure IP, and guaranteeing clean data persistence.
- Negative: We lose some CPU cycles on the backend due to HTML/Regex parsing latency before business rule execution.
