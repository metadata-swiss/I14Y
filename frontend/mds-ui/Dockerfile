FROM node:24-alpine

WORKDIR /app

RUN apk add --no-cache python3 make git curl

# 1. Copy root package files and the UI workspace package.json
COPY package.json package-lock.json ./
COPY opendata.swiss/ui/package.json ./opendata.swiss/ui/

# 2. Install workspace dependencies
RUN npm ci --workspace=opendata.swiss/ui --include-workspace-root

# 3. Copy the rest of the UI workspace files
COPY opendata.swiss/ui ./opendata.swiss/ui

# 4. Set the working directory to the UI folder for entrypoint / scripts
WORKDIR /app/opendata.swiss/ui

ENV PORT=80
ENV HOST=0.0.0.0

EXPOSE 80

CMD ["sh", "-c", "./entrypoint.sh"]
