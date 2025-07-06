@echo off

if "%BASE_URL%"=="" (
	set BASE_URL=https://host.docker.internal:7070
)

docker run --rm -i grafana/k6 ^
	-e BASE_URL=%BASE_URL% ^
	run --insecure-skip-tls-verify - < lock-seat.js
