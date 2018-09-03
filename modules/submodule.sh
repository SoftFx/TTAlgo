if [ ! $# -eq 1 ]
then
  echo 'USAGE: supply one of the following commands: init, refresh, update'
elif [ $1 = 'init' ]
then
  git submodule update --init --recursive --remote
elif [ $1 = 'refresh' ]
then
  git pull
  git submodule update --init --recursive --remote
  git pull --recurse-submodules
  git submodule foreach '(git checkout develop; git pull)'
elif [ $1 = 'update' ]
then
  git pull
  git submodule update --init --recursive --remote
  git pull --recurse-submodules
  git submodule foreach '(git checkout develop; git pull)'
  git add --all
  git commit -m 'Submodule Sync'
  git push
else
  echo 'USAGE: supply one of the following commands: init, refresh, update'
fi
