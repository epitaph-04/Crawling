install-dependency:
	brew install k3d

uninstall-dependency:
	brew uninstall k3d

create-cluster:
	k3d cluster create livematch-dev

delete-cluster:
	k3d cluster delete livematch-dev

build:
	dotnet build
	dotnet test

run-api:
	dotnet run --project Api/Api.csproj --urls http://+:80

run-livetv:
	dotnet run --project LiveTvWorker/LiveTvWorker.csproj --urls http://+:5000

run-sport365:
	dotnet run --project Sport365Worker/Sport365Worker.csproj --urls http://+:5001

docker-deploy:
	docker-compose up --build

docker-deploy-clean:
	docker-compose up --build

docker-dev-deploy:
	docker-compose -f docker-compose.yml -f docker-compose.Build.yml up --build

docker-dev-deploy-clean:
	docker-compose -f docker-compose.yml -f docker-compose.Build.yml down