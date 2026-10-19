// Swagger UI for /input-backend/openapi.json. "Try it out" calls /input-backend/, like the pages do.
// Swagger UI comes from cdnjs, pinned, with subresource integrity. It must be 5.18 or later: iop-core's document is
// OpenAPI 3.0.4.
const SWAGGER_UI = 'https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/5.29.1'

const html = `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta name="robots" content="noindex">
  <title>opendata.swiss API</title>
  <link rel="stylesheet" href="${SWAGGER_UI}/swagger-ui.min.css"
    integrity="sha384-OFyZ/BC2LThtP2XDBMEhSMeKdG9gk7udSIr1NlawKM1XL1APWlsIi856OIALfO5Q" crossorigin="anonymous">
</head>
<body>
  <div id="swagger-ui"></div>
  <script src="${SWAGGER_UI}/swagger-ui-bundle.min.js"
    integrity="sha384-o1NjSPoS/7sblYrybP8KfXpC7xOOIeuc538Mb1/4/307KIQPLwqmOWvrSS5e1T5s" crossorigin="anonymous"></script>
  <script>
    SwaggerUIBundle({ url: '/input-backend/openapi.json', dom_id: '#swagger-ui', supportedSubmitMethods: ['get'] })
  </script>
</body>
</html>
`

export default defineEventHandler((event) => {
  setResponseHeader(event, 'content-type', 'text/html; charset=utf-8')
  return html
})
