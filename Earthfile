ARG UNITY_VERSION=2020.1.17f1
ARG BUILD_TARGET=android
FROM unityci/editor:ubuntu-$UNITY_VERSION-$BUILD_TARGET-0.9.0

request-license:
    COPY earthly .
    ENV UNITY_VERSION $UNITY_VERSION
    RUN earthly/request-license.sh
    SAVE ARTIFACT Unity_v$UNITY_VERSION.alf AS LOCAL earthly/Unity_v$UNITY_VERSION.alf

activate-license:
    COPY earthly .
    ENV UNITY_LICENSE_FILE=earthly/license.ulf
    RUN earthly/activate-license.sh
