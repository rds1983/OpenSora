dotnet --version
dotnet build build\OpenSora.sln /p:Configuration=Release --no-incremental
call copy_zip_package_files.bat
rename "ZipPackage" "OpenSora.%APPVEYOR_BUILD_VERSION%"
7z a OpenSora.%APPVEYOR_BUILD_VERSION%.zip OpenSora.%APPVEYOR_BUILD_VERSION%
