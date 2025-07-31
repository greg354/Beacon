#!/bin/bash

output_dir="./bin"
mkdir -p "$output_dir"

zip_filename="files_archive.zip"

for i in {1..20}
do
    filesize=$((100 + RANDOM % 401)) # size in MB
    filename="$output_dir/file_$i.bin"
    dd if=/dev/urandom of="$filename" bs=1M count="$filesize" status=progress
    echo "Created $filename with size ${filesize}MB"
done

zip -r "$zip_filename" "$output_dir"
rm -rf "$output_dir"
echo "Compressed files into $zip_filename"