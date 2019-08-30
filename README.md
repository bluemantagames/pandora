# Pandora

This is a [Unity](https://unity.com) project

## Install packages from nuget.org

Example installing google protobuf library:

```
.\nuget install Google.Protobuf -Version 3.8.0 -ConfigFile .\nuget.config -Source https://api.nuget.org/v3/index.json
```

Then go to `packages\<package_name>\lib` and delete every folder except `net45`

