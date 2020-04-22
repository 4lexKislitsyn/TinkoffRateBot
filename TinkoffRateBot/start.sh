#!/bin/bash
if [ $# -lt 2 ]; then
  echo "not enough arguments (image version, container name)"
  exit 2
fi
docker build --tag tinkoffratebot:$1 .
docker run --detach --restart always --name $2 tinkoffratebot:$1