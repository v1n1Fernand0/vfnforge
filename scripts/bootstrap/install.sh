#!/usr/bin/env bash
set -euo pipefail

branch="main"
repo="https://github.com/v1n1Fernand0/vfnforge"
install_cli=true

usage() {
  cat <<'USAGE'
Uso: install.sh [opcoes]
  -b|--branch <nome>   Branch/tag do repo (default: main)
  --repo <url>         URL base do repo GitHub
  --no-cli             Nao instala o global tool (apenas template)
  -h|--help            Ajuda
USAGE
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -b|--branch)
      branch="$2"; shift 2;;
    --repo)
      repo="$2"; shift 2;;
    --no-cli)
      install_cli=false; shift;;
    -h|--help)
      usage; exit 0;;
    *)
      echo "Argumento desconhecido: $1" >&2
      usage; exit 1;;
  esac
done

zip_url="${repo}/archive/refs/heads/${branch}.zip"
tmp_dir="$(mktemp -d)"
zip_path="${tmp_dir}/repo.zip"
trap 'rm -rf "$tmp_dir"' EXIT

echo "Baixando ${zip_url} ..."
curl -sSL "$zip_url" -o "$zip_path"
unzip -q "$zip_path" -d "$tmp_dir"
repo_dir="$(find "$tmp_dir" -maxdepth 1 -type d -name '*vfnforge*' | head -n 1)"
if [[ -z "$repo_dir" ]]; then
  echo "Nao encontrei a pasta do repo no zip" >&2
  exit 1
fi

template_path="$repo_dir/templates/vfnforge"
echo "Instalando template..."
dotnet new install "$template_path" --force

if [[ "$install_cli" == true ]]; then
  cli_project="$repo_dir/tools/VFNForge.Cli/VFNForge.Cli.csproj"
  if [[ -f "$cli_project" ]]; then
    pack_dir="${tmp_dir}/cli-pack"
    mkdir -p "$pack_dir"
    echo "Empacotando CLI..."
    dotnet pack "$cli_project" -c Release -o "$pack_dir" >/dev/null
    dotnet tool uninstall --global VFNForge.Cli >/dev/null 2>&1 || true
    dotnet tool install --global --add-source "$pack_dir" VFNForge.Cli --ignore-failed-sources
  else
    echo "CLI nao encontrado, pulando instalacao." >&2
  fi
fi

echo "Tudo pronto! Rode 'vfnforge api -n MinhaApp'."
