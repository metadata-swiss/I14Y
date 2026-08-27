# Local build certificates

Empty by design. Drop a corporate root CA here **only** if you need to build the container image on
a workstation behind a TLS-inspecting proxy — `dotnet restore` inside the build container otherwise
fails with:

```
error NU1301: Unable to load the service index for source https://api.nuget.org/v3/index.json
  The remote certificate is invalid because of errors in the certificate chain: UntrustedRoot
```

The host trusts the proxy's CA; the build container has its own bundle and does not.

Export it (PowerShell, reads the store only):

```powershell
$c = Get-ChildItem Cert:\LocalMachine\Root | Where-Object { $_.Subject -like '*BIT Proxy Root CA 01*' }
[IO.File]::WriteAllBytes("certs\bit-proxy-root-ca.crt", $c.Export('Cert'))
```

Then `docker compose --profile container up -d --build`.

`*.crt` here is gitignored — never commit a corporate certificate. With this directory empty,
`update-ca-certificates` is a no-op, so CI builds are unaffected.

The same two lines in `Dockerfile.indexsearch` would fix `Dockerfile.core|admin|partner|iri` if you
need those locally too.
