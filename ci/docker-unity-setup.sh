docker run -it --rm \
	-e "UNITY_USERNAME=stoke95+unityci@gmail.com" \
	-e "UNITY_PASSWORD=zWHqQt9Lecxz64" \
	-e "UNITY_SERIAL=UU-DDAI-KAO-LOUIA-QQQQ-4444" \
	-e "TEST_PLATFORM=linux" \
	-e "WORKDIR=/root/project" \
	-v "$(pwd)/..:/root/project" unity-android bash
