#!/bin/bash

docker run --rm \
	-e "UNITY_USERNAME=stoke95+unityci@gmail.com" \
	-e "UNITY_PASSWORD=zWHqQt9Lecxz64" \
	-e "UNITY_SERIAL=UU-DDAI-KAO-LOUIA-QQQQ-4444" \
	-v "$(pwd):/root/project" -w /root/project/ci gableroux/unity3d:2019.2.2f1-android ./build.sh
