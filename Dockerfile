FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-cbl-mariner2.0

WORKDIR /app

ARG BIN_NAME=TelegramMonitor
ARG TARGETARCH

COPY out/linux-${TARGETARCH}/${BIN_NAME} /app/${BIN_NAME}


EXPOSE 5005

ENTRYPOINT ["/app/TelegramMonitor"]
