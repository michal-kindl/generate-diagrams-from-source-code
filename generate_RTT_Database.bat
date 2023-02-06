:: run from project root directory
:: modify paths to fit desired project location
:: parameters are [sources dir] [output dir] [showMethParams true/false] [showMethParamTypes true/false]
dotnet run -- c:\Michal\Projekty\RTT_Database\Code\RTT_Database c:\Michal\Projekty\RTT_Database\Code\RTT_Database\Documentation\Docs\uml\classes true true

:: generate diagrams as png
java -jar plantuml.jar "c:\Michal\Projekty\RTT_Database\Code\RTT_Database\Documentation\Docs\uml\classes\*.plantuml" "c:\Michal\Projekty\RTT_Database\Code\RTT_Database\Documentation\Docs\uml\classes"
