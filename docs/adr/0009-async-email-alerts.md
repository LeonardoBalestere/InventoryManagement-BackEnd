9. Async Email Alerts
Status: Accepted

Context: Critical inventory level alerts must be dispatched without blocking the main HTTP request thread. Standard SMTP is blocked by Azure (Port 25), necessitating HTTP API integration.

Decision: Asynchronous email alerts via SendGrid API orchestrated by .NET BackgroundServices (IHostedService) and the Outbox Pattern.

Consequences:
- Positive: We gain non-blocking HTTP responses and reliable email delivery.
- Negative: We add architectural complexity by introducing background workers and eventual consistency in the notification flow.
