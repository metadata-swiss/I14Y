Use localhost with SSL certificate
===

1. Create your own localhost certificate
---
- open git cli in `/ssl`
- execute command:
`openssl req -new -x509 -newkey rsa:2048 -sha256 -nodes -keyout localhost.key -days 3560 -out localhost.crt -config localhost-certificate.cnf`

This creates the files `localhost.crt` and `localhost.key` in the same directory.

2. Import certificate
---
In order for the browser to accept the self-signed certificate, it needs to be added to the computers trusted root certificates:
- in the cli, enter `mmc` (or `start mmc`) to open a management console
- choose _File_ --> _Add/remove snap-in_
- select _Certificates_, then _My user account_
- navigate to _Console Root_ --> _Certifificates - Current User_ --> _Trusted Root Certification Authorities_ --> _Certificates_
- right-click _Certificates_ --> _All tasks_ --> _Import_ and import the certificate `localhost.crt` created before

3. Start frontend with ssl
---

Start with either of the following commands:
- `npm run start-secure`, or
- `ng serve --ssl --ssl-cert ssl/localhost.crt --ssl-key ssl/localhost.key`
