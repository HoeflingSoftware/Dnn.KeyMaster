# Dnn Key Master

[![Build Status](https://hoeflingsoftware.visualstudio.com/Dnn%20Key%20Master/_apis/build/status/HoeflingSoftware.Dnn.KeyMaster?branchName=master)](https://hoeflingsoftware.visualstudio.com/Dnn%20Key%20Master/_build/latest?definitionId=72?branchName=master) [![Automated Release Notes by gren](https://img.shields.io/badge/%F0%9F%A4%96-release%20notes-00B2EE.svg)](https://github-tools.github.io/github-release-notes/)

A Dnn configuration extension to secure your website with Azure Key Vault by removing database connection strings from the web.config and into an Azure Key Vault

## Setup

1. Get the latest [installer](https://github.com/HoeflingSoftware/Dnn.KeyMaster/releases)
2. Install extension into your Dnn website
3. Create Azure Key Vault and add full connection string as a secret
4. Log into your Dnn website, log in as host and navigate to: Settings -> Key Master
5. Enter configuration secrets
    1. Click Test button to verify connection
    2. Click Save to create secrets file on your website
    3. Click Start Key Master to update the web.config and start using Key Master

Need more help? Take a look at our detailed [Getting Started Guide](GETTING-STARTED.md)

## What's Protected?

| Feature                    | Protected |
|----------------------------|-----------|
| Database Connection String | Yes       |
| App Settings               | Yes       |


### Platform Support

| Supported Platforms | Supported | Version     |
|---------------------|-----------|-------------|
| Dnn 9			      | Yes       | 9.2.2 +     |
| Dnn 8               | No        | No          |

| Supported Admin Controls | Supported |
|--------------------------|-----------|
| Persona Bar              | Yes       |
| Traditional Menu         | No        |


## Contribute

Download Code

1. `$ git clone https://github.com/HoeflingSoftware/Dnn.KeyMaster.git`
2. `$ cd ./Dnn.KeyMaster`

Build 

1. `$ ./build.ps1`

Package

1. `$ ./build.ps1 -Target Package`

Deploy for Debugging

1. Open Visual Studio
2. Open build.cake located in solution items
3. Change variable `websiteLocation` to the location of a local install of Dnn
4. Execute `$ ./build.ps1 -Target Package`
5. Install the generated installer into your local install of Dnn
6. Execute `$ ./build.ps1 -Target Deploy`
7. Debug->Attach to Process: select w3wp.exe process

## Technology

This Dnn extension is built using the following features of Dnn:

* Dnn Persona Bar
* Dnn Web API
* Dnn Data Providers
* Dnn Membership Providers

## Created By: [@Andrew_Hoefling](https://twitter.com/andrew_hoefling) of [Hoefling Software](https://www.hoeflingsoftware.com)

* Twitter: [@Andrew_Hoefling](https://twitter.com/andrew_hoefling)
* Website [hoeflingsoftware.com](https://www.hoeflingsoftware.com)
* Blog: [andrewhoefling.com](http://www.andrewhoefling.com)

### License

The MIT License (MIT) see License File
