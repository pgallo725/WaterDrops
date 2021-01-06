# WaterDrops
![GitHub License](https://img.shields.io/github/license/paolo257428/WaterDrops?style=flat-square)
![GitHub Release](https://img.shields.io/github/v/release/paolo257428/WaterDrops?style=flat-square)
![GitHub Release date](https://img.shields.io/github/release-date/paolo257428/WaterDrops?style=flat-square)
![Downloads](https://img.shields.io/github/downloads/paolo257428/WaterDrops/total?style=flat-square)
![Maintained state](https://img.shields.io/maintenance/no/2021?style=flat-square)

![WaterDrops Screenshot](./images/waterdrops.gif)

**WaterDrops** is a simple UWP (Universal Windows Platform) desktop app that works as an hydration companion, while offering other health-related features.

## Features

* Daily hydration progress tracking :heavy_check_mark:
* Periodic drink reminders (customizable in priority, time interval and water amount) :heavy_check_mark:
* Sleep reminders every day at midnight :heavy_check_mark:
* Included BMI *(Body Mass Index)* Calculator :heavy_check_mark:
* Works as a background task when minimized :heavy_check_mark:
* AutoStartup option to launch the app at Windows login :heavy_check_mark:
* Support for light/dark mode and accent colors :heavy_check_mark:
* Adaptive UI layouts :heavy_check_mark:

## Install

To install the application on your system, you should follow these steps:
 1) Download the latest *\*.zip* package from the [release](https://github.com/paolo257428/WaterDrops/releases) page
 2) Extract the archive on your computer
 3) Make sure you have app side-loading enabled on Windows (*Settings -> Updates and security -> For developers*)
 4) Install the WaterDrops certificate (*\*.cer* file) on your local computer as a `Reliable Root CA`
 5) Run the *.MSIXBUNDLE* package file and install the application (or update your app to the latest version) 

As an alternative of steps 4. and 5. you might be able to run the *Add-AppDevPackage* script in your PowerShell, and it will install the application package in the same way.

> :warning: **SECURITY WARNING** :warning:
WaterDrops, being an unofficial app, is shipped with a self-signed certificate which needs to be installed as a reliable Root Certification Authority (CA) on your system to be considered valid. CAs are an important element of IT security as they provide the "root of trust" on which all systems rely to consider an application or any piece of information as authenthic. Self-signed certificates provide a quick and easy way of delivering digitally signed content but do NOT comply with this security infrastructure, and therefore should only be used for testing or other internal purposes. **Install at your own risk**.

## Supported platforms

- **Windows 10 2004** (April 2020 Update)
- **Windows 10 20H2** (October 2020 Update)

Both ***x86*** and ***x64*** versions of the app are included in the provided *.MSIXBUNDLE* package and are available for installation.

###### NOTE: while other versions of Windows 10 may be able to run the app, compatibility has not been tested and therefore not guaranteed.

## Maintained status

This application has been developed entirely as a personal side-project, which has now been completed. Therefore it is no longer being actively maintained and, unless some breaking bugs or compatibility issues are unveiled, no support is guaranteed for by the author.

You are always free to open a [GitHub issue](https://github.com/paolo257428/WaterDrops/issues) to report any problem with the app and, in case, contribute with your own [pull requests](https://github.com/paolo257428/WaterDrops/pulls) to this project.
