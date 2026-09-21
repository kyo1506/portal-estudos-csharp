#!/usr/bin/env bash
set -euo pipefail

# ==============================================================================
# Script de Deploy OCI com Podman para o Google Cloud Run (Free Tier)
# ==============================================================================

REGION="${GCP_REGION:-us-central1}"
REPO_NAME="${GCP_REPO_NAME:-portal-repo}"
SERVICE_NAME="${GCP_SERVICE_NAME:-portal-estudos}"

echo "========================================================================"
echo "  Deploy OCI Podman -> Google Cloud Run ($SERVICE_NAME)"
echo "========================================================================"

if ! command -v podman &>/dev/null; then
    echo "❌ Erro: Podman não está instalado."
    exit 1
fi

if ! command -v gcloud &>/dev/null; then
    echo "❌ Erro: Google Cloud SDK (gcloud) não foi encontrado no PATH."
    echo ""
    echo "Para instalar no Fedora Linux:"
    echo "  sudo dnf install google-cloud-cli"
    echo ""
    echo "Ou via instalador oficial do Google:"
    echo "  curl https://sdk.cloud.google.com | bash"
    echo "  exec -l \$SHELL"
    exit 1
fi

# Obter o projeto ativo configurado no gcloud
PROJECT_ID="$(gcloud config get-value project 2>/dev/null || true)"
if [[ -z "$PROJECT_ID" || "$PROJECT_ID" == "(unset)" ]]; then
    echo "⚠️ Nenhum projeto ativo configurado no gcloud."
    echo "Execute: gcloud config set project SEU_PROJECT_ID"
    exit 1
fi

REGISTRY_HOST="${REGION}-docker.pkg.dev"
IMAGE_URI="${REGISTRY_HOST}/${PROJECT_ID}/${REPO_NAME}/${SERVICE_NAME}:latest"

echo "📌 Projeto GCP:     $PROJECT_ID"
echo "📌 Região:          $REGION"
echo "📌 Repositório OCI: $REPO_NAME"
echo "📌 Imagem Alvo:     $IMAGE_URI"
echo ""

# 1. Habilitar APIs necessárias no Google Cloud
echo "⚙️ [1/6] Habilitando APIs necessárias (Artifact Registry e Cloud Run)..."
gcloud services enable artifactregistry.googleapis.com run.googleapis.com --project="$PROJECT_ID"

# 2. Criar repositório no Artifact Registry (se não existir)
echo "📦 [2/6] Verificando repositório no Artifact Registry..."
if ! gcloud artifacts repositories describe "$REPO_NAME" --location="$REGION" --project="$PROJECT_ID" &>/dev/null; then
    echo "   Criando repositório OCI '$REPO_NAME'..."
    gcloud artifacts repositories create "$REPO_NAME" \
        --repository-format=docker \
        --location="$REGION" \
        --description="Imagens OCI do Portal de Estudos C#" \
        --project="$PROJECT_ID"
fi

# 3. Autenticar o Podman no Artifact Registry do Google
echo "🔑 [3/6] Autenticando Podman no registry $REGISTRY_HOST..."
gcloud auth print-access-token | podman login -u oauth2accesstoken --password-stdin "$REGISTRY_HOST"

# 4. Build da imagem local com Podman
echo "🔨 [4/6] Construindo a imagem OCI com Podman..."
podman build -t "$IMAGE_URI" -f Dockerfile .

# 5. Push da imagem OCI para o Artifact Registry
echo "🚀 [5/6] Enviando imagem OCI para o Artifact Registry via Podman..."
podman push "$IMAGE_URI"

# 6. Deploy no Google Cloud Run com Scale to Zero e 256MB RAM
echo "🌐 [6/6] Realizando deploy no Cloud Run..."
gcloud run deploy "$SERVICE_NAME" \
    --image="$IMAGE_URI" \
    --region="$REGION" \
    --platform=managed \
    --allow-unauthenticated \
    --port=8080 \
    --memory=256Mi \
    --cpu=1 \
    --min-instances=0 \
    --max-instances=2 \
    --project="$PROJECT_ID"

echo ""
echo "✅ Deploy concluído com sucesso no Google Cloud Run!"
gcloud run services describe "$SERVICE_NAME" --region="$REGION" --format="value(status.url)"
