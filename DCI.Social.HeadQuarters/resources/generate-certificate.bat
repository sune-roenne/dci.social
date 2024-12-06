openssl req -x509 -nodes -days 730 -newkey rsa:2048 -keyout hq.key -out hq.crt -config openssl.conf -extensions v3_req
openssl pkcs12 -export -out hq.pfx -inkey hq.key -in hq.crt -password pass:%PSWD%