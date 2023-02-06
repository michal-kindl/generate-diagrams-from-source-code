:: run from project root directory
:: modify paths to fit desired project location
:: parameters are [sources dir] [output dir] [showMethParams true/false] [showMethParamTypes true/false]
dotnet run -- c:\Michal\Projekty\RTT_UI c:\Michal\Projekty\RTT_UI\Docs\uml true false

:: generate diagrams as png
java -jar plantuml.jar "c:\Michal\Projekty\RTT_UI\Docs\uml\*.plantuml" "c:\Michal\Projekty\RTT_UI\Docs\uml"

