#!/usr/bin/env bash
set -euo pipefail

GITLAB_URL=${1:-https://git.6tm.eu/}
TOKEN=${2:-}
DESCRIPTION=${3:-local-docker-runner}
DOCKER_IMAGE=${4:-mcr.microsoft.com/dotnet/sdk:8.0}
TAGS=${5:-docker,dotnet}

if [ -z "$TOKEN" ]; then
  read -p "Registration token: " TOKEN
fi

docker run --rm -it -v /srv/gitlab-runner/config:/etc/gitlab-runner \
  gitlab/gitlab-runner register --non-interactive --url "$GITLAB_URL" --registration-token "$TOKEN" \
  --executor "docker" --description "$DESCRIPTION" --docker-image "$DOCKER_IMAGE" --tag-list "$TAGS" --run-untagged="true" --locked="false"

echo "Runner registration attempted. Verify in GitLab UI."
