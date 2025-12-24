# Deploy Guide

## Prerequisites

1. **Minikube** running
2. **CNPG Operator** installed

## Step 1: Install CNPG Operator

```bash
kubectl apply -f https://raw.githubusercontent.com/cloudnative-pg/cloudnative-pg/release-1.23/releases/cnpg-1.23.0.yaml

# Wait for operator to be ready
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=cloudnative-pg -n cnpg-system --timeout=120s
```

## Step 2: Deploy Resources

```bash
# Create namespace
kubectl apply -f k8s/namespace.yaml

# Create CNPG credentials
kubectl apply -f k8s/cnpg-secret.yaml

# Create PostgreSQL cluster (2 instances)
kubectl apply -f k8s/cnpg-cluster.yaml

# Wait for cluster to be ready
kubectl wait --for=condition=Ready cluster/expense-pg -n training --timeout=300s

# Check services created by CNPG
kubectl get svc -n training
# Expected: expense-pg-rw, expense-pg-ro, expense-pg-r

# Build Docker image
eval $(minikube docker-env)
docker build -t training-expensetracker-api:latest .

# Deploy API
kubectl apply -f k8s/api.yaml

# Check pods
kubectl get pods -n training
```

## Step 3: Verify

```bash
# Check logs for DB connection
kubectl logs -f deploy/expense-api -n training | grep "\[DB"

# Expected output:
# [DB] WriteConnection configured: expense-pg-rw
# [DB] ReadConnection configured: expense-pg-ro

# Test health endpoint
minikube service expense-api-service -n training --url
# Then visit: <URL>/health
```

## Step 4: Test Read/Write Split

```bash
# Register (WRITE)
curl -X POST http://<URL>/api/auth/register -H "Content-Type: application/json" \
  -d '{"username":"test1","password":"test123"}'

# Login (READ)
curl -X POST http://<URL>/api/auth/login -H "Content-Type: application/json" \
  -d '{"username":"test1","password":"test123"}'

# Check logs to see [DB:WRITE] and [DB:READ]
kubectl logs deploy/expense-api -n training | grep "\[DB:"
```

## Cleanup Old Postgres (Optional)

If you want to remove the old postgres deployment:

```bash
kubectl delete -f k8s/postgres.yaml
```

## Files

| File | Description |
|------|-------------|
| `cnpg-cluster.yaml` | CNPG PostgreSQL cluster (2 instances) |
| `cnpg-secret.yaml` | Database credentials |
| `api.yaml` | API deployment with Read/Write connection strings |
| `postgres.yaml` | OLD - Single PostgreSQL (can be deleted) |
