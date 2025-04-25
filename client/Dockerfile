# client/Dockerfile

# === BUILD STAGE ===
# Bruger Node.js til at bygge frontend applikationen
FROM node:20-alpine AS build

# Sætter arbejdsmappe
WORKDIR /app

# Kopierer package filer først for at cache npm install
COPY client/package*.json ./

# Installerer dependencies (npm ci er mere deterministisk end npm install)
RUN npm ci

# Kopierer al frontend kildekode
COPY client/ ./

# Sætter build-time variabel for API URL (bruges af Vite under build)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}

# Bygger produktions-version af frontend
RUN npm run build

# === RUNTIME STAGE ===
# Bruger Nginx til at serve statiske filer
FROM nginx:alpine

# Kopierer bygget frontend fra build stage til Nginx's public folder
COPY --from=build /app/dist /usr/share/nginx/html

# Kopierer custom Nginx konfiguration
COPY client/nginx.conf /etc/nginx/nginx.conf

# Eksponerer port 80 (standard HTTP)
EXPOSE 80

# Starter Nginx i forgrund (nødvendigt for Docker)
CMD ["nginx", "-g", "daemon off;"]