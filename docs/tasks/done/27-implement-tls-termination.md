---
title: Implement TLS Termination
---

## Description

Currently, the services are exposed over HTTP, which is insecure. This task is to implement TLS termination to ensure all external traffic is encrypted while maintaining a simple and clean setup process for developers.

## Proposed Solution

We will introduce a reverse proxy to handle TLS termination. This is a common pattern that centralizes TLS management and simplifies the application services.

### 1. Introduce a Reverse Proxy

We will add an Nginx reverse proxy to the `docker-compose.yml` file. This proxy will be the single entry point for all incoming traffic.

### 2. Configure TLS Termination

The Nginx proxy will be configured to:
- Listen on port `443` (HTTPS).
- Terminate the TLS connection using a certificate.
- Forward requests to the appropriate backend service (`vehicle-service` or `insurance-service`) over HTTP.

### 3. Streamline Developer Setup

To ensure the local development setup is as simple as possible, we will:
- Create a new `scripts/` directory to hold utility scripts.
- Rename the existing `configure-secrets.ps1` to `scripts/init.ps1`.
- Enhance this `init.ps1` script to handle the entire environment setup.

### 4. Certificate Management

The updated `scripts/init.ps1` script will automate the generation of self-signed certificates for local development.
- The script will check if certificates already exist and generate them only if they are missing.
- This ensures a developer only needs to run a single command to set up everything: secrets and certificates.
- For production environments, these self-signed certificates would be replaced with ones from a trusted Certificate Authority (CA).

### 5. Update Service Configuration

- The `vehicle-service` and `insurance-service` will no longer expose their ports directly in `docker-compose.yml`.
- The reverse proxy will be responsible for routing traffic to them.

### 6. Routing

The reverse proxy will be configured to route traffic based on the request path. For example:
- `https://localhost/vehicles/` -> `http://vehicle-service:8080/`
- `https://localhost/insurance/` -> `http://insurance-service:8080/`

This approach ensures that all external communication is encrypted while keeping the internal service-to-service communication simple and the developer setup process clean.

## Acceptance Criteria

- All external traffic must be served over HTTPS.
- The local development environment can be fully configured by running a single `init.ps1` script.
- A new `scripts/` directory is created.
- The `configure-secrets.ps1` script is moved and renamed to `scripts/init.ps1` and updated to include certificate generation.
- A new `nginx` service is added to the `docker-compose.yml` file.
- An `nginx.conf` file is created to configure the reverse proxy.
- The `README.md` file is updated to reflect the new setup process.
