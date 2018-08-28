cd ..
git pull
git submodule update --init --recursive --remote
git pull --recurse-submodules
git submodule foreach "(git checkout develop; git pull)"
git add --all
git commit -m "Submodule Sync"
git push
cd modules
