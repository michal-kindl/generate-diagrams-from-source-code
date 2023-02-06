:: run from project root directory
:: parameters are [sources dir] [output dir]
dotnet run -- c:\Michal\Projekty\RTT_UI c:\Michal\Projekty\RTT_UI\Docs\uml true false

:: generate png
java -jar plantuml.jar "c:\Michal\Projekty\RTT_UI\Docs\uml\*.plantuml" "c:\Michal\Projekty\RTT_UI\Docs\uml"

