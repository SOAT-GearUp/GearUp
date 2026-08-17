#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="gearup-validate-dev"

cleanup() {
  k3d cluster delete "$CLUSTER_NAME" >/dev/null 2>&1 || true
}
trap cleanup EXIT

k3d cluster create "$CLUSTER_NAME" --wait

kubectl create namespace gearup-dev --dry-run=client -o yaml | kubectl apply -f -

apply_with_retry() {
  local file="$1"
  local attempt=1
  until kubectl apply --dry-run=server -f "$file"; do
    if [ "$attempt" -ge 3 ]; then
      echo "Falha ao validar $file após $attempt tentativas" >&2
      exit 1
    fi
    attempt=$((attempt + 1))
    sleep 3
  done
}

for file in k8s/dev/*.yaml; do
  echo "Validando $file"
  apply_with_retry "$file"
done

echo "Todos os manifests de k8s/dev validaram com sucesso."
