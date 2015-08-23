#! /bin/bash

if [ "$#" -ne 1 ]; 
then echo "usage: make_release version.subversion"
     exit 1
fi

dir=UnityProlog-$1
mkdir $dir
cp ../Prolog/bin/Release/Prolog.dll ../Documentation/* $dir
zip -ur $dir.zip $dir
rm -r $dir
