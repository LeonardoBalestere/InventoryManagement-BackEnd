7. APM and Infrastructure Monitoring
Status: Accepted

Context: Local structured logging containers consume excessive RAM and disk space, threatening the Azure B1s VM constraints. Leveraging GitHub Student Pack tools provides Enterprise-grade observability at zero cost.

Decision: APM and infrastructure monitoring via Datadog Agent (OS-level), coupled with Sentry for real-time application crash reporting.

Consequences:
- Positive: We gain advanced audit capabilities, granular stack-trace capture, and infrastructure metrics without consuming the VM's resources.
- Negative: We introduce vendor lock-in with Datadog/Sentry and rely on outbound network calls to transmit telemetry data.
