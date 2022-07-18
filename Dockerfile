FROM archlinux AS build-env
WORKDIR /app

RUN pacman -Sy dotnet-runtime
bash <(curl -sSL https://raw.githubusercontent.com/twistsmyth/cypher/master/install/installDocker.sh)

