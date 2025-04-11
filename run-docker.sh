#!/bin/bash
# ./run-docker.sh '/Users/jacobingalls/Downloads/What Lurks Below (v1.1.1)/What Lurks Below'

docker run -it --rm -p 80:80 \
    -v "$1:/var/www/unity-webgl" \
    -v "$PWD/unity-nginx.conf:/etc/nginx/nginx.conf" \
    nginx