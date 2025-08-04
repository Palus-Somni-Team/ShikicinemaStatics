#!/bin/bash
#
# Will convert an image from specified path to AVIF format and store it near same directory
#
# HOW TO USE:
#
# ./convert-avif <image path> <quality of convertion>
#
# DEPENDECIES:
#
# - ImageStick (https://imagemagick.org/script/magick.php)

targetFile=${1}
quality=${2:-50}

[ -n "$targetFile" ] || { echo >&2 "specify image path to convert to AVIF"; exit 1; }
[ -e "$targetFile" ] || { echo >&2 "file does not exists"; exit 2; }
[ "$quality" -gt 0 ] || { echo >&2 "0 < quality ≤ 100"; exit 3; }
[ "$quality" -le 100 ] || { echo >&2 "0 < quality ≤ 100"; exit 3; }

file=$(realpath "$targetFile")
dir=$(dirname "$file")
extension="${file#*.}"
name=$(basename ${file%.$extension})

convert "$file" -quality "$quality" -define avif:speed=10 "$dir/${name}.avif"
