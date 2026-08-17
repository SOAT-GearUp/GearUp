#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="gearup-validate-prod"

cleanup() {
  k3d cluster delete "$CLUSTER_NAME" >/dev/null 2>&1 || true
}
trap cleanup EXIT

k3d cluster create "$CLUSTER_NAME" --wait

# k8s/prod/external-secret.yaml usa o CRD do External Secrets Operator.
# --server-side evita o erro "metadata.annotations: Too long" do apply
# client-side (CRD grande demais para a annotation last-applied-configuration).
kubectl apply --server-side -f https://raw.githubusercontent.com/external-secrets/external-secrets/main/deploy/crds/bundle.yaml
kubectl create namespace gearup-prod --dry-run=client -o yaml | kubectl apply -f -

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

for file in k8s/prod/*.yaml; do
  echo "Validando $file"
  apply_with_retry "$file"
done

echo "Todos os manifests de k8s/prod validaram com sucesso."
