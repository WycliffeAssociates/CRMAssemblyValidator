# CRMAssemblyValidator
A utility to help catch common mistakes in assemblies for dynamics crm

# Desciption
Sometimes there are mistakes that are made in CRM assemblies that can be scanned for before uploading an assembly into CRM. This
is also very useful inside of PR builds.

# Building
The easiest way to build is to use Visual Studio to compile the solution.

# Usage
This is a command line tool so you are going to need to open an instance of command prompt in order to run it.

Usage:

`CRMAssemblyValidator <path to assembly>`

Example:

`CRMWebResourceUpload.exe Plugins.dll`

The above example will check the Plugins.dll file for common issues and then output them one line at a time.

All issues are outputted one line at a time. If there are any issues an exit code of 1 will be returned.

# Contributing
If you find a bug or need additional functionality please submit an issue. If you are feeling
generous or adventurous feel free to fix bugs or add functionlity in your own fork and send us
a pull request.