#!/bin/bash

docker run --rm -v "$(pwd):/root/project" -w /root/project/ci gableroux/unity3d:2019.2.2f1-android ./build.sh
