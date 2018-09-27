%echo off

cd bin\Release

echo ---------------------------
echo Pesquisar no site
echo ---------------------------
echo.

SET /p site="Website: "
SET /p term="Termo: "

start FindTermInWebsite.exe %site% %term%