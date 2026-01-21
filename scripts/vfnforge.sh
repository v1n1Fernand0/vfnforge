#!/usr/bin/env bash
set -euo pipefail

command=${1:-install}
name=${2:-VFNForgeSample}
output=${3:-}

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo_root="$(cd "${script_dir}/.." && pwd)"
template_path="${repo_root}/templates/vfnforge"
if [[ -z "${output}" ]]; then
  smoke_root="${repo_root}/_out/smoke"
else
  mkdir -p "${output}"
  smoke_root="$(cd "${output}" && pwd)"
fi

install_template() {
  echo "Installing template from ${template_path}"
  dotnet new install "${template_path}" --force
}

uninstall_template() {
  echo "Uninstalling template at ${template_path}"
  dotnet new uninstall "${template_path}"
}

smoke_test() {
  install_template
  if [[ -d "${smoke_root}" ]]; then
    echo "Cleaning existing smoke directory: ${smoke_root}"
    rm -rf "${smoke_root}"
  fi
  mkdir -p "${smoke_root}"
  app_path="${smoke_root}/${name}"
  echo "Generating sample app ${name} in ${app_path}"
  dotnet new vfnforge -n "${name}" -o "${app_path}"
  sln="${app_path}/${name}.slnx"
  echo "Building ${sln}"
  dotnet build "${sln}"
  echo "Smoke test complete. Project located at ${app_path}"
}

case "${command}" in
  install)
    install_template
    ;;
  uninstall)
    uninstall_template
    ;;
  test)
    smoke_test
    ;;
  *)
    echo "Usage: $0 [install|uninstall|test] [AppName] [OutputDir]" >&2
    exit 1
    ;;
esac
