.PHONY: default build

default: build

build:
	dotnet build src/

test:
	dotnet run --configuration Release --project src/RSFlowControl.Tests/RSFlowControl.Tests.csproj --coverage --report-trx

clean-release:
	rm -rf src/RSFlowControl/bin/Release/

publish: clean-release
	dotnet pack src/RSFlowControl --configuration Release
	dotnet nuget push src/RSFlowControl/bin/Release/*.nupkg --api-key $(NUGET_KEY) --source https://api.nuget.org/v3/index.json
