# App

docker run --rm --name=postfix -e MTP_HOST=app.io -p 587:25 -d eeacms/postfix