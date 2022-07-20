# dietpi install
# su dietpi
# bash <(curl -sSL https://raw.githubusercontent.com/twistsmyth/cypher/docker/install/netDietPi.sh)
# bash <(curl -sSL https://raw.githubusercontent.com/cypher-network/cypher/master/install/install.sh)
# bash <(curl -sSL https://raw.githubusercontent.com/cypher-network/bamboo/master/install/install.sh)
# sudo systemctl status cypher-cypnode
# clibamwallet
sudo apt-get install libicu-dev libgmp-dev libsodium-dev libssl-dev libatomic1 -y
cd /tmp
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel Current
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
dotnet --version
