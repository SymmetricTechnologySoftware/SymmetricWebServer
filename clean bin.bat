SET destination="Symmetric Web Server Clean/bin/"
SET destinationWebContent="Symmetric Web Server Clean/bin/Web_Content/"
SET destinationLicenses="Symmetric Web Server Clean/bin/Licenses/"
SET destinationNorthwind="Symmetric Web Server Clean/Northwind Example/"

rd /s %destination%

mkdir %destination%
mkdir %destinationWebContent%

xcopy /f /y "bin/Symmetric Web Server.exe" %destination%
xcopy /f /y "bin/Enyim.Caching.dll" %destination%
xcopy /f /y "bin/Mono.Data.Sqlite.dll" %destination%
xcopy /f /y "bin/MySql.Data.dll" %destination%
xcopy /f /y "bin/Nancy.Authentication.Forms.dll" %destination%
xcopy /f /y "bin/Nancy.Bootstrappers.StructureMap.dll" %destination%
xcopy /f /y "bin/Nancy.dll" %destination%
xcopy /f /y "bin/Nancy.Hosting.Self.dll" %destination%
xcopy /f /y "bin/Nancy.Sessions.dll" %destination%
xcopy /f /y "bin/Nancy.Sessions.Memcache.dll" %destination%
xcopy /f /y "bin/sqlite3.dll" %destination%
xcopy /f /y "bin/StructureMap.dll" %destination%
xcopy /f /y "bin/StructureMap.Net4.dll" %destination%
xcopy /f /y "bin/System.Data.Portable.dll" %destination%
xcopy /f /y "bin/System.Transactions.Portable.dll" %destination%
xcopy /f /y "bin/UserShared.dll" %destination%

xcopy /f /y /s /EXCLUDE:exclusion.txt "bin/Web_Content/*.*" %destinationWebContent%
xcopy /f /y /s "bin/Licenses/*.*" %destinationLicenses%
xcopy /f /y /s "Northwind Example/*.*" %destinationNorthwind%

pause