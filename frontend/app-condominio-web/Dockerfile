FROM node:20-alpine AS build
WORKDIR /app
COPY frontend/app-condominio-web/package*.json ./
RUN npm install
COPY frontend/app-condominio-web/ ./
RUN npm run build

FROM nginx:1.27-alpine
COPY frontend/app-condominio-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/app-condominio-web/browser /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
