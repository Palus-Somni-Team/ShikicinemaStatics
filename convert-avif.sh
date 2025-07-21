#!/bin/bash
#
# Will convert all image files from specified directory to AVIF format
#
# HOW TO USE:
#
# ./convert-avif <path to static images directory> <file extension>
#
# DEPENDECIES:
#
# - ImageStick

targetDir=${1?:'specify target directory path'}
extension=${2:-jpeg}

for image in $targetDir/*.$extension; do
        file=$(realpath "$image")

        [ -e "$file" ] || continue

        animeId=$(basename ${file%.$extension})
        convert "$file" -quality 20 -define avif:speed=10 "$targetDir/${animeId}.avif"
        echo "'$file' -> '${animeId}.avif'"
        sleep .5
done
