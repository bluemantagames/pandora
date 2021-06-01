#!/bin/bash
# This script works around git stripping file extensions during merge on linux

set -x

F1=$(echo "$1" | tail -c '+2')
F2=$(echo "$2" | tail -c '+2')
F3=$(echo "$3" | tail -c '+2')

mv "$1" "$F1.yaml"
mv "$2" "$F2.yaml"
mv "$3" "$F3.yaml"

'/home/sandro/Unity/Hub/Editor/2020.1.17f1/Editor/Data/Tools/UnityYAMLMerge' merge -h -p --force "$F1.yaml" "$F2.yaml" "$F3.yaml" "$F3.yaml"

mv "$F1.yaml" "$1"
mv "$F2.yaml" "$2"
mv "$F3.yaml" "$3"
