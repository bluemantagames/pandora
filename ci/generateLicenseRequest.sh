docker run -ti --rm -v "$(pwd):/root" -w /root /root/project/ci gableroux/unity3d:2019.2.2f1-android /opt/unity/editor/unity -batchmode -nographics -logfile - -createManualActivationFile