FROM node:14-alpine as build
WORKDIR /build
ADD . .

RUN apk add git

RUN npm ci &&\
    npm run build

# ----------------------------------------------------------

FROM nginx:mainline-alpine AS final
LABEL maintainer="zekro <contact@zekro.de>"

WORKDIR /app
COPY --from=build /build/build .
ADD ./config/nginx.conf /etc/nginx/conf.d/voidsearch.conf

EXPOSE 8080
