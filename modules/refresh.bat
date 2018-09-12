cd ..
git pull
git submodule update --init --recursive --remote
git pull --recurse-submodules
git submodule foreach "(git checkout develop; git pull)"
cd modules
