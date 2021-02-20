We use [earthly](https://earthly.dev) to automate QA, internal and production releases.

# Prerequisites

- Docker
- [Earthly](https://earthly.dev/get-earthly)

# Request license file

First of all we need to generate a license file tied to the docker containers we use to build the game. This means that you *cannot* transfer this file between different computers.

Run:

```
earthly +request-license
```

This will generate a file `earthly/Unity_v$UNITY_VERSION.alf`; upload it to https://license.unity3d.com and save the resulting file to `earthly/license.ulf`
