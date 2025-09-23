#!/bin/bash

MICRONAUT_PATH=$1

VMIMAGES_PATH="../VmImages"
FALLBACK_PATH_FILE="/tmp/mpath"
FALLBACK_PATH_FILE_VM_IMAGES="/tmp/ipath"

touch $FALLBACK_PATH_FILE $FALLBACK_PATH_FILE_VM_IMAGES

if [[ ! -d "$VMIMAGES_PATH" ]]; then
  echo "relative route to VmImages not found. Searching / This may take a while..."
  find / -type d -name VmImages -print0 2>/dev/null | while IFS= read -r -d '' line; do
    if ! [[ $line =~ "/var/lib/docker" || $line =~ "/share/trash" ]]; then
      echo $line > $FALLBACK_PATH_FILE_VM_IMAGES
      break
    fi
  done
  VMIMAGES_PATH=$(cat $FALLBACK_PATH_FILE_VM_IMAGES)
fi


echo $VMIMAGES_PATH

if [[ ! -d "$VMIMAGES_PATH" ]]; then
  echo "VmImages directory not found. Recommend replacing this project with a fresh clone from https://github.com/s27920/backend.git. Otherwise add directory backend/ExecutorService/VmImages"
  exit 1
fi

if [ -d $VMIMAGES_PATH ]; then
  rm -rf $VMIMAGES_PATH/compiler-fs.ext4
  rm -rf $VMIMAGES_PATH/executor-fs.ext4
fi

if [[ -z "$MICRONAUT_PATH" ]]; then
  echo "no path received in arg1. Searching / This may take a while..."
    find / -type d -name MicronautCompilerService -print0 2>/dev/null | while IFS= read -r -d '' line; do
      if ! [[ $line =~ "/var/lib/docker" || $line =~ "/share/trash" ]]; then
        if [[ -n $(find "$line" -name "gradlew" | head -1) ]]; then
          echo "$line" > $FALLBACK_PATH_FILE
          break  
        fi
      fi
    done
  MICRONAUT_PATH=$(cat $FALLBACK_PATH_FILE | head -1)
fi

echo $MICRONAUT_PATH

rm -f "$FALLBACK_PATH_FILE"

if [[ -z "$MICRONAUT_PATH" ]]; then
  echo "Micronaut compiler microservice not found. Get it at https://github.com/s27920/MicronautCompilerService.git"
  exit 1
fi

echo "building compiler service"

cd $MICRONAUT_PATH
/bin/bash gradlew build 
cd -
echo "$VMIMAGES_PATH"
cd $VMIMAGES_PATH
cd ../VmBuild

/bin/bash build-compiler.sh "$MICRONAUT_PATH/build/libs/compiler-0.1-all.jar"
/bin/bash build-executor.sh 

if [ -d ../VmImages ]; then
  mv compiler-fs.ext4 ../VmImages/compiler-fs.ext4
  mv executor-fs.ext4 ../VmImages/executor-fs.ext4
else  
  echo "VmImages directory not found"
  exit 1
fi