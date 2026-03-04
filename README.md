# Unity Internal Access
This package provides reflection-based access to a number of Unity's internal engine and editor APIs.
It is mostly intended to support some of my [other packages](https://github.com/jonagill), but can be used on its own as well.

## Installation
### Install via Git
1. Open Window/Package Manager
2. Click the + button
3. Select Add Package From Git URL
4. Paste `https://github.com/jonagill/UnityInternalAccess.git?path=Packages/com.jonagill.unityinternalaccess` into the URL field
5. Click Install

### Installation via OpenUPM
To install via [OpenUPM](https://openupm.com/packages/com.jonagill.autofill/):

1. Open `Edit/Project Settings/Package Manager`
2. Add a new Scoped Registry (or edit the existing OpenUPM entry) to read:
    * Name: `package.openupm.com`
    * URL: `https://package.openupm.com`
    * Scope(s): `com.jonagill.unityinternalaccess`
3. Click Save (or Apply)
4. Open Window/Package Manager
5. Click the + button
6. Select Add package by name...
7. Enter `com.jonagill.unityinternalaccess` and click Add

