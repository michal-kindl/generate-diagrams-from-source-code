:: run from project root directory
:: modify paths to fit desired project location
:: parameters are [sources dir] [output dir] [showMethParams true/false] [showMethParamTypes true/false]
dotnet run -- c:\Michal\Projekty\RTT_Database\Code\RTT_UserManagementWPF\RTT_AlertingWPF c:\Michal\Projekty\UML_Diagrams\generate-diagrams-from-source-code\Docs\RTT_AlertingWPF\uml\classes  false false

:: generate diagrams as png
java -jar plantuml.jar "c:\Michal\Projekty\UML_Diagrams\generate-diagrams-from-source-code\Docs\RTT_AlertingWPF\uml\classes\*.plantuml" "c:\Michal\Projekty\UML_Diagrams\generate-diagrams-from-source-code\Docs\RTT_AlertingWPF\uml\classes"