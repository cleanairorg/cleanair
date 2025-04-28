FROM nginx:alpine

# Remove default nginx configurations
RUN rm -rf /etc/nginx/nginx.conf /etc/nginx/conf.d/*

# Copy your custom nginx.conf
COPY client/nginx.conf /etc/nginx/nginx.conf

# Copy the frontend build
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]