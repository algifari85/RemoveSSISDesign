# Remove SSIS Design
Version controll of SSIS-packages is complicated due to a lot of automatic information being generated in the sourcefile/.dtsx. 

This application makes that task easier by removing the design part of the package. It also sets the folowing attributes to default values:
* VersionBuild
* CreationDate
* CreatorName
* CreatorComputerName
* VersionGUID

The application looks for files named `*.dtsx` in the directory where the application is running. You can also specify the path to watch:
```
-folder C:\Path\To\Folder\
```

The application can run in watch-mode:
```
-watch true
```