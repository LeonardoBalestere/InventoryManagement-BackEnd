2. Strict containerization
Status: Accepted

Context: Deployment requires zero-cost leveraging of the Azure for Students 750h/month free tier. The 1GB RAM and 64GB SSD constraints demand a lightweight host OS, mandatory Swap file configuration (1GB), and container log rotation to prevent disk exhaustion.

Decision: Strict containerization via Docker Engine on an Azure B1s Linux VM (Ubuntu 24.04), fronted by an Nginx Reverse Proxy (HTTPS/Let's Encrypt) and strict Docker log-driver limits (max-size: 10m).

Consequences:
- Positive: We gain exact environment parity, predictable routing, and zero hosting costs.
- Negative: We lose PaaS conveniences, assuming full responsibility for OS hardening (UFW, Fail2Ban), SSH key management, and reverse proxy maintenance.
