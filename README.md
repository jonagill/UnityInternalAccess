# Unity Internal Access
This package provides reflection-based access to a number of Unity's internal engine and editor APIs.
It is mostly intended to support some of my [other packages](https://github.com/jonagill), but can be used on its own as well.

## Installation
We recommend you install the Bulk Editor package via [OpenUPM](https://openupm.com/packages/com.jonagill.unityinternalaccess/). Per OpenUPM's documentation:

1. Open `Edit/Project Settings/Package Manager`
2. Add a new Scoped Registry (or edit the existing OpenUPM entry) to read:
    * Name: `package.openupm.com`
    * URL: `https://package.openupm.com`
    * Scope(s): `com.jonagill.unityinternalaccess`
3. Click Save (or Apply)
4. Open Window/Package Manager
5. Click the + button
6. Select `Add package by name...` or `Add package from git URL...` 
7. Enter `com.jonagill.unityinternalaccess` and click Add