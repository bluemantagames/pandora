#!/usr/bin/env bash

MINOR="$(git describe --tags | grep -oE 'v[0-9]+\.[0-9]+' | grep -oE '.[0-9]+$' | tail -c +2)"

if [[ $(($MINOR % 2)) -eq 0 ]]; then
  RELEASE_TYPE="internal"
else
  RELEASE_TYPE="alpha"
fi

echo "::set-output name=release_type::${RELEASE_TYPE}"
