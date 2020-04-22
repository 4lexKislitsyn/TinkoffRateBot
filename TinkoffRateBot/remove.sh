#!/bin/bash
if [ $# -lt 1 ]; then
  echo "not enough arguments"
  exit 2
fi
docker stop $1
docker rm $1